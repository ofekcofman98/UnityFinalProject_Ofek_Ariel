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
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.ServerIntegration
{
    public class GameProgressSender : Singleton<GameProgressSender>
    {
        public GameProgressContainer m_progressContainer;
        private ServerCommunicator m_communicator;
        public event Action OnGameFetchComplete;

        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendGameProgress);
            OnGameFetchComplete += OnGameFetchCompleteAction;
        }



        public IEnumerator SendGameProgressToServer(GameProgressContainer gpc)
        {
            m_progressContainer = gpc;
            Debug.Log($"📤 m_gameKey value after function and before payload: {UniqueKeyManager.Instance.gameKey}");
            var payload = new Dictionary<string, object>
                 {
                    { "game", m_progressContainer },
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
                Debug.Log($"✅ GameProgressContainer contains : lives {gpc.Lives}, currentMissionIndex {gpc.currentMissionIndex}, gameCode : {UniqueKeyManager.Instance.gameKey}");
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
                SequenceManager.Instance.SetSequence(m_progressContainer.sequenceIndex);
                MissionsManager.Instance.LoadMissionSequence(SequenceManager.Instance.Current);

                MissionsManager.Instance.SetStatsFromLoadedGame(m_progressContainer.sequenceIndex, m_progressContainer.Lives, m_progressContainer.currentMissionIndex);
                GameManager.Instance.StartMissions();
                StateSender.Instance.UpdatePhone();
            }
            else
                Debug.Log("gps or container are null !");

        }

        public IEnumerator GetSavedGameFromServer(string key)
        {
            Debug.Log($"📤 m_gameKey value before sending a getGameProgress request: {key}");
            var payload = new Dictionary<string, string>
            {
                { "key", key }
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
                    UniqueKeyManager.Instance.SetGameKeyFromSavedGame(key);
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

        public void ValidateKeyAndLoadGame(string key, Action<bool> onValidationComplete)
        {
            StartCoroutine(ValidateKeyAndLoadGameCoroutine(key, onValidationComplete));
        }

        private IEnumerator ValidateKeyAndLoadGameCoroutine(string key, Action<bool> onValidationComplete)
        {
            UniqueKeyManager.Instance.SetGameKeyFromSavedGame(key);
            UnityWebRequest request = UnityWebRequest.Get(new ServerCommunicator(ServerCommunicator.Endpoint.ValidateKey).ServerUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                onValidationComplete?.Invoke(request.responseCode == 200);
            }


        }

        public bool IsSameAsLastSave(GameProgressContainer newContainer)
        {
            return m_progressContainer != null &&
                   m_progressContainer.sequenceIndex == newContainer.sequenceIndex &&
                   m_progressContainer.currentMissionIndex == newContainer.currentMissionIndex &&
                   m_progressContainer.Lives == newContainer.Lives;
        }

    }
}
