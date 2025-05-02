using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

    [SerializeField] private QueryValidator queryValidator;
    [SerializeField] private MissionSequence missionSequence;
    private int currentMissionIndex = 0;
    public MissionData currentMission => missionSequence.Missions[currentMissionIndex];
    public event Action<bool> OnQueryIsCorrect;

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
            queryExecutor.OnQueryExecuted += ValidateQuery;
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
    }

    private void startGame()
    {

    }

    private void goToNextMission()
    {
        if (currentMissionIndex < missionSequence.Missions.Count)
        {
            currentMissionIndex++;
        }
        else
        {
            Debug.Log("Game Over!");
        }
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

    private void ValidateQuery(JArray jsonResponse)
    {
        bool res = queryValidator.ValidateQuery(CurrentQuery, jsonResponse, missionSequence.Missions[currentMissionIndex]);

        if (res)
        {
            Debug.Log("Query is correct!");
            goToNextMission();
            OnQueryIsCorrect?.Invoke(true);
        }
        else
        {
            Debug.Log("Query is incorrect.");
            OnQueryIsCorrect?.Invoke(false);
        }
    }

}
