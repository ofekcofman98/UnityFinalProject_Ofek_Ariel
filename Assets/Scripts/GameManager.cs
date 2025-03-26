using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class GameManager : Singleton<GameManager>
{
    public bool SqlMode {get; set;}
    public Query CurrentQuery {get; private set;}
    [SerializeField] internal QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private TableDisplayer tableDisplayer;

    [SerializeField] private QuerySender querySender;
    [SerializeField] private QueryReceiver queryReceiver;

    void Awake()
    {
        if (querySender == null)
        {
            Debug.LogWarning("QuerySender is not assigned in the Inspector! Trying to find it...");
            querySender = FindObjectOfType<QuerySender>();
        }

        if (queryExecutor == null)
        {
            Debug.LogError("QueryExecutor is not assigned in the Inspector.");
        }
        else
        {
            queryExecutor.OnQueryExecuted += HandleQueryResults;
        }


    }
    void Start()
    {
        SqlMode = false;
    }

    internal void SetSqlMode(bool i_Visible = true)
    {
        if (CurrentQuery == null)
        {
            CurrentQuery = new Query();
        }
        queryBuilder.BuildQuery();

    }

    public void SaveQuery(Query i_Query)
    {

        if (i_Query == null)
        {
            Debug.LogError("SaveQuery() was called with NULL!");
            return;
        }

        if (i_Query.selectClause.Columns.Count == 0 && CurrentQuery?.selectClause.Columns.Count > 0)
        {
            i_Query.selectClause.Columns = new List<Column>(CurrentQuery.selectClause.Columns);
        }

        // if (i_Query.SelectedColumns.Count == 0 && CurrentQuery?.SelectedColumns.Count > 0)
        // {
        //     i_Query.SelectedColumns = new List<string>(CurrentQuery.SelectedColumns);
        // }

        CurrentQuery = i_Query;
        Debug.Log("Query saved in GameManager: " + i_Query.QueryString);

        if (queryReceiver != null)
        {
            queryReceiver.StopListening();
        }

    }

    public void ExecuteQuery()
    {
        if (queryExecutor == null)
        {
            Debug.LogError("QueryExecutor is missing!");
            return;
        }
        
        if (querySender != null)
        {
            Debug.Log("Sending Query: " + CurrentQuery.QueryString);
            querySender.SendQueryToServer(CurrentQuery);
        }
        else
        {
            Debug.LogWarning("QuerySender is missing, skipping sending.");
        }

        if (queryReceiver != null)
        {
            queryReceiver.StartListening();
        }
        else
        {
            Debug.LogWarning("QueryReceiver is missing, skipping listening.");
        }


        queryExecutor.Execute(CurrentQuery);
    }

    private void HandleQueryResults(JArray jsonResponse)
    {

    Debug.Log($"📥 GameManager received {jsonResponse.Count} rows!");

    if (CurrentQuery == null)
    {
        Debug.LogError("🚨 CurrentQuery is NULL!");
        return;
    }

    if (CurrentQuery.selectClause.Columns.Count == 0)
    {
        Debug.LogError("🚨 CurrentQuery.Columns is EMPTY!");
        return;
    }
    Debug.Log($"📌 Query Columns: {string.Join(", ", CurrentQuery.selectClause.Columns.Select(col => col.Name))}");

    if (tableDisplayer != null)
    {
        tableDisplayer.DisplayResults1(jsonResponse, CurrentQuery.selectClause.Columns);
    }
    else
    {
        Debug.LogError("❌ TableDisplayer is missing!");
    }


    // if (CurrentQuery.SelectedColumns.Count == 0)
    // {
    //     Debug.LogError("🚨 CurrentQuery.SelectedColumns is EMPTY!");
    //     return;
    // }

    // Debug.Log($"📌 Query Columns: {string.Join(", ", CurrentQuery.SelectedColumns)}");

    // if (tableDisplayer != null)
    // {
    //     tableDisplayer.DisplayResults(jsonResponse, CurrentQuery.SelectedColumns);
    // }
    // else
    // {
    //     Debug.LogError("❌ TableDisplayer is missing!");
    // }

        // Debug.Log($"GameManager received {jsonResponse.Count} rows from QueryExecutor.");
        
        // if (tableDisplayer != null)
        // {
        //     tableDisplayer.DisplayResults(jsonResponse, CurrentQuery.SelectedColumns);
        // }
        // else
        // {
        //     Debug.LogError("TableDisplayer is missing from GameManager.");
        // }
    }


}
