using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum eDropZoneType
{
    QueryPanel,
    ClausePanel,
    SelectionPanel
}

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private MonoBehaviour dropZoneBehavior;
    private IDropZoneStrategy behavior => dropZoneBehavior as IDropZoneStrategy;


    private QueryBuilder queryBuilder;
    // public bool isQueryPanel;
    public bool isSelectionPanel;
    public bool isClausePanel;
    public bool isQueryPanel;

public bool isSelectZone;
public bool isFromZone;
public bool isWhereZone;

// public bool isQueryPanel => isSelectZone || isFromZone || isWhereZone;

    

    private void Awake()
    {
        queryBuilder = FindObjectOfType<QueryBuilder>();
        if (queryBuilder == null)
        {
            Debug.LogError("DropZone could not find QueryBuilder!");
        }
        Debug.Log($"[DropZone] is awake");
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();

        if(draggable == null || !IsValidDrop(draggable))
        {
            return;
        }

        if(isQueryPanel && draggable.AssignedSection != null)
        {
            draggable.SetParentAndPosition(draggable.AssignedSection);
        }
        else
        {
            Debug.Log($"[IM HERE], isQueryPanel: {isQueryPanel}, draggable.AssignedSection: {draggable.AssignedSection.name} ");
            draggable.SetParentAndPosition(transform);
        }
        
        Debug.Log($"Dropped {draggable.gameObject.name} into {gameObject.name}");

    }

    public bool IsValidDrop(DraggableItem i_Draggable)
    {
        if (isQueryPanel || isSelectZone || isFromZone || isWhereZone)
        {
            return true;
        }

            // 🔹 Always allow dropping back to the origin section (AssignedSection)
    if (i_Draggable.AssignedSection == this.transform)
    {
        Debug.Log($"✅ Valid drop: returning to original section {this.name}");
        return true;
    }

    // 🔹 Clause buttons (SELECT, FROM, WHERE)
    if (i_Draggable.draggableType == eDraggableType.ClauseButton)
    {
        bool valid = isQueryPanel || isClausePanel;
        Debug.Log($"Clause drop to {this.name}: isQueryPanel={isQueryPanel}, isClausePanel={isClausePanel}, ✅ Valid: {valid}");
        return valid;
    }

    // 🔹 Selection buttons (table, column, value...)
    if (i_Draggable.draggableType == eDraggableType.SelectionButton)
    {
        bool valid = isQueryPanel || isSelectionPanel;
        Debug.Log($"Selection drop to {this.name}: isQueryPanel={isQueryPanel}, isSelectionPanel={isSelectionPanel}, ✅ Valid: {valid}");
        return valid;
    }

    Debug.LogWarning($"❌ Unknown type or invalid drop: {i_Draggable.gameObject.name}");
    return false;







        // bool res = false;

        // if (i_Draggable.AssignedSection == this.transform)
        // {
        //     return true;
        // }

        // if (i_Draggable.draggableType == eDraggableType.ClauseButton)
        // {
        //     // if 
        //     // res = isQueryPanel || isClausePanel;
        //     Debug.Log($"[BUTTON IS {res} HERE] isQueryPanel || isClausePanel");
        // }

        // if (i_Draggable.draggableType == eDraggableType.SelectionButton)
        // {
        //     res = isQueryPanel || isSelectionPanel;
        //     Debug.Log($"[BUTTON IS {res} HERE] isQueryPanel || isSelectionPanel");
        // }
        // Debug.Log($"WTF");

        // return res; 
    }

    private bool isOriginZone()
    {
        return !isQueryPanel;
    }

}
