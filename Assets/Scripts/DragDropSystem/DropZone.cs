using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    private QueryBuilder queryBuilder;
    public bool isQueryPanel;
    public bool isSelectionPanel;
    public bool isClausePanel;

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

        Debug.Log($"🔥 OnDrop triggered on: {gameObject.name}");
        DraggableItem draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (draggable == null)
        {
            Debug.Log($"❌ No DraggableItem found in");
            return;
        }

        Debug.Log($"✅ Dropped {draggable.gameObject.name} into {gameObject.name}");

        if (draggable != null)
        {
            Debug.Log($"Dropped {draggable.gameObject.name} into {gameObject.name}");
            
            if (IsValidDrop(draggable))
            {
                draggable.SetParentAndPosition(transform);
                queryBuilder.OnItemDropped(draggable);
            }
            else
            {
                Debug.LogWarning($"Invalid drop: {draggable.gameObject.name} cannot go here.");
                // draggable.SetParentAndPosition(draggable.transform.parent);
            }
        }
    }

    public bool IsValidDrop(DraggableItem i_Draggable)
    {
        bool res = false;

        if (i_Draggable.draggableType == eDraggableType.ClauseButton)
        {
            res = isQueryPanel || isClausePanel;
            Debug.Log($"[BUTTON IS {res} HERE] isQueryPanel || isClausePanel");
        }

        if (i_Draggable.draggableType == eDraggableType.SelectionButton)
        {
            res = isQueryPanel || isSelectionPanel;
            Debug.Log($"[BUTTON IS {res} HERE] isQueryPanel || isSelectionPanel");
        }
        Debug.Log($"WTF");

        return res; 
    }

}
