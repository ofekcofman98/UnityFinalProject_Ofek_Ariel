using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private MonoBehaviour dropZoneStrategy;
    private IDropZoneStrategy strategy => dropZoneStrategy as IDropZoneStrategy;


    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();

        if (draggable == null || strategy == null)
        {
            return;
        }

    if (!IsNewDrop(draggable.OriginalParent))
    {
        Debug.Log("[DropZone.OnDrop]: Drop is not new — ignoring.");
        return;
    }


        if (strategy.IsValidDrop(draggable))
        {
            strategy.HandleDrop(draggable, this);
        }
    }

    public bool IsNewDrop(Transform i_OriginalParent)
    {
        return strategy.IsNewDrop(i_OriginalParent);
    }

    public IDropZoneStrategy GetStrategy()
    {
        return strategy;
    }

}
