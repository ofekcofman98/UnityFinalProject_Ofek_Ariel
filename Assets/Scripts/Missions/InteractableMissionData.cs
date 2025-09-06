using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New SQL Mission", menuName = "SQL Detective/Mission/Interactable Mission")]
public class InteractableMissionData : MissionData
{
    public string requiredObjectId;
    private string _lastTriggeredId;
    public List<PersonDialogueTrigger> dialogueTriggers;
    public List<string> additionalHighlightObjectIds; 


    public void SetTriggeredObject(string id)
    {
        _lastTriggeredId = id;
    }

    public override bool Validate()
    {
        return _lastTriggeredId == requiredObjectId;
    }

    public override void ShowUI(MissionUIManager uiManager)
    {
        uiManager.DisplayStandardMission(this);
    }

}

[System.Serializable]
public class PersonDialogueTrigger
{
    public string personId;               // e.g., "P002"
    [TextArea(2, 5)]
    public string dialogueText;           // what they say during this mission
    public bool givesClue;
    public string clueId;
}

