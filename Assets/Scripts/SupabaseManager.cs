using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Security.Cryptography;


public class SupabaseManager : Singleton<SupabaseManager>
{
    private string supabaseUrl = "https://vwudsbcqlhwajpkmcpsz.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZ3dWRzYmNxbGh3YWpwa21jcHN6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkxNDQ5OTQsImV4cCI6MjA1NDcyMDk5NH0.9nDEBqlbCpUEmAjyEXwN0KzdCa89uBSjI_I2HkMn1_s";

    public List<Table> Tables {get; private set;}
    public Action OnTableNamesFetched;

    private Dictionary<string, JArray> tableDataCache = new Dictionary<string, JArray>(); // ✅ Cache

    void Start()
    {
        Tables = new List<Table>();
        StartCoroutine(FetchTables());
        
    }
    public string SupabaseUrl => supabaseUrl;
    public string ApiKey => apiKey;

    IEnumerator FetchTables()
    {
        string url = $"{supabaseUrl}/rest/v1/rpc/get_table_names";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}")); // Empty JSON body for POST

        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);
            Tables.Clear();

            foreach (JObject obj in jsonResponse)
            {
                string tableName = obj["table_name"].ToString();
                Table newTable = new Table(tableName, i_IsUnlocked: false);
                Tables.Add(newTable);
                StartCoroutine(FetchTableColumns(newTable));
                Debug.Log($"{tableName} table was added from SupaBase");
            }

            OnTableNamesFetched?.Invoke(); 
        }
        else
        {
            Debug.LogError($"Failed to fetch table names: {request.error}");
        }
    }

    private IEnumerator FetchTableColumns(Table i_Table)
    {
        Debug.Log($"Starting fetching colums for {i_Table.Name}");

        string url = $"{supabaseUrl}/rest/v1/{i_Table.Name}?select=*";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");


        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // JArray jsonResponse = JArray.Parse(request.downloadHandler.text);
            // List<Column> columns = new List<Column>();

            // foreach (JObject column in jsonResponse)
            // {
            //     string columnName = column["column_name"].ToString();

            //     Column col = new Column(columnName);
            //     columns.Add(col);
            // }

            // i_Table.SetColumns1(columns);
            // Debug.Log($"Columns for {i_Table.Name}: {string.Join(", ", columns.Select(col => col.Name))}");
            
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);

            if (jsonResponse.Count > 0)
            {
                JObject firstRow = (JObject)jsonResponse[0];                 
                List<Column> columns = new List<Column>();

                foreach (JProperty column in firstRow.Properties()) 
                {
                    columns.Add(new Column(column.Name));
                }

                i_Table.SetColumns1(columns);
                Debug.Log($"Columns for {i_Table.Name}: {string.Join(", ", columns.Select(col => col.Name))}");
            }
            else
            {
                Debug.LogWarning($"No data found for table: {i_Table.Name}");
            }
        }
        else
        {
            Debug.LogError($"Failed to fetch columns for {i_Table.Name}: {request.error}");
        }

    }
}

