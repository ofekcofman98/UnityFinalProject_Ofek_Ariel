using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Mission", menuName = "SQL Detective/Mission")]
public class MissionData : ScriptableObject
{
    public string missingTitle;
    [TextArea(3, 5)] public string missionDescription; 
    public Table requiredTable;
    public List<Column> requiredColumns;
    public string requiredCondition;
}
