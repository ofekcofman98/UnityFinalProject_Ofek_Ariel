using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuerySelectionUI : MonoBehaviour
{
    // private Query query;
    // private QueryUIRenderer uiRenderer;

    // [SerializeField] private Transform selectionParent;
    // [SerializeField] private Transform fromSection;
    // [SerializeField] private Transform selectSection;
    // [SerializeField] private GameObject selectionButtonPrefab;

    // public void Init(Query query, QueryUIRenderer uiRenderer)
    // {
    //     this.query = query;
    //     this.uiRenderer = uiRenderer;
    // }

    // public void ShowOptions()
    // {
    //     switch (query.queryState.CurrentState)
    //     {
    //         case eQueryState.SelectingTable:
    //             PopulateTableSelection();
    //             break;

    //         case eQueryState.SelectingColumns:
    //             PopulateColumnSelection(query.GetTable());
    //             break;

    //         case eQueryState.SelectingConditions:
    //             PopulateConditionSelection();
    //             break;
    //     }
    // }

    // public void PopulateTableSelection()
    // {
    //     var unlockedTables = SupabaseManager.Instance.Tables.Where(t => t.IsUnlocked);

    //     if (query.fromClause.table == null)
    //     {
    //         uiRenderer.populateSelectionButtons(
    //              i_Items: unlockedTables
    //             , i_OnItemDropped: table => query.SetTable(table)
    //             , i_GetLabel: table => table.Name
    //             , i_ParentTransform: selectionParent
    //             , i_AssignedSection: table => fromSection
    //             , i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

    //             , i_RemovalCondition: table => !query.fromClause.isClicked

    //             , i_OnItemRemoved: table => query.RemoveTable()
    //             );
    //     }
    // }

    // private void PopulateColumnSelection(Table i_Table)
    // {
    //     List<Column> visibleColumns = TableDataFilter.GetVisibleColumns(i_Table.Columns).ToList();

    //     visibleColumns = visibleColumns
    //         .Where(c => !query.selectClause.Columns.Contains(c))
    //         .ToList();

    //     ClearSelectionPanel();

    //     uiRenderer.populateSelectionButtons
    //     (
    //           i_Items: visibleColumns
    //         , i_OnItemDropped: col => query.AddColumn(col)
    //         , i_GetLabel: column => column.Name
    //         , i_ParentTransform: selectionParent
    //         , i_AssignedSection: col => selectSection
    //         , i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

    //         , i_RemovalCondition: column => query.fromClause.table == null ||
    //                                       !query.selectClause.isClicked

    //         , i_OnItemRemoved: col => query.RemoveColumn(col)
    //     );
    // }

    // private void PopulateConditionColumnSelection()
    // {
    //     if (query.fromClause.table != null)
    //     {
    //         uiRenderer.populateSelectionButtons
    //         (
    //              i_Items: query.fromClause.table.Columns
    //             , i_OnItemDropped: OnConditionColumnSelected
    //             , i_GetLabel: column => column.Name
    //             , i_ParentTransform: selectionParent
    //             , i_AssignedSection: col => whereSection
    //             // ,i_ButtonPool: uiRenderer.selectionButtonPool
    //             ,i_ButtonPrefab: uiRenderer.selectionButtonPrefab.GetComponent<Button>()

    //             , i_RemovalCondition: column => query.fromClause.table == null ||
    //                                           !query.whereClause.isClicked ||
    //                                           query.selectClause.IsEmpty()
    //             , i_OnItemRemoved: col =>
    //             {
    //                 query.whereClause.Conditions.Clear();

    //                 query.queryState.CurrentState = eQueryState.SelectingConditions;
    //                 ShowNextSelectionOptions();
    //             }
    //         );
    //     }
    // }




}
