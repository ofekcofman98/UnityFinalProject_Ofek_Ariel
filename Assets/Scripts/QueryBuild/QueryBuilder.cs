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
    private Query query;
    public bool IsReady { get; private set; } = false;
    [SerializeField] private MissionUIManager missionUIManager;
    [SerializeField] private QueryUIRenderer uiRenderer;

    [Header("QueryPreview")]
    [SerializeField] public GameObject QueryPanel;

    [Header("Selection")]
    public Transform selectionParent;

    [Header("Clauses")]
    private Dictionary<IQueryClause, Button> activeClauseButtons = new Dictionary<IQueryClause, Button>();
    // public int conditionIndex => query.whereClause.NewConditionIndex;

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
        uiRenderer.SetClausePopulator(
            // items: query.availableClauses,
            onDropped: clause => query.ToggleClause(clause, true),
            onRemoved: clause => query.ToggleClause(clause, false),
            assignedSection: clause => uiRenderer.MatchClauseToSection(clause),
            activeButtons: activeClauseButtons);

        uiRenderer.RenderClauseButtons(query.availableClauses);
    }

    public void PopulateTableSelection()
    {
        var unlockedTables = SupabaseManager.Instance.Tables.Where(t => t.IsUnlocked);

        if (query.fromClause.table == null)
        {
            uiRenderer.SetTablePopulator(
                // items: unlockedTables,
                onDropped: table => query.SetTable(table),
                onRemoved: table => query.RemoveTable(),
                assignedSection: table => uiRenderer.MatchClauseToSection(query.fromClause),
                removalCondition: table => !query.fromClause.isClicked
            );

            uiRenderer.RenderTableButtons(unlockedTables);
        }
    }

    private void PopulateColumnSelection(Table i_Table)
    {
        if (query.fromClause.table == null) return;

        List<Column> visibleColumns = TableDataFilter.GetVisibleColumns(query.fromClause.table.Columns)
            .Where(c => !query.selectClause.Columns.Contains(c))
            .ToList();

        ClearSelectionPanel();

        uiRenderer.SetColumnPopulator(
            // items: visibleColumns,
            onDropped: col => query.AddColumn(col),
            onRemoved: col => query.RemoveColumn(col),
            assignedSection: col => uiRenderer.MatchClauseToSection(query.selectClause),
            removalCondition: col => query.fromClause.table == null || !query.selectClause.isClicked
        );

        uiRenderer.RenderColumnButtons(visibleColumns);
    }


    private void PopulateConditionColumnSelection()
    {
        if (query.fromClause.table == null) return;
        if (query.whereClause.Conditions.Count >= WhereClause.k_MaxConditions && query.whereClause.newCondition == null)
            return;

        int conditionIndex = query.whereClause.NewConditionIndex;

        List<Column> visibleColumns = TableDataFilter.GetVisibleColumns(query.fromClause.table.Columns).ToList();

        uiRenderer.SetConditionColumnPopulator(
            onDropped: col =>
            {
                query.CreateNewCondition(col);
                PopulateOperatorSelection();
            },
            onRemoved: col => query.RemoveConditionByIndex(conditionIndex),
            assignedSection: col => uiRenderer.MatchClauseToSection(query.whereClause),
            removalCondition: col => query.fromClause.table == null ||
                                           !query.whereClause.isClicked ||
                                           query.selectClause.IsEmpty() ||
                                           !query.whereClause.IsValidForConditionColumn(conditionIndex),
                                           //query.fromClause.table == null || !query.whereClause.isClicked || query.selectClause.IsEmpty(),
            conditionIndexGetter: _ => conditionIndex
        );

        uiRenderer.RenderConditionColumnButtons(visibleColumns);//query.fromClause.table.Columns);
    }

    private void PopulateOperatorSelection()
    {
        Condition last = query.whereClause.FindLastCondition();
        if (last == null) return;
        Column column = last.Column;

        int conditionIndex = query.whereClause.NewConditionIndex;

        uiRenderer.SetOperatorPopulator(
            onDropped: op =>
            {
                query.SetConditionOperator(op);
                PopulateValueSelection();
            },
            onRemoved: op =>
            {
                query.RemoveConditionOperatorByIndex(conditionIndex);
                PopulateOperatorSelection();
            },
            assignedSection: op => uiRenderer.MatchClauseToSection(query.whereClause),
            removalCondition: op => !query.whereClause.IsValidForOperator(conditionIndex),
            conditionIndexGetter: _ => conditionIndex

        );

        uiRenderer.RenderOperatorButtons(OperatorFactory.GetOperators(column));
    }

    private void PopulateValueSelection()
    {
        var condition = query.whereClause.newCondition;
        if (condition == null || condition.Column == null)
        {
            Debug.LogError("PopulateValueSelection() - No condition column selected!");
            return;
        }

        ClearSelectionPanel();

        Column column = condition.Column;
        Transform clauseSection = uiRenderer.MatchClauseToSection(query.whereClause);

        int conditionIndex = query.whereClause.NewConditionIndex;

        switch (column.DataType)
        {
            case eDataType.Integer:
                uiRenderer.ShowValueInputOptions(
                    predefinedValues: new List<int> { 10, 20, 30, 40, 50, 60, 100 },
                    validateInput: raw => int.TryParse(raw, out _),
                    formatInput: raw => raw.Trim(),
                    parseInput: raw => int.TryParse(raw, out int val) ? val : 0,
                    onValueSelected: val => query.SetConditionValue(val),
                    canRemove: val => !query.whereClause.IsValidForValue(conditionIndex),
                    onRemoved: val =>
                    {
                        query.RemoveConditionValueByIndex(conditionIndex);
                        PopulateValueSelection();
                    },
                    clauseSection: clauseSection
                );
                break;

            case eDataType.String:
                uiRenderer.ShowValueInputOptions(
                    predefinedValues: new List<string>(), // no buttons
                    validateInput: raw => !string.IsNullOrWhiteSpace(raw),
                    formatInput: raw => raw.Trim('"'),
                    parseInput: raw => raw,
                    onValueSelected: val => query.SetConditionValue(val),
                    canRemove: val => !query.whereClause.IsValidForValue(conditionIndex),
                    onRemoved: val =>
                    {
                        query.RemoveConditionValueByIndex(conditionIndex);
                        PopulateValueSelection();
                    },
                    clauseSection: clauseSection
                );
                break;

            case eDataType.DateTime:
                // You can implement a custom `DateTimeInputPopulator` later
                uiRenderer.PickDateTime();
                break;

            default:
                Debug.LogWarning($"Unsupported data type: {column.DataType}");
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

        uiRenderer?.DisposeValueInputPopulator();

        List<Button> buttonsToRelease = new List<Button>();
        foreach (Transform child in selectionParent)
        {
            if (child.TryGetComponent<Button>(out var button))
            {
                // buttonsToRelease.Add(button);
                Destroy(button.gameObject);
            }
            else if (child.GetComponent<TMP_InputField>() != null)
            {
                // ✅ Destroy any orphan TMP_InputField
                Destroy(child.gameObject);
            }
            else if (child.GetComponent<Button>() == null && child.GetComponentInChildren<TextMeshProUGUI>()?.text == "Confirm")
            {
                // ✅ Destroy confirm button that was created manually
                Destroy(child.gameObject);
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

        foreach (var button in buttonsToRelease)
        {
            uiRenderer.selectionButtonPool.Release(button);
        }
    }

    public void ResetQuery()
    {
        foreach (var pair in activeClauseButtons)
        {
            uiRenderer.clauseButtonPool.Release(pair.Value);
        }

        activeClauseButtons.Clear();
        uiRenderer.ClearClauseSections(new[]
        {
            uiRenderer.MatchClauseToSection(query.selectClause),
            uiRenderer.MatchClauseToSection(query.fromClause),
            uiRenderer.MatchClauseToSection(query.whereClause),
        });

        ClearSelectionPanel();

        query = new Query();
        query.OnQueryUpdated += HandleQueryChanged;
        // query.OnAvailableClausesChanged += HandleQueryChanged;

        uiRenderer.RenderQueryPreview("", false);
        updateAvailableClauses();
        
        uiRenderer.RefreshPanelButtons(); 
        RefreshQueryPreview();             
    }
}

