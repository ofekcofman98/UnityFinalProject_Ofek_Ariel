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
    public class StateSender : Singleton<StateSender>
    {
        private ServerCommunicator m_communicator;
        private CancellationTokenSource _cts;
        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendState);
        }

        public void UpdatePhone()
        {
            if (!m_communicator.IsMobile)
            {
                var payload = new Dictionary<string, int>
                 {
                    { "seqNumber", SequenceManager.Instance.GetCurrentIndex() } ,
                    { "currentLevelIndex", MissionsManager.Instance.currentMissionIndex }
                 };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                Debug.Log($"📤 JSON Payload: {jsonPayload}");

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

                UnityWebRequest request = new UnityWebRequest(m_communicator.ServerUrl, "POST")
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

