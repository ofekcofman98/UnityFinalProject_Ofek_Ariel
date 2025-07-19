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
        public bool IsReady { get; private set; } = false;
    [SerializeField] private MissionUIManager missionUIManager;
    [SerializeField] private QueryUIRenderer uiRenderer;

    [Header("QueryPreview")]
    [SerializeField] public GameObject QueryPanel;
    [SerializeField] private Transform selectSection;
    [SerializeField] private Transform fromSection;
    [SerializeField] private Transform whereSection;


    [Header("Selection")]
    // public GameObject selectionButtonPrefab; 
    public Transform selectionParent;
    // private ObjectPoolService<Button> selectionButtonPool;


    [Header("Clauses")]
    // public GameObject ClausesButtonPrefab; 
    public Transform clausesParent;
    // private ObjectPoolService<Button> clauseButtonPool;
    private Dictionary<IQueryClause, Button> activeClauseButtons = new Dictionary<IQueryClause, Button>();

    private Query query;

    void Awake()
    {
        IsReady = true;
        QueryPanel.SetActive(false);
        Debug.Log("called");
    }

    void Start()
    {
        GameManager.Instance.OnQueryIsCorrect += missionUIManager.ShowResult;
    }

    public void BuildQuery()
    {
        if (!checkIsReady())
        {
            return;
        }

        QueryPanel.SetActive(true);

        if (query == null)
        {
            query = new Query();
            query.OnQueryUpdated += UpdateQueryPreview;
            query.OnAvailableClausesChanged += updateAvailableClauses;
            GameManager.Instance.CurrentQuery = query;
        }

        updateAvailableClauses();

        if (SupabaseManager.Instance.Tables.Count <= 0)
        {
            SupabaseManager.Instance.OnTableNamesFetched -= PopulateTableSelection;
            SupabaseManager.Instance.OnTableNamesFetched += PopulateTableSelection;
        }

        missionUIManager.ShowUI();
        // GameManager.Instance.OnQueryIsCorrect += queryUIManager.ShowResult;
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


    private void updateAvailableClauses()
    {
        uiRenderer.populateClauseButtons(
            i_Items: query.availableClauses,
            i_OnItemDropped: clause =>
            {
                query.ToggleClause(clause, true);
                query.UpdateQueryState();
                query.NotifyClauses();

                uiRenderer.RefreshPanelButtons();
                RefreshQueryPreview();

                // UpdateQueryButtons();
                UpdateSelectionVisibility();
            },
            i_GetLabel: clause => clause.DisplayName,
            i_ParentTransform: clausesParent,
            i_AssignedSection: clause => matchClauseToSection(clause),
            // i_ButtonPool: uiRenderer.clauseButtonPool,
            i_ButtonPrefab: uiRenderer.ClausesButtonPrefab.GetComponent<Button>(),
            i_ActiveButtons: activeClauseButtons
            , i_OnItemRemoved: clause =>
            {
                query.ToggleClause(clause, false);
                query.UpdateQueryState();
                query.NotifyClauses();

                uiRenderer.RefreshPanelButtons();
                RefreshQueryPreview();

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

    // private void OnJoinTableSelected(Table i_Table)
    // {
        
    // }

    public void PopulateTableSelection()
    {
        var unlockedTables = SupabaseManager.Instance.Tables.Where(t => t.IsUnlocked);

        if (query.fromClause.table == null)
        {
            uiRenderer.populateSelectionButtons(
                 i_Items: unlockedTables
                , i_OnItemDropped: OnTableSelected
                , i_GetLabel: table => table.Name
                , i_ParentTransform: selectionParent
                , i_AssignedSection: table => fromSection
                // , i_ButtonPool: uiRenderer.selectionButtonPool
                ,i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

                , i_RemovalCondition: table => query.fromClause.table == null ||
                                              !query.fromClause.isClicked

                , i_OnItemRemoved: table =>
                {
                    query.fromClause.ClearTable();
                    query.NotifyClauses();
                    // query.UpdateQueryState();
                    query.queryState.CurrentState = eQueryState.SelectingTable;

                    uiRenderer.RefreshPanelButtons();
                    RefreshQueryPreview();

                    UpdateSelectionVisibility();
                }
                );
        }
    }

    // private void PopulateJoinableTableSelection()
    // {
    //     Table baseTable = query.GetTable();

    //     List<Table> joinableTables = SupabaseManager.Instance.Tables
    //     .Where(t => baseTable.GetForeignKeysTo(t).Count > 0).ToList();

    //     uiRenderer.populateSelectionButtons(
    //     i_Items: joinableTables,
    //     i_OnItemDropped: OnJoinTableSelected,
    //     i_GetLabel: table => table.Name,
    //     i_ParentTransform: selectionParent,
    //     i_AssignedSection: table => fromSection,
    //     i_ButtonPool: uiRenderer.selectionButtonPool



    //     ); 
    // }

    private void PopulateColumnSelection(Table i_Table)
    {
        foreach (var col in i_Table.Columns)
        {
            Debug.Log($"🔍 Column found: {col.Name}");
        }

        ClearSelectionPanel();
        uiRenderer.populateSelectionButtons
        (
             i_Items: i_Table.Columns
            , i_OnItemDropped: col => query.AddColumn(col)
            , i_GetLabel: column => column.Name
            , i_ParentTransform: selectionParent
            , i_AssignedSection: col => selectSection
            // , i_ButtonPool: uiRenderer.selectionButtonPool
            ,i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

            , i_RemovalCondition: column => query.fromClause.table == null ||
                                          !query.selectClause.isClicked

            , i_OnItemRemoved: col =>
            {
                query.RemoveColumn(col);

                if (query.selectClause.IsEmpty())
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
            uiRenderer.populateSelectionButtons
            (
                 i_Items: query.fromClause.table.Columns
                , i_OnItemDropped: OnConditionColumnSelected
                , i_GetLabel: column => column.Name
                , i_ParentTransform: selectionParent
                , i_AssignedSection: col => whereSection
                // ,i_ButtonPool: uiRenderer.selectionButtonPool
                ,i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

                , i_RemovalCondition: column => query.fromClause.table == null ||
                                              !query.whereClause.isClicked ||
                                              query.selectClause.IsEmpty()
                , i_OnItemRemoved: col =>
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
        uiRenderer.populateSelectionButtons
        (
            i_Items: OperatorFactory.GetOperators(query.whereClause.newCondition.Column),
            i_OnItemDropped: OnConditionOperatorSelected,
            i_GetLabel: op => op.GetSQLRepresentation(),
            i_ParentTransform: selectionParent,
            i_AssignedSection: op => whereSection,
            // i_ButtonPool: uiRenderer.selectionButtonPool,
            i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>(), 
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

    private void UpdateSelectionVisibility()
    {
        ClearSelectionPanel();

        // Debug.Log($"state is: {query.queryState.CurrentState}");

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
                uiRenderer.ShowNumberInputOptions(
                values: new List<int> { 10, 20, 30, 40, 50, 60, 100 },
                onValueSelected: val =>
                {
                    query.SetConditionValue(val);
                    UpdateQueryPreview();
                },
                canRemove: val => query.fromClause.table == null || !query.whereClause.isClicked,
                clauseSection: whereSection);
                break;

            case eDataType.String:
                uiRenderer.ShowInputField(
                validateInput: input =>
                {
                    return !string.IsNullOrWhiteSpace(input);
                },
                formatInput: input => input.Trim('"'),
                onConfirm: formatted =>
                {
                    query.SetConditionValue(formatted);
                    UpdateQueryPreview();
                },
                canRemove: val => query.fromClause.table == null || !query.whereClause.isClicked,
                onRemove: val =>
                {
                    query.clearConditionValue();
                    PopulateValueSelection();
                },
                clauseSection: whereSection);
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
        buttonsToRelease.Add(button);
    }
}

// ✅ Now release safely
foreach (var button in buttonsToRelease)
{
    uiRenderer.selectionButtonPool.Release(button);
}
    }


public void SetConditionValue(object i_Value)
{
    query.SetConditionValue(i_Value);
    UpdateQueryPreview(); // optional: keep your query preview updated
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
        query.OnQueryUpdated += UpdateQueryPreview;
        query.OnAvailableClausesChanged += updateAvailableClauses;

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
