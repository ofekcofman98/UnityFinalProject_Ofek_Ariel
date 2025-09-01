using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.ServerIntegration
{
    public class StateListener : Singleton<StateListener>
    {
        private ServerCommunicator m_communicator;
        private Coroutine _pollCoroutine;

        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetState);
            Debug.Log($"StateListener.Awake → ServerUrl = {m_communicator.ServerUrl}");
        }

        public void StartListening()
        {
            Debug.Log($"📱 StateListener.StartListening | platform = {Application.platform}");
            if (m_communicator.m_isRunning) return;

            Debug.Log("🎧 Starting coroutine polling for state...");
            m_communicator.m_isRunning = true;
            _pollCoroutine = StartCoroutine(PollCoroutine());
        }

        public void StopListening()
        {
            if (!m_communicator.m_isRunning) return;
            Debug.Log("🛑 StateListener.StopListening called");
            m_communicator.m_isRunning = false;

            if (_pollCoroutine != null)
            {
                StopCoroutine(_pollCoroutine);
                _pollCoroutine = null;
                Debug.Log("🛑 State poll coroutine stopped");
            }
        }

        private IEnumerator PollCoroutine()
        {
            Debug.Log("🚀 State PollCoroutine started");

            while (true)
            {
                Debug.Log($"⏳ Polling {m_communicator.ServerUrl} for state...");

                using (UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl))
                {
                    yield return request.SendWebRequest();

                    int code = (int)request.responseCode;
                    Debug.Log($"📡 State response | Code={code} | Result={request.result}");

                    if (request.result == UnityWebRequest.Result.Success && code == 200)
                    {
                        string text = request.downloadHandler.text;
                        Debug.Log($"📥 State payload: {text}");
                        Dictionary<string, int> result = null;

                        try
                        {
                            result = JsonConvert.DeserializeObject<Dictionary<string, int>>(text);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"❌ State JSON parse failed: {ex.Message} | body={text}");
                        }

                        if (result != null)
                        {
                            int seqNumber = result.TryGetValue("seqNumber", out var seq) ? seq : -1;
                            int isLevelDone = result.TryGetValue("isLevelDone", out var isDone) ? isDone : -1;
                            int serverLevelIndex = result.TryGetValue("currentLevelIndex", out var s) ? s : -1;
                            int mobileIndex = MissionsManager.Instance.currentMissionIndex;

                            if (MissionsManager.Instance.MissionSequence == null)
                            {
                                Debug.Log($"📦 Loading sequence index {seqNumber}");
                                SequenceManager.Instance.SetSequence(seqNumber);
                                MissionsManager.Instance.LoadMissionSequence(SequenceManager.Instance.Current);
                            }

                            if (isLevelDone == 1 && mobileIndex == serverLevelIndex)
                            {
                                Debug.Log("✅ indices aligned → single DelayedAdvance");
                                CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
                            }
                            else if (mobileIndex < serverLevelIndex)
                            {
                                Debug.Log($"🔧 Mobile behind ({mobileIndex} -> {serverLevelIndex}) → sequential catch-up");
                                CoroutineRunner.Instance.StartCoroutine(AdvanceSequentially(mobileIndex, serverLevelIndex));
                            }
                        }
                    }
                    else if (code == 204)
                    {
                        Debug.Log("⏳ State → 204 No Content, continue polling...");
                    }
                    else
                    {
                        Debug.LogError($"❌ State unexpected response: {code} | {request.error} | body={request.downloadHandler.text}");
                    }
                }

                Debug.Log($"🔁 State loop complete, waiting {m_communicator.pollRateMilliSeconds} ms...");
                yield return new WaitForSecondsRealtime(m_communicator.pollRateMilliSeconds / 1000f);
            }
        }

        private IEnumerator AdvanceSequentially(int fromIndex, int toIndex)
        {
            for (int i = fromIndex; i < toIndex; i++)
            {
                yield return CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
                yield return null; // let UI settle
            }
            Debug.Log("✅ State catch-up complete.");
            GameManager.Instance.StartMissions();
        }
    }
}
