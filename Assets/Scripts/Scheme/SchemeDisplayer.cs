using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class SchemeDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform lineContainerTransform;
    [SerializeField] private float lineWidth = 20f;
    public SchemeLayoutManager layoutManager;
    private bool isVisible = false;


    public void ToggleScheme()
    {
        isVisible = !isVisible;
        layoutManager.layoutParent.gameObject.SetActive(isVisible);
        lineContainerTransform.gameObject.SetActive(isVisible);

        if (isVisible)
        {
            StartCoroutine(RedrawArrowsNextFrame());
        }

    }

    private IEnumerator RedrawArrowsNextFrame()
    {
        yield return null;
        DisplaySchema();
    }

    public void DisplaySchema()
    {
        layoutManager.LayoutTables(SupabaseManager.Instance.Tables);        
        Canvas.ForceUpdateCanvases();
        HandleForeignKeys();
    }



    private void HandleForeignKeys()
    {
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

        for (int i = 0; i<4; i++)
        {
            GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            startMarker.transform.localScale = Vector3.one * 10f;
            startMarker.transform.position = worldCorners[i];
        }

        GameObject startMarker1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        startMarker1.transform.localScale = Vector3.one * 10f;
        startMarker1.transform.position = worldMid;
        
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


}
