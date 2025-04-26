using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public bool SqlMode {get; set;}
    public Query CurrentQuery {get; set;}

    private QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private TableDisplayer tableDisplayer;
    [SerializeField] private SchemeDisplayer schemeDisplayer;

    [SerializeField] private bool simulateMobileInEditor = false;

    [SerializeField] private QuerySender querySender;
    [SerializeField] private QueryReceiver queryReceiver;
    // [SerializeField] private CanvasSwitcher canvasSwitcher;

    void Awake()
    {

    DontDestroyOnLoad(this.gameObject);
    SceneManager.sceneLoaded += OnSceneLoaded;

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
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MobileClientScene")
        {
            InitMobileScene();
        }
        else if (currentScene == "MainScene")
        {
            InitMainScene();
        }

    }

    private void InitMainScene()
    {
        SqlMode = false;

        if (queryExecutor == null)
        {
            queryExecutor = FindObjectOfType<QueryExecutor>();
            if (queryExecutor != null)
                queryExecutor.OnQueryExecuted += HandleQueryResults;
        }

        if (queryReceiver == null)
            queryReceiver = FindObjectOfType<QueryReceiver>();

        // if (canvasSwitcher != null)
        //     canvasSwitcher.ShowSqlModeCanvas(false);

    }

    private void InitMobileScene()
    {
        SqlMode = true;

        CurrentQuery ??= new Query();

        queryBuilder = FindObjectOfType<QueryBuilder>();
        // if (queryBuilder != null)
        // {
        //     queryBuilder.BuildQuery();
        // }
    }

    internal void SetSqlMode(bool i_Visible = true)
    {
        SqlMode = i_Visible;

        if (i_Visible)
        {
            CurrentQuery ??= new Query();
            if (SceneManager.GetActiveScene().name != SceneNames.k_MobileClientScene)
                SceneManager.LoadScene(SceneNames.k_MobileClientScene);
        }
        else
        {
            if (SceneManager.GetActiveScene().name != SceneNames.k_MainScene)
                SceneManager.LoadScene(SceneNames.k_MainScene);
        }

//             // If null, auto-detect based on platform
//     if (i_Visible == null)
//     {
// #if UNITY_EDITOR
//         // Simulate mobile manually (e.g., from a toggle in the Inspector)
//         i_Visible = simulateMobileInEditor;
// #else
//         i_Visible = Application.isMobilePlatform;
// #endif
//     }

//     SqlMode = i_Visible;

//     if (CurrentQuery == null)
//     {
//         CurrentQuery = new Query();
//     }

//     queryBuilder.BuildQuery();
//     canvasSwitcher.ShowSqlModeCanvas(SqlMode);
}

    
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "MobileClientScene")
    {
        InitMobileScene();
    }
    else if (scene.name == "MainScene")
    {
        InitMainScene();
    }
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
    }


}
