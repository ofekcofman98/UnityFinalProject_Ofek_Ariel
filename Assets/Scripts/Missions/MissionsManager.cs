using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ServerIntegration;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MissionsManager : Singleton<MissionsManager>
{
    [SerializeField] private MissionSequence missionSequence;

    private int currentMissionIndex = 0;
    public MissionData CurrentMission => missionSequence.Missions[currentMissionIndex];
    private int m_Lives = 3;
    public event Action<bool> OnMissionValidated;

    private void ValidateMission()
    {
        bool isValid = CurrentMission.Validate();
        if (isValid)
        {
            Debug.Log("✅ Mission complete!");
            checkUnlocking();
            GameStateSender.Instance.UpdatePhone();
            OnMissionValidated?.Invoke(true);
            CoroutineRunner.Instance.StartCoroutine(DelayedAdvance());
        }
        else 
        {
            Debug.Log("❌ Mission failed.");
            if (currentMissionIndex == missionSequence.Missions.Count - 1)
            {
                Debug.Log("❌ You arrested the wrong suspect !.");
                m_Lives--;
                if(m_Lives > 0)
                    Debug.Log($"You have {m_Lives} lives left .");
                else
                    Debug.Log("Game over :/");

            }
            OnMissionValidated?.Invoke(false);
        }
    }

    public void ValidateSqlMission(Query query, JArray result, QueryValidator validator)
    {
        if (CurrentMission is SQLMissionData sql)
        {
            sql.SetValidationContext(query, result, validator);
            ValidateMission();
        }
    }

    public void ValidateInteractableMission(string id)
    {
        if (CurrentMission is InteractableMissionData im)
        {
            im.SetTriggeredObject(id);
            ValidateMission();
        }
    }


    private void checkUnlocking()
    {
        if (CurrentMission.unlocksTable && !string.IsNullOrEmpty(CurrentMission.tableToUnlock))
        {
            Table unlockedTable = SupabaseManager.Instance.Tables.FirstOrDefault(
            t => t.Name == CurrentMission.tableToUnlock);

            if (unlockedTable != null)
            {
                unlockedTable.UnlockTable();
                Debug.Log($"🔓 Table '{unlockedTable.Name}' has been unlocked after mission success!");
            }
        }
    }

    public void ReportInteractableUsed(string id)
    {
        if (CurrentMission is InteractableMissionData m &&
            m.requiredObjectId == id)
        {
            Debug.Log("Correct Object!");
            GoToNextMission();
            
        }
        else
        {
            Debug.Log("Incorrect object!");
        }
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

    public IEnumerator DelayedAdvance()
    {
        Debug.Log("🟡 You unlocked a new table!");
        yield return new WaitForSeconds(2.5f);
        checkUnlocking();
        GoToNextMission(); 
        Debug.Log("🆕 New mission started: " + CurrentMission.missionTitle);
        GameManager.Instance.queryBuilder.ResetQuery();
        GameManager.Instance.MissionUIManager.ShowUI();
        
    }

}
