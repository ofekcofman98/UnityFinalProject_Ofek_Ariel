using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("PC UI Elements")]
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI missionDescriptionText;

    [SerializeField] private Button buildSQLButton;

    void Start()
    {
        HideHint();
        HideSQLButton();
    }

    public void ShowHint(string message)
    {
        hintText.text = message;
    }

    public void HideHint()
    {
        hintText.text = "";
    }

    public void SetMissionText(string mission)
    {
        missionText.text = mission;
    }

    public void SetMissionDescription(string description)
    {
        missionDescriptionText.text = description;
    }

    public void ShowSQLButton()
    {
        buildSQLButton.gameObject.SetActive(true);
    }

    public void HideSQLButton()
    {
        buildSQLButton.gameObject.SetActive(false);
    }
}
