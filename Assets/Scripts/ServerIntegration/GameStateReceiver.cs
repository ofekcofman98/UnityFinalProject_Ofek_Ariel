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
    internal class GameStateReceiver
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

                    using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
                    {
                        await AwaitUnityWebRequest(request);

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            string receivedJson = request.downloadHandler.text;
                            // Debug.Log("📥 Raw JSON: " + receivedJson);

                            try
                            {
                                Query receivedQuery = JsonConvert.DeserializeObject<Query>(receivedJson);

                                if (receivedQuery != null && !string.IsNullOrWhiteSpace(receivedQuery.QueryString))
                                {
                                    Debug.Log($"✅ Query received and parsed: {receivedQuery.QueryString}");

                                    receivedQuery.PostDeserialize();
                                    GameManager.Instance.SaveQuery(receivedQuery);
                                    GameManager.Instance.ExecuteLocally(receivedQuery);

                                    return; // Exit polling loop on success
                                }
                                else
                                {
                                    // Debug.Log("⏳ Received query object is empty or missing QueryString.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"❌ Failed to parse full Query object: {ex.Message}");
                            }
                        }
                        else if (request.responseCode == 204)
                        {
                            Debug.Log("⏳ Server responded with 204 No Content — no new query yet.");
                        }
                        else
                        {
                            Debug.LogError($"❌ Failed to fetch query: {request.responseCode} | {request.error}");
                        }
                    }

                    await Task.Delay(2000, token); // Wait 2 seconds before retrying
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