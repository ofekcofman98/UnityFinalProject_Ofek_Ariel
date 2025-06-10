using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eInteractableType
{
    Teleport,
    TableUnlock,
    MessagePopup,
    StartDialogue
}

public class InteractableObject : MonoBehaviour
{
    public string objectName;
    public eInteractableType interactableType;
    [SerializeField] private string interactableId;
    public string InteractableId => interactableId;


    public virtual void Interact()
    {
        Debug.Log($"Interacted with {objectName}");

        MissionsManager.Instance.ValidateInteractableMission(interactableId);
    }

}
