using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionData))]
public class MissionDataEditor : Editor
{
    private List<string> tableNames = new List<string>();
    private bool tablesFetched = false;

    public override void OnInspectorGUI()
    {
        MissionData mission = (MissionData)target;

        if (!tablesFetched)
        {
            FetchTablesFromSupabase();
            tablesFetched = true;
        }

        // Dropdown for requiredTable
        int selectedIndex = Mathf.Max(0, tableNames.IndexOf(mission.requiredTable.Name));
        selectedIndex = EditorGUILayout.Popup("Required Table", selectedIndex, tableNames.ToArray());

        if (selectedIndex >= 0 && selectedIndex < tableNames.Count)
        {
            // mission.requiredTable.Name = tableNames[selectedIndex];
        }

        DrawDefaultInspector();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(mission);
        }
    }

    private void FetchTablesFromSupabase()
    {
        tableNames.Clear();
        tableNames.Add("CrimeEvidence");
        tableNames.Add("Witnesses");
        tableNames.Add("Persons");
    }

}
