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
using System.Globalization;

public class QueryBuilder : MonoBehaviour
{
    public bool IsReady { get; private set; } = false;
    [SerializeField] private MissionUIManager missionUIManager;
    [SerializeField] private QueryUIRenderer uiRenderer;

    [Header("QueryPreview")]
    [SerializeField] public GameObject QueryPanel;
    [SerializeField] private Transform selectSection;
    [SerializeField] private Transform fromSection;
    [SerializeField] private Transform whereSection;


    [Header("Selection")]
    public Transform selectionParent;
    // private ObjectPoolService<Button> selectionButtonPool;

    [Header("Clauses")]
    public Transform clausesParent;
    // private ObjectPoolService<Button> clauseButtonPool;
    private Dictionary<IQueryClause, Button> activeClauseButtons = new Dictionary<IQueryClause, Button>();

    private Query query;

    void Awake()
    {
        IsReady = true;
        QueryPanel.SetActive(false);
    }

    void Start()
    {
        GameManager.Instance.OnQueryIsCorrect += missionUIManager.ShowResult;
    }

    public void BuildQuery()
    {
        if (!checkIsReady()) { return; }
        QueryPanel.SetActive(true);

        if (query == null)
        {
            query = new Query();
            GameManager.Instance.CurrentQuery = query;
            query.OnQueryUpdated += HandleQueryChanged;
        }

        updateAvailableClauses();

        if (SupabaseManager.Instance.Tables.Count <= 0)
        {
            SupabaseManager.Instance.OnTableNamesFetched -= PopulateTableSelection;
            SupabaseManager.Instance.OnTableNamesFetched += PopulateTableSelection;
        }
    }

    private void HandleQueryChanged()
    {
        // 1) preview
        uiRenderer.RenderQueryPreview(query.QueryString, query.IsValid);
        if (query.IsValid)
        {
            GameManager.Instance.SaveQuery(query);
        }
        updateAvailableClauses();        // 2) update clause buttons from model
        ShowNextSelectionOptions();     // 3) repopulate selection panel based on state
        uiRenderer.RefreshPanelButtons();// 4) If you still need, refresh panel cosmetics
    }


    private bool checkIsReady()
    {
        bool res = true;
        if (!IsReady)
        {
            Debug.LogWarning("⚠️ QueryBuilder.BuildQuery() called before initialization is complete.");
            res = false;
        }

        if (SupabaseManager.Instance == null || SupabaseManager.Instance.Tables == null)
        {
            Debug.LogWarning("⚠️ SupabaseManager or Tables not ready yet.");
            res = false;
        }

        if (SupabaseManager.Instance.Tables.Count == 0)
        {
            Debug.LogWarning("⚠️ Supabase tables are empty — cannot build query.");
            res = false;
        }

        return res;
    }

    private void OnEnable()
    {
        var supabase = SupabaseManager.Instance;
        if (supabase != null)
        {
            supabase.OnSchemeFullyLoaded += HandleSchemeReady;
        }
    }

    private void OnDisable()
    {
        var supabase = FindObjectOfType<SupabaseManager>();
        if (supabase != null)
        {
            supabase.OnSchemeFullyLoaded -= HandleSchemeReady;
        }
    }

    private void HandleSchemeReady()
    {
        Debug.Log("✅ QueryBuilder: Supabase scheme is fully loaded. Building query UI.");

        if (GameManager.Instance.CurrentQuery == null)
            GameManager.Instance.CurrentQuery = new Query();

        BuildQuery();
    }

    private void ShowNextSelectionOptions()
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

        // uiRenderer.RefreshPanelButtons(); 
    }

    private void updateAvailableClauses()
    {
        uiRenderer.populateClauseButtons(
            i_Items: query.availableClauses,
            i_OnItemDropped: clause => query.ToggleClause(clause, true),
            i_GetLabel: clause => clause.DisplayName,
            i_ParentTransform: clausesParent,
            i_AssignedSection: clause => matchClauseToSection(clause),
            // i_ButtonPool: uiRenderer.clauseButtonPool,
            i_ButtonPrefab: uiRenderer.ClausesButtonPrefab.GetComponent<Button>(),
            i_ActiveButtons: activeClauseButtons,
            i_OnItemRemoved: clause => query.ToggleClause(clause, false)
        );
    }

    public void PopulateTableSelection()
    {
        var unlockedTables = SupabaseManager.Instance.Tables.Where(t => t.IsUnlocked);

        if (query.fromClause.table == null)
        {
            uiRenderer.populateSelectionButtons(
                 i_Items: unlockedTables
                , i_OnItemDropped: table => query.SetTable(table)
                , i_GetLabel: table => table.Name
                , i_ParentTransform: selectionParent
                , i_AssignedSection: table => fromSection
                // , i_ButtonPool: uiRenderer.selectionButtonPool
                , i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

                , i_RemovalCondition: table =>// query.fromClause.table == null ||
                                              !query.fromClause.isClicked

                , i_OnItemRemoved: table => query.RemoveTable()
                );
        }
    }

    private void PopulateColumnSelection(Table i_Table)
    {
        List<Column> visibleColumns = TableDataFilter.GetVisibleColumns(i_Table.Columns).ToList();

        visibleColumns = visibleColumns
            .Where(c => !query.selectClause.Columns.Contains(c))
            .ToList();

        ClearSelectionPanel();

        uiRenderer.populateSelectionButtons
        (
              i_Items: visibleColumns
            , i_OnItemDropped: col => query.AddColumn(col)
            , i_GetLabel: column => column.Name
            , i_ParentTransform: selectionParent
            , i_AssignedSection: col => selectSection
            // , i_ButtonPool: uiRenderer.selectionButtonPool
            ,i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

            , i_RemovalCondition: column => query.fromClause.table == null ||
                                          !query.selectClause.isClicked

            , i_OnItemRemoved: col => query.RemoveColumn(col)
        );
    }

    private void PopulateConditionColumnSelection()
    {
        if (query.fromClause.table == null) return;

        if (query.whereClause.Conditions.Count >= WhereClause.k_MaxConditions
            && query.whereClause.newCondition == null)
            return;

        uiRenderer.populateSelectionButtons
        (
             i_Items: query.fromClause.table.Columns
            , i_OnItemDropped: col =>
            {
                query.CreateNewCondition(col);
                PopulateOperatorSelection();
            }
            , i_GetLabel: column => column.Name
            , i_ParentTransform: selectionParent
            , i_AssignedSection: col => whereSection
            // ,i_ButtonPool: uiRenderer.selectionButtonPool
            , i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

            , i_RemovalCondition: column => query.fromClause.table == null ||
                                          !query.whereClause.isClicked ||
                                          query.selectClause.IsEmpty()
            , i_OnItemRemoved: col => query.RemoveConditionColumn(col)
        );
        
    }

    private void PopulateOperatorSelection()
    {

        // Column column;
        // Condition last = query.whereClause.FindLastCondition();
        // if (last == null) return;
        // column = last.Column;

int targetIndex = query.whereClause.CurrentEditingConditionIndex;
    if (targetIndex < 0 || targetIndex >= WhereClause.k_MaxConditions)
    {
        Debug.LogError("Invalid condition index.");
        return;
    }

    Condition targetCondition = query.whereClause.newCondition ?? query.whereClause.Conditions[targetIndex];
    Column column = targetCondition.Column;
    if (column == null) return;


        uiRenderer.populateSelectionButtons
        (
            i_Items: OperatorFactory.GetOperators(column),
            i_OnItemDropped: op =>
            {
                query.SetConditionOperator(op);
                PopulateValueSelection();
            },
            i_GetLabel: op => op.GetSQLRepresentation(),
            i_ParentTransform: selectionParent,
            i_AssignedSection: op => whereSection,
            // i_ButtonPool: uiRenderer.selectionButtonPool,
            i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>(),
            i_RemovalCondition: _ => targetCondition.Column == null || targetCondition.Operator == null,
//op => !query.whereClause.IsValidForOperator()
            i_OnItemRemoved: op => 
            {
                query.RemoveConditionOperator();
                PopulateOperatorSelection();
            }
        );
    }

    private void PopulateValueSelection()
    {

        // if (query.whereClause.newCondition == null || query.whereClause.newCondition.Column == null)
        // {
        //     Debug.LogError("PopulateValueSelection() - No condition column selected!");
        //     return;
        // }

        // ClearSelectionPanel();

        // Column column = query.whereClause.newCondition.Column;
        // Transform clauseSection = whereSection;
        

    int targetIndex = query.whereClause.CurrentEditingConditionIndex;
    if (targetIndex < 0 || targetIndex >= WhereClause.k_MaxConditions)
    {
        Debug.LogError("Invalid condition index.");
        return;
    }

    Condition targetCondition = query.whereClause.newCondition ?? query.whereClause.Conditions[targetIndex];
    if (targetCondition == null || targetCondition.Column == null) return;

    ClearSelectionPanel();
    Column column = targetCondition.Column;
    Transform clauseSection = whereSection;



        switch (column.DataType)
        {
            case eDataType.Integer:
                uiRenderer.ShowNumberInputOptions(
                values: new List<int> { 10, 20, 30, 40, 50, 60, 100 },
                onValueSelected: val => query.SetConditionValue(val),
                canRemove: val => targetCondition.Value == null || targetCondition.Operator == null || targetCondition.Column == null,
                //val => !query.whereClause.IsValidForValue(),
                onRemove: val =>
                {
                    query.clearConditionValue();
                    PopulateValueSelection();
                },
                clauseSection: clauseSection);
                break;

            case eDataType.String:
                uiRenderer.ShowInputField(
                validateInput: input => { return !string.IsNullOrWhiteSpace(input); },
                formatInput: input => input.Trim('"'),
                onValueSelected: formatted => query.SetConditionValue(formatted),
                canRemove: val => targetCondition.Value == null || targetCondition.Operator == null || targetCondition.Column == null,
                //val => !query.whereClause.IsValidForValue(),
                onRemove: val =>
                {
                    query.clearConditionValue();
                    PopulateValueSelection();
                },
                clauseSection: clauseSection);
                break;

            case eDataType.DateTime:
                uiRenderer.PickDateTime();
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
        // queryPreviewText.text = query.QueryString;
        uiRenderer.RenderQueryPreview(query.QueryString, query.IsValid);

        if (query.IsValid)
        {
            GameManager.Instance.SaveQuery(query);
        }
    }
    
    public void RefreshQueryPreview()
    {
        if (query == null)
        {
            Debug.LogError("Query is null!");
            return;
        }

        uiRenderer.RenderQueryPreview(query.QueryString, query.IsValid);

        if (query.IsValid)
        {
            GameManager.Instance.SaveQuery(query);
        }
    }

    private void ClearSelectionPanel()
    {
        if (selectionParent.childCount == 0)
        {
            Debug.Log("[ClearSelectionPanel] No objects to clear, exiting early.");
            return;
        }

        List<Button> buttonsToRelease = new List<Button>();
        foreach (Transform child in selectionParent)
        {
            if (child.TryGetComponent<Button>(out var button))
            {
                // buttonsToRelease.Add(button);
                Destroy(button.gameObject); 
            }
        }

        if (uiRenderer != null)
        {
            if (uiRenderer.currentInputField != null)
            {
                GameObject.Destroy(uiRenderer.currentInputField);
                uiRenderer.currentInputField = null;
            }

            if (uiRenderer.currentConfirmButton != null)
            {
                GameObject.Destroy(uiRenderer.currentConfirmButton);
                uiRenderer.currentConfirmButton = null;
            }
        }


        // ✅ Now release safely
        foreach (var button in buttonsToRelease)
        {
            uiRenderer.selectionButtonPool.Release(button);
        }
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
        else if (name == QueryConstants.Where || name == QueryConstants.And)
        {
            section = whereSection;
        }

        return section;
    }

    public void ResetQuery()
    {
        foreach (var pair in activeClauseButtons)
        {
            uiRenderer.clauseButtonPool.Release(pair.Value);
        }

        activeClauseButtons.Clear();
uiRenderer.ClearClauseSections(new[] { selectSection, fromSection, whereSection });

        ClearSelectionPanel();

        query = new Query();
        query.OnQueryUpdated += HandleQueryChanged;
        // query.OnAvailableClausesChanged += HandleQueryChanged;

        uiRenderer.RenderQueryPreview("", false);
        updateAvailableClauses();
        
        
        uiRenderer.RefreshPanelButtons(); 
        RefreshQueryPreview();             


        // foreach (Transform child in selectSection)
        //     Debug.Log("🟡 Leftover in SELECT: " + child.name);

        // foreach (Transform child in fromSection)
        //     Debug.Log("🟡 Leftover in FROM: " + child.name);

        // foreach (Transform child in whereSection)
        //     Debug.Log("🟡 Leftover in WHERE: " + child.name);
    }
}

