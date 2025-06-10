using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

public enum ValidationMode
{
    ExactMatch,         // All rows and all columns must match exactly
    ContainsRow,        // Just check that one expected row exists
    RowCount,           // Validate number of rows (e.g., SELECT COUNT)
    ColumnValuesOnly,   // Match values but not necessarily structure
    Custom              // Let you write your own later
}

public class QueryValidator : MonoBehaviour
{
    public bool ValidateQuery(Query i_Query, JArray i_Result ,SQLMissionData i_MissionData)
    {
    if (i_Query == null || i_Query.fromClause == null || i_Query.fromClause.table == null)
    {
        Debug.LogError("❌ Invalid Query: fromClause or table is null.");
        return false;
    }

    if (i_Query.fromClause.table.Name != i_MissionData.requiredTable)
    {
        Debug.Log("[QueryValidator]: incorrect table!");
        return false;
    }

    if (i_Query.selectClause == null || i_Query.selectClause.Columns == null)
    {
        Debug.LogError("❌ Invalid Query: selectClause or Columns is null.");
        return false;
    }

        foreach (string col in i_MissionData.requiredColumns)
        {
            bool isContained = i_Query.selectClause.Columns.Any(c => c.Name == col);
            if (!isContained)
            {
                Debug.LogWarning($"[QueryValidator]: Missing required column: {col}");
                return false;
            }
        }

        return true;
    }
}
