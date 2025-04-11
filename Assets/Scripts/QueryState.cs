using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eQueryState
{
    None, 
    SelectingTable,
    SelectingColumns,
    SelectingConditions,
}

public class QueryState
{
    public eQueryState CurrentState { get; set; } = eQueryState.None;

    public void UpdateState(Query i_Query)
    {
        if (!i_Query.fromClause.isClicked)
        {
            CurrentState = eQueryState.None;
    Debug.Log($"[QueryState] State updated to: {CurrentState}");
            return;
        }
        
        if (i_Query.fromClause.table == null)
        {
            Debug.Log("i_Query.fromClause.table == null");
            CurrentState = eQueryState.SelectingTable;
            return;
        }

        if (!i_Query.selectClause.isClicked)
        {
            CurrentState = eQueryState.SelectingTable;
            return;
        }

        if (i_Query.selectClause.IsEmpty())
        {
            Debug.Log("!i_Query.selectClause.NotEmpty()");

            // i_Query.whereClause.Reset();
            CurrentState = eQueryState.SelectingColumns;
            return;
        }

        if (i_Query.whereClause.isClicked)
        {
            CurrentState = eQueryState.SelectingConditions;
        }
        else
        {
            CurrentState = eQueryState.SelectingColumns;
        }

        Debug.Log($"[QueryState] State updated to: {CurrentState}");
    }
}
