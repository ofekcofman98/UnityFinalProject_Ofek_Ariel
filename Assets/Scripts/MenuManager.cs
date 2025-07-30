using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : Singleton<MenuManager>
{
    [SerializeField] private GameObject menuUI;
    // [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject pauseMenuUI;
    // [SerializeField] private GameObject winPanelUI;
    // [SerializeField] private GameObject losePanelUI;

    private bool isPaused = false;
    
    private void Update()
    {
        // Handle pause input (optional)
        if (Input.GetKeyDown(KeyCode.Escape))
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
        GameManager.Instance.StartGameFromMenu(); // ✅ Delegate to GameManager
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
