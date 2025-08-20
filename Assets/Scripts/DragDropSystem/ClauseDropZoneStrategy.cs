using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(DropZone))]
public class ClauseDropZoneStrategy : MonoBehaviour, IDropZoneStrategy
{
    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {

        if (!IsValidDrop(draggable))
        {
            Debug.LogWarning($"[ClauseDropZoneStrategy] Invalid drop for {draggable.name} — skipping parent change.");
            return;
        }

        draggable.SetParentAndPosition(zone.transform);
        // draggable.OnRemoved?.Invoke();
        if (IsNewDrop(draggable.OriginalParent))
        {
            draggable.OnRemoved?.Invoke();
        }

    }

    public bool IsValidDrop(DraggableItem i_Draggable)
    {
        return i_Draggable.draggableType == eDraggableType.ClauseButton;
    }
    public bool IsNewDrop(Transform i_OriginalParent)
    {
        return i_OriginalParent != transform;
    }

}
