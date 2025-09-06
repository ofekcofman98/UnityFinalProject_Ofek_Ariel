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

[CustomEditor(typeof(MissionData), true)]
public class MissionDataEditor : Editor
{
    private List<TableStructure> tables;
    private string[] tableNames;
    private int selectedTableIndex;
    private Dictionary<string, bool> columnSelections = new Dictionary<string, bool>();

    private List<Dictionary<string, string>> fetchedRows = new List<Dictionary<string, string>>();
    private string[] rowLabels;
    private int selectedRowIndex = 0;
    private bool showExtraHighlights = true;
 
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
        MissionData baseMission = (MissionData)target;

        // Shared fields
        baseMission.missionTitle = EditorGUILayout.TextField("Mission Title", baseMission.missionTitle);
        baseMission.missionDescription = EditorGUILayout.TextArea(baseMission.missionDescription, GUILayout.MinHeight(60));

        baseMission.isTutorial = EditorGUILayout.Toggle("Is Tutorial?", baseMission.isTutorial);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Table Unlock Settings", EditorStyles.boldLabel);
        baseMission.unlocksTable = EditorGUILayout.Toggle("Unlocks Table?", baseMission.unlocksTable);

        if (baseMission.unlocksTable)
        {
            baseMission.tableToUnlock = DrawTableUnlockDropdown(baseMission.tableToUnlock);
        }

        EditorGUILayout.Space(15);

        // 🔍 Show specific section by subclass
        if (baseMission is SQLMissionData sql)
        {
            DrawSQLSection(sql);
        }
        else if (baseMission is InteractableMissionData interactable)
        {
            DrawInteractableSection(interactable);
        }
        else if (baseMission is CustomTutorialMissionData custom)
        {
            DrawCustomTutorialStepSection(custom);
        }
        else if (baseMission is TutorialPopupMissionData tutorial)
        {
            DrawTutorialPopupSection(tutorial);
        }



        if (GUI.changed)
            EditorUtility.SetDirty(baseMission);
    }
    private void DrawSQLSection(SQLMissionData mission)
    {
        selectedTableIndex = Mathf.Max(0, tables.FindIndex(t => t.TableName == mission.requiredTable));
        selectedTableIndex = EditorGUILayout.Popup("Required Table", selectedTableIndex, tableNames);
        mission.requiredTable = tableNames[selectedTableIndex];

        var selectedTable = tables[selectedTableIndex];

        EditorGUILayout.LabelField("Required Columns", EditorStyles.boldLabel);

        // Rebuild selection dictionary
        foreach (var col in selectedTable.Columns)
        {
            if (!columnSelections.ContainsKey(col.ColumnName))
            {
                bool isSelected = mission.requiredColumns?.Contains(col.ColumnName) == true;
                columnSelections[col.ColumnName] = isSelected;
            }
        }

        // Column checkboxes
        List<string> selectedCols = new List<string>();
        foreach (var col in selectedTable.Columns)
        {
            columnSelections[col.ColumnName] = EditorGUILayout.ToggleLeft(col.ColumnName, columnSelections[col.ColumnName]);
            if (columnSelections[col.ColumnName]) selectedCols.Add(col.ColumnName);
        }
        mission.requiredColumns = selectedCols;

        mission.requiredCondition = EditorGUILayout.TextField("Required Condition", mission.requiredCondition);

        DrawRowSelection(sqlMission: mission);
    }

    private void DrawInteractableSection(InteractableMissionData mission)
    {
        // mission.requiredObjectId = EditorGUILayout.TextField("Required Interactable ID", mission.requiredObjectId);
        mission.requiredObjectId = EditorGUILayout.TextField("Required Interactable ID", mission.requiredObjectId);

        EditorGUILayout.Space(8);
        showExtraHighlights = EditorGUILayout.Foldout(showExtraHighlights, "Additional Highlighted Objects");
        if (showExtraHighlights)
        {
            if (mission.additionalHighlightObjectIds == null)
                mission.additionalHighlightObjectIds = new List<string>();

            // List all existing additional IDs
            for (int i = 0; i < mission.additionalHighlightObjectIds.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                mission.additionalHighlightObjectIds[i] = EditorGUILayout.TextField($"Object {i + 1}", mission.additionalHighlightObjectIds[i]);

                if (GUILayout.Button("❌", GUILayout.Width(30)))
                {
                    mission.additionalHighlightObjectIds.RemoveAt(i);
                    i--; // Shift index to prevent skip
                    continue;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("➕ Add Highlight Object"))
            {
                mission.additionalHighlightObjectIds.Add(string.Empty);
            }
        }

    }

    private string DrawTableUnlockDropdown(string currentTable)
    {
        int unlockIndex = Mathf.Max(0, tables.FindIndex(t => t.TableName == currentTable));
        unlockIndex = EditorGUILayout.Popup("Table to Unlock", unlockIndex, tableNames);
        return tableNames[unlockIndex];
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

    private void DrawRowSelection(SQLMissionData sqlMission)
    {
        if (GUILayout.Button("Fetch Sample Rows"))
        {
            EditorCoroutineUtility.StartCoroutine(FetchTableData(sqlMission.requiredTable, OnRowsFetched), this);
        }

        if (fetchedRows.Count > 0)
        {
            string[] fieldOptions = fetchedRows[0].Keys.ToArray();

            int pkIndex = Array.IndexOf(fieldOptions, sqlMission.expectedPrimaryKeyField);
            if (pkIndex < 0) pkIndex = 0;

            pkIndex = EditorGUILayout.Popup("Primary Key Field", pkIndex, fieldOptions);
            sqlMission.expectedPrimaryKeyField = fieldOptions[pkIndex];

            selectedRowIndex = Mathf.Clamp(selectedRowIndex, 0, fetchedRows.Count - 1);
            selectedRowIndex = EditorGUILayout.Popup("Expected Row", selectedRowIndex, rowLabels);

            if (fetchedRows.Count > selectedRowIndex &&
                fetchedRows[selectedRowIndex].TryGetValue(sqlMission.expectedPrimaryKeyField, out string value))
            {
                sqlMission.expectedRowIdValue = value;
                EditorGUILayout.HelpBox($"Selected value: {value}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"⚠️ Row does not contain key '{sqlMission.expectedPrimaryKeyField}'", MessageType.Warning);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(sqlMission);
            }
        }
    }

    private void DrawCustomTutorialStepSection(CustomTutorialMissionData mission)
    {
        EditorGUILayout.LabelField("Custom Step", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Define a unique ID for the tutorial step the player must trigger (e.g. ClickMap, OpenSQL, etc.)", MessageType.Info);
        mission.requiredStepId = EditorGUILayout.TextField("Required Step ID", mission.requiredStepId);
    }

    private void DrawTutorialPopupSection(TutorialPopupMissionData mission)
    {
        // EditorGUILayout.LabelField("Popup Text", EditorStyles.boldLabel);
        // mission.popupText = EditorGUILayout.TextArea(mission.popupText, GUILayout.MinHeight(60));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Optional Popup Image", EditorStyles.boldLabel);
        mission.popupImage = (Sprite)EditorGUILayout.ObjectField("Popup Image", mission.popupImage, typeof(Sprite), allowSceneObjects: false);
    }




}
#endif
