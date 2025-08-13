using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FromClause : IQueryClause
{
    public Table table { get; set; }
    public string FromPart { get; set; } = QueryConstants.Empty;
    public string DisplayName => QueryConstants.From;
    public bool isClicked { get; private set; } = false;
    public bool isAvailable { get; set; } = true;

private Action _onRemoved;

    public void Activate()
    {
        isClicked = true;
    }

    public void Deactivate()
    {
        table = null;
        isClicked = false;
        _onRemoved?.Invoke();
    }

    public void SetTable(Table i_Table)
    {
        if (table == i_Table)
        {
            table = null;
        }
        else
        {
            table = i_Table;
        }
        UpdateString();
    }

    public void RemoveTable(bool i_IsSelectClicked = false)
    {
        table = null;
        // FromPart = QueryConstants.Empty;
        UpdateString();
    } 

    public void UpdateString()
    {
        if (isClicked)
        {
            FromPart = QueryConstants.From;
            if (table != null)
            {
                FromPart += " " + table.Name;
            }
            // Debug.Log($"from part is: {FromPart}");
        }
        else
        {
            FromPart = QueryConstants.Empty;
        }
    }

    public string ToSQL()
    {
        return FromPart;
    }

    public string ToSupabase()
    {
        return table.Name != null ? $"from={table.Name}" : QueryConstants.Empty;
    }

    public bool IsValid()
    {
        return table != null;
    }

    public bool CheckAvailableClause(Query query)
    {
        return isAvailable;
    } 

    public Table GetTable()
    {
        return table;
    }

    public void SetOnRemovedCallback(Action callback)
    {
        _onRemoved = callback;
    }


    public void Reset()
    {
        // table = null;
        // isClicked = false;
    }
}
