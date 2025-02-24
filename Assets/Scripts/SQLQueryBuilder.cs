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
    [SerializeField] public GameObject QueryPanel;
    public Transform columnParent;
    public TMP_Text queryPreviewText;
    public Button executeButton;
    public GameObject selectionButtonPrefab; 

    private const string k_Select = "SELECT ";
    private const string k_From = "\nFROM ";
    private const string k_Where = "";
    
    private string selectPart;
    private string fromPart;
    private string wherePart; 

    private bool isTableSelected = false;
    private bool isSelectSelected = false;
    private bool isFromSelected = false;
    private bool isColumnSelected = false;
    
    Query query;

    void Start()
    {
        QueryPanel.SetActive(false);
        executeButton.onClick.AddListener(ExecuteQuery);
    }

    public void BuildQuery()
    {
        QueryPanel.SetActive(true);
        
        if (query == null)
        {
            query = new Query();
        }

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

        foreach (string i_SelectedTable in SupabaseManager.Instance.TableNames)
        {
            GameObject tableButton = Instantiate(selectionButtonPrefab, columnParent);
            tableButton.transform.localScale = Vector3.one;

            TextMeshProUGUI btnText = tableButton.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = i_SelectedTable;

            Button btn = tableButton.GetComponent<Button>();
            btn.onClick.AddListener(() => OnTableSelected(i_SelectedTable));

            tableButton.SetActive(false);
        }

    }

    private void PopulateColumnSelection(string i_TableName)
    {
        Debug.Log($"Fetching columns for table: {i_TableName}");

        ClearSelectionPanel();

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

    private void OnTableSelected(string i_SelectedTable)
    {
        if (query.TableName == i_SelectedTable)
        {
            query.TableName = "";
            isTableSelected = false;
            fromPart = "\nFROM ";
            query.SelectedColumns.Clear();
            selectPart = "SELECT ";
        }
        else
        {
            query.TableName = i_SelectedTable;
            isTableSelected = true;
            fromPart = "\nFROM " + query.TableName + " ";

            PopulateColumnSelection(query.TableName);
        }

        UpdateSelectionVisibility();
        UpdateQueryPreview();
    }

    private void OnColumnSelected(string i_ColumnName)
    {
        if (selectPart.Contains(i_ColumnName))
        {
            query.SelectedColumns.Remove(i_ColumnName);
            selectPart = "SELECT " + string.Join(", ", query.SelectedColumns);
        }
        else
        {
            isColumnSelected = true;
            query.SelectedColumns.Add(i_ColumnName);
            selectPart = "SELECT " + string.Join(", ", query.SelectedColumns);
        }

        if (query.SelectedColumns.Count == 0)
        {
            isColumnSelected = false;
            selectPart = "SELECT ";
        }

        Debug.Log($"✅ Column Selected: {string.Join(", ", query.SelectedColumns)}");

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
            query.TableName = "";
            query.SelectedColumns.Clear();
            selectPart = isSelectSelected ? "SELECT " : "";

            ClearSelectionPanel();
        }
        else
        {
            isFromSelected = true;
            fromPart = "\nFROM ";
        }

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
                PopulateColumnSelection(query.TableName);
            }
            foreach (Transform child in columnParent) 
            {
                child.gameObject.SetActive(true);
            }
        }

        executeButton.interactable = isTableSelected && isColumnSelected;
    }


    public void OnWhereConditionEntered(string condition)
    {
        wherePart = "\nWHERE " + condition;
        UpdateQueryPreview();
    }

    private void UpdateQueryPreview()
    {
        if (query == null)
        {
            Debug.LogError("UpdateQueryPreview() - query is NULL before saving!");
        }

        query.QueryString = selectPart + fromPart + wherePart;
        queryPreviewText.text = query.QueryString;

        if (isQueryValid())
        {
            executeButton.interactable = true;
            GameManager.Instance.SaveQuery(query);
        }
    }

    private bool isQueryValid()
    {
        return isTableSelected && isColumnSelected;
    }

    private void ExecuteQuery()
    {
        GameManager.Instance.ExecuteQuery();
    }

}
