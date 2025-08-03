using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Person Dialogue Set", menuName = "SQL Detective/Dialogue/Person Dialogue Set")]
public class PersonDialogueSet : ScriptableObject
{
    public string personId; // e.g. "P002"
    public List<PersonDialogueLine> lines;
}
