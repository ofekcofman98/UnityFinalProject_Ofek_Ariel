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
    //public class StateListener : Singleton<StateListener>
    //{
    //    private ServerCommunicator m_communicator;
    //    private CancellationTokenSource _cts;

    //    public StateListener()
    //    {
    //        m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetState);
    //    }

    //    private void Awake()
    //    {
    //        m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetState);
    //        Debug.Log($"StateListener.Awake -> ServerUrl = {m_communicator.ServerUrl} | IsMobile={m_communicator.IsMobile}");

    //    }

    //    public void StartListening()
    //    {
    //        Debug.Log($"📱 m_isMobile = {m_communicator.IsMobile} | platform = {Application.platform}");
    //        Debug.Log("Inside StartListening of StateListener");

    //        if (m_communicator.m_isRunning) return;


    //        Debug.Log("🎧 Starting async polling for new state...");
    //        m_communicator.m_isRunning = true;
    //        _cts = new CancellationTokenSource();
    //        _ = PollAsync(_cts.Token);

    //    }

    //    public void StopListening()
    //    {
    //        if (!m_communicator.m_isRunning) return;

    //        Debug.Log("🛑 Stopping polling...");
    //        m_communicator.m_isRunning = false;
    //        _cts.Cancel();
    //    }

    //    private Task AwaitUnityWebRequest(UnityWebRequest request)
    //    {
    //        var tcs = new TaskCompletionSource<bool>();
    //        var operation = request.SendWebRequest();

    //        operation.completed += _ => tcs.SetResult(true);

    //        return tcs.Task;
    //    }

    //    private async Task PollAsync(CancellationToken token)
    //    {
    //        try
    //        {
    //            if (Application.isMobilePlatform)
    //            {
    //                while (!token.IsCancellationRequested)
    //                {
    //                    Debug.Log("⏳ Polling server for new state update...");
    //                    Debug.Log($"📡 Before sending the state get request : {m_communicator.ServerUrl}");
    //                    using (UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl))
    //                    {
    //                        await AwaitUnityWebRequest(request);

    //                        if(request.responseCode == 200)
    //                        {
    //                            string receivedJson = request.downloadHandler.text;
    //                            var settings = new JsonSerializerSettings();
    //                            settings.Converters.Add(new OperatorConverter());

    //                            Dictionary<string, int> result = JsonConvert.DeserializeObject<Dictionary<string, int>>(receivedJson, settings);
    //                            int ServerLevelIndex = result["currentLevelIndex"];
    //                            int currentLevelIndex = MissionsManager.Instance.currentMissionIndex;
    //                            int isLevelDone = result["isLevelDone"];

    //                            Debug.Log($"📡 Actual Response Code: {request.responseCode} | Result: {request.result} | Text: {request.downloadHandler.text}");
    //                            if (isLevelDone == 1 && currentLevelIndex + 1 == ServerLevelIndex)
    //                            {
    //                                Debug.Log("✅ found an Update ! entering DelyaedAdvance ✅");
    //                                Debug.Log("✅ 200 OK received, about to enter DelayedAdvance...");
    //                                CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
    //                            }
    //                            else if (isLevelDone == 0 && currentLevelIndex < ServerLevelIndex)
    //                            {
    //                                Debug.Log("⏳ Inconsistency detected, advancing sequentially...");
    //                                CoroutineRunner.Instance.StartCoroutine(AdvanceSequentially(currentLevelIndex, ServerLevelIndex));

    //                            }
    //                        }                                                   
    //                        else
    //                        {
    //                            Debug.LogError($"❌ Unexpected server response: {request.responseCode} | {request.error}");
    //                            Debug.LogError($"The url is : {m_communicator.ServerUrl}");
    //                        }
    //                    }

    //                    await Task.Delay(m_communicator.pollRateMilliSeconds, token);
    //                }
    //            }

    //        }
    //        catch (TaskCanceledException)
    //        {
    //            Debug.Log("🟡 Polling was cancelled.");
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.LogError($"❌ Unexpected error in polling: {ex.Message}");
    //        }
    //    }

    //    private IEnumerator AdvanceSequentially(int fromIndex, int toIndex)
    //    {
    //        for (int i = fromIndex; i < toIndex; i++)
    //        {
    //            // Wait for each advance to finish before starting the next
    //            yield return CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
    //        }
    //    }

    //}

    public class StateListener : Singleton<StateListener>
    {
        private ServerCommunicator m_communicator;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            // Init here, not in a constructor
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

                                if(MissionsManager.Instance.MissionSequence == null)
                                {
                                    Debug.Log($"Loading sequence number {seqNumber}");
                                    MissionsManager.Instance.LoadMissionSequence(seqNumber == 1 ? GameManager.Instance.MainGameSequence : GameManager.Instance.TutorialSequence);

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