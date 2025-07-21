using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Scripts.ServerIntegration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public bool SqlMode {get; set;}
    [SerializeField] private GameObject pcGameCanvas;
    [SerializeField] private GameObject mobileCanvas;
    [SerializeField] private GameObject mobileScreensaverCanvas;


    public Query CurrentQuery {get; set;}

    [SerializeField] public QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private DataGridDisplayer tableDisplayer;
    [SerializeField] public SchemeDisplayer schemeDisplayer;


    [SerializeField] private QuerySender querySender;
    public QuerySender QuerySender => querySender;

    [SerializeField] private QueryListener queryReceiver;
    // [SerializeField] private CanvasSwitcher canvasSwitcher;

    [SerializeField] private QueryValidator queryValidator;
    [SerializeField] public MissionsManager missionManager; 
    [SerializeField] public MissionUIManager MissionUIManager;
    [SerializeField] public PlatformUIManager platformUIManager;
    [SerializeField] private ResultsUI resultsUI;


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
                    missionManager.ValidateSqlMission(CurrentQuery, result, queryValidator);
                }
            };
            missionManager.OnMissionValidated += isCorrect =>
            {
                OnQueryIsCorrect?.Invoke(isCorrect);
    if (isCorrect)
    {
        QuerySender.MarkQueryAsSent(); // ✅ only mark as sent if the query is correct
    }
            };

        }
    }

    void Start()
    {
        MissionUIManager.Init(missionManager);
        ResetSender.Instance.SendResetToPhone();

        if (!Application.isMobilePlatform)
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
            // GameStateReceiver.Instance.StartListening();
        }

        if (!Application.isMobilePlatform && mobileScreensaverCanvas != null)
        {
            mobileScreensaverCanvas.SetActive(false);
        }
    }

    private void startGame()
    {

    }

    public IEnumerator SetSqlMode()
    {
        bool newMode = !SqlMode; 

        if (Application.isMobilePlatform)
        {
            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(newMode);
                Debug.Log($"📱 mobileCanvas set to {newMode}");
            }

            if (mobileScreensaverCanvas != null)
            {
                mobileScreensaverCanvas.SetActive(!newMode);
                Debug.Log($"🌙 mobileScreensaverCanvas set to {!newMode}");
            }

            if (pcGameCanvas != null)
            {
                pcGameCanvas.SetActive(false); // PC canvas never shows on mobile
            }

            if (newMode && queryBuilder != null)
            {
                queryBuilder.ResetQuery();
                queryBuilder.BuildQuery();
            }
        }
        else 
        {
            // PC: always show pcGameCanvas, hide both mobile canvases
            if (pcGameCanvas != null)
            {
                pcGameCanvas.SetActive(true);
            }

            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(false);
            }

            if (mobileScreensaverCanvas != null)
            {
                mobileScreensaverCanvas.SetActive(false);
            }
        }

        SqlMode = newMode;

        // Enable/disable movement and camera on both platforms
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = !SqlMode;

        MouseLook mouseLook = FindObjectOfType<MouseLook>();
        if (mouseLook != null) mouseLook.enabled = !SqlMode;

        CharacterController characterController = FindObjectOfType<CharacterController>();
        if (characterController != null) characterController.enabled = !SqlMode;

        Debug.Log($"🎮 SQL Mode toggled to {SqlMode}");
        yield break;
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
        
        CurrentQuery = i_Query;       

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

            if (QuerySender.IsQuerySent)
            {
                Debug.LogWarning("🚨 Query already accepted. Blocking further sends.");
                return;
            }

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

        resultsUI.ShowResults(
            jsonResponse,
            CurrentQuery.selectClause.Columns,
            CurrentQuery.GetTable().Name);
    }

    // private void UnlockInitialTables()
    // {
    //     Table crimeEvidence = SupabaseManager.Instance.Tables
    //         .FirstOrDefault(t => t.Name == "CrimeEvidence");
    //     Debug.Log($"TABLE first column: {crimeEvidence.Columns[0]}");
    //     if (crimeEvidence != null)
    //     {
    //         crimeEvidence.UnlockTable();
    //         Debug.Log("🔓 'CrimeEvidence' table unlocked at game start.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("⚠️ 'CrimeEvidence' table not found.");
    //     }
    // }

    internal void ToggleQueryUI()
    {
        isQueryUIVisible = !isQueryUIVisible;
        if (isQueryUIVisible)
        {
            if (pcGameCanvas != null)
            {
                pcGameCanvas.SetActive(isQueryUIVisible);
            }

            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(!isQueryUIVisible);
            }

        }
    }

    internal void TeleportPlayerTo(Vector3 position)
    {
        Debug.Log("Teleport!");
    }

    internal IEnumerator ResetGame()
    {
        CurrentQuery = null;
        SqlMode = false;

        Debug.Log($"inside ResetGame, missionManager : {missionManager}");
        if (missionManager != null)
        {
            Debug.Log("Inside condition, before ResetMissions");
            yield return MissionsManager.Instance.ResetMissions();
        }

        if (MissionUIManager != null)
        {
            MissionUIManager.ShowUI();
        }

        if (queryBuilder != null)
        {
            queryBuilder.ResetQuery();
            queryBuilder.BuildQuery();
        }

        querySender?.ResetQuerySendFlag();

        yield return null;
    }

    internal IEnumerator resetAction()
    {
        Debug.Log("Inside ResetAction, before ResetGame");
        yield return ResetGame();
        Debug.Log("Inside ResetAction, after ResetGame");
        //MissionUIManager.ShowUI();

    }
}
