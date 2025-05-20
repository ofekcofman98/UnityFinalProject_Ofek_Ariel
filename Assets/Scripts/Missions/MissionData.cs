using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Mission", menuName = "SQL Detective/Mission")]
public class MissionData : ScriptableObject
{

    public int missionNumber;
    public string missionTitle;
    [TextArea(3, 5)] public string missionDescription; 
    
    public string requiredTable;
    public List<string> requiredColumns;
    public string requiredCondition;

    public string expectedPrimaryKeyField;
    public string expectedRowIdValue;
    
    [Header("Table Unlock Settings")]

    public bool unlocksTable;
    public string tableToUnlock;

private void OnEnable() => Debug.Log("MissionData loaded");


}
