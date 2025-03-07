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
    public bool IsValid => fromClause.table != null && selectClause.Columns.Count > 0;
    public event Action OnQueryUpdated;  

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
        if (!fromClause.isClicked)
        {
            currentState = eQueryState.None;
        }
        else  // FROM
        {            
            if (!selectClause.isClicked)
            {
                currentState = eQueryState.SelectingTable;
            }
            else  // SELECT 
            {
                if (fromClause.table != null)  // table is selected
                {
                    currentState = eQueryState.SelectingColumns;
                }
                else
                {
                    currentState = eQueryState.SelectingTable;
                }
            }
        }

        if (whereClause.isClicked)
        {
            currentState = eQueryState.SelectingConditions;
        }

        Debug.Log($"current state is {currentState}");
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
        updateQueryString();
    }

    public void CreateNewCondition(Column i_Column)
    {
        whereClause.CreateNewCondition(i_Column);
        NotifyClauses();
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

    private void NotifyClauses()
    {
        foreach (var clause in clauses)
        {
            Debug.Log($"[NotifyClauses] Updating: {clause.DisplayName}");
            clause.OnQueryUpdated(this);
        }
    }

}
