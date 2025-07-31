using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Custom Tutorial Step", menuName = "SQL Detective/Mission/Custom Tutorial Step")]
public class CustomTutorialMissionData : MissionData
{
    public string requiredStepId; // e.g. "ClickMap", "ClickSql", etc.

    private bool completed = false;

    public void MarkAsCompleted()
    {
        completed = true;
    }

    public override bool Validate()
    {
        return completed;
    }

    public override void ShowUI(MissionUIManager uiManager)
    {
        uiManager.DisplayStandardMission(this);
    }

}
