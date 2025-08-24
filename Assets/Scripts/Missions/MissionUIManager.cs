using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MissionUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MobileMissionNumber;
    [SerializeField] private TextMeshProUGUI MobileMissionTitle;
    [SerializeField] private TextMeshProUGUI MobileMissionDescription;

    [SerializeField] private TextMeshProUGUI PcMissionNumber;
    [SerializeField] private TextMeshProUGUI PcMissionTitle;
    [SerializeField] private TextMeshProUGUI PcMissionDescription;

    [SerializeField] private NewTablePopup newTablePopup;
    [SerializeField] private AudioCue unlockCue;

    [SerializeField] private TutorialPopupUI tutorialPopupUI;


    private bool isQueryCorrect;
    private string k_IsCorrectString => isQueryCorrect ? "correct" : "incorrect";
    private Color k_Color => isQueryCorrect ? Color.green : Color.red;

    [SerializeField] public GameObject executeButton;
    [SerializeField] public GameObject clearButton;


    private bool _resultFlashActive = false;
    private Coroutine _resultFlashCo;


    public void ShowUI()
    {
        Debug.Log("?? Rebuilding query for: " + MissionsManager.Instance.CurrentMission.missionTitle);

        MissionData mission = MissionsManager.Instance.CurrentMission;

        MissionsManager.Instance.CurrentMission.ShowUI(this);
    }

    public void DisplayStandardMission(MissionData mission)
    {
        // if (_resultFlashActive)
        // {
        //     return;
        // }    

        // bool isTutorial = MissionsManager.Instance.MissionSequence.isTutorial;
        // string missionNumberText = isTutorial ? "Tutorial" : MissionsManager.Instance.GetCurrentMissionNumber().ToString();

    if (_resultFlashActive) return;

    bool isTutorial = MissionsManager.Instance.MissionSequence.isTutorial;
    string missionNumberText = isTutorial ? "Tutorial" : MissionsManager.Instance.GetCurrentMissionNumber().ToString();

    // 🔐 Use last valid SQL/Interaction mission for title/description if needed
    MissionData contentToShow = (mission is SQLMissionData or InteractableMissionData)
        ? mission
        : MissionsManager.Instance.GetLastLegalMission();


        MobileMissionNumber.text = missionNumberText;
        MobileMissionTitle.text = mission.missionTitle;
        MobileMissionDescription.text = mission.missionDescription;
        MobileMissionDescription.color = Color.white;

        PcMissionNumber.text = missionNumberText;
        PcMissionTitle.text = mission.missionTitle;
        PcMissionDescription.text = mission.missionDescription;
        PcMissionDescription.color = Color.white;

    }

    public void ShowResult(bool i_Result)
    {
        if (MissionsManager.Instance.CurrentMission is SQLMissionData)
        {
            isQueryCorrect = i_Result;

            MobileMissionTitle.text = "";
            MobileMissionDescription.text = $"Query is {k_IsCorrectString}";
            MobileMissionDescription.color = k_Color;

            PcMissionTitle.text = "";
            PcMissionDescription.text = $"Mission is {k_IsCorrectString}";
            PcMissionDescription.color = k_Color;

            StartResultFlash(2f);
        }
        // executeButton.gameObject.SetActive(!i_Result);
        // clearButton.gameObject.SetActive(i_Result);        

        if (MissionsManager.Instance.CurrentMission.unlocksTable)
        {
            string tableName = MissionsManager.Instance.CurrentMission.tableToUnlock;
            Table table = SupabaseManager.Instance.Tables.FirstOrDefault(t => t.Name == tableName);
            if (table != null)
            {
                table.UnlockTable(); // ✅ Do it here so Init sees the unlocked state
                showNewTable(tableName);
            }
        }

    }

    public void OnClearButtonClicked()
    {
        GameManager.Instance.ClearCurrentQuery();
    }

    private void showNewTable(string tableName)
    {
        Table table = SupabaseManager.Instance.Tables.FirstOrDefault(t => t.Name == tableName);
        if (table != null)
        {
            newTablePopup.Open(table);
            if (unlockCue != null)
            {
                SfxManager.Instance.Play2D(unlockCue);
            }
        }
    }

    // public void ShowTutorialPopup(string title, string message, Action onContinue)
    // {
    //     tutorialPopupUI.Show(title, message, onContinue);
    // }

public void ShowTutorialPopup(string title, string message, Sprite optionalImage, Action onContinue)
{
    tutorialPopupUI.Show(title, message, optionalImage, onContinue);
}

    public bool IsPopupOpen()
    {
        return newTablePopup.IsOpen || tutorialPopupUI.IsOpen;
    }
    

        private void StartResultFlash(float seconds)
    {
        if (_resultFlashCo != null) StopCoroutine(_resultFlashCo);
        _resultFlashActive = true;
        _resultFlashCo = StartCoroutine(ResultFlashCountdown(seconds));
    }

    private IEnumerator ResultFlashCountdown(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        _resultFlashActive = false;

        // After the flash, render the CURRENT mission’s standard UI (which may already be the next mission)
        var mission = MissionsManager.Instance.CurrentMission;
        if (mission != null)
            DisplayStandardMission(mission);

        _resultFlashCo = null;
    }


        
}
