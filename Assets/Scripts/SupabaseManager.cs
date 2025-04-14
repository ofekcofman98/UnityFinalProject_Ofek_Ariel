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
    public Action OnSchemeFullyLoaded { get; internal set; }
    private int _columnsLoaded = 0;

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
        UnityWebRequest request = createPostRequest("get_table_names", "{}");

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
        string json =  $"{{\"table_name\":\"{i_Table.Name}\"}}";
        UnityWebRequest request = createPostRequest("get_columns", json);
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {            
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);

            if (jsonResponse.Count > 0)
            {
                List<Column> columns = new List<Column>();

                foreach (JObject columnData in jsonResponse)
                {
                    string columnName = columnData["column_name"].ToString();
                    string columnType = columnData["data_type"].ToString();

                    eDataType mappedType = MapSupabaseType(columnType);

                    Column newColumn = new Column(columnName, mappedType);
                    newColumn.ParentTable = i_Table;
                    columns.Add(newColumn);
                }

                i_Table.SetColumns(columns);
            }
            else
            {
                Debug.LogWarning($"No data found for table: {i_Table.Name}");
            }
        }
        else
        {
            Debug.LogError($"Failed to fetch columns for {i_Table.Name}: {request.error}");
            yield break;
        }

        _columnsLoaded++;

        if (_columnsLoaded == Tables.Count)
        {
            StartCoroutine(FetchForeignKeys());
        }
    }

    private IEnumerator FetchForeignKeys()
    {
        UnityWebRequest request = createPostRequest("get_foreign_keys");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);
            if (jsonResponse.Count > 0)
            {
                foreach (JObject obj in jsonResponse)
                {
                    string fromTableText = obj["table_name"].ToString();
                    string fromColumnText = obj["column_name"].ToString();
                    string toTableText = obj["foreign_table_name"].ToString();
                    string toColumnText = obj["foreign_column_name"].ToString();
                
                    Table fromTable = Tables.FirstOrDefault(t => t.Name == fromTableText);            
                    if (fromTable != null)
                    {
                        Column fromColumn = fromTable.Columns.FirstOrDefault(col => col.Name == fromColumnText);
                        Table toTable = Tables.FirstOrDefault(t => t.Name == toTableText);
                        Column toColumn = toTable.Columns.FirstOrDefault(col => col.Name == toColumnText);
                        if (fromColumn != null && toTable != null && toColumn != null)
                        {
                            ForeignKey foreignKey = new ForeignKey(fromColumn, toTable, toColumn);
                            fromTable.AddForeignKey(foreignKey);
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ FK could not be resolved: {fromTableText}.{fromColumnText} -> {toTableText}.{toColumnText}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ {fromTableText} could not be found");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Failed to fetch foreign keys: " + request.error);
        }
        
Debug.Log("✅ OnSchemeFullyLoaded is about to fire");
        OnSchemeFullyLoaded?.Invoke();
    }

    private UnityWebRequest createPostRequest(string i_EndPoint, string i_JsonBody = "{}")
    {
        string url = $"{supabaseUrl}/rest/v1/rpc/{i_EndPoint}";
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(i_JsonBody));

        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    private eDataType MapSupabaseType(string supabaseType)
    {
        switch (supabaseType.ToLower())
        {
            case "int4":
            case "integer":
            case "bigint":
                return eDataType.Integer;

            case "text":
            case "varchar":
            case "char":
            case "uuid":
                return eDataType.String;

            case "date":
            case "timestamp":
                return eDataType.DateTime;

            default:
                Debug.LogWarning($"Unmapped Supabase type: {supabaseType}");
                return eDataType.String;
        }
    }

}

