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


public class QueryReceiver : MonoBehaviour
{
    private const string k_pcIP = "192.168.1.228"; 
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


    void Start()
    {
        StartCoroutine(CheckForNewQuery());
    }

    private IEnumerator CheckForNewQuery()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get(serverUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string receivedQuery = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(receivedQuery))
                {
                    Debug.Log("📥 Query Received from Server: " + receivedQuery);
                    GameManager.Instance.SaveQuery(receivedQuery);

                    // 🛑 Stop listening once a query is received
                    StopListening();
                }
            }
            else
            {
                Debug.LogError("❌ Failed to fetch query: " + request.error);
            }

            yield return new WaitForSeconds(2f);
        }
    }
}
