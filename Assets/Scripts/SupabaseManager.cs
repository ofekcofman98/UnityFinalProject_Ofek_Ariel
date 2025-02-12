using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;


public class SupabaseManager : Singleton<SupabaseManager>
{
    private string supabaseUrl = "https://vwudsbcqlhwajpkmcpsz.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZ3dWRzYmNxbGh3YWpwa21jcHN6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkxNDQ5OTQsImV4cCI6MjA1NDcyMDk5NH0.9nDEBqlbCpUEmAjyEXwN0KzdCa89uBSjI_I2HkMn1_s";

    public List<string> TableNames { get; private set; } = new List<string>();
    public Action OnTableNamesFetched;

    private Dictionary<string, JArray> tableDataCache = new Dictionary<string, JArray>(); // ✅ Cache

    void Start()
    {
        StartCoroutine(FetchTableNames());
        
    }
    public string SupabaseUrl
    {
        get {return supabaseUrl;}
    }
    public string ApiKey
    {
        get { return apiKey;}
    }

    IEnumerator FetchTableNames()
    {
        string url = $"{supabaseUrl}/rest/v1/rpc/get_table_names"; // ✅ This is now a valid endpoint

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}")); // Empty JSON body for POST

        // ✅ Set the required headers
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);
            TableNames.Clear();

            foreach (JObject obj in jsonResponse)
            {
                TableNames.Add(obj["table_name"].ToString());
            }

            Debug.Log("Tables in Supabase:");
            foreach (string name in TableNames)
            {
                Debug.Log(name);
            }

            OnTableNamesFetched?.Invoke(); 
        }
        else
        {
            Debug.LogError($"Failed to fetch table names: {request.error}");
        }
    }

    public void GetTableData(string tableName, System.Action<JArray> callback)
    {
        if (tableDataCache.ContainsKey(tableName))
        {
            // ✅ Use cached data instead of making a new request
            Debug.Log($"Using cached data for table: {tableName}");
            callback?.Invoke(tableDataCache[tableName]);
        }
        else
        {
            StartCoroutine(FetchTableData(tableName, callback));
        }
    }

    private IEnumerator FetchTableData(string tableName, System.Action<JArray> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/{tableName}?select=*";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);

            // ✅ Store the data in cache
            tableDataCache[tableName] = jsonResponse;

            callback?.Invoke(jsonResponse);
        }
        else
        {
            Debug.LogError($"Failed to fetch data from table {tableName}: {request.error}");
        }
    }
}

