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
        loadGameInput.text = "Enter game key..";
        commentText.gameObject.SetActive(false);

        keyContainer.SetActive(true);
        buildSQLButton.gameObject.SetActive(false);
        // buildSQLButton.gameObject;
    }

    private void OnConnectClicked()
    {
        string inputKey = loadGameInput.text.Trim();


        if (string.IsNullOrEmpty(inputKey) || !int.TryParse(inputKey, out int intkey))
        {
            commentText.color = Color.white;
            commentText.text = "Please enter a valid key.";
            commentText.gameObject.SetActive(true);

            return;
        }

        UniqueKeyManager.Instance.CompareKeys(inputKey, success =>
        {
            if (success)
            {
                // Hide UI
                keyContainer.SetActive(false);
                connectButton.gameObject.SetActive(false);
                loadGameInput.DeactivateInputField();  // ?? remove focus & close keyboard
                loadGameInput.gameObject.SetActive(false);
                commentText.gameObject.SetActive(false);

                commentText.color = Color.green;
                commentText.text = "Connected!";
                commentText.gameObject.SetActive(true);


                switchUI();
            }
            else
            {
                commentText.color = Color.red;
                commentText.text = "Invalid key. Try again.";
                commentText.gameObject.SetActive(true);

                loadGameInput.DeactivateInputField();
            }
        });
    }


    private void switchUI()
    {
        keyContainer.SetActive(false);
        buildSQLButton.gameObject.SetActive(true);
    }


    private void OnStartSQLClicked()
    {
        Debug.Log("SQL was clicked");
        GameManager.Instance.SetSqlMode();
    }

}
