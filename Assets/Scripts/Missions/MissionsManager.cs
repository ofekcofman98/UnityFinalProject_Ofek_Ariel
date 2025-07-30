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

    public int currentMissionIndex { get; private set; } = 0;
    public MissionData CurrentMission => missionSequence.Missions[currentMissionIndex];

    public int m_Lives { get; private set; } = 3;
    public event Action<bool> OnMissionValidated;

    private void Start()
    {
        StartCoroutine(GameProgressSender.Instance.GetSavedGameFromServer((gpc) =>
        {
            if (gpc != null)
            {
                Debug.Log($"the gpc values are : lives {gpc.Lives}, currentMissionindex {gpc.currentMissionIndex}, SQLmode {gpc.SqlMode}");
                m_Lives = gpc.Lives;

                //SuspectsManager.Instance.Lives = 1;
                //m_Lives = gpc.Lives;
                currentMissionIndex = gpc.currentMissionIndex;
                GameManager.Instance.SqlMode = gpc.SqlMode;
                UnlockTablesForSavedGame();
            }
            else
            {
                Debug.LogWarning("⚠️ Could not load saved game from server.");
            }
        }));

        SuspectsManager.Instance.SetFinalAnswerFromMissionSequence(missionSequence);
    }

    private void UnlockTablesForSavedGame()
    {
        for(int i = 0; i < missionSequence.Missions.Count && i <= currentMissionIndex; i++)
        {
            MissionData mission = missionSequence.Missions[i];
            try
            {
                if (mission.unlocksTable && !string.IsNullOrEmpty(mission.tableToUnlock))
                {
                    Table unlockedTable = SupabaseManager.Instance.Tables.FirstOrDefault(
                    t => t.Name == mission.tableToUnlock);

                    if (unlockedTable != null)
                    {
                        unlockedTable.UnlockTable();
                        Debug.Log($"🔓 Table '{unlockedTable.Name}' has been unlocked after mission success!");
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.Log($"AN EXCEPTION HAS OCCURED : {ex.ToString()}");
            }
        }
    }
    private void ValidateMission()
    {
        bool isValid = CurrentMission.Validate();
        if (isValid)
        {
            Debug.Log("✅ Mission complete!");
            checkUnlocking();
            StateSender.Instance.UpdatePhone();
            OnMissionValidated?.Invoke(true);
            CoroutineRunner.Instance.StartCoroutine(DelayedAdvance());
            //GameManager.Instance.SqlMode = (CurrentMission is SQLMissionData);
            //SQLmodeSender.Instance.SendSQLmodeToPhone();
            //if (currentMissionIndex == 3)
            //{
            //    GameProgressContainer gpc= new GameProgressContainer(GameManager.Instance.SqlMode, GameManager.Instance.missionManager.currentMissionIndex, GameManager.Instance.missionManager.m_Lives);
            //    GameProgressSender.Instance.StartCoroutine(GameProgressSender.Instance.SendGameProgressToServer(gpc));
            //    Debug.Log("✅✅✅ container sent to server !!");

            //}

        }
        else
        {
            Debug.Log("❌ Mission failed.");
            if (currentMissionIndex == missionSequence.Missions.Count - 1)
            {
                Debug.Log("❌ You arrested the wrong suspect !.");
                m_Lives--;
                if (m_Lives > 0)
                    Debug.Log($"You have {m_Lives} lives left .");
                else
                    Debug.Log("Game over :/");

            }

            GameManager.Instance.QuerySender?.ResetQuerySendFlag();
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
            if (im.requiredObjectId == id)
            {
                im.SetTriggeredObject(id);
                ValidateMission();
            }
            else
            {
                Debug.Log($"❌ Ignored interaction with wrong object: {id} (expected: {im.requiredObjectId})");
            }
        }
    }


    private void checkUnlocking()
    {
        Debug.Log("Entered checkUnlocking");
        try
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
        catch (Exception ex) 
        {
            Debug.Log($"AN EXCEPTION HAS OCCURED : {ex.ToString()}");
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
        Debug.Log($"➡️ Now at mission {currentMissionIndex}: {CurrentMission.missionTitle}");

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
        GameManager.Instance.QuerySender?.ResetQuerySendFlag();  
        //yield return new WaitForSeconds(2.0f);

        checkUnlocking();
        Debug.Log("✅ checkUnlocking passed");

        GoToNextMission();
        if (currentMissionIndex >= missionSequence.Missions.Count)
        {
            Debug.Log("🏁 Reached end of mission sequence — skipping mission update.");
            yield break;
        }

        GameManager.Instance.QuerySender?.ResetQuerySendFlag();
        Debug.Log("✅ ResetQuerySendFlag passed");

        Debug.Log("🆕 New mission started: " + CurrentMission.missionTitle);

        GameManager.Instance.queryBuilder.ResetQuery();
        GameManager.Instance.queryBuilder.BuildQuery(); 
        GameManager.Instance.MissionUIManager.ShowUI();
    }

    public IEnumerator ResetMissions()
    {
        Debug.Log("Hit the reset button !!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        currentMissionIndex = 0;
        m_Lives = 3;
        foreach (Table table in SupabaseManager.Instance.Tables)
        {
            table.LockTable();
        } 

        GameManager.Instance.MissionUIManager.ShowUI(); //! check if needed

        yield return null;
    }
}
