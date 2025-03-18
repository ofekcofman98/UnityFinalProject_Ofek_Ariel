using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eDraggableType
{
    ClauseButton,
    SelectionButton,
}

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public eDraggableType draggableType;
    private Transform originalParent;
    public event Action<DraggableItem> OnDropped;
    [HideInInspector] public Transform AssignedSection; 

    private Vector3 originalPosition;
    private Transform canvasTransform;
    private Transform originalContainer; 

    private void Awake()
    {
        canvasTransform = GetComponentInParent<Canvas>().transform;
        image = GetComponent<Image>();
        originalContainer = transform.parent; 
    }

    public void AssignSection(Transform section)
    {
        AssignedSection = section;
        Debug.Log($"🔵 {gameObject.name} assigned to section: {section.name}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} Begin drag");

        originalParent = transform.parent;
        originalPosition = transform.position;

        MoveToTopLayer();

        if (image != null) image.raycastTarget = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} End drag");

        DropZone dropZone = FindDropZone();
        if (image != null) image.raycastTarget = true;

        if (dropZone != null && dropZone.IsValidDrop(this))
        {
            Debug.Log("Dropped inside DropZone!");
            dropZone.OnDrop(eventData); 
            SetParentAndPosition(dropZone.transform);
            OnDropped?.Invoke(this); 
        }
        else
        {
            Debug.Log("Dropped outside, returning.");
            SetParentAndPosition(originalParent);
        }

        image.raycastTarget = true;

    // 🔥 Ensure Raycast Target is RE-ENABLED after dragging stops

    }


    private void MoveToTopLayer()
    {
        transform.SetParent(canvasTransform, true);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    // 🔥 Extracted method for setting parent & resetting position
    public void SetParentAndPosition(Transform newParent)
    {
        transform.SetParent(newParent, false);
        transform.localScale = Vector3.one;  // ✅ Ensure consistent size
        transform.localPosition = Vector3.zero;  // ✅ Reset position for layout
    }

    // 🔥 Extracted method for detecting DropZones via Raycast
    private DropZone FindDropZone()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            DropZone dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null)
            {
                Debug.Log($"found {dropZone.ToString()}");
                return dropZone;
            }
        }

        return null;
    }
}
