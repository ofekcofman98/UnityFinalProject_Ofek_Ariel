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
    [SerializeField] private Transform selectSection;
    [SerializeField] private Transform fromSection;
    [SerializeField] private Transform whereSection;
    public TextMeshProUGUI queryPreviewText;
    public Button executeButton;
    private Transform[] sections;
    // private Dictionary<object, Button> queryPanelButtons = new Dictionary<object, Button>();


        
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
private Dictionary<Button, (Func<bool> condition, Action removeAction)> removalConditions 
    = new Dictionary<Button, (Func<bool>, Action)>();


    void Start()
    {
        QueryPanel.SetActive(false);
        executeButton.onClick.AddListener(ExecuteQuery);
        clauseButtonPool = new ObjectPoolService<Button>(ClausesButtonPrefab.GetComponent<Button>(), clausesParent, 5, 20);
        selectionButtonPool = new ObjectPoolService<Button>(selectionButtonPrefab.GetComponent<Button>(), selectionParent);
    
        sections = new[] { selectSection, fromSection, whereSection};

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

        if(SupabaseManager.Instance.Tables.Count <= 0)
        {
            SupabaseManager.Instance.OnTableNamesFetched -= PopulateTableSelection;
            SupabaseManager.Instance.OnTableNamesFetched += PopulateTableSelection;
        }
    }

    private void updateAvailableClauses()
    {
        populateClauseButtons(
            i_Items: query.availableClauses,
            i_OnItemDropped: clause => 
            {
                query.ToggleClause(clause, true);
                query.UpdateQueryState();
                query.NotifyClauses();
                
                syncQueryUI();
                // UpdateQueryButtons();
                UpdateSelectionVisibility();
            },
            i_GetLabel: clause => clause.DisplayName,
            i_ParentTransform: clausesParent,
            i_AssignedSection: clause => matchClauseToSection(clause),
            i_ButtonPool: clauseButtonPool,
            i_ActiveButtons: activeClauseButtons
            ,i_OnItemRemoved: clause =>
            {
                query.ToggleClause(clause, false);
                query.UpdateQueryState();
                query.NotifyClauses();
                
                syncQueryUI();
                // UpdateQueryButtons();
                UpdateSelectionVisibility();
            }
        );
    }

    private void OnTableSelected(Table i_SelectedTable)
    {
        if (query.fromClause.table != null) 
        {
            return;
        }
        Debug.Log($"[SetTable]: {i_SelectedTable.Name}");
        query.SetTable(i_SelectedTable);
        query.UpdateQueryState();
        UpdateSelectionVisibility();
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

    private void PopulateTableSelection()
    {
        if (query.fromClause.table == null)
        {
            populateSelectionButtons(
                 i_Items: SupabaseManager.Instance.Tables
                ,i_OnItemDropped: OnTableSelected
                ,i_GetLabel: table => table.Name
                ,i_ParentTransform: selectionParent
                ,i_AssignedSection: table => fromSection
                ,i_ButtonPool: selectionButtonPool
                ,i_RemovalCondition: table => query.fromClause.table == null ||
                                              !query.fromClause.isClicked 
                ,i_OnItemRemoved: table => {
                    query.fromClause.ClearTable();
                    // query.ClearColumns();
                    // query.UpdateQueryState();
                    query.queryState.CurrentState = eQueryState.SelectingTable;
                    query.NotifyClauses();
                    syncQueryUI();
                    UpdateSelectionVisibility();
                    }
                );
        }
    }

    private void PopulateColumnSelection(Table i_Table)
    {
        populateSelectionButtons
        (
             i_Items: i_Table.Columns
            ,i_OnItemDropped: col => query.AddColumn(col)
            ,i_GetLabel: column => column.Name
            ,i_ParentTransform: selectionParent
            ,i_AssignedSection: col => selectSection
            ,i_ButtonPool: selectionButtonPool
            ,i_RemovalCondition: column => query.fromClause.table == null ||
                                          !query.selectClause.isClicked 
            ,i_OnItemRemoved: col => 
            {
                // if (query.queryState.CurrentState == eQueryState.SelectingConditions)
                // {
                //     UpdateSelectionVisibility();
                // }
                query.RemoveColumn(col);
                
                if(query.selectClause.IsEmpty())
                {
                    query.whereClause.isAvailable = false;
                }
            }
        );
    }

    private void PopulateConditionColumnSelection()
    {
        if (query.fromClause.table != null)
        {
            populateSelectionButtons
            (
                 i_Items: query.fromClause.table.Columns
                ,i_OnItemDropped: OnConditionColumnSelected
                ,i_GetLabel: column => column.Name
                ,i_ParentTransform: selectionParent
                ,i_AssignedSection: col => whereSection
                ,i_ButtonPool: selectionButtonPool
                ,i_RemovalCondition: column => query.fromClause.table == null ||
                                              !query.whereClause.isClicked ||
                                              query.selectClause.IsEmpty()
                ,i_OnItemRemoved: col => 
                {
                    query.whereClause.Conditions.Clear();

                    query.queryState.CurrentState = eQueryState.SelectingConditions;
                    UpdateSelectionVisibility();
                }
            );
        }
    }

    private void PopulateOperatorSelection()
    {
        populateSelectionButtons
        (
            i_Items: OperatorFactory.GetOperators(query.whereClause.newCondition.Column),
            i_OnItemDropped: OnConditionOperatorSelected,
            i_GetLabel: op => op.GetSQLRepresentation(),
            i_ParentTransform: selectionParent,
            i_AssignedSection: op => whereSection,
            i_ButtonPool: selectionButtonPool,
            i_RemovalCondition: op => query.fromClause.table == null ||
                                      !query.whereClause.isClicked ||
                                      !query.whereClause.isAvailable ||
                                      query.whereClause.Conditions.Count == 0
            ,i_OnItemRemoved: op => 
            {
                query.SetConditionOperator(null);
                PopulateOperatorSelection();
            }
        );
    }

    private void populateClauseButtons<T>(
        IEnumerable<T> i_Items,
        Action<T> i_OnItemDropped,
        Func<T, string> i_GetLabel,
        Transform i_ParentTransform,
        Func<T, Transform> i_AssignedSection,
        ObjectPoolService<Button> i_ButtonPool,
        Dictionary<T, Button> i_ActiveButtons,

        Func<T, bool> i_RemovalCondition = null,
        Action<T> i_OnItemRemoved = null)

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
                // button.transform.SetParent(i_ParentTransform, false);
                InsertButtonInSection(i_ParentTransform, button, eDraggableType.ClauseButton);

                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TextMeshProUGUI>().text = i_GetLabel(item);

                // button.onClick.RemoveAllListeners();
                // button.onClick.AddListener(() => i_OnItemSelected(item));

                DraggableItem draggableItem = button.GetComponent<DraggableItem>();
                if (draggableItem == null)
                {
                    draggableItem = button.gameObject.AddComponent<DraggableItem>();
                }

                draggableItem.AssignedSection = i_AssignedSection(item);                
                draggableItem.draggableType = eDraggableType.ClauseButton;
                draggableItem.OnDropped += (droppedItem) => i_OnItemDropped(item);
                draggableItem.OnRemoved += () => i_OnItemRemoved(item);


                i_ActiveButtons[item] = button;
            }
            // i_ActiveButtons[item].transform.SetSiblingIndex(index);
            index++;

        }
        
    }

    private void populateSelectionButtons<T>(
        IEnumerable<T> i_Items, 
        Action<T> i_OnItemDropped,
        Func<T,string> i_GetLabel,
        Transform i_ParentTransform,
        Func<T, Transform> i_AssignedSection,
        ObjectPoolService<Button> i_ButtonPool,
        bool i_ClearSelectionPanel = true,
        Func<T, bool> i_RemovalCondition = null,
        Action<T> i_OnItemRemoved = null)
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

Debug.Log($"[[{Time.time:F2}]] ");
Debug.Log($"query.selectClause.isClicked: {query.selectClause.isClicked} ");
Debug.Log($"query.fromClause.isClicked: {query.fromClause.isClicked} ");
Debug.Log($"query.whereClause.isClicked: {query.whereClause.isClicked} ");
Debug.Log($"Query State is: {query.queryState.CurrentState}");        
Debug.Log($"");        

        int index = 0; 
        foreach (T item in i_Items)
        {
            Button button = i_ButtonPool.Get();
            if (button == null || button.gameObject == null)
            {
                Debug.LogError("[populateSelection] Button from pool is NULL!");
                continue;
            }

            // button.transform.SetParent(i_ParentTransform, false);
            InsertButtonInSection(i_ParentTransform, button, eDraggableType.SelectionButton);

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
            draggableItem.ResetEvents();
            draggableItem.AssignedSection = i_AssignedSection(item);
        
            draggableItem.draggableType = eDraggableType.SelectionButton;
           
            draggableItem.OnDropped += (droppedItem) => i_OnItemDropped(item);
            draggableItem.OnRemoved += () => i_OnItemRemoved(item);

            if (i_RemovalCondition != null)
            {
                Action removeAction = () => i_OnItemRemoved?.Invoke(item);
                removalConditions[button] = (() => i_RemovalCondition(item), removeAction);
            }

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
        string formattedValue = FormatString(i_InputValue);
        
        // ClearSelectionPanel();
        UpdateQueryPreview();

        populateSelectionButtons(
            i_Items: new List<string> { formattedValue },
            i_OnItemDropped: val => OnConditionValueSelected(formattedValue),
            i_GetLabel: val => formattedValue,
            i_ParentTransform: selectionParent,
            i_AssignedSection: val => whereSection,
            i_ButtonPool: selectionButtonPool,
            i_ClearSelectionPanel: false,
            i_RemovalCondition: val => query.fromClause.table == null ||
                                       !query.whereClause.isClicked  
            ,i_OnItemRemoved: val => 
            {
                query.clearConditionValue();
                PopulateValueSelection();
            }      
        );
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
            i_OnItemDropped: val => OnConditionValueSelected(val),
            i_GetLabel: val => val.ToString(),
            i_ParentTransform: selectionParent,
            i_ButtonPool: selectionButtonPool,
            i_AssignedSection: val => whereSection,
            i_ClearSelectionPanel: false,
            i_RemovalCondition: val => query.fromClause.table == null ||
                                       !query.whereClause.isClicked        
        );
        selectionParent.GetChild(selectionParent.childCount - 2).SetAsFirstSibling(); // Input field
        selectionParent.GetChild(selectionParent.childCount - 1).SetSiblingIndex(1); // Confirm button
    }

    private void ExecuteQuery()
    {
        GameManager.Instance.ExecuteQuery();
    }

    private void UpdateSelectionVisibility()
    {
        ClearSelectionPanel();

        Debug.Log($"state is: {query.queryState.CurrentState}");

        switch (query.queryState.CurrentState)
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
                PopulateConditionColumnSelection();
                break;

            case eQueryState.None:
                ClearSelectionPanel();
                break;
        }
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

    private void syncQueryUI()
    {
        EvaluateQueryPanelButtons();
        UpdateQueryPreview();
    }

    private void EvaluateQueryPanelButtons()
    {
        foreach (var pair in removalConditions.ToList())
        {
            Button button = pair.Key;
            var (condition, removeAction) = pair.Value;

            if(condition())
            {
                // removeAction?.Invoke();
                removalConditions.Remove(button);
                selectionButtonPool.Release(button);
            }
        }
    }    

    private void InsertButtonInSection(Transform section, Button button, eDraggableType type)
    {

        int insertIndex = 0;

        for (int i = 0; i < section.childCount; i++)
        {
            DraggableItem existingDraggable = section.GetChild(i).GetComponent<DraggableItem>();
            if (existingDraggable == null) continue;

            if (type == eDraggableType.SelectionButton && existingDraggable.draggableType == eDraggableType.SelectionButton)
            {
                insertIndex = i + 1;
            }
        }

        button.transform.SetParent(section, false);
        button.transform.SetSiblingIndex(insertIndex);
    }
   
    private Transform matchClauseToSection(IQueryClause i_Clause)
    {
        Transform section = selectSection;

        string name = i_Clause.DisplayName;
        if (name == QueryConstants.Select)
        {
            section = selectSection;
        }
        else if (name == QueryConstants.From)
        {
            section = fromSection;
        }
        else if (name == QueryConstants.Where)
        {
            section = whereSection;
        }
        
        return section;
    }

}
