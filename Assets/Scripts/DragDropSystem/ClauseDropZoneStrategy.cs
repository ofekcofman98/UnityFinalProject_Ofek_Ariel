using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(DropZone))]
public class ClauseDropZoneStrategy : MonoBehaviour, IDropZoneStrategy
{
    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {
        throw new System.NotImplementedException();
    }

    public bool IsValidDrop(DraggableItem i_Draggable)
    {
        return i_Draggable.draggableType == eDraggableType.ClauseButton;
    }
}
