using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PersonDialogueLine
{
    [TextArea(2, 4)]
    public string dialogueText;

    public string unlockAfterMissionTitle;  // ✅ this replaces the missing missionId
    public bool oneTimeOnly = true;
    public bool isFallback = false;
}
