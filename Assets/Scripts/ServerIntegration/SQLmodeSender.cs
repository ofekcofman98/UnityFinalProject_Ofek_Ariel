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
    public class SQLmodeSender : Singleton<SQLmodeSender>
    {
        private ServerCommunicator m_communicator;

        public SQLmodeSender()
        {
            m_communicator = new ServerCommunicator("/send-sqlmode");
        }

        public void SendSQLmodeToPhone()
        {
            if (!Application.isMobilePlatform)
            {
                Debug.Log("Sending SQLmode to server...");
                // Construct the payload with the correct key and value
                var payload = new Dictionary<string, bool>
                 {
                    { "sqlmode", GameManager.Instance.SqlMode }
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
                        Debug.Log($"✅ SQLState Sent Successfully! Response: {request.downloadHandler.text}");
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
