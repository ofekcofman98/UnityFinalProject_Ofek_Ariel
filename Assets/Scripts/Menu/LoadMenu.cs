using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadMenu : MenuBase
{
    [SerializeField] private Button loadButton;
    [SerializeField] private TMP_InputField loadGameInput;
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
        LoadSavedGame();
    }

    private void LoadSavedGame()
    {
        string key = loadGameInput.text;

        if (string.IsNullOrWhiteSpace(key))
        {
            loadComment.color = Color.red;
            loadComment.text = "Please enter a game key.";
            return;
        }

        if (key.Length != 6 || !int.TryParse(key, out int _))
        {
            loadComment.color = Color.red;
            loadComment.text = "Key must be 6 digits.";
            return;
        }

        loadComment.color = Color.white;
        loadComment.text = "Validating key...";


        GameProgressSender.Instance.ValidateKeyAndLoadGame(key, (isValid) =>
        {
            if (isValid)
            {
                UniqueKeyMenu keyMenu = FindObjectOfType<UniqueKeyMenu>(true);
                if (keyMenu == null)
                {
                    Debug.LogError("? UniqueKeyMenu not found in scene.");
                    return;
                }
                keyMenu.registerExistingKey = true;
                UniqueKeyManager.Instance.SetGameKeyFromSavedGame(key);
                loadComment.color = Color.green;
                loadComment.text = "Game loaded!";

                MenuManager.Instance.HideMenu(eMenuType.Load);
                // MenuManager.Instance.ShowMenu(eMenuType.Key);
GameManager.Instance.StartGameWithKeyMenu(() => GameManager.Instance.StartSavedGame(key));

            }
            else
            {
                loadComment.color = Color.red;
                loadComment.text = "Invalid or expired key.";
            }
        });
    }

    private void OnReturnClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.Load);
        MenuManager.Instance.ShowMenu(eMenuType.Main);
    }
}

