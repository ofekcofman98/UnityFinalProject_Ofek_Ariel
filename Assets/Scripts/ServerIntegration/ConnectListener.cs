using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.ServerIntegration
{
    public class ConnectListener : Singleton<ConnectListener>
    {
        private ServerCommunicator m_communicator;
        private Coroutine _pollCoroutine;

        private void Awake()
        {
            Debug.Log("🟢 ConnectListener.Awake() called");
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetConnect);
        }

        public void StartListening()
        {
            Debug.Log($"📱 StartListening() | m_isMobile = {m_communicator.IsMobile} | platform = {Application.platform}");

            if (m_communicator.m_isRunning)
            {
                Debug.Log("⚠️ StartListening() ignored — already running.");
                return;
            }

            Debug.Log("🎧 Starting coroutine polling for new connect updates...");
            m_communicator.addGameKeyAsQueryParams();
            m_communicator.m_isRunning = true;

            _pollCoroutine = StartCoroutine(PollCoroutine());
        }

        public void StopListening()
        {
            Debug.Log("🛑 StopListening() called");
            if (!m_communicator.m_isRunning)
            {
                Debug.Log("⚠️ StopListening() ignored — not running.");
                return;
            }

            m_communicator.m_isRunning = false;

            if (_pollCoroutine != null)
            {
                Debug.Log("🛑 Stopping poll coroutine manually");
                StopCoroutine(_pollCoroutine);
                _pollCoroutine = null;
            }
        }

        private IEnumerator PollCoroutine()
        {
            Debug.Log("🚀 PollCoroutine() started");

            while (true)
            {
                // Debug.Log("⏳ [Loop Start] Polling server for new connect update...");

                using (UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl))
                {
                    // Debug.Log($"🌐 Sending request to {m_communicator.ServerUrl}");
                    yield return request.SendWebRequest();

                    // Debug.Log($"📡 Response received | Result={request.result} | Code={(int)request.responseCode}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        if ((int)request.responseCode == 200)
                        {
                            Debug.Log("✅ 200 OK received, connecting...");
                            GameManager.Instance.MobileConnected = true;
                        }
                        else if ((int)request.responseCode == 204)
                        {
                            // Debug.Log("⏳ 204 No Content — keep polling");
                        }
                        else
                        {
                            Debug.LogError($"❌ Unexpected response: Code={(int)request.responseCode} | Error={request.error}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"❌ UnityWebRequest failed: {request.error}");
                    }
                }

                // Debug.Log($"🔁 Loop iteration complete, waiting {m_communicator.pollRateMilliSeconds} ms before next poll...");
                yield return new WaitForSecondsRealtime(m_communicator.pollRateMilliSeconds / 1000f);
                // Debug.Log("🔄 Wait complete, next iteration will start now...");
            }
        }

        private void OnDestroy()
        {
            Debug.Log("🛑 ConnectListener.OnDestroy() called");
            if (_pollCoroutine != null)
            {
                StopCoroutine(_pollCoroutine);
                _pollCoroutine = null;
                Debug.Log("🛑 Poll coroutine stopped in OnDestroy()");
            }
        }
    }
}
