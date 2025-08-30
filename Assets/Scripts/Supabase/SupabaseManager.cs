using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


public class SupabaseManager : Singleton<SupabaseManager>
{
    private string supabaseUrl = ServerData.k_SupabaseUrl;
    private string apiKey = ServerData.k_ApiKey;

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
        UnityWebRequest request = SupabaseUtility.CreatePostRpcRequest("get_table_names", "{}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            JArray jsonResponse = JArray.Parse(request.downloadHandler.text);
            Debug.Log($"📦 Received {jsonResponse.Count} tables from Supabase");

            Tables.Clear();

            foreach (JObject obj in jsonResponse)
            {
                string tableName = obj["table_name"].ToString();
                Table newTable = new Table(tableName, i_IsUnlocked: false);
                Tables.Add(newTable);
                StartCoroutine(FetchTableColumns(newTable));
                // Debug.Log($"{tableName} table was added from SupaBase");
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
        UnityWebRequest request = SupabaseUtility.CreatePostRpcRequest("get_columns", json);
        
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
            // StartCoroutine(LoadPersonsAfterScheme());
        }
    }

    private IEnumerator FetchForeignKeys()
    {
        UnityWebRequest request = SupabaseUtility.CreatePostRpcRequest("get_foreign_keys");
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

    public bool IsTableUnlocked(string tableName)
    {
        Table table = Tables.FirstOrDefault(t => t.Name == tableName);
        return table != null && table.IsUnlocked;
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
                // return eDataType.DateTime;
                return eDataType.String;


            default:
                Debug.LogWarning($"Unmapped Supabase type: {supabaseType}");
                return eDataType.String;
        }
    }

}