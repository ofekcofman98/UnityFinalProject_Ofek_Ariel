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
    public Transform OriginalParent;
    public event Action<DraggableItem> OnDropped;
    [HideInInspector] public Transform AssignedSection; 
    public IDropZoneStrategy CurrentDropZone { get; private set; }
    private Vector3 originalPosition;
    private Transform canvasTransform;
    private int originalSiblingIndex;


    private void Awake()
    {
        canvasTransform = GetComponentInParent<Canvas>().transform;
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} Begin drag");

        OriginalParent = transform.parent;
        originalPosition = transform.position;
        originalSiblingIndex = transform.GetSiblingIndex(); 
        MoveToTopLayer();

        if (image != null) image.raycastTarget = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DropZone dropZone = FindDropZone();

        if (image != null) 
        {
            image.raycastTarget = true;
        }


        if (dropZone != null)
        {
            IDropZoneStrategy strategy = dropZone.GetStrategy();
            bool isNewDrop = dropZone.IsNewDrop(OriginalParent);

            if (!isNewDrop)
            {
                dropBackToOriginal();
                return;
            }

            if (strategy != null && strategy.IsValidDrop(this))
            {
                dropZone.OnDrop(eventData); 
                OnDropped?.Invoke(this); 
            }
            else
            {
                transform.SetParent(OriginalParent, true);
                transform.position = originalPosition;
            }
        }
        else
        {
            dropBackToOriginal();
        }

        image.raycastTarget = true;

    }

    private void MoveToTopLayer()
    {
        transform.SetParent(canvasTransform, true);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    private void dropBackToOriginal()
    {
        transform.SetParent(OriginalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    public void SetParentAndPosition(Transform newParent)
    {
        transform.SetParent(newParent, false);
        transform.localScale = Vector3.one;  
    }

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
