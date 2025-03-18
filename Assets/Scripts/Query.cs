using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using Unity.VisualScripting;

public enum eQueryState
{
    None, 
    SelectingTable,
    SelectingColumns,
    SelectingConditions,
}

public class Query
{
    private string m_QueryString;

    public SelectClause selectClause; 
    public FromClause fromClause;
    public WhereClause whereClause;
    public List<IQueryClause> clauses;
    public List<IQueryClause> availableClauses;
    public event Action OnAvailableClausesChanged;
    public eQueryState currentState { get; set; } = eQueryState.None;

    public List<Dictionary<string, string>> Results { get; set; } 
    public bool IsValid => fromClause.table != null && selectClause.NotEmpty();
    public event Action OnQueryUpdated;  


private Dictionary<IQueryClause, Button> clauseButtons = new Dictionary<IQueryClause, Button>();
private Dictionary<Column, Button> selectionButtons = new Dictionary<Column, Button>();

    public Query()
    {
        selectClause = new SelectClause();
        fromClause   = new FromClause();
        whereClause  = new WhereClause();
        clauses = new List<IQueryClause> { selectClause, fromClause, whereClause };
        availableClauses = new List<IQueryClause> { selectClause, fromClause };

        Results = new List<Dictionary<string, string>>();
    }    

    public string QueryString
    {
        get { return m_QueryString ;}
        
        set
        {
            m_QueryString = value;
            OnQueryUpdated?.Invoke();
        }
    }

    public void UpdateQueryState()
    {
        currentState = eQueryState.None;

        if (!fromClause.isClicked)
        {
            return;
        }

        if (fromClause.table == null)
        {
            currentState = eQueryState.SelectingTable;
            return;
        }

        if (!selectClause.isClicked)
        {
            currentState = eQueryState.SelectingTable;
            return;
        }

        currentState = eQueryState.SelectingColumns;

        if (whereClause.isClicked)
        {
            currentState = eQueryState.SelectingConditions;
        }
    }


    public void UpdateQueryState1()
    {
        if (!fromClause.isClicked)
        {
            currentState = eQueryState.None;
            selectClause.Reset();
            fromClause.Reset();
            whereClause.Reset();
            
            CheckAvailableClause(); // Ensure UI updates
            updateQueryString();

            return;
        }

        if (fromClause.table == null)
        {
            currentState = eQueryState.SelectingTable;
            selectClause.Reset();
            whereClause.Reset();

            CheckAvailableClause(); // Ensure UI updates
            updateQueryString();

            return;
        }

        if (!selectClause.isClicked)
        {
            currentState = eQueryState.SelectingTable;
            whereClause.Reset();

            CheckAvailableClause(); // Ensure UI updates
            updateQueryString();

            return;
        }

        currentState = eQueryState.SelectingColumns;

        if (selectClause.NotEmpty())
        {
            whereClause.isAvailable = true;
        }
        else
        {
            whereClause.Reset();
        }

        if(whereClause.isClicked)
        {
            currentState = eQueryState.SelectingConditions;
        }

        Debug.Log($"[UpdateQueryState] current state: {currentState}");
        CheckAvailableClause(); // Ensure UI updates
        updateQueryString();
    }


    public void CheckAvailableClause()
    {
        availableClauses.Clear();

        foreach (IQueryClause clause in clauses)
        {
            if (clause.isAvailable)
            {
                availableClauses.Add(clause);
            }
        }

        OnAvailableClausesChanged?.Invoke();
    }

    public void ToggleClause(IQueryClause clause)
    {
        if (clause != null)
        {
            clause.Toggle();
            updateQueryString();
        }
    }

    public void SetTable(Table i_Table)
    {  
        fromClause.SetTable(i_Table);
        NotifyClauses();
        updateQueryString();
    }

    public void AddColumn(Column i_ColumnToAdd)
    {
        selectClause.AddColumn(i_ColumnToAdd);
        NotifyClauses();
        updateQueryString();
    }

    public void RemoveColumn(Column i_ColumnToRemove)
    {
        selectClause.RemoveColumn(i_ColumnToRemove);
        NotifyClauses();
        UpdateQueryState();
        updateQueryString();
    }

    public void CreateNewCondition(Column i_Column)
    {
        whereClause.CreateNewCondition(i_Column);
        NotifyClauses();
        UpdateQueryState();
        updateQueryString();
    }

    public void SetConditionOperator(IOperatorStrategy i_Operator)
    {
        whereClause.newCondition.Operator = i_Operator;
        NotifyClauses();
        updateQueryString();
    }

    public void SetConditionValue(object i_Value)
    {
        whereClause.newCondition.Value = i_Value;
        AddCondition();
        updateQueryString();
    }

    public void AddCondition()
    {
        whereClause.AddCondition();
        NotifyClauses();
        updateQueryString();
    }


    public void ClearColumns()
    {
        if (selectClause.Columns.Count > 0)
        {
            selectClause.ClearColumns();
        }
        // NotifyClauses();
        updateQueryString();
    }


    private void updateQueryString()
    {
        Debug.Log($"QUERY STRING IS: {QueryString}");
        QueryString = selectClause.ToSQL() + "\n" + fromClause.ToSQL() + "\n" + whereClause.ToSQL();
    }

    internal string GetSelectFields()
    {
        return selectClause.ToSupabase();
    }

    public Table GetTable()
    {
        return fromClause.GetTable();
    }

    public void NotifyClauses()
    {
        foreach (var clause in clauses)
        {
            Debug.Log($"[NotifyClauses] Updating: {clause.DisplayName}");
            clause.OnQueryUpdated(this);
        }
        
    }

}
