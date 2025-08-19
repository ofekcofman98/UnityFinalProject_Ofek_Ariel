using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SuspectsManager : Singleton<SuspectsManager>
{
    public event Action<int> OnLivesChanged;
    public event Action<bool, AudioCue> OnGuessResult;
    public event Action OnSuspectsChanged;
    public List<PersonData> Suspects = new();
    public string FinalAnswerSuspectId { get; private set; }

    [SerializeField] private AudioCue guessCorrectCue;
    [SerializeField] private AudioCue guessWrongCue;


    public void AddSuspect(PersonData suspect)
    {
        if (!Suspects.Contains(suspect))
        {
            Suspects.Add(suspect);
            Debug.Log($"🟢 Suspects count: {Suspects.Count}");
            OnSuspectsChanged?.Invoke();
        }
        else
        {
            Debug.Log("⚠️ Suspect already added.");
        }
    }

//TODO AddSuspectFromRow(JObject row) → creates a PersonData and reuses PersonDataManager.GetById() if already loaded

//TODO 💡 OR remove AddSuspectFromRow(JObject) altogether and use PersonDataManager.GetById() directly!

    // public void invokeLivesChanged()
    // {
    //     OnLivesChanged.Invoke(MissionsManager.Instance.m_Lives);
    // }

    public void AddSuspectFromRow(JObject row)
    {
        if (!row.TryGetValue("person_id", out var idToken)) return;

        string id = idToken?.ToString();
        if (string.IsNullOrEmpty(id)) return;

        // Check if already added
        if (Suspects.Any(s => s.id == id)) return;

        string firstName = row.TryGetValue("first_name", out var f) ? f?.ToString() : "";
        string lastName = row.TryGetValue("last_name", out var l) ? l?.ToString() : "";
        string pictureUrl = row.TryGetValue("profile_picture_url", out var p) ? p?.ToString() : null;
        string description = row.TryGetValue("description", out var d) ? d?.ToString() : null;

        PersonData suspect = new PersonData
        {
            id = id,
            first_name = firstName,
            last_name = lastName,
            photo_url = pictureUrl,
            description = description
        };

        Suspects.Add(suspect);
        Debug.Log($"🕵️ Added suspect: {firstName} {lastName} ({id})");
    }


    public void RemoveSuspect(PersonData suspect)
    {
        if (Suspects.Remove(suspect))
        {
            Debug.Log($"🗑️ Removed suspect: {suspect.name}");
            OnSuspectsChanged?.Invoke();
        }
    }

    public void GuessSuspect(String suspectId)
    {
        if (string.IsNullOrEmpty(FinalAnswerSuspectId))
        {
            Debug.LogWarning("❓ Final criminal not set — guessing is blocked.");
            return;
        }

        bool correct = suspectId == FinalAnswerSuspectId;

        if (correct)
        {
            HandleCorrectGuess();
        }
        else
        {
            HandleWrongGuess();
        }
    }

    private void HandleCorrectGuess()
    {
        Debug.Log("🎉 Correct suspect guessed!");
        OnGuessResult?.Invoke(true, guessCorrectCue);


        if (SequenceManager.Instance.Current.isTutorial)
        {
            Debug.Log("🏁 Tutorial complete — returning to main menu.");
            GameManager.Instance.ShowMainMenu();
        }
        else if (SequenceManager.Instance.HasNext)
        {
            Debug.Log("➡️ Correct guess — advancing to next sequence.");
            SequenceManager.Instance.LoadNextSequence();
        }
        else
        {
            Debug.Log("🏆 Final suspect found — game complete!");
            MenuManager.Instance.ShowMenu(eMenuType.Main); // Make sure this menu exists
        }

        // Move to the next MissionSequence / end-game flow.
        // GameManager.Instance.AdvanceToNextSequence();

    }

    private void HandleWrongGuess()
    {
        Debug.Log("❌ Wrong guess");
        int remaining = LivesManager.Instance.Decrement();

        if (remaining <= 0)
        {
            Debug.Log("💀 Game Over — no lives remaining.");
            MenuManager.Instance.ShowMenu(eMenuType.Lose);
        }
        else
        {
            OnGuessResult?.Invoke(false, guessWrongCue);
        }
    }
    
    public void SetFinalAnswerFromMissionSequence(MissionSequence sequence)
    {
        FinalAnswerSuspectId = sequence?.FinalAnswerPersonId;
        if (!string.IsNullOrEmpty(FinalAnswerSuspectId))
        {
            Debug.Log($"🎯 Final answer loaded: {FinalAnswerSuspectId}");
        }
        else
        {
            Debug.LogWarning("⚠️ FinalAnswerPersonId is empty in MissionSequence!");
        }
    }

    internal void ResetSuspects()
    {
        Suspects.Clear();
        FinalAnswerSuspectId = null;
        OnSuspectsChanged?.Invoke();
    }
}
