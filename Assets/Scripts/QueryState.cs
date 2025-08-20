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
            // Debug.Log($"[QueryState] State updated to: {CurrentState}");
            return;
        }

        if (i_Query.fromClause.table == null)
        {
            // Debug.Log("i_Query.fromClause.table == null");
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
            CurrentState = eQueryState.SelectingColumns;
            return;
        }

        if (i_Query.whereClause.isClicked)
        {
            // if (!i_Query.whereClause.CompletedCondition())  // where is clicked but didnt finish condition yet
if (i_Query.whereClause.HasActiveEditingCondition() ||
        !i_Query.whereClause.IsValid())
            {
                CurrentState = eQueryState.SelectingConditions;
            }
            else                                            // where is clicked and condition is finished
            {
                CurrentState = eQueryState.None;
            }
        }
        else
        {
            CurrentState = eQueryState.SelectingColumns;
        }

        if (i_Query.andClause.isClicked)
        {
            CurrentState = eQueryState.SelectingConditions;
        }
        else
        {
            if (i_Query.andClause.isAvailable)
            {
                CurrentState = eQueryState.None;
            }
        }

        Debug.Log($"[QueryState] State updated to: {CurrentState}");
    }
    
}
