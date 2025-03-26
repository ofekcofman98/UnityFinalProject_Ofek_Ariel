using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DropZone))]
public class QueryDropZoneStrategy : MonoBehaviour, IDropZoneStrategy
{
    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {
        draggable.SetParentAndPosition(draggable.AssignedSection);
    }

    public bool IsValidDrop(DraggableItem draggable)
    {
        bool valid = draggable.AssignedSection != null && transform == draggable.AssignedSection.parent;
        Debug.Log($"[QueryDropZoneStrategy] IsValidDrop: draggable: {draggable.name}, drop target: {transform.name}, assigned section parent: {draggable.AssignedSection?.parent?.name}, valid: {valid}");
        return valid;
    }

    // public bool DoesTriggerDropAction(DraggableItem i_Draggable)
    // {
    //     return i_Draggable.CurrentDropZone != this;
    // }


}
