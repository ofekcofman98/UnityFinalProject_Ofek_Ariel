using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QueryUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MobileMissionNumber;
    [SerializeField] private TextMeshProUGUI MobileMissionTitle;
    [SerializeField] private TextMeshProUGUI MobileMissionDescription;

    [SerializeField] private TextMeshProUGUI PcMissionNumber;
    [SerializeField] private TextMeshProUGUI PcMissionTitle;
    [SerializeField] private TextMeshProUGUI PcMissionDescription;



    private MissionsManager missionManager;

    private bool isQueryCorrect;
    private string k_IsCorrectString => isQueryCorrect? "correct" : "incorrect";
    private Color k_Color => isQueryCorrect? Color.green : Color.red;
   
    [SerializeField] public GameObject executeButton;
    [SerializeField] public GameObject continueButton;
    
    public void Init(MissionsManager missionsManager)
    {
        this.missionManager = missionsManager;
    }

    public void ShowUI()
    {
        if (missionManager == null)
        {
            Debug.LogError("MissionManager not set on QueryUIManager.");
            return;
        }

        MissionData mission = missionManager.CurrentMission;

        MobileMissionNumber.text = missionManager.GetCurrentMissionNumber().ToString();
        MobileMissionTitle.text = mission.missionTitle;
        MobileMissionDescription.text = mission.missionDescription;
        MobileMissionDescription.color = Color.white;

        PcMissionNumber.text = missionManager.GetCurrentMissionNumber().ToString();
        PcMissionTitle.text = mission.missionTitle;
        PcMissionDescription.text = mission.missionDescription;
        PcMissionDescription.color = Color.white;

    }

    public void ShowResult(bool i_Result)
    {
        isQueryCorrect = i_Result;
        
        MobileMissionTitle.text = "";
        MobileMissionDescription.text = $"Query is {k_IsCorrectString}";
        MobileMissionDescription.color = k_Color;
        
        PcMissionTitle.text = "";
        PcMissionDescription.text = $"Mission is {k_IsCorrectString}";
        PcMissionDescription.color = k_Color;

        executeButton.gameObject.SetActive(!i_Result);
        continueButton.gameObject.SetActive(i_Result);
    }

    public void OnContinueButtonClicked()
    {
        continueButton.gameObject.SetActive(false);
        executeButton.gameObject.SetActive(true);

        // Reset the query
        GameManager.Instance.queryBuilder.ResetQuery();

        if (GameManager.Instance.missionManager.CurrentMission.unlocksTable)
        {
            string tableName = GameManager.Instance.missionManager.CurrentMission.tableToUnlock;
            showNewTable(tableName);
        }
        // Move to next mission
        // GameManager.Instance.missionManager.GoToNextMission();

        // Refresh mission UI
        ShowUI();
    }

    private void showNewTable(string tableName)
    {
        Table table = SupabaseManager.Instance.Tables.FirstOrDefault(t => t.Name == tableName);
        if (table != null)
        {
            GameManager.Instance.schemeDisplayer.ShowSchemaWithNewUnlock(tableName);
        }
    }
}
