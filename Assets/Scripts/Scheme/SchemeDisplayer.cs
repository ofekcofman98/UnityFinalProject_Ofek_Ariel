using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class SchemeDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject tableBoxPrefab;
    [SerializeField] private Transform layoutParent;
    [SerializeField] private GameObject linePrefab;
[SerializeField] private Transform lineContainerTransform;

    private Dictionary<string, TableBoxUI> tableBoxMap = new Dictionary<string, TableBoxUI>();

    void Start()
    {
        // SupabaseManager.Instance.OnTableNamesFetched += DisplaySchema;
    }

    public void DisplaySchema()
    {
        float startX = -300f;
        float spacing = 200f;

        for (int i = 0; i < SupabaseManager.Instance.Tables.Count; i++)
        {
            Table table = SupabaseManager.Instance.Tables[i];

            GameObject tableBoxGO = Instantiate(tableBoxPrefab, layoutParent);
            
            var boxUI = tableBoxGO.GetComponent<TableBoxUI>();
            boxUI.Init(table);

            tableBoxMap[table.Name] = boxUI;

            // Manual positioning
            RectTransform rt = tableBoxGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);
        }

        // foreach (Table table in SupabaseManager.Instance.Tables)
        // {
        //     GameObject tableBoxObject = Instantiate(tableBoxPrefab, layoutParent);
        //     TableBoxUI tableBoxUI = tableBoxObject.GetComponent<TableBoxUI>();
        //     tableBoxUI.Init(table);
        // }

        DrawForeignKeyLines();
    }

    private void DrawForeignKeyLines()
    {
        foreach (Table table in SupabaseManager.Instance.Tables)
        {
            foreach(ForeignKey fk in table.ForeignKeys)
            {
                TableBoxUI fromTableUI = tableBoxMap[fk.fromColumn.ParentTable.Name];
                TableBoxUI toTableUI = tableBoxMap[fk.toTable.Name];

                RectTransform fromRect = fromTableUI.GetColumnRect(fk.fromColumn.Name);
                RectTransform toRect = toTableUI.GetColumnRect(fk.toColumn.Name);
            
                if (fromRect == null || toRect == null)
                {
                    Debug.LogWarning($"Cannot draw FK line: {fk.fromColumn.Name} to {fk.toColumn.Name}");
                    continue;
                }

                GameObject lineGameObject = Instantiate(linePrefab, lineContainerTransform);
                LineRenderer line = lineGameObject.GetComponent<LineRenderer>();

                Vector3 start = fromRect.TransformPoint(fromRect.rect.center);
                Vector3 end = toRect.TransformPoint(toRect.rect.center);


                line.positionCount = 2;
                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        }
    }
}
