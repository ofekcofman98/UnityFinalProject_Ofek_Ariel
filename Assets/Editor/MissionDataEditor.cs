#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System;
using UnityEngine.Networking;

[CustomEditor(typeof(MissionData))]
public class MissionDataEditor : Editor
{ 
    private List<TableStructure> tables;
    private string[] tableNames;
    private int selectedTableIndex;
    private Dictionary<string, bool> columnSelections = new Dictionary<string, bool>();

    private List<Dictionary<string, string>> fetchedRows = new List<Dictionary<string, string>>();
    private string[] rowLabels;
    private int selectedRowIndex = 0;


    private void OnEnable()
    {
        // Load all table structure assets
        string[] guids = AssetDatabase.FindAssets("t:TableStructure");
        tables = guids.Select(guid =>
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<TableStructure>(path);
        }).ToList();

        tableNames = tables.Select(t => t.TableName).ToArray();
    }

    public override void OnInspectorGUI()
    {
        MissionData mission = (MissionData)target;

        // Title & description
        mission.missionTitle = EditorGUILayout.TextField("Mission Title", mission.missionTitle);
        mission.missionDescription = EditorGUILayout.TextArea(mission.missionDescription, GUILayout.MinHeight(60));

        EditorGUILayout.Space(10);

        // Table selection
        selectedTableIndex = Mathf.Max(0, tables.FindIndex(t => t.TableName == mission.requiredTable));
        selectedTableIndex = EditorGUILayout.Popup("Required Table", selectedTableIndex, tableNames);
        mission.requiredTable = tableNames[selectedTableIndex];

        var selectedTable = tables[selectedTableIndex];

        EditorGUILayout.LabelField("Required Columns", EditorStyles.boldLabel);

        // Initialize selection dictionary if needed
        foreach (var col in selectedTable.Columns)
        {
            if (!columnSelections.ContainsKey(col.ColumnName))
            {
                bool isSelected = mission.requiredColumns?.Contains(col.ColumnName) == true;
                columnSelections[col.ColumnName] = isSelected;
            }
        }

        // Show checkboxes
        List<string> selectedColumns = new List<string>();
        foreach (var col in selectedTable.Columns)
        {
            columnSelections[col.ColumnName] = EditorGUILayout.ToggleLeft(col.ColumnName, columnSelections[col.ColumnName]);
            if (columnSelections[col.ColumnName])
                selectedColumns.Add(col.ColumnName);
        }

        mission.requiredColumns = selectedColumns;

        EditorGUILayout.Space(10);
        mission.requiredCondition = EditorGUILayout.TextField("Required Condition", mission.requiredCondition);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(mission);
        }


        if (GUILayout.Button("Fetch Sample Rows"))
        {
            EditorCoroutineUtility.StartCoroutine(FetchTableData(mission.requiredTable, OnRowsFetched), this);
        }


        if (fetchedRows.Count > 0)
        {
            // Get all available keys from the first row
            string[] fieldOptions = fetchedRows[0].Keys.ToArray();

            // Make primary key dropdown
            int pkIndex = Array.IndexOf(fieldOptions, mission.expectedPrimaryKeyField);
            if (pkIndex < 0) pkIndex = 0;

            pkIndex = EditorGUILayout.Popup("Primary Key Field", pkIndex, fieldOptions);
            mission.expectedPrimaryKeyField = fieldOptions[pkIndex];

            // Now show the row dropdown
            selectedRowIndex = Mathf.Clamp(selectedRowIndex, 0, fetchedRows.Count - 1);
            selectedRowIndex = EditorGUILayout.Popup("Expected Row", selectedRowIndex, rowLabels);

            if (fetchedRows.Count > selectedRowIndex &&
            fetchedRows[selectedRowIndex].TryGetValue(mission.expectedPrimaryKeyField, out string value))
            {
                mission.expectedRowIdValue = value;
                EditorGUILayout.HelpBox($"Selected value: {value}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"⚠️ Row does not contain key '{mission.expectedPrimaryKeyField}'", MessageType.Warning);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(mission);
            }
        }
    }

    private void OnRowsFetched(JArray data)
    {
        fetchedRows.Clear();
        rowLabels = new string[data.Count];

        for (int i = 0; i < data.Count; i++)
        {
            var row = data[i] as JObject;
            var dict = row.Properties().ToDictionary(p => p.Name, p => p.Value.ToString());
            fetchedRows.Add(dict);

            rowLabels[i] = string.Join(", ", dict.Take(2).Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        // ✅ Reset the index to avoid out-of-range
        selectedRowIndex = Mathf.Clamp(selectedRowIndex, 0, Mathf.Max(0, fetchedRows.Count - 1));
        Debug.Log("🧩 Keys in first row: " + string.Join(", ", fetchedRows[0].Keys));

        Repaint();
    }

    private IEnumerator FetchTableData(string tableName, Action<JArray> onComplete)
    {
        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/{tableName}?select=*";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", ServerData.k_ApiKey);
        request.SetRequestHeader("Authorization", $"Bearer {ServerData.k_ApiKey}");
        request.SetRequestHeader("Accept", "application/json");


        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"📡 Fetching data from table: {tableName}");
            Debug.Log($"Raw JSON: {request.downloadHandler.text}");

            JArray json = JArray.Parse(request.downloadHandler.text);
            onComplete?.Invoke(json);
        }
        else
        {
            Debug.LogError($"❌ Failed to fetch data from Supabase: {request.error}");
        }
    }

}
#endif
