using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class SchemeDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform lineContainerTransform;
    [SerializeField] private float lineWidth = 20f;
    [SerializeField] private TextMeshProUGUI SchemaText;
    [SerializeField] private Popup popup;
    
    public SchemeLayoutManager layoutManager;
    private bool alreadyDrawn = false;


    public void Awake()
    {
        popup.OnPopupOpened += OnPopupOpened;
        popup.OnPopupClosed += OnPopupClosed;
        Table.OnTableUnlocked += HandleTableUnlocked;
    }

    private void OnDestroy()
    {
        Table.OnTableUnlocked -= HandleTableUnlocked;
    }

    public void OnPopupOpened()
    {
        lineContainerTransform.gameObject.SetActive(true);
        if (!alreadyDrawn)
        {
            StartCoroutine(RedrawArrowsNextFrame());
            alreadyDrawn = true;
        }
    }

    public void OnPopupClosed()
    {
        lineContainerTransform.gameObject.SetActive(false);
    }

    private void HandleTableUnlocked(Table table)
    {
        layoutManager.ClearLayout();
        StartCoroutine(RedrawArrowsNextFrame());
    }


    private IEnumerator RedrawArrowsNextFrame()
    {
        yield return new WaitForEndOfFrame(); // ✅ Better than null for layout stuff
        Canvas.ForceUpdateCanvases();         // ✅ Ensures layout completes
        yield return null; 
        DisplaySchema();
    }

    public void DisplaySchema()
    {
        layoutManager.LayoutTables(SupabaseManager.Instance.Tables);
        SchemaText.text = "Schema";
        StartCoroutine(WaitThenDrawArrows()); 

    }

    private IEnumerator WaitThenDrawArrows()
    {
        // Let Unity fully apply layout
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        yield return null; // Give another frame just to be safe

        HandleForeignKeys(); // ✅ now draw lines only when positions are finalized
    }

    private void HandleForeignKeys()
    {
        foreach (Transform child in lineContainerTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (Table table in SupabaseManager.Instance.Tables)
        {
            foreach (ForeignKey fk in table.ForeignKeys)
            {
                TableBoxUI fromTableBoxUI = layoutManager.GetBoxForTable(table.Name);
                TableBoxUI toTableBoxUI = layoutManager.GetBoxForTable(fk.toTable.Name);

                if (fromTableBoxUI == null || toTableBoxUI == null)
                {
                    Debug.LogWarning($"FK skipped: could not find boxes for {fk.fromColumn.Name} → {fk.toColumn.Name}");
                    continue;
                }

                RectTransform fromRect = fromTableBoxUI.GetColumnRect(fk.fromColumn.Name);
                RectTransform toRect = toTableBoxUI.GetColumnRect(fk.toColumn.Name);

                drawUILine(fromRect, toRect);
            }
        }

    }


    private Vector3 GetMidEdge(RectTransform rect, bool isLeft)
    {
        Vector3[] worldCorners = new Vector3[4];
        rect.GetWorldCorners(worldCorners);

        // corners: 0 = BL, 1 = TL, 2 = TR, 3 = BR
        Vector3 bottom = isLeft ? worldCorners[0] : worldCorners[3];
        Vector3 top = isLeft ? worldCorners[1] : worldCorners[2];
        Vector3 worldMid = (top + bottom) * 0.5f;

        
        return worldMid;
    }


    private void drawUILine(RectTransform fromRect, RectTransform toRect)
    {
        Vector2 fromCenter = fromRect.TransformPoint(fromRect.rect.center);
        Vector2 toCenter   = toRect.TransformPoint(toRect.rect.center);

        Vector2 dir = toCenter - fromCenter;

        Vector3 fromMid = GetMidEdge(fromRect, dir.x >= 0 ? false : true);
        Vector3 toMid = GetMidEdge(toRect, dir.x >= 0 ? true : false);

        DrawUILineLocal(fromMid, toMid);
    }

    private void DrawUILineLocal(Vector3 start, Vector3 end)
    {
        Vector3 localStart = lineContainerTransform.InverseTransformPoint(start);
        Vector3 localEnd = lineContainerTransform.InverseTransformPoint(end);

        Vector3 direction = localEnd - localStart;
        float distance = direction.magnitude;

        GameObject lineGO = Instantiate(linePrefab, lineContainerTransform);
        RectTransform lineRT = lineGO.GetComponent<RectTransform>();

        lineRT.pivot = new Vector2(0f, 0.5f);
        lineRT.localPosition = localStart;
        lineRT.sizeDelta = new Vector2(distance, lineWidth);
        lineRT.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    internal void ResetSchema()
    {
        throw new NotImplementedException();
    }
}
