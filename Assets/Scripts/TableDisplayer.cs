using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;


public class TableDisplayer : MonoBehaviour
{
    [SerializeField] private Transform resultsContainer;
    [SerializeField] private GameObject rowPrefab;

    public void DisplayResults(JArray jsonResponse, List<string> columnNames)
    {
        ClearResults();

        if (jsonResponse.Count == 0)
        {
            Debug.LogWarning("No data returned from query.");
            return;
        }

        foreach (JObject row in jsonResponse)
        {
            GameObject newRow = Instantiate(rowPrefab, resultsContainer);

            foreach (Transform child in newRow.transform)
            {
                Destroy(child.gameObject);
            }

            // if (columnNames.Count == 0)
            // {
            //     Debug.LogError("🚨 columnNames is EMPTY! The loop won't run.");
            // }

            foreach (string columnName in columnNames)
            {

                if (!row.ContainsKey(columnName))
                {
                    Debug.LogWarning($"⚠️ Column '{columnName}' not found in row! Skipping.");
                    continue;
                }

                string cellValue = row[columnName]?.ToString() ?? "N/A";
                Debug.Log($"✅!!!!!! Column: {columnName}, Value: {cellValue}");
                Debug.Log($"📌 Selected Columns: {string.Join(", ", columnNames)}");

                GameObject textCell = new GameObject(columnName);
                textCell.transform.SetParent(newRow.transform);

                TextMeshProUGUI textComponent = textCell.AddComponent<TextMeshProUGUI>();
                textComponent.text = row[columnName]?.ToString() ?? "N/A";
                textComponent.fontSize = 18;
                textComponent.alignment = TextAlignmentOptions.Center;
            }
        }

        Debug.Log($"📊 Displayed {jsonResponse.Count} rows.");
        // foreach (JObject row in jsonResponse)
        // {
        //     Debug.Log($"📌 Selected Columns: {string.Join(", ", selectedColumns)}");
        // }

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