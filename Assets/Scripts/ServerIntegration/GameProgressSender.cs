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
        public GameProgressContainer m_progressContainer;
        private ServerCommunicator m_communicator;
        public event Action OnGameFetchComplete;
        private string m_gameKey;
        private bool m_isGameSaved = false;

        
        public GameProgressSender()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendGameProgress);
            OnGameFetchComplete += OnGameFetchCompleteAction;
        }

        
        private IEnumerator getUniqueKey()
        {
            UnityWebRequest request = UnityWebRequest.Get(new ServerCommunicator(ServerCommunicator.Endpoint.GenerateKey).ServerUrl);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string receivedJson = request.downloadHandler.text;
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new OperatorConverter());

                    Dictionary<string,string> result = JsonConvert.DeserializeObject<Dictionary<string,string>>(receivedJson, settings);
                    m_gameKey = result["key"];
                    Debug.Log($"gameKey value returned from server : {m_gameKey}");

                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Failed to parse full Query object: {ex.Message}");
                }
            }
            else if (request.responseCode == 204)
            {
                Debug.Log("⏳ Server responded with 204 No Content — no new query yet.");
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch query: {request.responseCode} | {request.error}");
            }


        }

        public IEnumerator SendGameProgressToServer(GameProgressContainer gpc)
        {
            if (m_isGameSaved) 
            {
                Debug.LogError($"Game/container is already sent.");
                yield return null;

            }
            m_progressContainer = gpc;
            yield return StartCoroutine(getUniqueKey());
            Debug.Log($"📤 m_gameKey value after function and before payload: {m_gameKey}");
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
                Debug.Log($"✅ GameProgressContainer contains : lives {gpc.Lives}, currentMissionIndex {gpc.currentMissionIndex}, gameCode : {m_gameKey}");

                m_isGameSaved = true;
            }
            else
            {
                Debug.LogError($"❌ Failed to send container: {request.responseCode} | {request.error}");
                Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
            }
        }

        private void OnGameFetchCompleteAction()
        {
            if (m_progressContainer != null)
            {
                MissionsManager.Instance.LoadMissionSequence(m_progressContainer.sequenceIndex == 1 ? GameManager.Instance.MainGameSequence : GameManager.Instance.TutorialSequence);
                MissionsManager.Instance.SetStatsFromLoadedGame(m_progressContainer.sequenceIndex, m_progressContainer.Lives, m_progressContainer.currentMissionIndex);
                GameManager.Instance.StartMissions();
                StateSender.Instance.UpdatePhone();
            }
            else
                Debug.Log("gps or container are null !");
            
        }
        public IEnumerator GetSavedGameFromServer()
        {
            Debug.Log($"📤 m_gameKey value before sending a getGameProgress request: {m_gameKey}");
            var payload = new Dictionary<string, string>
            {
                { "key", "183673" }
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

                    m_progressContainer = JsonConvert.DeserializeObject<GameProgressContainer>(receivedJson, settings);
                    Debug.Log("✅ Game object deserialized and placed into the object's container !");
                    OnGameFetchComplete.Invoke();


                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Deserialization error: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"❌ Server request failed: {request.responseCode} | {request.error}");
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
