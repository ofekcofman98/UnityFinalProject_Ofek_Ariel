using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPanelStrategy : MonoBehaviour, IDropZoneStrategy

{
    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {
        draggable.SetParentAndPosition(transform);
    }

    public bool IsValidDrop(DraggableItem draggable)
    {
        return draggable.draggableType == eDraggableType.SelectionButton;
    }
    public bool DoesTriggerDropAction(DraggableItem i_Draggable)
    {
        return false;
    }

}