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
    private const string k_pcIP = "localhost";//"192.168.1.228"; 
    private string serverUrl = $"http://{k_pcIP}:8080/get-query";
    private Coroutine listeningCoroutine;
    
    public void StartListening()
    {
        if (listeningCoroutine == null)
        {
            listeningCoroutine = StartCoroutine(CheckForNewQuery());
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
        UnityWebRequest request = UnityWebRequest.Get(serverUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string receivedJson = request.downloadHandler.text;
            Debug.Log($"📥 Raw Query JSON: {receivedJson}");

            try
            {
                JObject parsedJson = JObject.Parse(receivedJson);
                string queryString = parsedJson["query"]?.ToString();

                if (!string.IsNullOrEmpty(receivedJson))
                {
                    Debug.Log($"📥 Raw Query JSON: {receivedJson}");

                    Query receivedQuery = new Query();
                    receivedQuery.QueryString = receivedJson.Trim();

if (GameManager.Instance.CurrentQuery != null)
{
    receivedQuery.fromClause = GameManager.Instance.CurrentQuery.fromClause;
    receivedQuery.selectClause = GameManager.Instance.CurrentQuery.selectClause;
    receivedQuery.whereClause = GameManager.Instance.CurrentQuery.whereClause;
}

                    GameManager.Instance.SaveQuery(receivedQuery);
                }
                else
                {
                    Debug.LogWarning("⚠️ Received an empty query. Waiting for new queries...");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ JSON Parsing Error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"❌ Failed to fetch query: {request.responseCode} | {request.error}");
        }

        yield return new WaitForSeconds(2f);
    }
}
}
    

