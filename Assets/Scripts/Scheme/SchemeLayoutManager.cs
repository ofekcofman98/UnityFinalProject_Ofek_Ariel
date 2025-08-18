using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SchemeLayoutManager : MonoBehaviour
{
    [SerializeField] public Transform layoutParent;
    [SerializeField] private GameObject tableBoxPrefab;
    [SerializeField] private float cellWidth = 150f;
    [SerializeField] private float cellHeight = 300f;
    [SerializeField] private int columns = 3;

    [SerializeField] private float horizontalSpacing = 20f;
    [SerializeField] private float verticalSpacing = 20f;
    [SerializeField] private float paddingLeft = 20f;
    [SerializeField] private float paddingTop = 60f;

    private Dictionary<string, TableBoxUI> tableBoxMap = new Dictionary<string, TableBoxUI>();
    // public bool IsTableDrawn(string tableName) => tableBoxMap.ContainsKey(tableName);

    public void LayoutTables(List<Table> i_Tables)
    {
        // ClearLayout();

        for (int i = 0; i < i_Tables.Count; i++)
        {
            Table table = i_Tables[i];
            
            GameObject tableBoxObject = Instantiate(tableBoxPrefab, layoutParent);
            TableBoxUI tableBoxUI = tableBoxObject.GetComponent<TableBoxUI>();
            tableBoxUI.Init(table);
            tableBoxMap[table.Name] = tableBoxUI;
            RectTransform rt = tableBoxObject.GetComponent<RectTransform>();

            int row = i / columns;
            int col = i % columns;

            Vector2 topLeftOffset = getOffset();

            Vector2 position = new Vector2(
                paddingLeft + col * (cellWidth + horizontalSpacing),
            -paddingTop - row * (cellHeight + verticalSpacing)
            );
            rt.anchoredPosition = position;
        }
    }

    private Vector2 getOffset()
    {
        Vector2 topLeftOffset = new Vector2(0, 0); // default fallback

        RectTransform layoutRect = layoutParent.GetComponent<RectTransform>();
        if (layoutRect != null)
        {
            // Align top-left of grid to the top-left of the layout container
            float startX = 0 + cellWidth / 2f;
            float startY = 0 - cellHeight / 2f;
            topLeftOffset = new Vector2(startX, startY);
        }

        return topLeftOffset;
    }

    public TableBoxUI GetBoxForTable(string tableName)
    {
        return tableBoxMap.TryGetValue(tableName, out TableBoxUI box) ? box : null;
    }


    public void ClearLayout()
    {
        foreach (Transform child in layoutParent)
        {
            Destroy(child.gameObject);
        }
        tableBoxMap.Clear();
    }
}
