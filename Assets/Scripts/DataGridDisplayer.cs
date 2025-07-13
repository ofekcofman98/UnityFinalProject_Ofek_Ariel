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
    [SerializeField] private GameObject addSuspectButtonPrefab;   // Button prefab for persons

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
            if (isPersonsTable)
            {
                // var localId = row.ContainsKey("person_id") ? row["person_id"]?.ToString() : null;
                // var localName = row.ContainsKey("name") ? row["name"]?.ToString() : "Unknown";

                GameObject buttonGO = Instantiate(addSuspectButtonPrefab, dataRow.transform);
                LayoutElement layout = buttonGO.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.preferredWidth = columnWidths[^1];
                }
                else
                {
                    Debug.LogWarning("⚠️ No LayoutElement found on AddSuspectButton prefab.");
                }

                AddSuspectButton suspectButton = buttonGO.GetComponent<AddSuspectButton>();
                Debug.Log($"Label: {suspectButton.label?.name}, Button: {suspectButton.button?.name}");
                if (suspectButton == null || suspectButton.button == null || suspectButton.label == null)
                {
                    Debug.LogError("❌ AddSuspectButton prefab is missing required references.");
                    return;
                }

                suspectButton.label.text = "Add Suspect";
                suspectButton.label.alignment = TextAlignmentOptions.Center;
suspectButton.button.onClick.AddListener(() =>
{
    SuspectsManager.Instance.AddSuspectFromRow(row);
});

// suspectButton.button.onClick.AddListener(() =>
// {
//     Debug.Log($"im here: {localName} ({localId})");

//     if (!string.IsNullOrEmpty(localId))
//     {
//         SuspectData suspect = new SuspectData { Id = localId, Name = localName };
//         SuspectsManager.Instance.AddSuspect(suspect);
//         Debug.Log($"🕵️ Added suspect: {localName} ({localId})");
//     }
// });
            }
        }

        Debug.Log($"✅ Displayed {jsonRows.Count} rows.");
    }

    public void DisplaySuspects(List<SuspectData> suspects)
    {
        ClearResults();

        if (suspects == null || suspects.Count == 0)
        {
            Debug.Log("No suspects to display.");
            return;
        }

        // Define columns for suspects
        List<string> columnNames = new List<string> { "ID", "Full Name", "Description" };
        List<float> columnWidths = new List<float> { 60f, 150f, 200f };

        // 🔷 Header Row
        GameObject headerRow = Instantiate(rowPrefab, resultsContainer);
        for (int i = 0; i < columnNames.Count; i++)
        {
            CreateTextCell(headerRow.transform, columnNames[i], columnWidths[i]);
        }

        // 🔷 Data Rows
        foreach (var suspect in suspects)
        {
            GameObject row = Instantiate(rowPrefab, resultsContainer);
            CreateTextCell(row.transform, suspect.Id, columnWidths[0]);
            CreateTextCell(row.transform, suspect.FullName, columnWidths[1]);
            CreateTextCell(row.transform, suspect.Description ?? "—", columnWidths[2]);
        }

        Debug.Log($"✅ Displayed {suspects.Count} suspects.");
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