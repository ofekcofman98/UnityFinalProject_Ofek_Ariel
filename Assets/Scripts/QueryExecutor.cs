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
        // Debug.Log("Starting SQL Query Execution...");

        if (query.fromClause.table == null || string.IsNullOrEmpty(query.fromClause.table.Name))
        {
            Debug.LogError("No table name found in the query. Query.table is NULL or has no name.");
            yield break;
        }

        QueryDecorator.Enrich(query);

        //https://vwudsbcqlhwajpkmcpsz.supabase.co/rest/v1/Persons?select=age&age=eq.40

        // string url = $"{ServerData.k_SupabaseUrl}/rest/v1/{query.fromClause.table.Name}?select={query.GetSelectFields()}&{query.whereClause.ToSupabase()}";

        string url = $"{ServerData.k_SupabaseUrl}/rest/v1/{query.fromClause.table.Name}?select={query.GetSelectFields()}";

        string filters = query.whereClause.ToSupabase();
        if (!string.IsNullOrWhiteSpace(filters))
        { 
            url += $"&{filters}";
        }
            
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
