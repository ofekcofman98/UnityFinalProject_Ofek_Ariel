using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class MissionData : ScriptableObject
{
    public int missionNumber;
    public string missionTitle;
    [TextArea(3, 5)] public string missionDescription;

    [Header("Table Unlock Settings")]
    public bool unlocksTable;
    public string tableToUnlock;
    public bool isTutorial = false;
    // private void OnEnable() => Debug.Log("MissionData loaded");
    public abstract bool Validate();
    public abstract void ShowUI(MissionUIManager uiManager);

}
