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

        public StateListener()
        {
            m_communicator = new ServerCommunicator("/get-state");
        }

        public void StartListening()
        {
            Debug.Log($"📱 m_isMobile = {m_communicator.IsMobile} | platform = {Application.platform}");
            Debug.Log("Inside StartListening of StateListener");

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
                if(Application.isMobilePlatform)
                {
                    while (!token.IsCancellationRequested)
                    {
                        Debug.Log("⏳ Polling server for new state update...");
                        Debug.Log($"📡 Before sending the state get request : {m_communicator.ServerUrl}");
                        using (UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl))
                        {
                            await AwaitUnityWebRequest(request);

                            Debug.Log($"📡 Actual Response Code: {request.responseCode} | Result: {request.result} | Text: {request.downloadHandler.text}");
                            if ((int)request.responseCode == 200)
                            {
                                Debug.Log("✅ 200 OK received, about to enter DelayedAdvance...");
                                Debug.Log("✅ found an Update ! entering DelyaedAdvance ✅");
                                CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
                            }
                            else if ((int)request.responseCode == 204)
                            {
                                Debug.Log("⏳ Server responded with 204 No Content — no new state update.");
                            }
                            else
                            {
                                Debug.LogError($"❌ Unexpected server response: {request.responseCode} | {request.error}");
                                Debug.LogError($"The url is : {m_communicator.ServerUrl}");
                            }
                        }

                        await Task.Delay(m_communicator.pollRateMilliSeconds, token); 
                    }
                }
                
            }
            catch (TaskCanceledException)
            {
                Debug.Log("🟡 Polling was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Unexpected error in polling: {ex.Message}");
            }
        }


    }
}