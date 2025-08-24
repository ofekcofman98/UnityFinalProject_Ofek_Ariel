using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MenuBase
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button tutorialsButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartClicked);    
        loadButton.onClick.AddListener(OnLoadClicked);
        tutorialsButton.onClick.AddListener(OnTutorialsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnStartClicked()
    {
        // GameManager.Instance.StartSequence(eSequence.Main);
        MenuManager.Instance.HideMenu(eMenuType.Main);
        GameManager.Instance.StartGameWithKeyMenu(() => GameManager.Instance.StartSequence(eSequence.Main));

    }

    private void OnLoadClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Main);
        MenuManager.Instance.ShowMenu(eMenuType.Load);
    }

    private void OnTutorialsClicked()
    {
        // GameManager.Instance.StartSequence(eSequence.Tutorials);
        MenuManager.Instance.HideMenu(eMenuType.Main);
        GameManager.Instance.StartGameWithKeyMenu(() => GameManager.Instance.StartSequence(eSequence.Tutorials));

    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
