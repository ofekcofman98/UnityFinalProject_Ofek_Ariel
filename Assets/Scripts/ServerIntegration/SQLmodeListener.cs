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
    public class SQLmodeListener : Singleton<SQLmodeListener>
    {
        private ServerCommunicator m_communicator;
        private CancellationTokenSource _cts;

        public SQLmodeListener()
        {
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetSQLMode);
        }

        public void StartListening()
        {
            Debug.Log($"📱 m_isMobile = {m_communicator.IsMobile} | platform = {Application.platform}");
            if (m_communicator.m_isRunning) return;


            Debug.Log("🎧 Starting async polling for sqlmode...");
            m_communicator.m_isRunning = true;
            _cts = new CancellationTokenSource();
            _ = PollAsync(_cts.Token); // Fire-and-forget

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
                if (Application.isMobilePlatform)
                {
                    while (!token.IsCancellationRequested)
                    {
                        Debug.Log("⏳ Polling server for new SQLMode update...");

                        using (UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl))
                        {
                            await AwaitUnityWebRequest(request);

                            Debug.Log($"📡 Actual Response Code: {request.responseCode} | Result: {request.result}");
                            if ((int)request.responseCode == 200 && !GameManager.Instance.SqlMode) // Go into sql mode
                            {
                                Debug.Log("✅ 200 OK received, entering sql mode...✅");
                                GameManager.Instance.SqlMode = true;
                                // GameManager.Instance.SwitchMobileCanvas(true);
                            }
                            else if((int)request.responseCode == 201 && GameManager.Instance.SqlMode) // Go out of sql mode
                            {
                                Debug.Log("✅ 201 OK received, leaving sql mode...✅");
                                GameManager.Instance.SqlMode = false;
                                // GameManager.Instance.SwitchMobileCanvas(false);
                            }                          
                            else if((int)request.responseCode != 200 && (int)request.responseCode != 201)
                            {
                                Debug.LogError($"❌ Unexpected server response: {request.responseCode} | {request.error}");
                                Debug.LogError($"The url is : {m_communicator.ServerUrl}");
                            }
                        }

                        await Task.Delay(m_communicator.pollRateMilliSeconds, token); // Wait before polling again
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
