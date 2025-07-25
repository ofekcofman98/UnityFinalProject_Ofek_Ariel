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
        private string m_gameKey;
        private bool m_isGameSaved = false;

        public GameProgressSender()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendGameProgress);
        }



        public IEnumerator SendGameProgressToServer(GameProgressContainer gpc)
        {
            m_progressContainer = gpc;
            var payload = new Dictionary<string, string>
                 {
                    { "game", JsonConvert.SerializeObject(m_progressContainer, JsonUtility.Settings) },
                    { "key" , m_gameKey }
                 };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            Debug.Log($"📤 JSON Payload: {jsonPayload}");


            var encoding = new System.Text.UTF8Encoding();
            byte[] bodyRaw = encoding.GetBytes(jsonPayload);

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
                Debug.Log($"✅ Query Sent Successfully! Response: {request.downloadHandler.text}");
                m_isGameSaved = true;
            }
            else
            {
                Debug.LogError($"❌ Failed to send query: {request.responseCode} | {request.error}");
                Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
            }
        }
    }
}
