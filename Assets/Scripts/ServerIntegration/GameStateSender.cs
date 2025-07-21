using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading;


namespace Assets.Scripts.ServerIntegration
{
    public class GameStateSender : Singleton<GameStateSender>
    {
        private const string k_pcIP = ServerData.k_pcIP;
        private string serverUrl;
        private bool m_isMobile = Application.isMobilePlatform;
        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        public GameStateSender()
        {
            serverUrl = "https://python-query-server-591845120560.us-central1.run.app/send-state";
        }

        public void UpdatePhone()
        {
            if (!m_isMobile)
            {
                // Construct the payload with the correct key and value
                var payload = new Dictionary<string, bool>
                 {
                    { "isLevelDone", true }
                 };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                Debug.Log($"📤 JSON Payload: {jsonPayload}");

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

                UnityWebRequest request = new UnityWebRequest(serverUrl, "POST")
                //UnityWebRequest request = new UnityWebRequest("https://python-query-server-591845120560.us-central1.run.app/send-state", "POST")
                {
                    uploadHandler = new UploadHandlerRaw(bodyRaw),
                    downloadHandler = new DownloadHandlerBuffer()
                };

                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                request.SetRequestHeader("Content-Type", "application/json");

                // Send request asynchronously
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                operation.completed += _ =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"✅ State Sent Successfully! Response: {request.downloadHandler.text}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to send state: {request.responseCode} | {request.error}");
                        Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
                    }
                };
            }
        }


    }

}

