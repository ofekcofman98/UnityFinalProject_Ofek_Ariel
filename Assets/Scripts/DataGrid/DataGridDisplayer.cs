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
        Func<T, List<string>> rowExtractor,
        List<IDataGridAction<T>> actions = null,
        bool injectPortraitAndName = false)
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

// 🔹 Step 1: Compute max width per column using `columnNames` directly
for (int i = 0; i < columnNames.Count; i++)
{
    string col = columnNames[i];
if (col == "portrait")
{
    maxWidths[i] = 60f; // 🔥 Force match with RawImage
}
else
{
    maxWidths[i] = GetTextWidth(col);
}

    foreach (T item in data)
    {
        string val = "—";
        if (col == "portrait")
        {
            val = ""; // no text, but reserve space
        }
        else if (col == "name")
        {
            if (item is JObject jRow && jRow.TryGetValue("__name", out var nameToken))
                val = nameToken.ToString();
        }
        else if (item is JObject jRow && jRow.TryGetValue(col, out var token))
        {
            val = token.ToString();
        }

        float textWidth = GetTextWidth(val);
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

// Go through columnNames to keep correct visual + data order
for (int i = 0; i < columnNames.Count; i++)
{
    string col = columnNames[i];

    if (col == "portrait")
    {
        if (item is JObject jRow && jRow.TryGetValue("__personId", out var personIdToken))
        {
            string personId = personIdToken.ToString();
            var person = PersonDataManager.Instance.GetById(personId);
            if (person != null)
            {
                CreatePortraitCell(row.transform, person.portrait, 60f);
                continue;
            }
        }
        CreateTextCell(row.transform, "❌", 60f); // fallback
    }
    else if (col == "name")
    {
        if (item is JObject jRow && jRow.TryGetValue("__name", out var nameToken))
        {
            CreateTextCell(row.transform, nameToken.ToString(), 100f);
        }
        else
        {
            CreateTextCell(row.transform, "—", 100f);
        }
    }
    else
    {
        string val = (item as JObject)?[col]?.ToString() ?? "—";
        CreateTextCell(row.transform, val, maxWidths[i]);
    }
}


            // for (int i = 0; i < cellValues.Count; i++)
            // {
            //     CreateTextCell(row.transform, cellValues[i], maxWidths[i]);
            // }

            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    int colIndex = columnNames.Count + i;
                    GameObject buttonGO = Instantiate(actionButtonPrefab, row.transform);

                    LayoutElement layout = buttonGO.GetComponent<LayoutElement>();
                    if (layout != null)
                    {
                        // layout.preferredWidth = maxWidths[colIndex];
                        // layout.minWidth = maxWidths[colIndex];
                        // layout.flexibleWidth = 0f;

                        layout.preferredWidth = maxWidths[colIndex];
layout.minWidth = maxWidths[colIndex];
layout.flexibleWidth = 0f;
layout.preferredHeight = 80f; // or portrait height
layout.minHeight = 80f;

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

    private void CreatePortraitCell(Transform parent, Texture2D portrait, float width)
    {
        GameObject cell = Instantiate(cellPrefab, parent);

        // Disable TextLabel
        var textObj = cell.transform.Find("TextLabel")?.GetComponent<TextMeshProUGUI>();
        if (textObj != null) textObj.gameObject.SetActive(false);

        // Enable PortraitImage
        var imageObj = cell.transform.Find("PortraitImage")?.GetComponent<RawImage>();
        if (imageObj == null)
        {
            Debug.LogError("❌ Cell prefab is missing a child named 'PortraitImage' with a RawImage component.");
            return;
        }

        imageObj.gameObject.SetActive(true);
        imageObj.texture = portrait;
        imageObj.rectTransform.sizeDelta = new Vector2(width, width);


        LayoutElement layout = cell.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.preferredWidth = width;
            layout.minWidth = width;
            layout.preferredHeight = width + 10f;  // Ensure enough height
            layout.minHeight = width + 10f;
        }
    }
    

    
}