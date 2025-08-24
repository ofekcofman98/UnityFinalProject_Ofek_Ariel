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
using Assets.Scripts.ServerIntegration;
using System.Threading.Tasks;


public class QuerySender : MonoBehaviour
{  
public bool IsQuerySent { get; private set; } = false;
    private ServerCommunicator m_communicator;

    private void Awake()
    {
        m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.SendQuery);
    }
   
    public void SendQueryToServer(Query query)
    {     
        query.PostDeserialize();
        StartCoroutine(SendQuery(query));
        new WaitForSeconds(m_communicator.screensaverDelayAfterQuery);
        Debug.Log("⌛ Delay finished, about to show screensaver again.");
        if (Application.isMobilePlatform)
        {
            Debug.Log("📲 Calling screensaverController.ShowScreensaver()");
            GameManager.Instance.screensaverController?.ShowScreensaver();
        }
    }

    private IEnumerator SendQuery(Query query)
    {
        if (string.IsNullOrEmpty(query.QueryString))
        {
            Debug.LogError("QueryString is empty! Cannot send query.");
            yield break;
        }

        string jsonPayload = JsonConvert.SerializeObject(query, JsonUtility.Settings);

        Debug.Log($"📤 JSON Payload: {jsonPayload}");

        var encoding = new System.Text.UTF8Encoding();
        byte[] bodyRaw = encoding.GetBytes(jsonPayload);


        UnityWebRequest request = new UnityWebRequest(m_communicator.ServerUrl, "POST")
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
            Debug.Log($"✅✅ Query Sent Successfully! serverlURL: {m_communicator.ServerUrl}");
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
