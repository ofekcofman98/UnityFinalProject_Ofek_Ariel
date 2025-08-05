#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MissionSequence))]
public class MissionSequenceEditor : Editor
{
    private JArray fetchedRows;
    private string[] suspectNames;
    private int selectedIndex;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MissionSequence sequence = (MissionSequence)target;

        // Draw everything manually
        EditorGUILayout.PropertyField(serializedObject.FindProperty("case_id")); // 👈 Add this line
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isTutorial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Missions"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FinalAnswerPersonId"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎯 Final Suspect Selection", EditorStyles.boldLabel);

        if (GUILayout.Button("Fetch Suspects from Supabase"))
        {
            EditorCoroutineUtility.StartCoroutine(FetchSuspectData(), this);
        }

        if (fetchedRows != null && suspectNames != null)
        {
            selectedIndex = Mathf.Max(0, Enumerable.Range(0, fetchedRows.Count)
                .FirstOrDefault(i => fetchedRows[i]["person_id"]?.ToString() == sequence.FinalAnswerPersonId));

            selectedIndex = EditorGUILayout.Popup("Choose Final Suspect", selectedIndex, suspectNames);

            string selectedId = fetchedRows[selectedIndex]["person_id"]?.ToString();
            if (!string.IsNullOrEmpty(selectedId))
            {
                sequence.FinalAnswerPersonId = selectedId;
                EditorGUILayout.HelpBox($"Selected ID: {selectedId}", MessageType.Info);
                EditorUtility.SetDirty(sequence);
            }
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("💬 Person Dialogue Sets", EditorStyles.boldLabel);

        SerializedProperty dialogueList = serializedObject.FindProperty("PersonDialogues");
        EditorGUILayout.PropertyField(dialogueList, new GUIContent("Dialogue Sets"), true);

        if (GUILayout.Button("Auto-Fill from Project"))
        {
            var allDialogueSets = AssetDatabase.FindAssets("t:PersonDialogueSet")
                .Select(guid => AssetDatabase.LoadAssetAtPath<PersonDialogueSet>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(ds => ds != null)
                .ToList();

            foreach (var set in allDialogueSets)
            {
                if (!sequence.PersonDialogues.Contains(set))
                {
                    sequence.PersonDialogues.Add(set);
                    EditorUtility.SetDirty(sequence);
                }
            }

            Debug.Log($"✅ Added {allDialogueSets.Count} PersonDialogueSets to this MissionSequence.");
        }

        serializedObject.ApplyModifiedProperties();

    }

    private IEnumerator FetchSuspectData()
    {
        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/Persons?select=*";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", ServerData.k_ApiKey);
        request.SetRequestHeader("Authorization", $"Bearer {ServerData.k_ApiKey}");
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            fetchedRows = JArray.Parse(request.downloadHandler.text);

            suspectNames = fetchedRows.Select(row =>
            {
                string fn = row.Value<string>("first_name");
                string ln = row.Value<string>("last_name");
                string id = row.Value<string>("person_id");
                return $"{fn} {ln} ({id})";
            }).ToArray();

            Repaint();
        }
        else
        {
            Debug.LogError("❌ Failed to fetch suspects: " + request.error);
        }
    }
}
#endif
