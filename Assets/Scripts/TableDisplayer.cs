using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;


public class TableDisplayer : MonoBehaviour
{
    public Transform tableContent;
    public Transform headerRow;
    public GameObject rowPrefab;

    public TMP_Dropdown tableDropdown;
    public TMP_Text tableNameLabel;
    public Button fetchButton; 

    private string selectedTableName;

    void Start()
    {
        fetchButton.onClick.AddListener(OnFetchTableData);
        SupabaseManager.Instance.OnTableNamesFetched += InitializeDropdown;
    }


    private void InitializeDropdown()
    {
        PopulateDropdown();

        if (SupabaseManager.Instance.TableNames.Count > 0)
        {
            LoadTable(SupabaseManager.Instance.TableNames[0]); 
        }
        else
        {
            Debug.Log("No tables found.");
        }
    }

    private void OnFetchTableData()
    {
        if (string.IsNullOrEmpty(selectedTableName)) return;

        tableNameLabel.text = $"Table: {selectedTableName}";
        SupabaseManager.Instance.GetTableData(selectedTableName, PopulateTable);
    }

    private void PopulateDropdown()
    {
        tableDropdown.ClearOptions();
        tableDropdown.AddOptions(SupabaseManager.Instance.TableNames);

        if (SupabaseManager.Instance.TableNames.Count > 0)
        {
            selectedTableName = SupabaseManager.Instance.TableNames[0];
            Debug.Log($"name: {selectedTableName}");
            tableDropdown.value = 0;
        }

        tableDropdown.onValueChanged.AddListener(delegate { OnTableSelected(); });
    }

    private void OnTableSelected()
    {
        selectedTableName = tableDropdown.options[tableDropdown.value].text;
    }

    public void LoadTable(string tableName)
    {
        SupabaseManager.Instance.GetTableData(tableName, PopulateTable);
    }


    void PopulateTable(JArray data)
    {
        if (data.Count == 0)
        {
            Debug.Log("No data found in the table.");
            return;
        }

        List<string> columnNames = new List<string>();
        JObject firstRow = (JObject)data[0];

        foreach (JProperty column in firstRow.Properties())
        {
            columnNames.Add(column.Name);
        }

        foreach (Transform child in headerRow)
        {
            Destroy(child.gameObject);
        }
        GenerateColumnHeaders(columnNames);

        foreach (Transform child in tableContent)
        {
            Destroy(child.gameObject);
        }

        foreach (JObject row in data)
        {
            GameObject newRow = Instantiate(rowPrefab, tableContent);

            foreach (Transform child in newRow.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (string columnName in columnNames)
            {
                GameObject textCell = new GameObject(columnName);
                textCell.transform.SetParent(newRow.transform);
                
                TextMeshProUGUI textComponent = textCell.AddComponent<TextMeshProUGUI>();
                textComponent.text = row[columnName]?.ToString() ?? "N/A";
                textComponent.fontSize = 18;
                textComponent.alignment = TextAlignmentOptions.Center;

                LayoutElement layoutElement = textCell.AddComponent<LayoutElement>();
                layoutElement.minWidth = 100; // Adjust width per column
                layoutElement.flexibleWidth = 1;

            }
        }
    }

    private void GenerateColumnHeaders(List<string> columnNames)
    {
        // float columnWidth = 150f; // Set default width per column
        // float totalWidth = columnNames.Count * columnWidth;

        // RectTransform contentRT = headerRow.GetComponent<RectTransform>();
        // contentRT.sizeDelta = new Vector2(totalWidth, contentRT.sizeDelta.y);

        foreach (string columnName in columnNames)
        {
            GameObject headerCell = new GameObject(columnName);
            headerCell.transform.SetParent(headerRow);

            TextMeshProUGUI headerText = headerCell.AddComponent<TextMeshProUGUI>();
            headerText.text = columnName;
            headerText.fontSize = 16;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Center;

            LayoutElement layoutElement = headerCell.AddComponent<LayoutElement>();
            layoutElement.minWidth = 100;
            layoutElement.flexibleWidth = 1;
        }
    }

    // void PopulateTable(JArray data)
    // {
    // if (data.Count == 0) return;

    // // Get column names from the first row
    // List<string> columnNames = new List<string>();
    // JObject firstRow = (JObject)data[0]; 
    
    // foreach (JProperty column in firstRow.Properties()) // Correct way to iterate over properties
    // {
    //     columnNames.Add(column.Name); // Use .Name correctly
    // }


    // // Generate column headers (Optional)
    // GenerateColumnHeaders(columnNames);

    // // Create rows dynamically
    // foreach (JObject row in data)
    // {
    //     GameObject newRow = new GameObject("Row");
    //     newRow.transform.SetParent(tableContent);
    //     newRow.AddComponent<HorizontalLayoutGroup>(); // Align items horizontally

    //     foreach (string columnName in columnNames)
    //     {
    //         GameObject cell = new GameObject(columnName);
    //         cell.transform.SetParent(newRow.transform);

    //         TextMeshProUGUI textComponent = cell.AddComponent<TextMeshProUGUI>();
    //         textComponent.text = row[columnName]?.ToString() ?? "N/A";
    //     }
    // }
    // }

    // private void GenerateColumnHeaders(List<string> columnNames)
    // {
    //     foreach (Transform child in tableContent)
    //     {
    //         Destroy(child.gameObject);
    //     }

    //     GameObject headerRow = new GameObject("HeaderRow");
    //     headerRow.transform.SetParent(tableContent);
    //     headerRow.AddComponent<HorizontalLayoutGroup>(); // Align headers horizontally

    //     foreach (string columnName in columnNames)
    //     {
    //         GameObject headerCell = new GameObject(columnName);
    //         headerCell.transform.SetParent(headerRow.transform);

    //         TextMeshProUGUI headerText = headerCell.AddComponent<TextMeshProUGUI>();
    //         headerText.text = columnName;
    //         headerText.fontSize = 24;
    //         headerText.fontStyle = FontStyles.Bold;
    //     }
    // }
}
