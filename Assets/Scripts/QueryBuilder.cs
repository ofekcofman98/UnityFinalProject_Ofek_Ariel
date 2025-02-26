using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;
using System.Text.RegularExpressions;
using System.Linq;



public class QueryBuilder : MonoBehaviour
{
    [SerializeField] public GameObject QueryPanel;
    public Transform columnParent;
    public TMP_Text queryPreviewText;
    public Button executeButton;
    public GameObject selectionButtonPrefab; 
    
    Query query;

    private const string k_Select = "SELECT ";
    private const string k_From = "\nFROM ";
    private const string k_Where = "\nWHERE ";
    private const string k_Comma = ", ";

    
    private string selectPart;
    private string fromPart;
    private string wherePart; 

    private bool isTableSelected = false;
    private bool isSelectSelected = false;
    private bool isFromSelected = false;
    private bool isColumnSelected = false;
    

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

        if(SupabaseManager.Instance.Tables.Count > 0)
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

        if (SupabaseManager.Instance.Tables.Count == 0)
        {
            Debug.Log("No tables found.");
            return;
        }

        foreach (Table table in SupabaseManager.Instance.Tables)
        {
            GameObject tableButton = Instantiate(selectionButtonPrefab, columnParent);
            tableButton.transform.localScale = Vector3.one;
    
            TextMeshProUGUI btnText = tableButton.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = table.Name;

            Button btn = tableButton.GetComponent<Button>();
            btn.onClick.AddListener(() => OnTableSelected(table));

            tableButton.SetActive(false);
        }
    }

    private void PopulateColumnSelection(Table i_Table)
    {
        Debug.Log($"Fetching columns for table: {i_Table.Name}");

        ClearSelectionPanel();

        // if (i_Table.Columns.Count == 0)
        // {
        //     Debug.LogWarning($"No columns found for table: {i_Table.Name}");
        //     return;
        // }


        // foreach (string column in i_Table.Columns)
        // {
        //     GameObject columnButton = Instantiate(selectionButtonPrefab, columnParent);
        //     columnButton.transform.localScale = Vector3.one;
        //     columnButton.GetComponentInChildren<TextMeshProUGUI>().text = column;

        //     Button btn = columnButton.GetComponent<Button>();
        //     btn.onClick.RemoveAllListeners();
        //     btn.onClick.AddListener(() => OnColumnSelected(column));
        // }

        if (i_Table.Columns1.Count == 0)
        {
            Debug.LogWarning($"No columns found for table: {i_Table.Name}");
            return;
        }

        foreach (Column column in i_Table.Columns1)
        {
            GameObject columnButton = Instantiate(selectionButtonPrefab, columnParent);
            columnButton.transform.localScale = Vector3.one;
            columnButton.GetComponentInChildren<TextMeshProUGUI>().text = column.Name;

            Button btn = columnButton.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnColumnSelected(column));
        }

        UpdateSelectionVisibility(); 
        
    }


    private void OnTableSelected(Table i_SelectedTable)
    {
        if (query.table != null && query.table.Name == i_SelectedTable.Name)
        {
            query.table = null;
            isTableSelected = false;
            fromPart = k_From;
            // query.SelectedColumns.Clear();
            query.Columns.Clear();
            selectPart = k_Select;
        }
        else
        {
            query.table = i_SelectedTable;
            isTableSelected = true;
            fromPart = k_From + query.table.Name + " ";

            PopulateColumnSelection(i_SelectedTable);
        }
        UpdateSelectionVisibility();
        UpdateQueryPreview();
    }


    // private void OnColumnSelected(string i_ColumnName)
    // {
    //     if (selectPart.Contains(i_ColumnName))
    //     {
    //         query.SelectedColumns.Remove(i_ColumnName);
    //         selectPart = k_Select + string.Join(k_Comma, query.SelectedColumns);
    //     }
    //     else
    //     {
    //         isColumnSelected = true;
    //         query.SelectedColumns.Add(i_ColumnName);
    //         selectPart = k_Select + string.Join(k_Comma, query.SelectedColumns);
    //     }

    //     if (query.SelectedColumns.Count == 0)
    //     {
    //         isColumnSelected = false;
    //         selectPart = k_Select;
    //     }

    //     Debug.Log($"Column Selected: {string.Join(k_Comma, query.SelectedColumns)}");

    //     UpdateQueryPreview();

    // }
    private void OnColumnSelected(Column i_Column)
    {
        if (selectPart.Contains(i_Column.Name))
        {
            query.Columns.Remove(i_Column);
            selectPart = k_Select + string.Join(k_Comma, query.Columns.Select(col => col.Name));
        }
        else
        {
            isColumnSelected = true;
            query.Columns.Add(i_Column);
            selectPart = k_Select + string.Join(k_Comma, query.Columns.Select(col => col.Name));
        }

        if (query.Columns.Count == 0)
        {
            isColumnSelected = false;
            selectPart = k_Select;
        }

        Debug.Log($"Column Selected: {string.Join(k_Comma, query.Columns.Select(col => col.Name))}");

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
            selectPart = k_Select;
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
            query.table = null;
            // query.SelectedColumns.Clear();
            query.Columns.Clear();
            selectPart = isSelectSelected ? k_Select : "";

            ClearSelectionPanel();
        }
        else
        {
            isFromSelected = true;
            fromPart = k_From;
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
                PopulateColumnSelection(query.table);
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
        wherePart = k_Where + condition;
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

}
