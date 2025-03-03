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

public enum eQueryState
{
    None, 
    SelectingTable,
    SelectingColumns,
    SelectingConditions,
}

public class QueryBuilder : MonoBehaviour
{
    [SerializeField] public GameObject QueryPanel;
    public Transform selectionParent;
    public TextMeshProUGUI queryPreviewText;
    public Button executeButton;
    public GameObject selectionButtonPrefab; 
    
    [SerializeField] private GameObject inputFieldPrefab;
    [SerializeField] private GameObject confirmButtonPrefab;
    [SerializeField] private GameObject whereButton;
    
    Query query;
    private eQueryState currentState = eQueryState.None; 
    private bool isSelectClicked = false;
    private bool isFromClicked = false;
    private bool isWhereClicked = false;


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
            query.OnQueryUpdated += UpdateQueryPreview;
        }

        if(SupabaseManager.Instance.Tables.Count > 0)
        {
            // PopulateTableSelection();
        }
        else
        {
            SupabaseManager.Instance.OnTableNamesFetched += PopulateTableSelection;
        }
    }

    public void OnSelectClicked()
    {
        isSelectClicked = !isSelectClicked;

        if (!isSelectClicked)
        {
            query.ClearColumns();
            // currentState = eQueryState.None;
        }
        else
        {
            query.ActivateSelect();

            if (isFromClicked && query.table != null)
            {
                currentState = eQueryState.SelectingColumns;
            }
        }

        UpdateSelectionVisibility();
    }

    public void OnFromClicked()
    {
        isFromClicked = !isFromClicked;

        if (!isFromClicked)
        {
            query.ClearTable(isSelectClicked);  
            currentState = eQueryState.None;
        }
        else
        {
            query.ActivateFrom();
            currentState = eQueryState.SelectingTable;
        }

        UpdateSelectionVisibility();
    }

    public void OnWhereClicked()
    {
        isWhereClicked = !isWhereClicked;

        if (!isWhereClicked)
        {
            query.ClearConditions();
        }
        else
        {
            query.ActivateWhere();
            if (query.Columns.Count > 0)
            {
                currentState = eQueryState.SelectingConditions;
            }
        }

        UpdateSelectionVisibility();
    }


    private void OnTableSelected(Table i_SelectedTable)
    {
        query.SetTable(i_SelectedTable);
        currentState = (isSelectClicked && query.table != null) ? eQueryState.SelectingColumns : eQueryState.SelectingTable;
        UpdateSelectionVisibility();
    }


    private void OnColumnSelected(Column i_Column)
    {
        if (query.Columns.Contains(i_Column))
        {
            query.RemoveColumn(i_Column);
        }
        else
        {
            query.AddColumn(i_Column);
        }
    }

    private void OnConditionColumnSelected(Column i_Column)
    {
        query.CreateNewCondition(i_Column);
        PopulateOperatorSelection();
    }

    private void OnConditionOperatorSelected(IOperatorStrategy i_Operator)
    {
        if (query.newCondition == null) return;        
        query.newCondition.Operator = i_Operator;
        PopulateValueSelection();
    }

    private void OnConditionValueSelected(object i_Value)
    {
        query.newCondition.Value = i_Value;
        query.AddCondition();
    }


    private void UpdateSelectionVisibility()
    {
        ClearSelectionPanel();

        switch (currentState)
        {
            case eQueryState.SelectingTable:
                PopulateTableSelection();
                break;

            case eQueryState.SelectingColumns:
                if (query.table != null)
                {
                    PopulateColumnSelection(query.table);
                }
                break;

            case eQueryState.SelectingConditions:
                PopulateConditionSelection();
                break;

            case eQueryState.None:

                break;
        }
    }

    private void UpdateQueryPreview()
    {
        if (query == null)
        {
            Debug.LogError("UpdateQueryPreview() - query is NULL before saving!");
        }
        queryPreviewText.text = query.QueryString;

        if (query.IsValid)
        {
            executeButton.interactable = true;
            whereButton.SetActive(true);
            GameManager.Instance.SaveQuery(query);
        }
        else
        {
            executeButton.interactable = false;
            whereButton.SetActive(false);
        }
    }

    private void PopulateTableSelection()
    {
        populateSelection(
            SupabaseManager.Instance.Tables,
            OnTableSelected,
            table => table.Name);
    }

    private void PopulateColumnSelection(Table i_Table)
    {
        populateSelection(
            i_Table.Columns,
            OnColumnSelected,
            column => column.Name);
    }

    private void PopulateConditionSelection()
    {
        populateSelection(query.table.Columns, OnConditionColumnSelected, column => column.Name);
    }

    private void PopulateOperatorSelection()
    {
        populateSelection(
            OperatorFactory.GetAllOperators(),
            OnConditionOperatorSelected,
            op => op.GetSQLRepresentation()
        );
    }

    private void PopulateValueSelection()
    {
        if (query.newCondition == null || query.newCondition.Column == null)
        {
            Debug.LogError("PopulateValueSelection() - No condition column selected!");
            return;
        }

        ClearSelectionPanel();
        switch (query.newCondition.Column.DataType)
        {
            case eDataType.Integer:
                ShowNumberInputOptions();
                break;

            case eDataType.String:
                ShowInputField();
                break;

            case eDataType.DateTime:
                pickDateTime();
                break;
                
            default:
                Debug.LogWarning($"Unsupported data type: {query.newCondition.Column.DataType}");
                break;
        }
    }

    private void populateSelection<T>(IEnumerable<T> i_Items, Action<T> i_OnItemSelected, Func<T,string> i_GetLabel, bool i_ClearSelectionPanel = true)
    {
        if (i_ClearSelectionPanel)
        {
            ClearSelectionPanel();
        }

        if (!i_Items.Any())
        {
            Debug.LogWarning("No items available for selection.");
            return;
        }

        foreach (T item in i_Items)
        {
            GameObject button = Instantiate(selectionButtonPrefab, selectionParent);
            button.transform.localScale = Vector3.one;
            button.GetComponentInChildren<TextMeshProUGUI>().text = i_GetLabel(item);

            Button btn = button.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => i_OnItemSelected(item));

            button.SetActive(true);
        }
    }

    private void ShowInputField()
    {
        GameObject inputFieldObject = Instantiate(inputFieldPrefab, selectionParent);
        inputFieldObject.transform.localScale = Vector3.one;
        TMP_InputField inputField = inputFieldObject.GetComponent<TMP_InputField>();

        if (inputField == null)
        {
            Debug.LogError("InputFieldPrefab is missing a TMP_InputField component!");
            return;
        }

        inputField.text = "";
        inputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter value...";
        inputField.Select();
        inputField.ActivateInputField();

        GameObject confirmButtonObject = Instantiate(confirmButtonPrefab, selectionParent);
        confirmButtonObject.transform.localScale = Vector3.one;

        Button confirmButton = confirmButtonObject.GetComponent<Button>();
        if (confirmButton == null)
        {
            Debug.LogError("ConfirmButtonPrefab is missing a Button component!");
            return;
        }

        confirmButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm";

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => OnConditionValueEntered(inputField.text));

    }

    private void OnConditionValueEntered(string i_InputValue)
    {
        if (string.IsNullOrWhiteSpace(i_InputValue))
        {
            Debug.LogWarning("No value entered!");
            return;
        }

        if (!checkValidInput(i_InputValue))
        {
            return;
        }
        
        query.newCondition.Value = FormatString(i_InputValue);

        ClearSelectionPanel();
        UpdateQueryPreview();
    }

    private bool checkValidInput(string i_InputValue)
    {
        bool res = true;
        if (query.newCondition.Column.DataType == eDataType.Integer)
        {
            Debug.Log("!!!! here !!!!");
            if (!int.TryParse(i_InputValue, out int o_Number))
            {
                Debug.LogWarning($"Invalid number: {i_InputValue}");
                res = false;
            }
        }

        return res;
    }

    private string FormatString(string i_InputValue)
    {
        return i_InputValue.Trim('"');
    }

    private void pickDateTime()
    {
        throw new NotImplementedException();
    }


    private void ShowNumberInputOptions()
    {
        ShowInputField();

        List<int> integerValues = new List<int> { 10, 20, 30, 40, 50, 60, 100};
        populateSelection(integerValues, val => OnConditionValueSelected(val), val => val.ToString(), i_ClearSelectionPanel: false);
    }

    private void ExecuteQuery()
    {
        GameManager.Instance.ExecuteQuery();
    }



    private void ClearSelectionPanel()
    {
        foreach (Transform child in selectionParent)
        {
            if (child != null) 
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }

}
