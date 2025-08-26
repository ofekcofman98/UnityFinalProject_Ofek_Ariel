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
    public class ConnectSender : Singleton<ConnectSender>
    {
        private ServerCommunicator m_communicator;

        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendConnect);
        }

        public void SendConnectToPC(string key)
        {
            if (Application.isMobilePlatform)
            {
                Debug.Log("sending connect request to server");
                var payload = new Dictionary<string, string>
                 {
                    { "key", key }
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
                        Debug.Log($"✅ Connect Sent Successfully! key sent: {key}. Response: {request.downloadHandler.text}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to send connect: {request.responseCode} | {request.error}");
                        Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
                    }
                };
            }
        }

    }
}
