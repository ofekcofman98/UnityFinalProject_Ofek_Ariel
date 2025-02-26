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

    public void DisplayResults1(JArray jsonResponse, List<Column> i_Columns)
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

            foreach (Column column in i_Columns)
            {

                if (!row.ContainsKey(column.Name))
                {
                    Debug.LogWarning($"⚠️ Column '{column.Name}' not found in row! Skipping.");
                    continue;
                }

                string cellValue = row[column.Name]?.ToString() ?? "N/A";
                Debug.Log($"Column: {column.Name}, Value: {cellValue}");
                Debug.Log($"Selected Columns: {string.Join(", ", i_Columns)}");

                GameObject textCell = new GameObject(column.Name);
                textCell.transform.SetParent(newRow.transform);

                TextMeshProUGUI textComponent = textCell.AddComponent<TextMeshProUGUI>();
                textComponent.text = row[column.Name]?.ToString() ?? "N/A";
                textComponent.fontSize = 18;
                textComponent.alignment = TextAlignmentOptions.Center;
            }
        }

        Debug.Log($"Displayed {jsonResponse.Count} rows.");
    }



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

            foreach (string columnName in columnNames)
            {

                if (!row.ContainsKey(columnName))
                {
                    Debug.LogWarning($"⚠️ Column '{columnName}' not found in row! Skipping.");
                    continue;
                }

                string cellValue = row[columnName]?.ToString() ?? "N/A";
                Debug.Log($"Column: {columnName}, Value: {cellValue}");
                Debug.Log($"Selected Columns: {string.Join(", ", columnNames)}");

                GameObject textCell = new GameObject(columnName);
                textCell.transform.SetParent(newRow.transform);

                TextMeshProUGUI textComponent = textCell.AddComponent<TextMeshProUGUI>();
                textComponent.text = row[columnName]?.ToString() ?? "N/A";
                textComponent.fontSize = 18;
                textComponent.alignment = TextAlignmentOptions.Center;
            }
        }

        Debug.Log($"Displayed {jsonResponse.Count} rows.");
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