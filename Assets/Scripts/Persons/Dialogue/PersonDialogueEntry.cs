using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Person Dialogue Entry", menuName = "SQL Detective/Dialogue/Person Dialogue Entry")]
public class PersonDialogueEntry : ScriptableObject
{
    public string personId;      // "002"
    public string missionId;     // "InterrogateWitnesses" or missionTitle
    [TextArea(2, 5)] public string dialogueText;
    public bool givesClue;
    public string clueId;
}
