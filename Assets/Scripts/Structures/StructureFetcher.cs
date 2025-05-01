using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class StructureFetcher : MonoBehaviour
{
    // public string savePath = "Assets/SQLDetective/Data/Schema";

    // public IEnumerator FetchSchemaCoroutine()
    // {
    //     var task = SupabaseManager.EditorInitializeAsync();
    //     while (!task.IsCompleted) yield return null;

    //     if (SupabaseManager.Instance == null)
    //     {
    //         Debug.LogError("❌ SupabaseManager.Instance is null.");
    //         yield break;
    //     }

    //     var tables = SupabaseManager.Instance.Tables;
    //     if (tables == null || tables.Count == 0)
    //     {
    //         Debug.LogWarning("⚠️ No tables found from Supabase.");
    //         yield break;
    //     }

    //     string folderPath = savePath;
    //     if (!Directory.Exists(folderPath))
    //     {
    //         Directory.CreateDirectory(folderPath);
    //         Debug.Log($"📁 Created missing folder: {folderPath}");
    //     }

    //     foreach (var table in tables)
    //     {
    //         if (table == null)
    //         {
    //             Debug.LogWarning("⚠️ Skipping null table.");
    //             continue;
    //         }

    //         TableStructure tableAsset = ScriptableObject.CreateInstance<TableStructure>();
    //         tableAsset.TableName = table.Name;

    //         foreach (var col in table.Columns)
    //         {
    //             if (col == null) continue;

    //             tableAsset.Columns.Add(new ColumnStructure
    //             {
    //                 ColumnName = col.Name,
    //                 DataType = col.DataType.ToString()
    //             });
    //         }

    //         string fullPath = Path.Combine(folderPath, $"{table.Name}_Schema.asset");
    //         Debug.Log($"💾 Creating: {fullPath}");

    //         AssetDatabase.CreateAsset(tableAsset, fullPath);
    //     }

    //     AssetDatabase.SaveAssets();
    //     AssetDatabase.Refresh();

    //     Debug.Log("✅ Supabase schema fetched and saved.");
    // }
}
