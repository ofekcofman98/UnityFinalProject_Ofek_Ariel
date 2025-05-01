#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MissionData))]
public class MissionDataEditor : Editor
{
    private List<TableStructure> tables;
    private string[] tableNames;
    private int selectedTableIndex;

    private Dictionary<string, bool> columnSelections = new Dictionary<string, bool>();

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
    }
}
#endif
