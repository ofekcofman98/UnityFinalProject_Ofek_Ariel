using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using UnityEngine;

public class SequenceManager : Singleton<SequenceManager>
{
    [SerializeField] private List<MissionSequence> allSequences;
    public int CurrentSequenceIndex { get; private set; } = 0;
    public MissionSequence Current => allSequences[CurrentSequenceIndex];
    public bool HasNext => CurrentSequenceIndex < allSequences.Count - 1;
    public MissionSequence MainGameSequence => allSequences[(int)eSequence.Main];
    public MissionSequence TutorialSequence => allSequences[(int)eSequence.Tutorials];

    public void StartSequence(eSequence sequence)
    {
        Time.timeScale = 1f;
        MenuManager.Instance.HideMenu(eMenuType.Main);
        StartCoroutine(StartSequenceRoutine(sequence));
    }

    private IEnumerator StartSequenceRoutine(eSequence sequence)
    {
        yield return MissionsManager.Instance.ResetMissions();

        CurrentSequenceIndex = (int)sequence;
        MissionsManager.Instance.LoadMissionSequence(Current);

        GameManager.Instance.StartMissions();
        ResetSender.Instance.SendResetToPhone();

        StateSender.Instance.UpdatePhone();

    }


    public void LoadNextSequence()
    {
        CurrentSequenceIndex++;
        if (CurrentSequenceIndex < allSequences.Count)
        {
            
            MissionsManager.Instance.LoadMissionSequence(Current);
            GameManager.Instance.StartMissions();
        }
    }

    public IEnumerator RestartSequence()
    {
        SuspectsManager.Instance.ResetSuspects();
        LocationManager.Instance.TeleportTo(LocationManager.Instance.OfficeSpawnPoint);
        GameManager.Instance.resultsUI.ResetResults();

yield return StartCoroutine(StartSequenceRoutine((eSequence)CurrentSequenceIndex));


//         yield return MissionsManager.Instance.ResetMissions();
//         MissionsManager.Instance.LoadMissionSequence(Current);
//         GameManager.Instance.StartMissions();

// //        StartSequenceRoutine(eSequence.Main);

    }


    public int GetCurrentIndex()
    {
        return CurrentSequenceIndex;
    }

    public void SetSequence(int index)
    {
        CurrentSequenceIndex = Mathf.Clamp(index, 0, allSequences.Count - 1);

    }
}
