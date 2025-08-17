using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadMenu : MenuBase
{
    [SerializeField] private Button loadButton;
    [SerializeField] private GameObject input;
    [SerializeField] private Button returnButton; // return back to main menu
    [SerializeField] private TextMeshProUGUI loadComment; // says if load succeed ot not 

    private void Awake()
    {
        loadButton.onClick.AddListener(OnLoadClicked);
        returnButton.onClick.AddListener(OnReturnClicked);

        if (loadComment != null)
        {
            loadComment.text = ""; // clear comment at start
        }
    }

    private void OnLoadClicked()
    {
        // add here "if saved game is legal": hide PauseMenu
        MenuManager.Instance.HideMenu(eMenuType.Load); //TODO only if game number is legal, add "if" block
        LoadSavedGame();
    }

    private void LoadSavedGame()
    {
        GameManager.Instance.StartSavedGame();
    }

    private void OnReturnClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Load);
        MenuManager.Instance.ShowMenu(eMenuType.Main);
    }
}

