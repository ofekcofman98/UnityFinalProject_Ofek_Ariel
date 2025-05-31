using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private InteractableObject currentInteractable;

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Entered something");
        if (other.TryGetComponent<InteractableObject>(out var interactable))
        {
            currentInteractable = interactable;
            // show UI hint: "Press E to interact"
            UIManager.Instance.ShowHint("Press E to interact"); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<InteractableObject>(out var interactable) &&
            interactable == currentInteractable)
        {
            currentInteractable = null;
            UIManager.Instance.HideHint();
        }
    }


}
