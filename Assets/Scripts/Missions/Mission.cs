using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [CreateAssetMenu(fileName = "New Mission", menuName = "SQL Detective/Mission")]
public class Mission : ScriptableObject
{
    public string MissionName;
    public string MissionDescription;
    public QueryData ExpectedQueryData;
    public string ExpectedResultQuery;  
}
