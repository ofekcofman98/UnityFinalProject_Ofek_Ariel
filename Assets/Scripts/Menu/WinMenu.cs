using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WinMenu : MenuBase
{
    [SerializeField] private Button nextCaseButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private TextMeshProUGUI savedGameLabel;
    [SerializeField] private TextMeshProUGUI gameKeyLabel;
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
        savedGameLabel.gameObject.SetActive(false);
        gameKeyLabel.gameObject.SetActive(false);

        GameSaver.Instance.SaveGame(gameKey =>
        {
            savedGameLabel.text = "Game saved successfully";
            gameKeyLabel.text = $"Game key: {gameKey}";

            saveButton.gameObject.SetActive(false);
            savedGameLabel.gameObject.SetActive(true);
            gameKeyLabel.gameObject.SetActive(true);
        });

    }

    public override void Show()
    {
        base.Show();
        saveButton.gameObject.SetActive(true);
        gameKeyLabel.gameObject.SetActive(false);
        savedGameLabel.gameObject.SetActive(false);
    }

    public override void Hide()
    {
        base.Hide();
        saveButton.gameObject.SetActive(true);
        gameKeyLabel.gameObject.SetActive(false);
        savedGameLabel.gameObject.SetActive(false);
    }

}
