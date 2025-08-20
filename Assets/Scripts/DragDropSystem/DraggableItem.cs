using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public enum eDraggableType
{
    ClauseButton,
    SelectionButton,
    None,
}

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public eDraggableType draggableType;
    public Transform OriginalParent;
    public Action<DraggableItem> OnDropped;
    public Action OnRemoved;
    [HideInInspector] public Transform AssignedSection;
    public IDropZoneStrategy CurrentDropZone { get; private set; }
    private Vector3 originalPosition;
    private Transform canvasTransform;
    private int originalSiblingIndex;
    public int? ConditionIndex { get; set; } // for knowing which condition the button is 

    [SerializeField] private AudioCue dragCue;
    [SerializeField] private AudioCue dropCueCorrect;
    [SerializeField] private AudioCue dropCueWrong;


    private bool _isDragging = false;   // ✅ Prevents re-entering drag state

    private void Awake()
    {
        canvasTransform = GetComponentInParent<Canvas>().transform;
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (_isDragging) return;               // ✅ Prevents duplicate execution
        _isDragging = true;                    // ✅ Mark that drag started

        // Debug.Log($"🟡 BEGIN DRAG: {gameObject.name}");

        OriginalParent = transform.parent;
        originalPosition = transform.position;
        originalSiblingIndex = transform.GetSiblingIndex();
        MoveToTopLayer();

        if (image != null) image.raycastTarget = false;

        if (dragCue != null)
        {
            SfxManager.Instance.Play2D(dragCue);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
            var dropZone = FindDropZone();

    if (image != null) image.raycastTarget = true;

    if (dropZone == null)
    {
        dropBackToOriginal();
        PlayWrong();
        _isDragging = false;
        return;
    }

    var strategy = dropZone.GetStrategy();
    var isNewDrop = dropZone.IsNewDrop(OriginalParent);

    if (!isNewDrop)
    {
        // same family (Query/Section) – treat as “return”
        dropBackToOriginal();
        PlayWrong();
        _isDragging = false;
        return;
    }

    if (strategy != null && strategy.IsValidDrop(this))
    {
        dropZone.OnDrop(eventData);
        PlayCorrect();
    }
    else
    {
        // ❗ for ClauseDropZone invalid drops, use the SAME return path
        dropBackToOriginal();
        PlayWrong();
    }

    _isDragging = false;

        // DropZone dropZone = FindDropZone();

        // if (image != null)
        // {
        //     image.raycastTarget = true;
        // }


        // if (dropZone != null)
        // {
        //     IDropZoneStrategy strategy = dropZone.GetStrategy();
        //     bool isNewDrop = dropZone.IsNewDrop(OriginalParent);

        //     if (!isNewDrop)
        //     {
        //         // Debug.Log("[OnEndDrag]: wasn't dropped in a new place, dropped back to original");
        //         dropBackToOriginal();

        //         if (dropCueWrong != null)
        //         {
        //             SfxManager.Instance.Play2D(dropCueWrong);
        //         }

        //         return;
        //     }

        //     if (strategy != null && strategy.IsValidDrop(this))
        //     {
        //         // Debug.Log($"[OnEndDrag]: Valid drop in {dropZone.transform.name}");
        //         dropZone.OnDrop(eventData);
        //         // OnDropped?.Invoke(this); 

        //         if (dropCueCorrect != null)
        //         {
        //             SfxManager.Instance.Play2D(dropCueCorrect);
        //         }

        //     }
        //     else
        //     {
        //         // Debug.Log($"[OnEndDrag]: NOT valid drop in {dropZone.transform.name}");
        //         transform.SetParent(OriginalParent, true);
        //         transform.position = originalPosition;

        //         if (dropCueWrong != null)
        //         {
        //             SfxManager.Instance.Play2D(dropCueWrong);
        //         }

        //     }
        // }
        // else
        // {
        //     // Debug.Log("[OnEndDrag]: dropZone is null, dropped back to original");
        //     dropBackToOriginal();
        //     if (dropCueWrong != null)
        //     {
        //         SfxManager.Instance.Play2D(dropCueWrong);
        //     }

        // }

        // image.raycastTarget = true;
        // _isDragging = false;  // ✅ Allow drag again after completing one

    }

    private void MoveToTopLayer()
    {
        transform.SetParent(canvasTransform, true);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    private void dropBackToOriginal()
    {
        if (OriginalParent == null)
        {
            Debug.LogError("OriginalParent is null!");
            return;
        } //!

        transform.SetParent(OriginalParent, false);
        
        // transform.SetSiblingIndex(originalSiblingIndex);
        StartCoroutine(DelaySetSiblingIndex(originalSiblingIndex));
        // LayoutRebuilder.MarkLayoutForRebuild((RectTransform)OriginalParent);
    }

private IEnumerator DelaySetSiblingIndex(int index)
{
    yield return null; // wait one frame
    transform.SetSiblingIndex(index);
    LayoutRebuilder.MarkLayoutForRebuild((RectTransform)OriginalParent);
}

    public void SetParentAndPosition(Transform newParent)
    {
        if (newParent == null)
        {
            Debug.LogWarning("[DraggableItem] Tried to set parent to null — ignoring.");
            return;
        }

        transform.SetParent(newParent, false);
        transform.localScale = Vector3.one;
    }

    private DropZone FindDropZone()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            DropZone dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null)
            {
                // Debug.Log($"found {dropZone.ToString()}");
                return dropZone;
            }
        }

        return null;
    }

private void PlayCorrect() { if (dropCueCorrect != null) SfxManager.Instance.Play2D(dropCueCorrect); }
private void PlayWrong()   { if (dropCueWrong   != null) SfxManager.Instance.Play2D(dropCueWrong); }


    public void ResetEvents()
    {
        OnDropped = null;
    }

    public void Reset()
    {
        OriginalParent = null;
        AssignedSection = null;
        CurrentDropZone = null;
        OnDropped = null;
        OnRemoved = null;
        draggableType = eDraggableType.None;
    }

}
