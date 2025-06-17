using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPanelStrategy : MonoBehaviour, IDropZoneStrategy

{
    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {
        draggable.SetParentAndPosition(transform);
        // draggable.OnRemoved?.Invoke();
    if (IsNewDrop(draggable.OriginalParent))
    {
        draggable.OnRemoved?.Invoke();
    }

    }

    public bool IsValidDrop(DraggableItem draggable)
    {
        return draggable.draggableType == eDraggableType.SelectionButton;
    }

    public bool IsNewDrop(Transform i_OriginalParent)
    {
        return i_OriginalParent != transform;
    }
}