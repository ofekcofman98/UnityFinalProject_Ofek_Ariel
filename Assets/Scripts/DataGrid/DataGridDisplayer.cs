using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;


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
        IDataGridRowAdapter<T> adapter,
        List<IDataGridAction<T>> actions = null)
    {
        ClearResults();

        if (!ValidateInputs(columnNames, columnWidths, data))
            return;
        
        float[] maxWidths = CalculateMaxWidths(columnNames, data, adapter, actions);

        BuildHeaderRow(columnNames, maxWidths, actions);

        foreach (T item in data)
        {
            BuildDataRow(item, columnNames, maxWidths, adapter, actions);
        }
    }

    private float[] CalculateMaxWidths<T>(
        List<string> columnNames,
        List<T> data,
        IDataGridRowAdapter<T> adapter,
        List<IDataGridAction<T>> actions)
    {
        int numCols = columnNames.Count + (actions?.Count ?? 0);
        float[] maxWidths = new float[numCols];

        for (int i = 0; i < columnNames.Count; i++)
        {
            string col = columnNames[i];
            if (col == "portrait")
            {
                maxWidths[i] = 60f;
            }
            else
            {
                maxWidths[i] = GetTextWidth(col);
            }

            foreach (T item in data)
            {
                string val = "—";

                if (col == "portrait") val = "";
                else if (col == "name") val = adapter.GetDisplayName(item);
                else
                {
                    var values = adapter.GetColumnValues(item);
                    if (i < values.Count) val = values[i];
                }

                float textWidth = GetTextWidth(val);
                maxWidths[i] = Mathf.Max(maxWidths[i], textWidth);
            }
        }

        if (actions != null)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                int index = columnNames.Count + i;
                maxWidths[index] = Mathf.Max(100f, GetTextWidth(actions[i].Label));
            }
        }

        return maxWidths;
    }

    private void BuildHeaderRow<T>(
        List<string> columnNames,
        float[] maxWidths,
        List<IDataGridAction<T>> actions) // Generic constraint workaround
    {
        GameObject headerRow = Instantiate(rowPrefab, resultsContainer);

        for (int i = 0; i < columnNames.Count; i++)
        {
            new TextCell(columnNames[i], cellPrefab).Create(headerRow.transform, maxWidths[i]);
        }

        if (actions != null)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                int index = columnNames.Count + i;
                new TextCell(actions[i].Label, cellPrefab).Create(headerRow.transform, maxWidths[index]);
            }
        }
    }

    private bool ValidateInputs<T>(List<string> columnNames, List<float> columnWidths, List<T> data)
    {
        bool res = true;
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("No data to display.");
            res = false;
        }

        if (columnNames.Count != columnWidths.Count)
        {
            Debug.LogError("Column names and widths count mismatch.");
            res = false;
        }

        return res;
    }

    private void BuildDataRow<T>(
        T item,
        List<string> columnNames,
        float[] maxWidths,
        IDataGridRowAdapter<T> adapter,
        List<IDataGridAction<T>> actions)
    {
        GameObject row = Instantiate(rowPrefab, resultsContainer);
        List<string> cellValues = adapter.GetColumnValues(item);

        for (int i = 0; i < columnNames.Count; i++)
        {
            string col = columnNames[i];
            string value = "—";

            if (col == "name")
            {
                value = adapter.GetDisplayName(item);
            }
            else if (col != "portrait")
            {
                var values = adapter.GetColumnValues(item);
                if (i < values.Count)
                    value = values[i];
            }

            Texture2D portrait = (col == "portrait") ? adapter.GetPortrait(item) : null;

            IDataGridCell cell = CreateCell(col, value, portrait);
            cell.Create(row.transform, maxWidths[i]);
        }

        if (actions != null)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                int colIndex = columnNames.Count + i;
                GameObject buttonGO = Instantiate(actionButtonPrefab, row.transform);
                LayoutElement layout = buttonGO.GetComponent<LayoutElement>();
                layout.preferredWidth = maxWidths[colIndex];
                layout.minWidth = maxWidths[colIndex];
                layout.flexibleWidth = 0f;

                var button = buttonGO.GetComponent<Button>();
                var label = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                label.text = actions[i].Label;
                label.alignment = TextAlignmentOptions.Center;

                int actionIndex = i;
                button.onClick.AddListener(() => actions[actionIndex].Execute(item));
            }
        }
    }


    private IDataGridCell CreateCell(string col, string value, Texture2D portrait)
    {
        if (col == "portrait")
            return new PortraitCell(portrait, cellPrefab);

        return new TextCell(value, cellPrefab);
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

    public void ClearResults()
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