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
using Assets.Scripts.ServerIntegration;


public class QueryListener : MonoBehaviour
{
    private Coroutine listeningCoroutine;
    private ServerCommunicator m_communicator;

    private void Awake()
    {
        m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GetQuery);
    }
    //public QueryListener()
    //{
    //    m_communicator = new ServerCommunicator("/get-query");
    //}
  
    public void StartListening()
    {
        Debug.Log("🎧 StartListening() called.");

        if(!m_communicator.IsMobile)
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
      
                UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string receivedJson = request.downloadHandler.text;

                    try
                    {

                        var settings = new JsonSerializerSettings();
                        settings.Converters.Add(new OperatorConverter());

                        Query receivedQuery = JsonConvert.DeserializeObject<Query>(receivedJson, settings);

                        if (receivedQuery != null && !string.IsNullOrWhiteSpace(receivedQuery.QueryString))
                        {
                            Debug.Log($"✅ Query received and parsed: {receivedQuery.QueryString}");

                            receivedQuery.PostDeserialize();
                            GameManager.Instance.SaveQuery(receivedQuery);
                            GameManager.Instance.ExecuteLocally(receivedQuery);

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


                yield return new WaitForSeconds(m_communicator.pollRateMilliSeconds / 1000f);  // Wait before next poll
            }    
    }
}


