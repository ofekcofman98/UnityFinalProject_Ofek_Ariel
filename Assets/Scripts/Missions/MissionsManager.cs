using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.ServerIntegration;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MissionsManager : Singleton<MissionsManager>
{
    private MissionSequence missionSequence;
    public MissionSequence MissionSequence => missionSequence;

    public int currentMissionIndex { get; private set; } = 0;
    public MissionData CurrentMission => missionSequence.Missions[currentMissionIndex];
    public event Action<bool> OnMissionValidated;

    public int m_Lives
    {
        get => LivesManager.Instance.Lives;
        set => LivesManager.Instance.SetLives(value);
    }

    private void Start()
    {

        //StartCoroutine(GameProgressSender.Instance.GetSavedGameFromServer((gpc) =>
        //{
        //    if (gpc != null)
        //    {
        //        Debug.Log($"the gpc values are : lives {gpc.Lives}, currentMissionindex {gpc.currentMissionIndex}, SQLmode {gpc.SqlMode}");
        //        //m_Lives = gpc.Lives;
        //        m_Lives = 2;
        //        currentMissionIndex = gpc.currentMissionIndex;
        //        GameManager.Instance.SqlMode = gpc.SqlMode;
        //        SuspectsManager.Instance.initLivesFromMissiomsManager();
        //        UnlockTablesForSavedGame();
        //    }
        //    else
        //    {
        //        Debug.LogWarning("⚠️ Could not load saved game from server.");
        //    }
        //}));

        // SuspectsManager.Instance.Lives = m_Lives;
        // SuspectsManager.Instance.invokeLivesChanged();
        // SuspectsManager.Instance.SetFinalAnswerFromMissionSequence(missionSequence);
    }

    public void SetStatsFromLoadedGame(int i_seqIndex, int i_lives, int i_levelIndex)
    {
        GameManager.Instance.sequenceNumber = i_seqIndex;
        LivesManager.Instance.SetLives(i_lives);
        currentMissionIndex = i_levelIndex;
        // SuspectsManager.Instance.initLivesFromMissiomsManager();
        UnlockTablesForSavedGame();
    }

    public void LoadMissionSequence(MissionSequence sequence)
    {
        missionSequence = sequence;
        currentMissionIndex = 0;
        LivesManager.Instance.ResetLives();

        if (missionSequence == null || missionSequence.Missions.Count == 0)
        {
            Debug.LogError("Mission sequence is null or empty.");
            return;
        }

        CoroutineRunner.Instance.StartCoroutine(LoadCaseMetadata());

        // SuspectsManager.Instance.Lives = m_Lives;
        // SuspectsManager.Instance.initLivesFromMissiomsManager();
        // SuspectsManager.Instance.invokeLivesChanged();

        SuspectsManager.Instance.SetFinalAnswerFromMissionSequence(missionSequence);
        GameManager.Instance.MissionUIManager.ShowUI();
        HighlightManager.Instance?.HighlightTutorialStep(CurrentMission);
    }

    private IEnumerator LoadCaseMetadata()
    {
        string caseId = missionSequence.case_id;
        if (string.IsNullOrEmpty(caseId))
        {
            Debug.LogWarning("❌ No case_id in MissionSequence");
            yield break;
        }

        // ✅ Call the async method and wait for completion manually
        Task task = CaseManager.Instance.LoadCaseData(caseId);
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            Debug.LogError(task.Exception);
    }


    private void UnlockTablesForSavedGame()
    {
        for (int i = 0; i < missionSequence.Missions.Count && i <= currentMissionIndex; i++)
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
        }
        else
        {
            GameManager.Instance.QuerySender?.ResetQuerySendFlag();
            OnMissionValidated?.Invoke(false);

            if (CurrentMission is SQLMissionData)  // <<< guard: only SQL missions
            {
                CoroutineRunner.Instance.StartCoroutine(ReturnToCurrentMissionAfterDelay()); // <<< add
            }
        }
    }

    private IEnumerator ReturnToCurrentMissionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f);

        GameManager.Instance.queryBuilder.ResetQuery();
        GameManager.Instance.queryBuilder.BuildQuery();
        GameManager.Instance.MissionUIManager.ShowUI();
        HighlightManager.Instance?.HighlightTutorialStep(CurrentMission);
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
                // Debug.Log($"❌ Ignored interaction with wrong object: {id} (expected: {im.requiredObjectId})");
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
    
    public void ReportTutorialStep(string stepId)
    {
        if (CurrentMission is CustomTutorialMissionData custom && custom.requiredStepId == stepId)
        {
            custom.MarkAsCompleted();
            ValidateMission();
        }
    }

    public void CheckPopupMission()
    {
        if (CurrentMission is TutorialPopupMissionData popupMissionData)
        {
            ValidateMission();
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

    // public void CheckForTutorialMission()
    // {
    //     if (CurrentMission is TutorialPopupMissionData tutorialMission)
    //     {
    //         GameManager.Instance.MissionUIManager.ShowTutorialPopup(
    //             tutorialMission.missionTitle,
    //             tutorialMission.popupText,
    //             () =>
    //             {
    //                 MarkMissionAsCompleted();
    //                 CoroutineRunner.Instance.StartCoroutine(DelayedAdvance());
    //             });
    //     }

    // }

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
        // if (CurrentMission is SQLMissionData)  // <<< guard: only SQL missions
        // {
        //     yield return new WaitForSecondsRealtime(2f);  // <<< add this line
        // }

        GameManager.Instance.QuerySender?.ResetQuerySendFlag();
        checkUnlocking();
        // Debug.Log("✅ checkUnlocking passed");

        GoToNextMission();
        if (currentMissionIndex >= missionSequence.Missions.Count)
        {
            Debug.Log("🏁 Reached end of mission sequence — skipping mission update.");
            yield break;
        }

        GameManager.Instance.QuerySender?.ResetQuerySendFlag();
        // Debug.Log("✅ ResetQuerySendFlag passed");

        Debug.Log($"🆕 mission number {GetCurrentMissionNumber()} started: " + CurrentMission.missionTitle);

        GameManager.Instance.queryBuilder.ResetQuery();
        GameManager.Instance.queryBuilder.BuildQuery();
        GameManager.Instance.MissionUIManager.ShowUI();
        HighlightManager.Instance?.HighlightTutorialStep(CurrentMission);
    }



    public IEnumerator ResetMissions()
    {
        currentMissionIndex = 0;
        LivesManager.Instance.ResetLives();
        foreach (Table table in SupabaseManager.Instance.Tables)
        {
            table.LockTable();
        }

        // GameManager.Instance.MissionUIManager.ShowUI(); //! check if needed

        yield return null;
    }
}
