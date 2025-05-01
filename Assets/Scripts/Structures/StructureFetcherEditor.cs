#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;

public class SchemaFetcherEditor : EditorWindow
{
    private const string SupabaseUrl = "https://vwudsbcqlhwajpkmcpsz.supabase.co";
    private const string ApiKey  = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZ3dWRzYmNxbGh3YWpwa21jcHN6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkxNDQ5OTQsImV4cCI6MjA1NDcyMDk5NH0.9nDEBqlbCpUEmAjyEXwN0KzdCa89uBSjI_I2HkMn1_s";
    private const string SavePath = "Assets/SQLDetective/Data/Schema";

    [MenuItem("SQL Detective/Fetch Supabase Schema")]
    public static void ShowWindow()
    {
        var window = GetWindow<SchemaFetcherEditor>();
        window.titleContent = new GUIContent("Fetch Supabase Schema");
        EditorCoroutineUtility.StartCoroutine(window.FetchTables(), window);
    }

    private IEnumerator FetchTables()
    {
        Debug.Log("🔍 Fetching Supabase table names...");

        UnityWebRequest request = CreatePostRequest("get_table_names", "{}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Failed to fetch table names: {request.error}");
            yield break;
        }

        JArray json = JArray.Parse(request.downloadHandler.text);

        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        foreach (JObject tableJson in json)
        {
            string tableName = tableJson["table_name"]?.ToString();
            Debug.Log($"📦 Table: {tableName}");

            UnityWebRequest colRequest = CreatePostRequest("get_columns", $"{{\"table_name\":\"{tableName}\"}}");
            yield return colRequest.SendWebRequest();

            if (colRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"⚠️ Failed to fetch columns for {tableName}: {colRequest.error}");
                continue;
            }

            JArray colJson = JArray.Parse(colRequest.downloadHandler.text);

            TableStructure table = ScriptableObject.CreateInstance<TableStructure>();
            table.TableName = tableName;

            foreach (JObject col in colJson)
            {
                table.Columns.Add(new ColumnStructure
                {
                    ColumnName = col["column_name"].ToString(),
                    DataType = col["data_type"].ToString()
                });
            }

Debug.Log($"📋 Creating table: {tableName}");
            string path = Path.Combine(SavePath, $"{tableName}_Schema.asset");
Debug.Log($"💾 Full path: {path}");

// Just before saving:
bool fileExists = File.Exists(path);
Debug.Log($"File already exists? {fileExists}");


         
            AssetDatabase.CreateAsset(table, path);
            Debug.Log($"💾 Saved {path}");

        }
 
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Schema fetch complete.");
    }

    private UnityWebRequest CreatePostRequest(string endpoint, string jsonBody)
    {
        string url = $"{SupabaseUrl}/rest/v1/rpc/{endpoint}";
        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("apikey", ApiKey);
        request.SetRequestHeader("Authorization", $"Bearer {ApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }
}
#endif
