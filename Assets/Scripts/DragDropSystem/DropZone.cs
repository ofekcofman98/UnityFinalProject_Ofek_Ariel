using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private MonoBehaviour dropZoneStrategy;
    private IDropZoneStrategy strategy => dropZoneStrategy as IDropZoneStrategy;

    private QueryBuilder queryBuilder;

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

        if (draggable == null || strategy == null)
        {
            Debug.Log("[][]");
            return;
        }

        if (strategy.IsValidDrop(draggable))
        {
            Debug.Log("[][] it's valid");
            // if (strategy.DoesTriggerDropAction(draggable))
            // {
                strategy.HandleDrop(draggable, this);
            // }
            
            // draggable.OnDropped?.Invoke(draggable);
        }
        else
        {
            Debug.Log("[][] it's NOT valid");
        }
    }

    public IDropZoneStrategy GetStrategy()
    {
        return strategy;
    }

}
