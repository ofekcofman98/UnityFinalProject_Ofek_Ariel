using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QueryUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionTitle;
    [SerializeField] private TextMeshProUGUI missionDescription;
    private bool isQueryCorrect;
    private string k_IsCorrectString => isQueryCorrect? "correct" : "incorrect";
    private Color k_Color => isQueryCorrect? Color.green : Color.red;

    public void ShowUI()
    {
        missionTitle.text = GameManager.Instance.currentMission.missionTitle;
        missionDescription.text = GameManager.Instance.currentMission.missionDescription;
    }

    public void ShowResult(bool i_Result)
    {
        isQueryCorrect = i_Result;
        
        missionTitle.text = "";
        missionDescription.text = $"Query is {k_IsCorrectString}";
        missionDescription.color = k_Color;
    }

}
