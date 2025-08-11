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

    [SerializeField] protected AudioCue interactCue;

    public string InteractableId
    {
        get => interactableId;
        set => interactableId = value;
    }


    public virtual void Interact()
    {
        // Debug.Log($"Interacted with {objectName}");

        if (interactCue != null)
        {
            SfxManager.Instance.Play2D(interactCue);
            Debug.Log("[interact] Interact fired");
        }
        else
        {
            Debug.Log("[interact] Im null !");
        }

        MissionsManager.Instance.ValidateInteractableMission(interactableId);
    }

}
