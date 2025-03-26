using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

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
    public bool isInQueryPanel { get; private set; } = false;

    private void Awake()
    {
        canvasTransform = GetComponentInParent<Canvas>().transform;
        image = GetComponent<Image>();
        originalContainer = transform.parent; 
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

        if (dropZone != null &&  dropZone.IsValidDrop(this))
        {
            dropZone.OnDrop(eventData); 

            if (isDroppedInQuery(dropZone))
            {
                Debug.Log($"### {gameObject.name} should be placed in {AssignedSection.name}");
                SetParentAndPosition(AssignedSection);
                
            }
            else
            {
                SetParentAndPosition(dropZone.transform);
            }

            OnDropped?.Invoke(this); 

        }
        else
        {
            SetParentAndPosition(originalParent);
        }

        image.raycastTarget = true;
    }

    private bool isDroppedInQuery(DropZone i_DropZone)
    {

        return i_DropZone.isQueryPanel || transform.parent.GetComponent<DropZone>()?.isQueryPanel == true;
        
        // return (i_DropZone.isQueryPanel || i_DropZone.isSelectZone || i_DropZone.isWhereZone) 
        //         && AssignedSection != null;
 
 
        // bool wasInQueryPanel = isInQueryPanel;
        // isInQueryPanel = i_DropZone.isQueryPanel;

        // return wasInQueryPanel != isInQueryPanel;
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
        // transform.localPosition = Vector3.zero;  // ✅ Reset position for layout
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
