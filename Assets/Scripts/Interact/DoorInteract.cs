using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteract : InteractableObject
{
    public override void Interact()
    {
        base.Interact();
        LocationManager.Instance.ShowMenu();
    }
}
