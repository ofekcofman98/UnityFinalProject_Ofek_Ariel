using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DropZone))]
public class SectionDropZoneStrategy : MonoBehaviour, IDropZoneStrategy
{
    [SerializeField] private Transform section;

    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {
        draggable.SetParentAndPosition(draggable.AssignedSection);
    }

    public bool IsValidDrop(DraggableItem draggable)
    {
        return true;//draggable.AssignedSection == section;
    }

    public bool DoesTriggerDropAction(DraggableItem i_Draggable)
    {
        return false;
    }


}
