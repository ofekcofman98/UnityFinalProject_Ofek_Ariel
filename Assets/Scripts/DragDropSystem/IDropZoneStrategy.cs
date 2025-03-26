using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDropZoneStrategy
{
    bool IsValidDrop(DraggableItem draggable);
    void HandleDrop(DraggableItem draggable, DropZone zone);
    // bool DoesTriggerDropAction(DraggableItem i_Draggable);
}
