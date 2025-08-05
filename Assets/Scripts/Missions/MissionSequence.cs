using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission Sequence", menuName = "SQL Detective/Mission Sequence")]
public class MissionSequence : ScriptableObject
{
    public bool isTutorial = false;
    public List<MissionData> Missions = new List<MissionData>();
    [Header("Case ID (from Supabase)")]
    public string case_id; // 👈 YES. Add this field

    [Header("Dialogues")]
    public List<PersonDialogueSet> PersonDialogues;

    [Header("Final Suspect")]
    public string FinalAnswerPersonId;  // 👈 This is the one criminal

}
