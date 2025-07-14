using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission Sequence", menuName = "SQL Detective/Mission Sequence")]
public class MissionSequence : ScriptableObject
{
    public List<MissionData> Missions = new List<MissionData>();

    [Header("Final Suspect")]
    public string FinalAnswerPersonId;  // 👈 This is the one criminal

}
