using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Text;
using System;


public class QuerySender : MonoBehaviour
{
    private const string k_pcIP = "192.168.1.228";
    private string serverUrl = $"http://{k_pcIP}:8080/send-query"; 

    public void SendQueryToServer(Query query)
    {
        StartCoroutine(SendQuery(query));
    }

    private IEnumerator SendQuery(Query query)
    {

        if (string.IsNullOrEmpty(query.QueryString))
        {
            Debug.LogError("QueryString is empty! Cannot send query.");
            yield break;
        }

        string escapedQueryString = query.QueryString
            .Replace("\\", "\\\\")  // Escape backslashes
            .Replace("\"", "\\\"")  // Escape double quotes
            .Replace("\n", " ")     // Replace newlines
            .Replace("\r", " ");    // Replace carriage returns

        string jsonPayload = $"{{\"query\":\"{escapedQueryString}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            method = UnityWebRequest.kHttpVerbPOST
        };

        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"📤 Sending Query: {jsonPayload}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Query Sent Successfully! Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"❌ Failed to send query: {request.responseCode} | {request.error}");
            Debug.LogError($"❌ Server Response: {request.downloadHandler.text}");
        }

        // string jsonPayload = JsonUtility.ToJson(query); // Convert Query to JSON
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // UnityWebRequest request = new UnityWebRequest(serverUrl, "POST")
        // {
        //     uploadHandler = new UploadHandlerRaw(bodyRaw),
        //     downloadHandler = new DownloadHandlerBuffer(),
        //     method = UnityWebRequest.kHttpVerbPOST
        // };

        // request.SetRequestHeader("Content-Type", "application/json");

        // yield return request.SendWebRequest();

        // if (request.result == UnityWebRequest.Result.Success)
        // {
        //     Debug.Log("Query Sent Successfully!");
        // }
        // else
        // {
        //     Debug.LogError("Failed to send query: " + request.error);
        // }
    }
}
