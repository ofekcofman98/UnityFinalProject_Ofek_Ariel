using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;

public class Query
{
    private string m_QueryString;
    public string SelectPart {get; set;}
    public string FromPart {get; set;}
    public string WherePart {get; set;}
    public string WherePartSupaBase {get; set;}

    public Table table { get; set; }
    public List<Column> Columns { get; set; } 
    public List<Condition> Conditions { get; private set; }
    public Condition newCondition {get; set;}
    public List<Dictionary<string, string>> Results { get; set; } 
    public bool IsValid => table != null && Columns.Count > 0;
    public event Action OnQueryUpdated;  

    public Query()
    {
        SelectPart = QueryConstants.Empty;
        FromPart   = QueryConstants.Empty; 
        WherePart  = QueryConstants.Empty; 

        Columns = new List<Column>();
        Conditions = new List<Condition>();
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

    public void SetTable(Table i_Table)
    {
        if (table == i_Table)
        {
            table = null;
            FromPart = QueryConstants.From;
            Columns.Clear();
            SelectPart = QueryConstants.Select;
        }
        else
        {
            table = i_Table;
            FromPart = QueryConstants.From + table.Name + " ";
        }

        updateQueryString();
    }

    public void AddColumn(Column i_ColumnToAdd)
    {
        if (!Columns.Contains(i_ColumnToAdd))
        {
            Columns.Add(i_ColumnToAdd);
            SelectPart = QueryConstants.Select + string.Join(QueryConstants.Comma, Columns.Select(col => col.Name));
            updateQueryString();
        }
    }

    public void RemoveColumn(Column i_ColumnToRemove)
    {
        if (Columns.Remove(i_ColumnToRemove))
        {
            SelectPart = QueryConstants.Select + string.Join(QueryConstants.Comma, Columns.Select(col => col.Name));
            updateQueryString();
        }
    }

    public void CreateNewCondition(Column i_Column)
    {
        newCondition = new Condition();
        newCondition.OnConditionUpdated += UpdateWherePart; 

        newCondition.Column = i_Column;
    }

    public void AddCondition()
    {
        if (newCondition == null || newCondition.Value == null)
        {
            Debug.LogWarning("Cannot add condition: Condition or Value is null.");
            return;
        }

        Conditions.Add(newCondition);
        newCondition = null;
    }




    private void UpdateWherePart()
    {
        Debug.Log($"check condition: {newCondition.ColumnPart} {newCondition.OperatorPart}");
        
        Debug.Log($"check condition: {newCondition.ColumnPart} {newCondition.OperatorPart}");

        string temp, tempSupaBase;
        if (Conditions.Count == 0)
        {
            temp = newCondition.ConditionString;
            tempSupaBase = newCondition.ConditionStringSupaBase;
        }
        else
        {
            temp = string.Join(QueryConstants.And, Conditions.Select(cond => cond.ConditionString));
            tempSupaBase = string.Join(QueryConstants.And, Conditions.Select(cond => cond.ConditionStringSupaBase));
        }

        WherePart = QueryConstants.Where + temp;
        WherePartSupaBase = tempSupaBase;
        
        Debug.Log($"whereParT: {WherePart}");
        Debug.Log($"whereParTSupaBase: {WherePartSupaBase}");
        updateQueryString();
     }

    public void ClearTable(bool i_IsSelectClicked = false)
    {
        table = null;
        Columns.Clear();
        FromPart = QueryConstants.Empty;
        SelectPart = i_IsSelectClicked ? QueryConstants.Select : QueryConstants.Empty;
        updateQueryString();
    }

    public void ClearColumns()
    {
        SelectPart = QueryConstants.Empty;
        Columns.Clear();
        updateQueryString();
    }

    public void ActivateSelect()
    {
        SelectPart = QueryConstants.Select;
        updateQueryString();
    }

    private void updateQueryString()
    {
        Debug.Log($"QUERY STRING IS: {QueryString}");
        QueryString = SelectPart + FromPart + WherePart;
    }

    internal void ActivateFrom()
    {
        FromPart = QueryConstants.From;
        updateQueryString();
    }

    internal void ClearConditions()
    {
        WherePart = QueryConstants.Empty;
        Conditions.Clear();
        updateQueryString();
    }

    internal void ActivateWhere()
    {
        WherePart = QueryConstants.Where;
        updateQueryString();
    }

    internal string GetSelectFields()
    {
        return Columns.Count > 0 ? string.Join(",", Columns.Select(col => col.Name)) : "*";
    }
}
