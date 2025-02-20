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
    // void Start()
    // {
    //     Debug.Log("✅ SQLQuerySender is active in the scene!");
    // }

    public void SendQueryToServer(string query)
    {
        StartCoroutine(SendQuery(query));
    }

    private IEnumerator SendQuery(string query)
    {

        string cleanedQuery = query.Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\""); 

        // Convert to JSON format
        string jsonPayload = "{\"query\":\"" + cleanedQuery + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            method = UnityWebRequest.kHttpVerbPOST
        };

        request.SetRequestHeader("Content-Type", "application/json"); 

        Debug.Log("📤 Sending Query to Server: " + jsonPayload); // Debugging

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Query Sent Successfully! Server Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ Failed to send query: " + request.responseCode + " | " + request.error);
        }

        // string url = "http://localhost:8080/send-query"; // Keep using localhost for now

        // // Ensure the query string does not contain any control characters
        // string cleanedQuery = query.Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\""); 

        // // Convert to JSON format
        // string jsonPayload = "{\"query\":\"" + cleanedQuery + "\"}";

        // byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        // UnityWebRequest request = new UnityWebRequest(url, "POST")
        // {
        //     uploadHandler = new UploadHandlerRaw(bodyRaw),
        //     downloadHandler = new DownloadHandlerBuffer(),
        //     method = UnityWebRequest.kHttpVerbPOST
        // };

        // request.SetRequestHeader("Content-Type", "application/json"); // Ensure correct headers

        // yield return request.SendWebRequest();

        // if (request.result == UnityWebRequest.Result.Success)
        // {
        //     Debug.Log("📤 Query Sent Successfully: " + cleanedQuery);
        // }
        // else
        // {
        //     Debug.LogError("❌ Failed to send query: " + request.responseCode + " | " + request.error);
        // }
    }


}
