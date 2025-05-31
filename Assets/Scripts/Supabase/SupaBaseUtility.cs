using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class SupabaseUtility
{
    public static UnityWebRequest CreateGetRequest(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", ServerData.k_ApiKey);
        request.SetRequestHeader("Authorization", $"Bearer {ServerData.k_ApiKey}");
        request.SetRequestHeader("Accept", "application/json");

        return request;
    }

    public static UnityWebRequest CreatePostRpcRequest(string endpoint, string jsonBody = "{}")
    {
        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/rpc/{endpoint}";
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", ServerData.k_ApiKey);
        request.SetRequestHeader("Authorization", $"Bearer {ServerData.k_ApiKey}");

        return request;
    }
}