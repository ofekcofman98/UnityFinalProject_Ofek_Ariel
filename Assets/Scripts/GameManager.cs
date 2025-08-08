using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
    public ScreensaverController screensaverController { get; private set; }


    public Query CurrentQuery {get; set;}

    [SerializeField] public QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private DataGridDisplayer tableDisplayer;
    [SerializeField] public SchemeDisplayer schemeDisplayer;


    [SerializeField] private QuerySender querySender;
    public QuerySender QuerySender => querySender;

    [SerializeField] private QueryListener queryReceiver;

    [SerializeField] private QueryValidator queryValidator;
    [SerializeField] public MissionsManager missionManager; 
    [SerializeField] public MissionUIManager MissionUIManager;

    [SerializeField] private MissionSequence mainGameSequence;
    [SerializeField] private MissionSequence tutorialSequence;
    public int sequenceNumber;


    [SerializeField] private ResultsUI resultsUI;
    public string UniqueMobileKey { get; private set; }

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
        
        Debug.Log($"📦 mobileCanvas = {mobileCanvas}, screensaverCanvas = {mobileScreensaverCanvas}");

    }

    void Start()
    {
        MissionUIManager.Init(missionManager);    
        Application.targetFrameRate = 60;
        ShowMainMenu();

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
    screensaverController = new ScreensaverController(mobileCanvas, mobileScreensaverCanvas);
    screensaverController.ShowScreensaver();
            Debug.Log("📱 Mobile detected — not starting listener (mobile only sends queries).");
            // StateListener.Instance.StartListening();
        }

        // if (!Application.isMobilePlatform && mobileScreensaverCanvas != null)
        // {
        //     mobileScreensaverCanvas.SetActive(false);
        // }
    }


    public void StartGame()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 0f;
        MenuManager.Instance.ShowMainMenu();
    }

    public void StartGameFromMenu()
    {
        Time.timeScale = 1f;
        MenuManager.Instance.HideMainMenu(); // ✅ UI-only
        sequenceNumber = 1;
        MissionsManager.Instance.LoadMissionSequence(mainGameSequence); // dynamically chosen
        StartMissions();
        ResetSender.Instance.SendResetToPhone();
    }

    public void StartTutorial()
    {
        Time.timeScale = 1f;
        MenuManager.Instance.HideMainMenu();
        sequenceNumber = 0;
        MissionsManager.Instance.LoadMissionSequence(tutorialSequence); // dynamically chosen
        StartMissions();
        ResetSender.Instance.SendResetToPhone();
    }

    public void StartMissions()
    {
        MissionUIManager.ShowUI(); // This will handle popup or normal mission
    }

    // public void SwitchMobileCanvas(bool i_sqlMode)
    // {
    //     if (Application.isMobilePlatform)
    //     {
    //         if (mobileCanvas != null)
    //         {
    //             mobileCanvas.SetActive(i_sqlMode);
    //             Debug.Log($"📱 mobileCanvas set to {i_sqlMode}");
    //         }

    //         if (mobileScreensaverCanvas != null)
    //         {
    //             mobileScreensaverCanvas.SetActive(!i_sqlMode);
    //             Debug.Log($"🌙 mobileScreensaverCanvas set to {!i_sqlMode}");
    //         }

    //         if (pcGameCanvas != null)
    //         {
    //             pcGameCanvas.SetActive(false); // PC canvas never shows on mobile
    //         }

    //         if (i_sqlMode && queryBuilder != null)
    //         {
    //             queryBuilder.ResetQuery();
    //             queryBuilder.BuildQuery();
    //         }
    //     }

    // }

    public void SetSqlMode()
    {
        SqlMode = !SqlMode;

        // if (pcGameCanvas != null) pcGameCanvas.SetActive(!SqlMode);
        // if (mobileCanvas != null) mobileCanvas.SetActive(SqlMode);

        HandleMovement();

        Debug.Log($"🎮 SQL Mode toggled to {SqlMode}");


        if (Application.isMobilePlatform && SqlMode)
        {

            screensaverController?.HideScreensaver();

            queryBuilder.ResetQuery();
            queryBuilder.BuildQuery();
        }

    }


    private void HandleMovement()
    {
        // Disable/Enable movement
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = !SqlMode;

        // Disable/Enable camera look
        MouseLook mouseLook = FindObjectOfType<MouseLook>();
        if (mouseLook != null) mouseLook.enabled = !SqlMode;

        // Optional: CharacterController
        CharacterController characterController = FindObjectOfType<CharacterController>();
        if (characterController != null) characterController.enabled = !SqlMode;

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



    private async void HandleQueryResults(JArray jsonResponse)
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

        await PersonDataManager.Instance.WaitUntilReady();

        QueryResultDecorator.Enrich(jsonResponse, CurrentQuery.fromClause.table.Name, CurrentQuery.selectClause.Columns);

        resultsUI.ShowResults(
            jsonResponse,
            CurrentQuery.selectClause.Columns,
            CurrentQuery.GetTable().Name);
                  
 

    }


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

    public void ClearCurrentQuery()
    {
        if (queryBuilder != null)
        {
            queryBuilder.ResetQuery();
            queryBuilder.BuildQuery();
            Debug.Log("🧹 Query manually cleared by user.");
        }

        if (QuerySender != null)
        {
            QuerySender.ResetQuerySendFlag(); // Allow re-submission
        }
    }

    internal IEnumerator ResetGame()
    {
        CurrentQuery = null;
        SqlMode = false;

        Debug.Log($"inside ResetGame, missionManager : {missionManager}");
        if (missionManager != null)
        {
            Debug.Log("Inside condition, before ResetMissions");
            MissionsManager.Instance.LoadMissionSequence(sequenceNumber == 1 ? mainGameSequence : tutorialSequence);
            yield return MissionsManager.Instance.ResetMissions();
        }

        if (queryBuilder != null)
        {
            queryBuilder.ResetQuery();
        }

        querySender?.ResetQuerySendFlag();

        yield return null;
    }

    internal IEnumerator resetAction()
    {
        Debug.Log("Inside ResetAction, before ResetGame");
        yield return ResetGame();
        Debug.Log("Inside ResetAction, after ResetGame");
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        StartCoroutine(ResetSender.Instance.ResetServerOnDestroy());
    }
}
