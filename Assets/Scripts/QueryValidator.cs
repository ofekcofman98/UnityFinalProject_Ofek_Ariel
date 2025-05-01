using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QueryValidator : MonoBehaviour
{
    public bool ValidateQuery(Query i_Query, MissionData i_MissionData)
    {
        bool res = true;

        if (i_Query.fromClause.table.Name != i_MissionData.requiredTable)
        {
            Debug.Log("[QueryValidator]: incorrect table!");
            
            return false;
        }

        foreach (string col in i_MissionData.requiredColumns)
        {
            bool isContained = i_Query.selectClause.Columns.Any(c => c.Name == col);
            if (!isContained)
            {
                return false;
            }
        }


        return res;
    }
}
