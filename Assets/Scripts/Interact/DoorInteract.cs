using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteract : InteractableObject
{
    public Transform teleportTarget;

    public override void Interact()
    {
        base.Interact();
        GameManager.Instance.TeleportPlayerTo(teleportTarget.position);
        // SupabaseManager.Instance.UnlockTable("CrimeEvidence");
    }
}
