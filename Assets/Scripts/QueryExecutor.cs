using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class QueryExecutor : MonoBehaviour
{
    public delegate void QueryResultHandler(JArray jsonResponse);
    public event QueryResultHandler OnQueryExecuted;

    public void Execute(Query query)
    {
        if (query == null)
        {
            Debug.LogError("Query is NULL, cannot execute.");
            return;
        }

        StartCoroutine(RunQuery(query));
    }

    private IEnumerator RunQuery(Query query)
    {
        Debug.Log("Starting SQL Query Execution...");

        if (query.fromClause.table == null || string.IsNullOrEmpty(query.fromClause.table.Name))
        {
            Debug.LogError("No table name found in the query. Query.table is NULL or has no name.");
            yield break;
        }

if (query.fromClause.table.Name.ToLower() == "persons")
{
    if (!query.selectClause.Columns.Any(c => c.Name == "person_id"))
        query.selectClause.Columns.Insert(0, new Column("person_id", eDataType.String));
}


        //https://vwudsbcqlhwajpkmcpsz.supabase.co/rest/v1/Persons?select=age&age=eq.40

        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/{query.fromClause.table.Name}?select={query.GetSelectFields()}&{query.whereClause.ToSupabase()}";

        // if (!string.IsNullOrEmpty(query.WherePartSupaBase))
        // {
        //     url += $"&{query.WherePartSupaBase}";  // Append filters as URL params
        // }

        UnityWebRequest request = SupabaseUtility.CreateGetRequest(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"Query Success: {responseText}");

            JArray jsonResponse = JArray.Parse(responseText);


            if (jsonResponse.Count == 0)
            {
                Debug.LogWarning("⚠️ Supabase returned NO DATA!");
            }

            OnQueryExecuted?.Invoke(jsonResponse);
        }
        else
        {
            Debug.LogError($"Failed to execute query: {request.error} | Response: {request.downloadHandler.text}");
        }
    }
}
