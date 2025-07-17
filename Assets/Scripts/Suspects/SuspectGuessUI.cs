using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectGuessUI : MonoBehaviour
{
    [SerializeField] private Transform suspectListContainer; // set this to Content inside Scroll View
    [SerializeField] private DataGridDisplayer dataGridDisplayer;
    [SerializeField] private GameObject guessButtonPrefab;
    [SerializeField] private Popup popup;
    [SerializeField] private MessagePopup messagePopup;

    private void OnEnable()
    {
        PopulateSuspects();
        SuspectsManager.Instance.OnSuspectsChanged += PopulateSuspects;
        SuspectsManager.Instance.OnGuessResult += HandleGuessResult;
        SuspectsManager.Instance.OnLivesChanged += HandleLivesChanged;
    }

    private void OnDisable()
    {
        if (SuspectsManager.HasInstance)
        {
            SuspectsManager.Instance.OnSuspectsChanged -= PopulateSuspects;
            SuspectsManager.Instance.OnGuessResult -= HandleGuessResult;
            SuspectsManager.Instance.OnLivesChanged -= HandleLivesChanged;
        }
    }

    public void Open()
    {
        popup.Open();
    }

    public void Close()
    {
        popup.Close();
    }

    private void PopulateSuspects()
    {
        List<SuspectData> suspects = SuspectsManager.Instance.Suspects;

        dataGridDisplayer.DisplayGrid<SuspectData>(
            new List<string> { "ID", "Full Name" },
            new List<float> { 60f, 150f },
            suspects,
            s => new List<string>
            {
                    s.Id,
                    s.FullName,
                // string.IsNullOrEmpty(s.Description) ? "—" : s.Description
            },
            new List<IDataGridAction<SuspectData>>
            {
                    new GuessSuspectAction(),
                    new RemoveSuspectAction()
            }
        );
    }

    private void HandleGuessResult(bool correct)
    {
        if (correct)
        {
            messagePopup.ShowMessage("Correct! You caught the criminal!");
        }
        else
        {
            int livesLeft = SuspectsManager.Instance.Lives;
            messagePopup.ShowMessage($"Wrong guess!\nLives remaining: {livesLeft}");
        }
    }

    private void HandleLivesChanged(int lives)
    {
        if (lives <= 0)
        {
            messagePopup.ShowMessage("Game Over. No lives left.");
        }
    }


}
