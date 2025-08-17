using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LoseMenu : MenuBase
{
    [SerializeField] private Button startAgainButton;
    [SerializeField] private Button mainMenuButton;


    private void Awake()
    {
        startAgainButton.onClick.AddListener(OnStartAgainButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnMainMenuClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Lose);
        MenuManager.Instance.QuitToMainMenu();
    }

    private void OnStartAgainButtonClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Lose);
        CoroutineRunner.Instance.StartCoroutine(SequenceManager.Instance.RestartSequence());
    }
}
