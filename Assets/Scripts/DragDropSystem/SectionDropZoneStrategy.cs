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
        draggable.OnDropped?.Invoke(draggable);
    }

    public bool IsValidDrop(DraggableItem draggable)
    {
        return true;
    }

    public bool IsNewDrop(Transform i_OriginalParent)
    {
        bool res = true;

        IDropZoneStrategy originalStrategy = i_OriginalParent.GetComponent<DropZone>()?.GetStrategy();
        IDropZoneStrategy currentStrategy = GetComponent<DropZone>()?.GetStrategy();

        if ((originalStrategy is QueryDropZoneStrategy || originalStrategy is SectionDropZoneStrategy) &&
            (currentStrategy is QueryDropZoneStrategy  || currentStrategy is SectionDropZoneStrategy))
            {
                res = false;
            }
        
        return res;
    }
}
