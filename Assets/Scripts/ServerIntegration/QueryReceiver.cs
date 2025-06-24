using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;
using System.Threading;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class QueryReceiver : MonoBehaviour
{
    private const string k_pcIP = ServerData.k_pcIP;
    private string serverUrl = $"https://{k_pcIP}/get-query";
    private bool m_isMobile = Application.isMobilePlatform;
    private Coroutine listeningCoroutine;

    public void StartListening()
    {
        Debug.Log("🎧 StartListening() called.");

        if(!m_isMobile)
        {
            if (listeningCoroutine == null)
            {
                Debug.Log("✅ Starting CheckForNewQuery coroutine.");
                listeningCoroutine = StartCoroutine(CheckForNewQuery());
            }
            else
            {
                Debug.Log("ℹ️ Already listening.");
            }
        }       
    }


    public void StopListening()
    {
        if (listeningCoroutine != null)
        {
            StopCoroutine(listeningCoroutine);
            listeningCoroutine = null;
        }
    }

    private IEnumerator CheckForNewQuery()
    {
        
            while (true)
            {
                Debug.Log("📡 Polling the server for new query...");

                UnityWebRequest request = UnityWebRequest.Get("https://python-query-server-591845120560.us-central1.run.app/get-query");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string receivedJson = request.downloadHandler.text;

                    try
                    {
                        Query receivedQuery = JsonConvert.DeserializeObject<Query>(receivedJson);

                        if (receivedQuery != null && !string.IsNullOrWhiteSpace(receivedQuery.QueryString))
                        {
                            Debug.Log($"✅ Query received and parsed: {receivedQuery.QueryString}");

                            receivedQuery.PostDeserialize();
                            GameManager.Instance.SaveQuery(receivedQuery);
                            GameManager.Instance.ExecuteLocally(receivedQuery);

                            yield break;
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

                yield return new WaitForSeconds(2f);  // Wait before next poll
            }    
    }
}


