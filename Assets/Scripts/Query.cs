using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using Unity.VisualScripting;
using Newtonsoft.Json;


public class Query
{
    [JsonProperty] private string m_QueryString;

    public SelectClause selectClause;
    public FromClause fromClause;
    public WhereClause whereClause;
    public AndClause andClause;

    [JsonIgnore] public List<IQueryClause> Clauses => new() { selectClause, fromClause, whereClause };
    [JsonIgnore] public List<IQueryClause> availableClauses;

    public QueryState queryState;
    public bool IsValid => Clauses.All(clause => clause.IsValid());

    // public event Action OnAvailableClausesChanged;
    public event Action OnQueryUpdated;

    private bool _isRecomputing = false;

    public Query()
    {
        selectClause = new SelectClause();
        fromClause = new FromClause();
        fromClause.SetOnRemovedCallback(() =>
        {
            selectClause.ClearColumns();
            fromClause.RemoveTable();
        });

        whereClause = new WhereClause();

        andClause = new AndClause(() =>
        {
            // Ensure WHERE is active and move UI state to pick another condition
            whereClause.Activate();
            whereClause.StartNewCondition();
            UpdateQueryState();
            // nothing to change in SQL string now; user will pick column -> operator -> value
        });

        availableClauses = new List<IQueryClause> { selectClause, fromClause };
        queryState = new QueryState();

        Recompute();
    }

    public string QueryString
    {
        get => m_QueryString;
        private set { m_QueryString = value; }
        // set
        // {
        //     m_QueryString = value;
        //     OnQueryUpdated?.Invoke();
        // }
    }

    public void Recompute(bool fireEvents = true)
    {
        if (_isRecomputing)
        {
            return;
        }
        _isRecomputing = true;

        try
        {
            CheckAvailableClause();
            UpdateClausesStrings();
            UpdateQueryState();
            updateQueryString();
        }
        finally
        {
            _isRecomputing = false;
        }

        if (fireEvents)
        {
            // OnAvailableClausesChanged?.Invoke();
            OnQueryUpdated?.Invoke();
        }
    }


    public void CheckAvailableClause()
    {
        availableClauses.Clear();

        foreach (IQueryClause clause in Clauses)
        {
            if (clause.CheckAvailableClause(this))
            {
                availableClauses.Add(clause);
            }
        }

        if (andClause.CheckAvailableClause(this)) availableClauses.Add(andClause);
    }

    public void UpdateQueryState()
    {
        queryState.UpdateState(this);
    }

    public void ToggleClause(IQueryClause clause, bool isToggledOn)
    {
        if (clause == null) return;

        if (isToggledOn)
        {
            clause.Activate();
        }
        else
        {
            clause.Deactivate();

            if (clause == andClause)
            {
                RemoveSecondCondition();
            }

        }

        Recompute();
    }

    public void SetTable(Table i_Table)
    {
        if (fromClause.table == null)
        {
            fromClause.SetTable(i_Table);
        }
        Recompute();
    }

    public void RemoveTable()
    {
        fromClause.RemoveTable();
        selectClause.ClearColumns();
        Recompute();
    }


    public void AddColumn(Column i_ColumnToAdd)
    {
        selectClause.AddColumn(i_ColumnToAdd);
        Recompute();
    }

    public void RemoveColumn(Column i_ColumnToRemove)
    {
        selectClause.RemoveColumn(i_ColumnToRemove);
        Recompute();
    }

    public void ClearColumns()
    {
        if (selectClause.Columns.Count > 0)
        {
            selectClause.ClearColumns();
        }
        Recompute();
    }

    public void AddCondition()
    {
        whereClause.AddCondition();
        Recompute();
    }


    public void CreateNewCondition(Column i_Column)
    {
        whereClause.CreateNewCondition(i_Column);
        Recompute();
    }

    // public void RemoveConditionColumn(Column i_Column)
    // {
    //     whereClause.RemoveConditionsByColumn(i_Column);
    //     Recompute();
    // }
    public void RemoveConditionByIndex(int conditionIndex)
    {
        whereClause.RemoveConditionByIndex(conditionIndex);
        Recompute();
    }


    public void SetConditionOperator(IOperatorStrategy i_Operator)
    {
        whereClause.SetOperator(i_Operator);
        Recompute();
    }

    public void RemoveSecondCondition()
    {
        whereClause.RemoveConditionByIndex(1);
        Recompute();
    }

    public void RemoveConditionOperatorByIndex(int index)
    {
        whereClause.RemoveOperatorByIndex(index);
        Recompute();
    }    

    public void SetConditionValue(object i_Value)
    {
        whereClause.SetValue(i_Value);
        Recompute();
    }


    public void RemoveConditionValueByIndex(int index)
    {
        whereClause.RemoveValueByIndex(index);
        Recompute();
    }

    public bool CompletedCondition()
    {
        return whereClause.CompletedCondition();
    }

    public void UpdateClausesStrings()
    {
        foreach (IQueryClause clause in Clauses)
        {
            clause.UpdateString();
        }
    }

    private void updateQueryString()
    {
        QueryString = string.Join("\n", Clauses.Select(c => c.ToSQL()));
        Debug.Log($"query is: {QueryString}");
    }

    internal string GetSelectFields()
    {
        return selectClause.ToSupabase();
    }

    public Table GetTable()
    {
        return fromClause.GetTable();
    }


    public void Reset()
    {
        foreach (IQueryClause clause in Clauses)
        {
            clause.Deactivate();
            clause.Reset();
        }

        Recompute();
    }

    public void PostDeserialize()
    {
        if (whereClause != null && whereClause.Conditions != null && whereClause.Conditions.Count > 0)
        {
            whereClause.Activate();              // ✅ So it shows in the panel
            whereClause.isAvailable = true;            // ✅ So it’s added to availableClauses
            availableClauses.Add(whereClause);         // ✅ Explicitly add it
        }

        foreach (var condition in whereClause?.Conditions ?? new List<Condition>())
        {
            condition.Refresh();
        }

        updateQueryString();
    }

    public int GetConditionIndexByColumn(Column col)
    {
        return whereClause.GetConditionIndexByColumn(col);
    }

}

