using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FromClause : IQueryClause
{
    public Table table { get; set; }
    public string FromPart { get; private set; } = QueryConstants.Empty;
    public string DisplayName => QueryConstants.From;
    public bool isClicked { get; private set; } = false;
    public bool isAvailable { get; set; } = true;

    // public void Toggle()
    // {
    //     isClicked = !isClicked;

    //     if (!isClicked)
    //     {
    //         table = null;
    //     }
    //     UpdateString();

    //     // OnFromChanged?.Invoke();
    // }
    public void Activate()
    {
        isClicked = true;
    }

    public void Deactivate()
    {
        table = null;
        isClicked = false;
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

    public void ClearTable(bool i_IsSelectClicked = false)
    {
        table = null;
        FromPart = QueryConstants.Empty;
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
            Debug.Log($"from part is: {FromPart}");
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

    public void OnQueryUpdated(Query query)
    {        
        // if (table == null)
        // {
        //     // FromPart = QueryConstants.Empty;
        //     query.ClearColumns();
        // }
    }

    public Table GetTable()
    {
        return table;
    }


    public List<object> GetOrderedElements()
    {
        List<object> elements = new List<object>();

        if (isClicked)
        {
            elements.Add(this); // FROM clause first
            if (table != null)
            {
                elements.Add(table); // Then the table name
            }
        }

        return elements;
    }
 

    public void Reset()
    {
        // table = null;
        // isClicked = false;
    }
}
