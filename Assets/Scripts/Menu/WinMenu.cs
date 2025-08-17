using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinMenu : MenuBase
{
    [SerializeField] private Button nextCaseButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        nextCaseButton.onClick.AddListener(OnNextCaseClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnEnable()
    {
        nextCaseButton.gameObject.SetActive(SequenceManager.Instance.HasNext);
    }

    private void OnNextCaseClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Win);
        SequenceManager.Instance.LoadNextSequence();
    }

    private void OnMainMenuClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Win);
        MenuManager.Instance.ShowMenu(eMenuType.Main);
    }

    private void OnSaveClicked()
    {
        //TODO
    }

}
