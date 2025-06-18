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
    internal class GameStateReceiver : Singleton<GameStateReceiver>
    {
        private const string k_pcIP = ServerData.k_pcIP;
        private string serverUrl = $"https://{k_pcIP}/get-state";
        private bool m_isMobile = Application.isMobilePlatform;
        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        public GameStateReceiver(string i_ServerUrl)
        {
            serverUrl = i_ServerUrl;
        }

        public void StartListening()
        {
            if (_isRunning) return;

            if (m_isMobile)
            {
                Debug.Log("🎧 Starting async polling...");
                _isRunning = true;
                _cts = new CancellationTokenSource();
                _ = PollAsync(_cts.Token); // Fire-and-forget
            }
        }

        public void StopListening()
        {
            if (!_isRunning) return;

            Debug.Log("🛑 Stopping polling...");
            _isRunning = false;
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
                    Debug.Log("⏳ Polling server for new state update...");

                    using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + "/get-state"))
                    {
                        await AwaitUnityWebRequest(request);

                        if ((int)request.responseCode == 200)
                        {
                            MissionsManager.Instance.DelayedAdvance();
                        }
                        else if ((int)request.responseCode == 204)
                        {
                            Debug.Log("⏳ Server responded with 204 No Content — no new state update.");
                        }
                        else
                        {
                            Debug.LogError($"❌ Unexpected server response: {request.responseCode} | {request.error}");
                        }
                    }

                    await Task.Delay(2000, token); // Wait before polling again
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