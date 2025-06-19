using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;


public class TableDisplayer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform resultsContainer;           // The vertical layout group
    [SerializeField] private GameObject rowPrefab;                 // HorizontalLayoutGroup
    [SerializeField] private GameObject cellPrefab;                // Contains TextMeshProUGUI + LayoutElement
    // [SerializeField] private GameObject addSuspectButtonPrefab;   // Button prefab for persons

    [Header("Column Settings")]
    [SerializeField] private float defaultColumnWidth = 50f;


    public void DisplayResults(JArray jsonRows, List<Column> columns, string tableName)
    {
        ClearResults();

        if (columns == null || columns.Count == 0)
        {
            Debug.LogWarning("No columns to display.");
            return;
        }

        if (jsonRows == null || jsonRows.Count == 0)
        {
            Debug.Log("Query returned no rows.");
        }

        // Dynamically assign column widths
        List<float> columnWidths = new List<float>();
        for (int i = 0; i < columns.Count; i++)
        {
            columnWidths.Add(defaultColumnWidth);
        }

        // Add extra column for action if needed
        bool isPersonsTable = tableName.ToLower() == "persons";
        if (isPersonsTable)
        {
            columnWidths.Add(100f); // Action column
        }

        // 🔷 Header Row
        GameObject headerRow = Instantiate(rowPrefab, resultsContainer);
        for (int i = 0; i < columns.Count; i++)
        {
            CreateTextCell(headerRow.transform, columns[i].Name, columnWidths[i]);
        }
        if (isPersonsTable)
        {
            CreateTextCell(headerRow.transform, "Action", columnWidths[^1]);
        }

        // 🔷 Data Rows
        foreach (JObject row in jsonRows)
        {
            GameObject dataRow = Instantiate(rowPrefab, resultsContainer);
            for (int i = 0; i < columns.Count; i++)
            {
                string colName = columns[i].Name;
                string val = row.ContainsKey(colName) ? row[colName]?.ToString() ?? "N/A" : "N/A";
                CreateTextCell(dataRow.transform, val, columnWidths[i]);
            }

            // ➕ Add suspect button (only for persons table)
            // if (isPersonsTable)
            // {
            //     string id = row.ContainsKey("person_id") ? row["person_id"]?.ToString() : null;
            //     string name = row.ContainsKey("name") ? row["name"]?.ToString() : "Unknown";

            //     GameObject buttonGO = Instantiate(addSuspectButtonPrefab, dataRow.transform);
            //     buttonGO.GetComponent<LayoutElement>().preferredWidth = columnWidths[^1];

            //     TextMeshProUGUI btnText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            //     btnText.text = "Add";
            //     btnText.alignment = TextAlignmentOptions.Center;

            //     Button btn = buttonGO.GetComponent<Button>();
            //     btn.onClick.AddListener(() =>
            //     {
            //         if (!string.IsNullOrEmpty(id))
            //         {
            //             var suspect = new SuspectData { Id = id, Name = name };
            //             SuspectsManager.Instance.AddSuspect(suspect);
            //             Debug.Log($"🕵️ Added suspect: {name} ({id})");
            //         }
            //     });
            // }
        }

        Debug.Log($"✅ Displayed {jsonRows.Count} rows.");
    }


    private void CreateTextCell(Transform parent, string text, float width)
    {
        GameObject cell = Instantiate(cellPrefab, parent);
        TextMeshProUGUI tmp = cell.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;

        LayoutElement layout = cell.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
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