using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreensaverUI : MonoBehaviour
{
    [SerializeField] private GameObject keyContainer;  
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_InputField loadGameInput;
    [SerializeField] private TextMeshProUGUI commentText;
    [SerializeField] private Button buildSQLButton;

    private void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        buildSQLButton.onClick.AddListener(OnStartSQLClicked);

        keyContainer.SetActive(true);
        buildSQLButton.gameObject.SetActive(false);
    }

    private void OnConnectClicked()
    {
        string inputKey = loadGameInput.text.Trim();

        if (string.IsNullOrEmpty(inputKey))
        {
            commentText.color = Color.white;
            commentText.text = "Please enter a key.";
            return;
        }

        if (inputKey == UniqueKeyManager.Instance.gameKey)
        {
            GameManager.Instance.ConnectMobile();
            GameManager.Instance.SetSqlMode(); // handles canvas switching + query init

            commentText.color = Color.green;
            commentText.text = "Connected!";

            switchUI();
        }
        else
        {
            commentText.color = Color.red;
            commentText.text = "Invalid key. Try again.";
        }
    }

    private void switchUI()
    {
        keyContainer.SetActive(false);
        buildSQLButton.gameObject.SetActive(true);
    }

    private void OnStartSQLClicked()
    {
        GameManager.Instance.SetSqlMode();
    }

}
