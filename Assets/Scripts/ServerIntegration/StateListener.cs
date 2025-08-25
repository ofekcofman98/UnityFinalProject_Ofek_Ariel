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
    
    public class StateListener : Singleton<StateListener>
    {
        private ServerCommunicator m_communicator;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetState);

            Debug.Log($"StateListener.Awake -> ServerUrl = {m_communicator.ServerUrl} | IsMobile={m_communicator.IsMobile}");
        }

        public void StartListening()
        {
            Debug.Log($"📱 m_isMobile = {m_communicator.IsMobile} | platform = {Application.platform}");
            if (m_communicator.m_isRunning) return;

            Debug.Log("🎧 Starting async polling for new state...");
            m_communicator.m_isRunning = true;
            _cts = new CancellationTokenSource();
            _ = PollAsync(_cts.Token);
        }

        public void StopListening()
        {
            if (!m_communicator.m_isRunning) return;
            Debug.Log("🛑 Stopping polling...");
            m_communicator.m_isRunning = false;
            _cts.Cancel();
        }

        private Task AwaitUnityWebRequest(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.SetResult(true);
            return tcs.Task;
        }

        private async Task PollAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Debug.Log($"⏳ Polling {m_communicator.ServerUrl} ...");

                    using (var request = UnityWebRequest.Get(m_communicator.ServerUrl))
                    {
                        await AwaitUnityWebRequest(request);
                        var code = (int)request.responseCode;

                        if (code == 200)
                        {
                            var text = request.downloadHandler.text;
                            Dictionary<string, int> result = null;

                            try
                            {
                                result = JsonConvert.DeserializeObject<Dictionary<string, int>>(text);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"❌ JSON parse failed: {ex.Message} | body: {text}");
                            }

                            if (result != null)
                            {
                                int seqNumber = result.TryGetValue("seqNumber", out var seq) ? seq : -1;
                                int isLevelDone = result.TryGetValue("isLevelDone", out var isDone) ? isDone : -1;
                                int serverLevelIndex = result.TryGetValue("currentLevelIndex", out var s) ? s : -1;
                                int mobileIndex = MissionsManager.Instance.currentMissionIndex;

                                if (MissionsManager.Instance.MissionSequence == null)
                                {
                                    //! removed (ofek 17.8)
                                    // Debug.Log($"Loading sequence number {seqNumber}");
                                    // MissionsManager.Instance.LoadMissionSequence(seqNumber == 1 ? GameManager.Instance.MainGameSequence : GameManager.Instance.TutorialSequence);
                                    //! added (ofek 17.8)
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
                        else
                        {
                            Debug.LogError($"❌ Unexpected response: {code} | {request.error} | body: {request.downloadHandler.text}");
                        }
                    }

                    await Task.Delay(m_communicator.pollRateMilliSeconds, token);
                }
            }
            catch (TaskCanceledException) { Debug.Log("🟡 Polling cancelled"); }
            catch (Exception ex) { Debug.LogError($"❌ Poll loop error: {ex.Message}"); }
            finally { m_communicator.m_isRunning = false; 
            }
        }

        private IEnumerator AdvanceSequentially(int fromIndex, int toIndex)
        {
            for (int i = fromIndex; i < toIndex; i++)
            {
                yield return CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
                yield return null; // let UI settle
            }
            Debug.Log("✅ Catch-up complete.");
            GameManager.Instance.StartMissions();
        }
    }

}