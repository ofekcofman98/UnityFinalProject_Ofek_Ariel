using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteract : InteractableObject
{
    public override void Interact()
    {
        base.Interact();
            Debug.Log("[DoorInteract] Interact fired");

    var cueField = typeof(InteractableObject)
        .GetField("interactCue", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
        .GetValue(this) as AudioCue;
    Debug.Log($"[DoorInteract] Cue={(cueField ? cueField.name : "NULL")}");

        LocationManager.Instance.ShowMenu();
    }
}
