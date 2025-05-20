using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MissionsManager : MonoBehaviour
{
    [SerializeField] private MissionSequence missionSequence;

    private int currentMissionIndex = 0;
    public MissionData CurrentMission => missionSequence.Missions[currentMissionIndex];
    public event Action<bool> OnMissionValidated;

    public void ValidateMission(Query query, JArray result, QueryValidator validator)
    {
        bool isValid = validator.ValidateQuery(query, result, CurrentMission);

        if (isValid)
        {
            Debug.Log("✅ Mission complete!");
            OnMissionValidated?.Invoke(true);
            // CoroutineRunner.Instance.StartCoroutine(DelayedAdvance());
        }
        else
        {
            Debug.Log("❌ Mission failed.");
            OnMissionValidated?.Invoke(false);

        }

        OnMissionValidated?.Invoke(isValid);
    }

    public void GoToNextMission()
    {
        if (currentMissionIndex < missionSequence.Missions.Count - 1)
        {
            currentMissionIndex++;
        }
        else
        {
            Debug.Log("🏁 All missions completed! Game over.");
        }
    }

    public int GetCurrentMissionNumber()
    {
        return currentMissionIndex + 1;
    }


    public void MarkMissionAsCompleted()
    {
        Debug.Log("✅ Mission complete!");
        OnMissionValidated?.Invoke(true);
        // Wait for player to click Continue
    }

    private IEnumerator DelayedAdvance()
    {
        Debug.Log("🟡 You unlocked a new table!");
        
        yield return new WaitForSeconds(2.5f);

        GoToNextMission();

        Debug.Log("🆕 New mission started: " + CurrentMission.missionTitle);
        GameManager.Instance.queryUIManager.ShowUI();

    }

}
