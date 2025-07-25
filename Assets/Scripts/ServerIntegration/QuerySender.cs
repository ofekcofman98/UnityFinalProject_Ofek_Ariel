using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Text;
using System;
using Newtonsoft.Json;


public class QuerySender : MonoBehaviour
{
    private const string k_pcIP = ServerData.k_pcIP;
    private string serverUrl = $"https://{k_pcIP}/send-query";
    public bool IsQuerySent { get; private set; } = false;

    public void SendQueryToServer(Query query)
    {
        // if (IsQuerySent)
        // {
        //     Debug.LogWarning("🚨 Query already sent this level. Ignoring duplicate.");
        //     return;
        // }

query.PostDeserialize();
        StartCoroutine(SendQuery(query));
    }

    private IEnumerator SendQuery(Query query)
    {
        if (string.IsNullOrEmpty(query.QueryString))
        {
            Debug.LogError("QueryString is empty! Cannot send query.");
            yield break;
        }

        // query.clauses = null;
        // query.availableClauses = null;

        string jsonPayload = JsonConvert.SerializeObject(query, JsonUtility.Settings);

        // string jsonPayload = JsonConvert.SerializeObject(query);
        Debug.Log($"📤 JSON Payload: {jsonPayload}");

        var encoding = new System.Text.UTF8Encoding();
        byte[] bodyRaw = encoding.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest("https://python-query-server-591845120560.us-central1.run.app/send-query", "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            method = UnityWebRequest.kHttpVerbPOST
        };

        request.disposeUploadHandlerOnDispose = true;
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Content-Length", bodyRaw.Length.ToString());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Query Sent Successfully! Response: {request.downloadHandler.text}");
            IsQuerySent = true;
        }
        else
        {
            Debug.LogError($"❌ Failed to send query: {request.responseCode} | {request.error}");
            Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
        }
    }

    public void ResetQuerySendFlag()
    {
        IsQuerySent = false;
    }

    public void MarkQueryAsSent()
    {
        IsQuerySent = true;
    }



}
