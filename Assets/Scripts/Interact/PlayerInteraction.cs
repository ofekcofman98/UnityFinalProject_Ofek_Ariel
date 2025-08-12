using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{

    [Header("Interaction")]
    [SerializeField] private float maxDistance = 3.0f;
    [SerializeField] private float aimAssistRadius = 0.15f; // 0 = pure ray, >0 = small cone
    [SerializeField] private LayerMask interactableMask;    // set to Interactable layer(s) in Inspector
    [SerializeField] private Camera cam;                    // assign your player camera

    [Header("UI")]
    [SerializeField] private string hintText = "Press E to interact";

    private InteractableObject currentInteractable;


    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }


    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.SqlMode)
        {
            ClearTarget();
            return;
        }

        UpdateTarget();


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

    private void UpdateTarget()
    {
        InteractableObject newTarget = null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        bool hitSomething = aimAssistRadius > 0f
            ? Physics.SphereCast(ray, aimAssistRadius, out hit, maxDistance, interactableMask, QueryTriggerInteraction.Collide)
            : Physics.Raycast(ray, out hit, maxDistance, interactableMask, QueryTriggerInteraction.Collide);

        if (hitSomething)
        {
            hit.collider.TryGetComponent(out newTarget);
        }

        if (newTarget != currentInteractable)
        {
            if (newTarget != null)
                UIManager.Instance.ShowHint("Press E to interact");
            else
                UIManager.Instance.HideHint();

            currentInteractable = newTarget;
        }
    }

    private void ClearTarget()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
            UIManager.Instance.HideHint();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (cam == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * maxDistance);
    }
#endif
}
