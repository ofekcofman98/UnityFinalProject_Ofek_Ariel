using Assets.Scripts.ServerIntegration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : Singleton<MenuManager>
{
    [SerializeField] private GameObject menuUI;
    // [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject loadMenuUI;
    // private LoadMenu loadMenu;
    // [SerializeField] private GameObject winPanelUI;
    // [SerializeField] private GameObject losePanelUI;

    private bool isPaused = false;

    private void Update()
    {
        // Handle pause input (optional)
        if (Input.GetKeyDown(KeyCode.Escape) && !menuUI.activeSelf)
        {
            if (!pauseMenuUI.activeSelf)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void ShowMainMenu()
    {
        menuUI.SetActive(true);
        // gameUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        // winPanelUI.SetActive(false);
        // losePanelUI.SetActive(false);
    }

    public void HideMainMenu()
    {
        menuUI.SetActive(false);
    }


    public void OnStartButtonClicked()
    {
        GameManager.Instance.StartGameFromMenu();
    }

    public void OnTutorialsButtonClicked()
    {
        GameManager.Instance.StartTutorial();
    }

    public void OnLoadSavedGameClicked()
    {
        ShowLoadMenu();
    }

    public void OnSaveCurrentGame()
    {
        GameProgressContainer gpc = new GameProgressContainer(GameManager.Instance.sequenceNumber, MissionsManager.Instance.currentMissionIndex, LivesManager.Instance.Lives);
        StartCoroutine(GameProgressSender.Instance.SendGameProgressToServer(gpc));
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        // gameUI.SetActive(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        // gameUI.SetActive(true);
    }

    public void OnQuitToMainMenuClicked()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        StartCoroutine(GameManager.Instance.resetAction()); // properly resets everything
        ShowMainMenu(); // finally show the main menu
    }

    public void ShowLoadMenu()
    {
        loadMenuUI.SetActive(true);
        menuUI.SetActive(false);
    }

    public void OnLoadClicked()
    {
        GameManager.Instance.StartSavedGame();
    }

    // public void ShowWinPanel()
    // {
    //     Time.timeScale = 0f;
    //     winPanelUI.SetActive(true);
    //     gameUI.SetActive(false);
    // }

    // public void ShowLosePanel()
    // {
    //     Time.timeScale = 0f;
    //     losePanelUI.SetActive(true);
    //     gameUI.SetActive(false);
    // }
}
