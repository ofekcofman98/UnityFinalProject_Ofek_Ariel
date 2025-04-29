using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public bool SqlMode {get; set;}
    [SerializeField] private GameObject pcCanvas;
    [SerializeField] private GameObject mobileCanvas;


    public Query CurrentQuery {get; set;}

    [SerializeField] private QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private TableDisplayer tableDisplayer;
    [SerializeField] private SchemeDisplayer schemeDisplayer;


    [SerializeField] private QuerySender querySender;
    [SerializeField] private QueryReceiver queryReceiver;
    // [SerializeField] private CanvasSwitcher canvasSwitcher;

    protected override void Awake()
    {

    // DontDestroyOnLoad(this.gameObject);
    // SceneManager.sceneLoaded += OnSceneLoaded;
        base.Awake(); 
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
        if (pcCanvas != null)
        {
            pcCanvas.SetActive(true);
        }
        if (mobileCanvas != null)
        {
            mobileCanvas.SetActive(false);
        }

        // string currentScene = SceneManager.GetActiveScene().name;
        // if (currentScene == "MobileClientScene")
        // {
        //     InitMobileScene();
        // }
        // else if (currentScene == "MainScene")
        // {
        //     InitMainScene();
        // }

    }

    internal void SetSqlMode(bool i_IsSqlMode)
    {
        SqlMode = i_IsSqlMode;

        if (pcCanvas != null)
        {
            pcCanvas.SetActive(!i_IsSqlMode);
        }

        if (mobileCanvas != null)
        {
            mobileCanvas.SetActive(i_IsSqlMode);
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


private void SetupSplitScreen()
{
    Camera[] allCameras = FindObjectsOfType<Camera>();

    if (allCameras.Length < 2)
    {
        Debug.LogError("❌ Expected at least two cameras for split screen setup!");
        return;
    }

    // Example: assume first camera is MainScene camera, second camera is MobileClientScene camera
    Camera mainCamera = allCameras[0];
    Camera mobileCamera = allCameras[1];

    // Configure main camera to left half
    mainCamera.rect = new Rect(0f, 0f, 0.5f, 1f);

    // Configure mobile camera to right half
    mobileCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);

    Debug.Log("✅ Split screen setup done!");
}

}
