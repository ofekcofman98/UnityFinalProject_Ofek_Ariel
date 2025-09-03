using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MenuBase
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button syncButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject blockerPanel;


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
        MenuManager.Instance.HideMenu(eMenuType.Pause);
        MenuManager.Instance.ShowMenu(eMenuType.SavedGame);
    }

    private void OnSyncClicked()
    {
        StateSender.Instance.UpdatePhone();
    }

    private void OnQuitClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Pause);
        MenuManager.Instance.QuitToMainMenu();
    }

    public override void Show()
    {
        base.Show();
        blockerPanel.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();
        blockerPanel.SetActive(false);
    }

}
