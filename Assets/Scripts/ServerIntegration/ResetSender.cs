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
    public class ResetSender : Singleton<ResetSender>
    {
        private ServerCommunicator m_communicator;

        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendReset);
        }

        public void SendResetToPhone()
        {
            if (!Application.isMobilePlatform)
            {
                Debug.Log("SENDING RESET MESSAGE TO SERVER");
                // Construct the payload with the correct key and value
                var payload = new Dictionary<string, int>
                 {
                    { "seqNumber", SequenceManager.Instance.GetCurrentIndex() }
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

        public IEnumerator ResetServerOnDestroy()
        {
            UnityWebRequest request = UnityWebRequest.Get(new ServerCommunicator(ServerCommunicator.Endpoint.ServerReset).ServerUrl);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Server contents have been reset upon destruction");
            }           
            else
            {
                Debug.LogError($"❌ An error has occured : {request.responseCode} | {request.error}");
            }


        }
    }
}
