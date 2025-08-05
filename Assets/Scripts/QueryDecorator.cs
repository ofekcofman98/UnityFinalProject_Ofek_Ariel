using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class QueryDecorator
{
    public static void Enrich(Query query)
    {
        string tableName = query.fromClause.table.Name;

        if (RequiresCaseId(tableName))
        {
            InsertColumnToQuery(query, "case_id", eDataType.String);
        }

        if (query.fromClause.table.Name.ToLower() == "persons")
        {
            InsertColumnToQuery(query, "person_id", eDataType.String);
        }

    }

    private static void InsertColumnToQuery(Query query, string columnName, eDataType dataType, bool isFirst = true)
    {
        if (!query.selectClause.Columns.Any(c => c.Name == columnName))
        {
            if (isFirst)
            {
                query.selectClause.Columns.Insert(0, new Column(columnName, dataType));
            }
            else
            { 
                query.selectClause.Columns.Add(new Column(columnName, dataType));
            }
        }
    }

    private static bool RequiresCaseId(string tableName)
    {
        return tableName == "Witnesses" || tableName == "CrimeEvidence";
    }

}
