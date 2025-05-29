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
    [SerializeField] private GameObject pcGameCanvas;
    [SerializeField] private GameObject pcQueryCanvas;
    [SerializeField] private GameObject mobileCanvas;


    public Query CurrentQuery {get; set;}

    [SerializeField] public QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private TableDisplayer tableDisplayer;
    [SerializeField] public SchemeDisplayer schemeDisplayer;


    [SerializeField] private QuerySender querySender;
    [SerializeField] private QueryReceiver queryReceiver;
    // [SerializeField] private CanvasSwitcher canvasSwitcher;

    [SerializeField] private QueryValidator queryValidator;
    [SerializeField] public MissionsManager missionManager;
    [SerializeField] public QueryUIManager queryUIManager;


    public event Action<bool> OnQueryIsCorrect;


    private bool isQueryUIVisible = false;


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
            queryExecutor.OnQueryExecuted += result =>
            {
                if (missionManager != null)
                {
                    missionManager.ValidateMission(CurrentQuery, result, queryValidator);
                }
            };
            missionManager.OnMissionValidated += isCorrect => 
            {
                OnQueryIsCorrect?.Invoke(isCorrect);
            };

        }
    }

    void Start()
    {
        queryUIManager.Init(missionManager);

        bool isMobile = Application.isMobilePlatform;

        // Show the correct canvas
        if (pcQueryCanvas != null)
            pcQueryCanvas.SetActive(!isMobile);

        if (mobileCanvas != null)
            mobileCanvas.SetActive(isMobile);

        // Platform-specific logic
        if (!isMobile)
        {
            // PC: Start polling the server for queries
            if (queryReceiver != null)
            {
                Debug.Log("🖥 PC detected — starting QueryReceiver to listen for mobile queries.");
                queryReceiver.StartListening();
            }
            else
            {
                Debug.LogWarning("⚠️ QueryReceiver is null — cannot listen for queries.");
            }
        }
        else
        {
            Debug.Log("📱 Mobile detected — not starting listener (mobile only sends queries).");
        }
            SupabaseManager.Instance.OnSchemeFullyLoaded += () => UnlockInitialTables();
    }


    private void startGame()
    {

    }


    public void SetSqlMode()
    {
        SqlMode = !SqlMode;

        if (pcGameCanvas != null)
        {
            pcGameCanvas.SetActive(!SqlMode);
        }

        if (pcQueryCanvas != null)
        {
            pcQueryCanvas.SetActive(SqlMode);
        }


        if (mobileCanvas != null)
        {
            mobileCanvas.SetActive(SqlMode);
        }

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = !SqlMode;
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
            Debug.Log("📤 Sending query to server: " + CurrentQuery.QueryString);
            querySender.SendQueryToServer(CurrentQuery);
        }

        if (queryReceiver != null)
        {
            Debug.Log("🎧 Preparing to listen for the next query...");
            queryReceiver.StartListening();  // this triggers polling
        }
    }

    public void ExecuteLocally(Query i_Query)
    {
        if (queryExecutor == null)
        {
            Debug.LogError("QueryExecutor is missing!");
            return;
        }

        Debug.Log("🧠 Executing received query locally: " + CurrentQuery?.QueryString);
        queryExecutor.Execute(i_Query);
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
            Debug.Log($"🖥 pcCanvas active? {pcQueryCanvas?.activeSelf}");
            Debug.Log($"📊 tableDisplayer is null? {tableDisplayer == null}");
            Debug.Log($"🧪 Columns: {string.Join(", ", CurrentQuery.selectClause.Columns.Select(c => c.Name))}");
            tableDisplayer.DisplayResults1(jsonResponse, CurrentQuery.selectClause.Columns);
        }
        else
        {
            Debug.LogError("❌ TableDisplayer is missing!");
        }
    }

    private void UnlockInitialTables()
    {
        Table crimeEvidence = SupabaseManager.Instance.Tables
            .FirstOrDefault(t => t.Name == "CrimeEvidence");
        Debug.Log($"TABLE first column: {crimeEvidence.Columns[0]}");
        if (crimeEvidence != null)
        {
            crimeEvidence.UnlockTable();
            Debug.Log("🔓 'CrimeEvidence' table unlocked at game start.");
        }
        else
        {
            Debug.LogWarning("⚠️ 'CrimeEvidence' table not found.");
        }
    }

    internal void ToggleQueryUI()
    {
        isQueryUIVisible = !isQueryUIVisible;
        if (isQueryUIVisible)
        {
            if (pcGameCanvas != null)
            {
                pcGameCanvas.SetActive(!isQueryUIVisible);
            }

            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(isQueryUIVisible);
            }

        }
    }
}
