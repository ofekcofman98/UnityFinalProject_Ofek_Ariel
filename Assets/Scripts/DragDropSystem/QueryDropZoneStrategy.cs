using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DropZone))]
public class QueryDropZoneStrategy : MonoBehaviour, IDropZoneStrategy
{
    public void HandleDrop(DraggableItem draggable, DropZone zone)
    {
        draggable.SetParentAndPosition(draggable.AssignedSection);
        draggable.OnDropped?.Invoke(draggable);
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

    public bool IsValidDrop(DraggableItem draggable)
    {
        bool valid = draggable.AssignedSection != null && transform == draggable.AssignedSection.parent;
        // Debug.Log($"[QueryDropZoneStrategy] IsValidDrop: draggable: {draggable.name}, drop target: {transform.name}, assigned section parent: {draggable.AssignedSection?.parent?.name}, valid: {valid}");
        return valid;
    }

}
