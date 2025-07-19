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
    public class GameStateReceiver : Singleton<GameStateReceiver>
    {
        private const string k_pcIP = ServerData.k_pcIP;
        private string serverUrl = "https://python-query-server-591845120560.us-central1.run.app/get-state";
        private bool m_isMobile = Application.isMobilePlatform;
        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        public GameStateReceiver(string i_ServerUrl)
        {
            serverUrl = i_ServerUrl;
        }

        public void StartListening()
        {
            Debug.Log($"📱 m_isMobile = {m_isMobile} | platform = {Application.platform}");
            if (_isRunning) return;


            Debug.Log("🎧 Starting async polling...");
            _isRunning = true;
            _cts = new CancellationTokenSource();
            _ = PollAsync(_cts.Token); // Fire-and-forget

        }

        public void StopListening()
        {
            if (!_isRunning) return;

            Debug.Log("🛑 Stopping polling...");
            _isRunning = false;
            _cts.Cancel();
        }

        private void SendToRootEndpoint()
        {
            using (UnityWebRequest request = UnityWebRequest.Get("https://python-query-server-591845120560.us-central1.run.app/"))
            {
                AwaitUnityWebRequest(request);

            }

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
                        Debug.Log("⏳ Polling server for new state update...");

                        using (UnityWebRequest request = UnityWebRequest.Get("https://python-query-server-591845120560.us-central1.run.app/get-state"))
                        {
                            await AwaitUnityWebRequest(request);

                            Debug.Log($"📡 Actual Response Code: {request.responseCode} | Result: {request.result} | Text: {request.downloadHandler.text}");
                            // if ((int)request.responseCode == 200)
                            // {
                            //     Debug.Log("✅ 200 OK received, about to enter DelayedAdvance...");
                            //     Debug.Log("✅ found an Update ! entering DelyaedAdvance ✅");
                            //     CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
                            // }

                            if ((int)request.responseCode == 200)
                            {
                                string json = request.downloadHandler.text;
                                Debug.Log($"📥 JSON Response: {json}");

                                try
                                {
                                    var state = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                                    if (state != null && state.ContainsKey("isLevelDone") && (bool)state["isLevelDone"])
                                    {
                                        Debug.Log("✅ 'isLevelDone' is true — entering DelayedAdvance.");
                                        CoroutineRunner.Instance.StartCoroutine(MissionsManager.Instance.DelayedAdvance());
                                    }
                                    else
                                    {
                                        Debug.Log("⏳ 'isLevelDone' is false or missing — will not proceed.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"❌ Failed to parse JSON from state response: {ex.Message}");
                                }
                            }

                            else if ((int)request.responseCode == 204)
                            {
                                Debug.Log("⏳ Server responded with 204 No Content — no new state update.");
                            }
                            else
                            {
                                Debug.LogError($"❌ Unexpected server response: {request.responseCode} | {request.error}");
                                Debug.LogError($"The url is : {serverUrl}");
                            }
                        }

                        await Task.Delay(500, token); // Wait before polling again
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

        void OnApplicationPause(bool pauseStatus)
{
    if (pauseStatus)
    {
        GameStateReceiver.Instance.StopListening();
    }
}

void OnApplicationQuit()
{
    GameStateReceiver.Instance.StopListening();
}



    }
    

    
}