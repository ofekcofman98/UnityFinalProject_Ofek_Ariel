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

    [Header("QueryPreview")]
    [SerializeField] public GameObject QueryPanel;
    public TextMeshProUGUI queryPreviewText;
    public Button executeButton;


    
    [Header("Selection")]
    public GameObject selectionButtonPrefab; 
    public Transform selectionParent;
    [SerializeField] private GameObject inputFieldPrefab;
    [SerializeField] private GameObject confirmButtonPrefab;
    private ObjectPoolService<Button> selectionButtonPool;

    
    [Header("Clauses")]
    public GameObject ClausesButtonPrefab; 
    public Transform clausesParent;
    private ObjectPoolService<Button> clauseButtonPool;
    private Dictionary<IQueryClause, Button> activeClauseButtons = new Dictionary<IQueryClause, Button>();

    private Query query;

    void Start()
    {
        QueryPanel.SetActive(false);
        executeButton.onClick.AddListener(ExecuteQuery);
        clauseButtonPool = new ObjectPoolService<Button>(ClausesButtonPrefab.GetComponent<Button>(), clausesParent, 5, 20);
        selectionButtonPool = new ObjectPoolService<Button>(selectionButtonPrefab.GetComponent<Button>(), selectionParent);
    }

    public void BuildQuery()
    {
        QueryPanel.SetActive(true);
        
        if (query == null)
        {
            query = new Query();
            query.OnQueryUpdated += UpdateQueryPreview;
            query.OnAvailableClausesChanged += updateAvailableClauses;
        }

        updateAvailableClauses();

        if(SupabaseManager.Instance.Tables.Count > 0)
        {
            // PopulateTableSelection();
        }
        else
        {
            SupabaseManager.Instance.OnTableNamesFetched -= PopulateTableSelection;
            SupabaseManager.Instance.OnTableNamesFetched += PopulateTableSelection;
        }
    }

    private void updateAvailableClauses()
    {
        populateClauseButtons(
            i_Items: query.availableClauses,
            i_OnItemSelected: clause => 
            {
                query.ToggleClause(clause);
                query.UpdateQueryState();
                query.NotifyClauses();
                UpdateSelectionVisibility();
            },
            i_GetLabel: clause => clause.DisplayName,
            i_ParentTransform: clausesParent,
            i_ButtonPool: clauseButtonPool,
            i_ActiveButtons: activeClauseButtons
        );
    }

    private void OnTableSelected(Table i_SelectedTable)
    {
        query.SetTable(i_SelectedTable);
        query.UpdateQueryState();
        UpdateSelectionVisibility();
    }


    private void OnColumnSelected(Column i_Column)
    {
        if (query.selectClause.Columns.Contains(i_Column))
        {
            Debug.Log($"removing {i_Column.Name} from columns");
            query.RemoveColumn(i_Column);
        }
        else
        {
            Debug.Log($"adding {i_Column.Name} to columns");
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
        if (query.whereClause.newCondition == null) 
        {
            Debug.Log("newCondition is NULL");
            return;
        }        
        query.SetConditionOperator(i_Operator);
        PopulateValueSelection();
    }

    private void OnConditionValueSelected(object i_Value)
    {
        Debug.Log("new condition added");
        query.SetConditionValue(i_Value);
    }


    private void UpdateSelectionVisibility()
    {
        ClearSelectionPanel();

        Debug.Log($"state is: {query.currentState}");
        switch (query.currentState)
        {
            case eQueryState.SelectingTable:
                PopulateTableSelection();
                break;

            case eQueryState.SelectingColumns:
                if (query.fromClause.table != null)
                {
                    PopulateColumnSelection(query.fromClause.table);
                }
                break;

            case eQueryState.SelectingConditions:
                PopulateConditionSelection();
                break;

            case eQueryState.None:
                ClearSelectionPanel();
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
            GameManager.Instance.SaveQuery(query);
        }
        else
        {
            executeButton.interactable = false;
        }
    }

    private void PopulateTableSelection()
    {
        populateSelectionButtons(
            i_Items: SupabaseManager.Instance.Tables,
            i_OnItemSelected: OnTableSelected,
            i_GetLabel: table => table.Name,
            i_ParentTransform: selectionParent,
            i_ButtonPool: selectionButtonPool
            // i_ButtonPrefab: selectionButtonPrefab
            );
    }

    private void PopulateColumnSelection(Table i_Table)
    {
        populateSelectionButtons(
            i_Items: i_Table.Columns,
            i_OnItemSelected: OnColumnSelected,
            i_GetLabel: column => column.Name,
            i_ParentTransform: selectionParent,
            i_ButtonPool: selectionButtonPool
            // i_ButtonPrefab: selectionButtonPrefab
            );
    }

    private void PopulateConditionSelection()
    {
        if (query.fromClause.table != null)
        {
            populateSelectionButtons(
                i_Items: query.fromClause.table.Columns,
                i_OnItemSelected: OnConditionColumnSelected,
                i_GetLabel: column => column.Name,
                i_ParentTransform: selectionParent,
                i_ButtonPool: selectionButtonPool
                // i_ButtonPrefab: selectionButtonPrefab
                );
        }
    }

    private void PopulateOperatorSelection()
    {
        populateSelectionButtons(
            i_Items: OperatorFactory.GetOperators(query.whereClause.newCondition.Column),
            i_OnItemSelected: OnConditionOperatorSelected,
            i_GetLabel: op => op.GetSQLRepresentation(),
            i_ParentTransform: selectionParent,
            i_ButtonPool: selectionButtonPool
            // i_ButtonPrefab: selectionButtonPrefab
        );
    }

    private void PopulateValueSelection()
    {
        if (query.whereClause.newCondition == null || query.whereClause.newCondition.Column == null)
        {
            Debug.LogError("PopulateValueSelection() - No condition column selected!");
            return;
        }

        ClearSelectionPanel();
        switch (query.whereClause.newCondition.Column.DataType)
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
                Debug.LogWarning($"Unsupported data type: {query.whereClause.newCondition.Column.DataType}");
                break;
        }
    }

    private void populateClauseButtons<T>(
        IEnumerable<T> i_Items,
        Action<T> i_OnItemSelected,
        Func<T, string> i_GetLabel,
        Transform i_ParentTransform,
        ObjectPoolService<Button> i_ButtonPool,
        Dictionary<T, Button> i_ActiveButtons)
    {

        foreach (var key in i_ActiveButtons.Keys.ToList()) // Loop through all stored buttons
        {
            if (!i_Items.Contains(key)) // If the clause is no longer available
            {
                i_ButtonPool.Release(i_ActiveButtons[key]); // Release the button back to the pool
                i_ActiveButtons.Remove(key); // Remove the entry from the dictionary
            }
        }

        int index = 0; 
        foreach (T item in i_Items)
        {
            if (!i_ActiveButtons.ContainsKey(item)) 
            {
                Button button = i_ButtonPool.Get();
                button.transform.SetParent(i_ParentTransform, false);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TextMeshProUGUI>().text = i_GetLabel(item);

                // button.onClick.RemoveAllListeners();
                // button.onClick.AddListener(() => i_OnItemSelected(item));

                DraggableItem draggableItem = button.GetComponent<DraggableItem>();
                if (draggableItem == null)
                {
                    draggableItem = button.gameObject.AddComponent<DraggableItem>();
                }

                draggableItem.draggableType = eDraggableType.ClauseButton;
                draggableItem.OnDropped += OnItemDropped;


                i_ActiveButtons[item] = button;
            }
            i_ActiveButtons[item].transform.SetSiblingIndex(index);
            index++;

        }
    }

    private void populateSelectionButtons<T>(
        IEnumerable<T> i_Items, 
        Action<T> i_OnItemSelected,
        Func<T,string> i_GetLabel,
        Transform i_ParentTransform,
        ObjectPoolService<Button> i_ButtonPool,
        bool i_ClearSelectionPanel = true)
    {

        if (i_Items == null || !i_Items.Any())
        {
            Debug.LogWarning("No items available for selection.");
            return;
        }

        if (i_ClearSelectionPanel)
        {
            foreach (Transform child in i_ParentTransform)
            {
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    i_ButtonPool.Release(child.GetComponent<Button>());
                }
            }
        }

        int index = 0; 
        foreach (T item in i_Items)
        {
            Button button = i_ButtonPool.Get();
            if (button == null || button.gameObject == null)
            {
                Debug.LogError("[populateSelection] Button from pool is NULL!");
                continue;
            }

            button.transform.SetParent(i_ParentTransform, false);
            button.transform.SetSiblingIndex(index);  
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TextMeshProUGUI>().text = i_GetLabel(item);

            // button.onClick.RemoveAllListeners();
            // button.onClick.AddListener(() => i_OnItemSelected(item));

            DraggableItem draggableItem = button.GetComponent<DraggableItem>();
            if (draggableItem == null)
            {
                draggableItem = button.gameObject.AddComponent<DraggableItem>();
            }

            draggableItem.draggableType = eDraggableType.SelectionButton;
            draggableItem.OnDropped += OnItemDropped; // 🔥 Attach event listener


            index++;
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
        query.SetConditionValue(FormatString(i_InputValue));
        
        ClearSelectionPanel();
        UpdateQueryPreview();
    }

    private bool checkValidInput(string i_InputValue)
    {
        bool res = true;
        if (query.whereClause.newCondition.Column.DataType == eDataType.Integer)
        {
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
        populateSelectionButtons(
            i_Items: integerValues,
            i_OnItemSelected: val => OnConditionValueSelected(val),
            i_GetLabel: val => val.ToString(),
            i_ParentTransform: selectionParent,
            i_ButtonPool: selectionButtonPool,
            i_ClearSelectionPanel: false);

        selectionParent.GetChild(selectionParent.childCount - 2).SetAsFirstSibling(); // Input field
        selectionParent.GetChild(selectionParent.childCount - 1).SetSiblingIndex(1); // Confirm button
    }

    private void ExecuteQuery()
    {
        GameManager.Instance.ExecuteQuery();
    }

    private void ClearSelectionPanel()
    {        
        if (selectionParent.childCount == 0)
        {
            Debug.Log("[ClearSelectionPanel] No objects to clear, exiting early.");
            return; 
        }

        foreach (Transform child in selectionParent)
        {
            if (child != null) 
            {
                child.gameObject.SetActive(false);
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    selectionButtonPool.Release(button);
                }
            }
        }
    }

    internal void OnItemDropped(DraggableItem i_Draggable)
    {
        if (i_Draggable.draggableType == eDraggableType.ClauseButton)
        {
            AddClauseToQuery(i_Draggable);
        }
        else if (i_Draggable.draggableType == eDraggableType.SelectionButton)
        {
            AddSelectionToQuery(i_Draggable);
        }
    }

    private void AddSelectionToQuery(DraggableItem i_Draggable)
    {
        
    }

    private void AddClauseToQuery(DraggableItem i_Draggable)
    {
        
    }
}
