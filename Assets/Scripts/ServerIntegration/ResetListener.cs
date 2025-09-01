using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.ServerIntegration
{
    public class ResetListener : Singleton<ResetListener>
    {
        private ServerCommunicator m_communicator;
        private Coroutine _pollCoroutine;

        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetReset);
        }

        public void StartListening()
        {
            Debug.Log($"📱 ResetListener.StartListening | platform = {Application.platform}");

            if (m_communicator.m_isRunning) return;

            Debug.Log("🎧 Starting coroutine polling for reset...");
            m_communicator.m_isRunning = true;
            _pollCoroutine = StartCoroutine(PollCoroutine());
        }

        public void StopListening()
        {
            if (!m_communicator.m_isRunning) return;

            Debug.Log("🛑 ResetListener.StopListening called");
            m_communicator.m_isRunning = false;

            if (_pollCoroutine != null)
            {
                StopCoroutine(_pollCoroutine);
                _pollCoroutine = null;
                Debug.Log("🛑 Reset poll coroutine stopped");
            }
        }

        private IEnumerator PollCoroutine()
        {
            Debug.Log("🚀 Reset PollCoroutine started");

            while (true)
            {
                Debug.Log("⏳ Polling server for reset...");

                using (UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl))
                {
                    yield return request.SendWebRequest();

                    Debug.Log($"📡 Reset response | Code={(int)request.responseCode} | Result={request.result}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        if ((int)request.responseCode == 200)
                        {
                            Debug.Log("✅ Reset 200 OK received, parsing JSON...");
                            string receivedJson = request.downloadHandler.text;
                            Debug.Log($"📥 Reset payload: {receivedJson}");

                            try
                            {
                                var settings = new JsonSerializerSettings();
                                settings.Converters.Add(new OperatorConverter());

                                Dictionary<string, int> result =
                                    JsonConvert.DeserializeObject<Dictionary<string, int>>(receivedJson, settings);

                                SequenceManager.Instance.SetSequence(result["seqNumber"]);
                                CoroutineRunner.Instance.StartCoroutine(SequenceManager.Instance.RestartSequence());
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"❌ Reset JSON parse failed: {ex.Message}");
                            }
                        }
                        else if ((int)request.responseCode == 204)
                        {
                            Debug.Log("⏳ Reset → 204 No Content, continue polling...");
                        }
                        else
                        {
                            Debug.LogError($"❌ Reset unexpected response: {(int)request.responseCode} | {request.error}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"❌ Reset poll request failed: {request.error}");
                    }
                }

                Debug.Log($"🔁 Reset loop complete, waiting {m_communicator.pollRateMilliSeconds} ms...");
                yield return new WaitForSecondsRealtime(m_communicator.pollRateMilliSeconds / 1000f);
            }
        }
    }
}
