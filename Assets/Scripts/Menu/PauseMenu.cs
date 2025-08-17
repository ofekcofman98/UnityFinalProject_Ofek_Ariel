using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MenuBase
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button syncButton;
    [SerializeField] private Button quitButton;


    private void Awake()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        syncButton.onClick.AddListener(OnSyncClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnResumeClicked()
    {
        MenuManager.Instance.ResumeGame();
    }

    private void OnSaveClicked()
    {
        // TODO
    }

    private void OnSyncClicked()
    {
        // TODO
    }

    private void OnQuitClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Pause);
        MenuManager.Instance.QuitToMainMenu();
    }
}
