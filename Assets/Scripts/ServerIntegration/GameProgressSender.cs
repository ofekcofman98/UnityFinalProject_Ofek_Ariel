using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Text;
using System;
using Newtonsoft.Json;
using Assets.Scripts.ServerIntegration;

namespace Assets.Scripts.ServerIntegration
{
    public class GameProgressSender : Singleton<GameProgressSender>
    {
        private GameProgressContainer m_progressContainer;
        private ServerCommunicator m_communicator;
        private string m_gameKey = "12345";
        private bool m_isGameSaved = false;

        // private void Awake()
        // {
        //    // m_gameKey = DeviceKeyManager.GetOrCreateDeviceKey();
        // }
        public GameProgressSender()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendGameProgress);
        }



        public IEnumerator SendGameProgressToServer(GameProgressContainer gpc)
        {
            m_progressContainer = gpc;
            var payload = new Dictionary<string, object>
                 {
                    { "game", m_progressContainer },
                    { "key" , m_gameKey }
                 };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            Debug.Log($"📤 JSON Payload: {jsonPayload}");


            var encoding = new System.Text.UTF8Encoding();
            byte[] bodyRaw = encoding.GetBytes(jsonPayload);

            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendGameProgress);

            UnityWebRequest request = new UnityWebRequest(m_communicator.ServerUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer(),
                method = UnityWebRequest.kHttpVerbPOST
            };

            request.disposeUploadHandlerOnDispose = true;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Content-Length", bodyRaw.Length.ToString());

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ GameProgressContainer Sent Successfully! Response: {request.downloadHandler.text}");
                Debug.Log($"✅ GameProgressContainer contains : lives {gpc.Lives}, currentMissionIndex {gpc.currentMissionIndex}, SQLmode {gpc.SqlMode}");

                m_isGameSaved = true;
            }
            else
            {
                Debug.LogError($"❌ Failed to send container: {request.responseCode} | {request.error}");
                Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
            }
        }

        public IEnumerator GetSavedGameFromServer(Action<GameProgressContainer> onComplete)
        {
            var payload = new Dictionary<string, string>
            {
                { "key", m_gameKey }
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            Debug.Log($"📤 JSON Payload: {jsonPayload}");

            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetGameProgress);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            UnityWebRequest request = new UnityWebRequest(m_communicator.ServerUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer(),
                method = UnityWebRequest.kHttpVerbPOST
            };

            request.disposeUploadHandlerOnDispose = true;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Content-Length", bodyRaw.Length.ToString());

            yield return request.SendWebRequest(); // ✅ wait for response!

            if (request.result == UnityWebRequest.Result.Success)
            {
                string receivedJson = request.downloadHandler.text;
                Debug.Log($"📥 Received JSON: {receivedJson}");

                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new OperatorConverter());

                    GameProgressContainer result = JsonConvert.DeserializeObject<GameProgressContainer>(receivedJson, settings);
                    Debug.Log("✅ Game object deserialized");

                    onComplete?.Invoke(result);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Deserialization error: {ex.Message}");
                    onComplete?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"❌ Server request failed: {request.responseCode} | {request.error}");
                onComplete?.Invoke(null);
            }
        }

        //public GameProgressContainer GetSavedGameFromServer()
        //{

        //    GameProgressContainer result = new GameProgressContainer(GameManager.Instance.SqlMode, GameManager.Instance.missionManager.currentMissionIndex, GameManager.Instance.missionManager.m_Lives);
        //    var payload = new Dictionary<string, string>
        //         {
        //            { "key" , m_gameKey }
        //         };

        //    string jsonPayload = JsonConvert.SerializeObject(payload);
        //    Debug.Log($"📤 JSON Payload: {jsonPayload}");

        //    m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetGameProgress);
        //    var encoding = new System.Text.UTF8Encoding();
        //    byte[] bodyRaw = encoding.GetBytes(jsonPayload);

        //    UnityWebRequest request = new UnityWebRequest(m_communicator.ServerUrl, "POST")
        //    {
        //        uploadHandler = new UploadHandlerRaw(bodyRaw),
        //        downloadHandler = new DownloadHandlerBuffer(),
        //        method = UnityWebRequest.kHttpVerbPOST
        //    };

        //    request.disposeUploadHandlerOnDispose = true;
        //    request.SetRequestHeader("Content-Type", "application/json");
        //    request.SetRequestHeader("Content-Length", bodyRaw.Length.ToString());

        //    request.SendWebRequest();

        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string receivedJson = request.downloadHandler.text;

        //        try
        //        {
        //            var settings = new JsonSerializerSettings();
        //            settings.Converters.Add(new OperatorConverter());

        //            result = JsonConvert.DeserializeObject<GameProgressContainer>(receivedJson, settings);

        //            if (result != null && !string.IsNullOrWhiteSpace(result.ToString()))
        //            {
        //                Debug.Log($"✅ Game Object received : {result.ToString()}");

        //                //receivedContainer.PostDeserialize();
        //                //GameManager.Instance.SaveQuery(receivedContainer);
        //                //GameManager.Instance.ExecuteLocally(receivedContainer);

        //            }
        //            else
        //            {
        //                Debug.Log("⏳ Received container object is empty.");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.LogError($"❌ an error has occured : {ex.Message}");
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("⏳ Received container object is empty.");
        //    }


        //    return result;
        //}

    }
}
