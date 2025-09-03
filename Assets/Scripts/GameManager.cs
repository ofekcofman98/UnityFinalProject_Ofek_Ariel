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

    [Header("Canvases")]
    [SerializeField] private GameObject pcGameCanvas;
    [SerializeField] private GameObject mobileCanvas;
    [SerializeField] private GameObject mobileScreensaverCanvas;
    public ScreensaverController screensaverController { get; private set; }


    [Header("System")]
    [SerializeField] public QueryBuilder queryBuilder;
    [SerializeField] private QueryExecutor queryExecutor;
    [SerializeField] private DataGridDisplayer tableDisplayer;
    [SerializeField] public SchemeDisplayer schemeDisplayer;
    [SerializeField] private QuerySender querySender;
    public QuerySender QuerySender => querySender;
    [SerializeField] public QueryListener queryReceiver;
    [SerializeField] private QueryValidator queryValidator;
    [SerializeField] public MissionUIManager MissionUIManager;
    [SerializeField] public ResultsUI resultsUI;


    public Query CurrentQuery { get; set; }
    public string UniqueMobileKey { get; private set; }
    public event Action<bool> OnQueryIsCorrect;
    private bool isQueryUIVisible = false;
    public bool SkipMobileWaiting { get; private set; } = false;
    public bool MobileConnected { get; set; } = false;


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
                MissionsManager.Instance.ValidateSqlMission(CurrentQuery, result, queryValidator);
            };
            MissionsManager.Instance.OnMissionValidated += isCorrect =>
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
        Application.targetFrameRate = 60;

        if (!Application.isMobilePlatform)
        {
            // PC: Start polling the server for queries
            if (queryReceiver != null)
            {
                Debug.Log("🖥 PC detected — starting QueryReceiver to listen for mobile queries.");
                //queryReceiver.StartListening();
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

    public void ForceStartGameFromPC()
    {
        SkipMobileWaiting = true;
        UIManager.Instance.ShowSQLButton();
        Debug.Log("UIManager.Instance.ShowSQLButton();");
    }

    public void TurnOffSkipOnMobile()
    {
        // SkipMobileWaiting = false;
        // UIManager.Instance.ShowSQLButton();
        // if (SkipMobileWaiting)
        // {
        //     UIManager.Instance.ShowSQLButton(); // ✅ Player chose to play solo on PC
        // }
        // else
        // {
        //     UIManager.Instance.HideSQLButton(); // ✅ Mobile connected properly
        // }

        SkipMobileWaiting = false; // reset only *after* the decision has been acted on
        UIManager.Instance.HideSQLButton();
    }


    public void StartGame()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 0f;
        MenuManager.Instance.ShowMenu(eMenuType.Main);
    }

    public void StartSequence(eSequence sequence)
    {
        UniqueKeyManager.Instance.GenerateGameKey();
        SequenceManager.Instance.StartSequence(sequence);
    }
    
    public void StartMissions()
    {
        MissionUIManager.ShowUI(); // This will handle popup or normal mission
    }

    public void StartSavedGame(string key)
    {
        GameProgressSender gps = new GameProgressSender();
        StartCoroutine(GameProgressSender.Instance.GetSavedGameFromServer(key));
    }

    public void StartGameWithKeyMenu(Action onKeyAccepted)
    {

        if (SqlMode)
        {
            Debug.Log("📱 Mobile already connected — skipping UniqueKeyMenu");
            onKeyAccepted?.Invoke(); // Start game immediately
            return;
        }
        UniqueKeyMenu keyMenu = FindObjectOfType<UniqueKeyMenu>(true);
        QRMenu qrMenu = FindObjectOfType<QRMenu>(true);

        if (keyMenu == null)
        {
            Debug.LogError("❌ UniqueKeyMenu not found in scene.");
            return;
        }

        if (qrMenu == null)
        {
            Debug.LogError("❌ QRMenu not found in scene.");
            return;
        }

        keyMenu.Show(onKeyAccepted);
        MenuManager.Instance.ShowMenu(eMenuType.QR);
    }

    public void InitMobile()
    {
        if(Application.isMobilePlatform)
        {
            Debug.Log("🖥 Running on Mobile — inside the InitMobile method");
            StateListener.Instance.StartListening();
            Debug.Log("🖥 Running on Mobile — after StateListener listening and before ResetListener listening");
            ResetListener.Instance.StartListening();
            Debug.Log("🖥 Running on Mobile — after ResetListener listening and before ResetGame");
            GameManager.Instance.ResetGame();
        }
        
    }

    public void SetSqlMode()
    {
        SqlMode = !SqlMode;

        if (SkipMobileWaiting)
        {
            if (pcGameCanvas != null) pcGameCanvas.SetActive(!SqlMode);
            if (mobileCanvas != null) mobileCanvas.SetActive(SqlMode);
        }
        else
        {
            if (pcGameCanvas != null && !Application.isMobilePlatform) pcGameCanvas.SetActive(!SqlMode);
            if (mobileCanvas != null && Application.isMobilePlatform) mobileCanvas.SetActive(SqlMode);


            if (SqlMode)
            {
                MissionsManager.Instance.ReportTutorialStep("ClickSQL");
            }

        }

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

            // if (QuerySender.IsQuerySent)
            // {
            //     Debug.LogWarning("🚨 Query already accepted. Blocking further sends.");
            //     return;
            // }

            Debug.Log("📤 Sending query to server: " + CurrentQuery.QueryString);
            querySender.SendQueryToServer(CurrentQuery);
        }

        if (queryReceiver != null)
        {
            Debug.Log("🎧 Preparing to listen for the next query...");
            queryReceiver.StartListening();  // this triggers polling
        }
        SetSqlMode();
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
        // Debug.Log($"📥 GameManager received {jsonResponse.Count} rows!");
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
        // Debug.Log($"📌 Query Columns: {string.Join(", ", CurrentQuery.selectClause.Columns.Select(col => col.Name))}");

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

        if (LocationManager.Instance.OfficeSpawnPoint == null)
            Debug.Log($"LocationManager.Instance.OfficeSpawnPoint is null");  
        LocationManager.Instance.TeleportTo(LocationManager.Instance.OfficeSpawnPoint);
        SuspectsManager.Instance?.ResetSuspects();
        resultsUI.ResetResults();

        yield return MissionsManager.Instance.ResetMissions();

        MissionsManager.Instance.LoadMissionSequence(SequenceManager.Instance.Current);

        queryBuilder?.ResetQuery();
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

    private void OnApplicationQuit()
    {
        StartCoroutine(ResetSender.Instance.ResetServerOnDestroy());
    }
    
}
