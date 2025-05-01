#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TableStructureViewer : EditorWindow
{
    private Vector2 scrollPos;
    private List<TableStructure> allTables;

    [MenuItem("SQL Detective/View Supabase Table Structures")]
    public static void ShowWindow()
    {
        var window = GetWindow<TableStructureViewer>("Supabase Tables");
        window.LoadAllStructures();
    }

    private void LoadAllStructures()
    {
        string[] guids = AssetDatabase.FindAssets("t:TableStructure");
        allTables = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<TableStructure>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(structure => structure != null)
            .ToList();
    }

    private void OnGUI()
    {
        if (allTables == null)
        {
            if (GUILayout.Button("Load Supabase Tables"))
                LoadAllStructures();
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var table in allTables)
        {
            EditorGUILayout.LabelField($"🧩 {table.TableName}", EditorStyles.boldLabel);
            foreach (var col in table.Columns)
            {
                EditorGUILayout.LabelField($"  • {col.ColumnName} ({col.DataType})");
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif
