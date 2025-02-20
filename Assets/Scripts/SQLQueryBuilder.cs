using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;
using System.Text.RegularExpressions;



public class SQLQueryBuilder : MonoBehaviour
{
    public Transform columnParent;
    public TMP_Text queryPreviewText;
    public Button executeButton;
    public Transform resultsContainer;
    public GameObject rowPrefab;
    public GameObject columnCellPrefab;
    public GameObject selectionButtonPrefab; 

    private List<string> selectedColumns = new List<string>();
    private string selectedTable;

    private const string k_Select = "SELECT ";
    private const string k_From = "\nFROM ";
    private const string k_Where = "";
    
    private string sqlQueryStr = "";
    private string selectPart;
    private string fromPart;
    private string wherePart; 

    private bool isTableSelected = false;
    private bool isSelectSelected = false;
    private bool isFromSelected = false;
    private bool isColumnSelected = false;


    void Start()
    {
        executeButton.onClick.AddListener(ExecuteQuery);

        selectPart  = "";
        fromPart    = "";
        wherePart   = "";

        if(SupabaseManager.Instance.TableNames.Count > 0)
        {
            PopulateTableSelection();
        }
        else
        {
            SupabaseManager.Instance.OnTableNamesFetched += PopulateTableSelection;
        }
    }

    private void PopulateTableSelection()
    {
        ClearSelectionPanel(); 

        if (SupabaseManager.Instance.TableNames.Count == 0)
        {
            Debug.Log("No tables found.");
            return;
        }

        foreach (string tableName in SupabaseManager.Instance.TableNames)
        {
            GameObject tableButton = Instantiate(selectionButtonPrefab, columnParent);
            tableButton.transform.localScale = Vector3.one;

            TextMeshProUGUI btnText = tableButton.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = tableName;

            Button btn = tableButton.GetComponent<Button>();
            btn.onClick.AddListener(() => OnTableSelected(tableName));

            tableButton.SetActive(false);
        }

    }

    private void ClearSelectionPanel()
    {
        foreach (Transform child in columnParent)
        {
            if (child != null) 
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

        }
    }

    private void OnTableSelected(string tableName)
    {
        if (selectedTable == tableName)
        {
            selectedTable = "";
            isTableSelected = false;
            fromPart = "\nFROM ";
            selectedColumns.Clear();
            selectPart = "SELECT ";
        }
        else
        {
            selectedTable = tableName;
            isTableSelected = true;
            fromPart = "\nFROM " + selectedTable + " ";

            PopulateColumnSelection(selectedTable);
        }

        UpdateSelectionVisibility();
        UpdateQueryPreview();
    }

    public void OnSelectClicked()
    {
        if (isSelectSelected)
        {
            isSelectSelected = false;
            isColumnSelected = false;
            selectPart = "";
        }
        else
        {
            isSelectSelected = true;
            selectPart = "SELECT";
        }

        UpdateSelectionVisibility();
        UpdateQueryPreview();
    }

    public void OnFromClicked()
    {
        if (isFromSelected)
        {
            isFromSelected = false;
            isTableSelected = false;
            fromPart = "";
            selectedTable = "";
            selectedColumns.Clear(); 
            selectPart = isSelectSelected ? "SELECT " : "";

            ClearSelectionPanel();
        }
        else
        {
            isFromSelected = true;
            fromPart = "\nFROM ";
        }
        // PopulateTableSelection();
        UpdateSelectionVisibility();
        UpdateQueryPreview();
    }

    private void UpdateSelectionVisibility()
    {
        foreach (Transform child in columnParent)
        {
            child.gameObject.SetActive(false);
        }

        if (isFromSelected && !isTableSelected)
        {
            if (columnParent.childCount == 0)
            {
                PopulateTableSelection();
            }
            foreach (Transform child in columnParent)
            {
                child.gameObject.SetActive(true);
            }
        }

        if (isFromSelected && isSelectSelected && isTableSelected)
        {
            if (columnParent.childCount == 0)
            {
                PopulateColumnSelection(selectedTable);
            }
            foreach (Transform child in columnParent) 
            {
                child.gameObject.SetActive(true);
            }
        }

        executeButton.interactable = isTableSelected && isColumnSelected;
    }

    private void PopulateColumnSelection(string i_TableName)
    {
        Debug.Log($"Fetching columns for table: {i_TableName}");

        ClearSelectionPanel();
        // selectedColumns.Clear();

        SupabaseManager.Instance.GetTableData(i_TableName, (data) =>
        {

            if (data == null)
            {
                Debug.LogError("Supabase returned NULL data!");
                return;
            }

            if (data.Count == 0)    
            {   
                Debug.LogWarning($"No data found for table: {i_TableName}"); 
                return;  
            }

            Debug.Log($"Data retrieved: {data.ToString()}");

            JObject firstRow = (JObject)data[0];
            foreach (JProperty column in firstRow.Properties())
            {
                Debug.Log($"Column found: {column.Name}");

                GameObject columnButton = Instantiate(selectionButtonPrefab, columnParent);
                columnButton.transform.localScale = Vector3.one;
                columnButton.GetComponentInChildren<TextMeshProUGUI>().text = column.Name;

                Button btn = columnButton.GetComponent<Button>();
                btn.onClick.RemoveAllListeners(); 
                btn.onClick.AddListener(() => OnColumnSelected(column.Name));
            }

            UpdateSelectionVisibility(); 
        });
    }

    private void OnColumnSelected(string i_ColumnName)
    {
        if (selectPart.Contains(i_ColumnName))
        {
            selectedColumns.Remove(i_ColumnName);
            selectPart = "SELECT " + string.Join(", ", selectedColumns);
        }
        else
        {
            isColumnSelected = true;
            selectedColumns.Add(i_ColumnName);
            selectPart = "SELECT " + string.Join(", ", selectedColumns);
        }

        if (selectedColumns.Count == 0)
        {
            isColumnSelected = false;
            selectPart = "SELECT ";
        }

        UpdateQueryPreview();

    }

    public void OnWhereConditionEntered(string condition)
    {
        wherePart = "\nWHERE " + condition;
        UpdateQueryPreview();
    }

    private void UpdateQueryPreview()
    {
        sqlQueryStr = selectPart + fromPart + wherePart;
        queryPreviewText.text = sqlQueryStr;

        if (isQueryValid())
        {
            executeButton.interactable = true;
            GameManager.Instance.SaveQuery(sqlQueryStr);
        }
    }

    private bool isQueryValid()
    {
        return isTableSelected && isColumnSelected;
    }

    private void ExecuteQuery()
    {
        StartCoroutine(RunQuery(queryPreviewText.text));
    }

    private IEnumerator RunQuery(string i_SQlQuery)
    {
        Debug.Log("Starting SQL Query Coroutine...");

        // ✅ Extract the table name from the query
        string tableName = ExtractTableName(i_SQlQuery);

        if (string.IsNullOrEmpty(tableName))
        {
            Debug.LogError("No table name found in the query.");
            yield break;
        }

        string url = $"{SupabaseManager.Instance.SupabaseUrl}/rest/v1/{tableName}?select=*";

        Debug.Log("🔵 Executing REST API Query: " + url);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", SupabaseManager.Instance.ApiKey);
        request.SetRequestHeader("Authorization", $"Bearer {SupabaseManager.Instance.ApiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"Query Success: {responseText}");

            JArray jsonResponse = JArray.Parse(responseText);
            DisplayResults(jsonResponse);
        }
        else
        {
            Debug.LogError($"Failed to execute query: {request.error} | Response: {request.downloadHandler.text}");
        }

        Debug.Log("SQL Query Coroutine Finished.");
        GameManager.Instance.RunQuery();
    }

    private string ExtractTableName(string query)
    {
        Match match = Regex.Match(query, @"FROM\s+([a-zA-Z0-9_""\.]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string tableName = match.Groups[1].Value;
            tableName = tableName.Replace("\"", ""); // Remove quotes if present
            return tableName;
        }
        return null;
    }


    private void DisplayResults(JArray i_Data)
    {
        foreach (Transform child in resultsContainer)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        if (i_Data.Count == 0)
        {
            Debug.LogWarning("No data returned from query.");
            return;
        }


        foreach (JObject row in i_Data)
        {
            GameObject newRow = Instantiate(rowPrefab, resultsContainer);

            foreach (Transform child in newRow.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (string columnName in selectedColumns)
            {
                GameObject textCell = new GameObject(columnName);
                textCell.transform.SetParent(newRow.transform);

                TextMeshProUGUI textComponent = textCell.AddComponent<TextMeshProUGUI>();
                textComponent.text = row[columnName]?.ToString() ?? "N/A";
                textComponent.fontSize = 18;
                textComponent.alignment = TextAlignmentOptions.Center;

                LayoutElement layoutElement = textCell.AddComponent<LayoutElement>();
                layoutElement.minWidth = 100;
                layoutElement.flexibleWidth = 1;
            }
        }
    }

    public string GetBuiltQuery()
    {
        return queryPreviewText.text;
    }

}
