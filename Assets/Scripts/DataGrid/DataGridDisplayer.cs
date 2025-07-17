using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;


public class DataGridDisplayer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform resultsContainer;           // The vertical layout group
    [SerializeField] private GameObject rowPrefab;                 // HorizontalLayoutGroup
    [SerializeField] private GameObject cellPrefab;                // Contains TextMeshProUGUI + LayoutElement
    [SerializeField] private GameObject actionButtonPrefab;   // Button prefab for persons

    [Header("Column Settings")]
    [SerializeField] private float defaultColumnWidth = 50f;

    public void DisplayGrid<T>(
        List<string> columnNames,
        List<float> columnWidths,
        List<T> data,
        Func<T, List<string>> rowExtractor,
        List<IDataGridAction<T>> actions = null)
    {
        ClearResults();

        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("No data to display.");
            return;
        }

        if (columnNames.Count != columnWidths.Count)
        {
            Debug.LogError("Column names and widths count mismatch.");
            return;
        }

        int numCols = columnNames.Count + (actions?.Count ?? 0);
        float[] maxWidths = new float[numCols];

        // 🔹 Step 1: Calculate max widths for each column (header + all rows)
        for (int i = 0; i < columnNames.Count; i++)
        {
            maxWidths[i] = GetTextWidth(columnNames[i]);
        }

        foreach (T item in data)
        {
            var values = rowExtractor(item);
            for (int i = 0; i < values.Count; i++)
            {
                float textWidth = GetTextWidth(values[i]);
                maxWidths[i] = Mathf.Max(maxWidths[i], textWidth);
            }
        }

        if (actions != null)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                maxWidths[columnNames.Count + i] = Mathf.Max(100f, GetTextWidth(actions[i].Label));
            }
        }

        // 🔷 Step 2: Header Row
        GameObject headerRow = Instantiate(rowPrefab, resultsContainer);
        for (int i = 0; i < columnNames.Count; i++)
        {
            CreateTextCell(headerRow.transform, columnNames[i], maxWidths[i]);
        }

        if (actions != null)
        {
            // foreach (var action in actions)
            // {
            //     int index = columnNames.Count + actions.IndexOf(action);
            //     CreateTextCell(headerRow.transform, action.Label, maxWidths[index]);
            // }
            for (int i = 0; i < actions.Count; i++)
{
    int index = columnNames.Count + i;
    CreateTextCell(headerRow.transform, actions[i].Label, maxWidths[index]);
}

        }

        // 🔷 Step 3: Data Rows
        foreach (T item in data)
        {
            GameObject row = Instantiate(rowPrefab, resultsContainer);
            List<string> cellValues = rowExtractor(item);

            for (int i = 0; i < cellValues.Count; i++)
            {
                CreateTextCell(row.transform, cellValues[i], maxWidths[i]);
            }

            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    int colIndex = columnNames.Count + i;
                    GameObject buttonGO = Instantiate(actionButtonPrefab, row.transform);

                    LayoutElement layout = buttonGO.GetComponent<LayoutElement>();
                    if (layout != null)
                    {
                        layout.preferredWidth = maxWidths[colIndex];
                        layout.minWidth = maxWidths[colIndex];
                        layout.flexibleWidth = 0f;
                    }

                    var button = buttonGO.GetComponent<Button>();
                    var label = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                    if (button == null || label == null)
                    {
                        Debug.LogError("🚨 Button or label missing on actionButtonPrefab.");
                        continue;
                    }

                    label.text = actions[i].Label;
                    label.alignment = TextAlignmentOptions.Center;
int actionIndex = i;
button.onClick.AddListener(() => actions[actionIndex].Execute(item));
                }
            }
        }

        Debug.Log($"✅ Displayed {data.Count} rows.");
    }

    private void CreateTextCell(Transform parent, string text, float width)
    {
        GameObject cell = Instantiate(cellPrefab, parent);
        TextMeshProUGUI tmp = cell.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;

        LayoutElement layout = cell.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.flexibleWidth = 0f;

    }

    private float GetTextWidth(string text)
    {
        var tmpGO = new GameObject("TempText");
        var tmp = tmpGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24; // match your prefab settings!
        tmp.enableAutoSizing = false;

        Vector2 size = tmp.GetPreferredValues();
        Destroy(tmpGO);
        return size.x + 20f; // padding for safety
    }

    private void ClearResults()
    {
        foreach (Transform child in resultsContainer)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

    }
}