using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectClause : IQueryClause
{
    public string DisplayName => QueryConstants.Select;
    [JsonProperty] public List<Column> Columns { get; set; }
    [JsonProperty] public string SelectPart { get; set; } = QueryConstants.Empty;
    [JsonProperty] public bool isClicked { get; set; } = false;
    [JsonProperty] public bool isAvailable { get; set; } = true;

    public SelectClause()
    {
        Columns = new List<Column>();
    }

    public void Activate()
    {
        isClicked = true;
    }

    public void Deactivate()
    {
        if (isClicked)
        {
            Reset();
        }
        else
        {
            isClicked = false;
        }
    }

    public void AddColumn(Column i_ColumnToAdd)
    {
        if (!Columns.Contains(i_ColumnToAdd))
        {
            Columns.Add(i_ColumnToAdd);
            UpdateString();
        }
    }
    
    public void RemoveColumn(Column i_ColumnToRemove)
    {
        if (Columns.Remove(i_ColumnToRemove))
        {
            // Columns.Remove(i_ColumnToRemove);
            
            UpdateString();
        }
    }

    public void ClearColumns()
    {
        Columns.Clear();
        UpdateString();
    }

    public void UpdateString()
    {
        if (isClicked)
        {
            SelectPart = QueryConstants.Select;
            if (Columns.Count > 0)
            {
                SelectPart += " " + string.Join(QueryConstants.Comma, Columns.Select(col => col.Name));
            }
        }
        else
        {
            SelectPart = QueryConstants.Empty;
        }
    }

    public string ToSQL()
    {
        return SelectPart;
    }

    public string ToSupabase()
    {
        return Columns.Count > 0 ? string.Join(QueryConstants.Comma, Columns.Select(col => col.Name)) : "*";
    }

    public bool IsEmpty()
    {
        return Columns.Count == 0;
    }

    public bool IsValid()
    {
        return !IsEmpty();
    }

    public bool CheckAvailableClause(Query query)
    {
        return isAvailable;
    } 

    public void Reset()
    {
        isClicked = false; // CHECK IT 
        SelectPart = QueryConstants.Select;
        ClearColumns();
    }
}
