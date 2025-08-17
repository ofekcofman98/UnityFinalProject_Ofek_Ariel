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
        List<PersonData> suspects = SuspectsManager.Instance.Suspects;

        List<string> columnNames = new() { "person_id", "portrait", "name" };
        List<float> columnWidths = new() { 100f, 60f, 100f };

        dataGridDisplayer.DisplayGrid<PersonData>(
        columnNames,
        columnWidths,
        suspects,
        new PersonRowAdapter(columnNames),
        new List<IDataGridAction<PersonData>>
        {
        new GuessSuspectAction(),
        new RemoveSuspectAction()
        }
    );

    }

    private void HandleGuessResult(bool correct, AudioCue audioCue)
    {
        if (correct)
        {
            // messagePopup.ShowMessage("Correct! You caught the criminal!");
            MenuManager.Instance.ShowMenu(eMenuType.Win);
        }
        else
        {
            //int livesLeft = SuspectsManager.Instance.Lives;
            messagePopup.ShowMessage($"Wrong guess!");
        }
        SfxManager.Instance.Play2D(audioCue);
    }

    private void HandleLivesChanged(int lives)
    {
        if (lives <= 0)
        {
            // messagePopup.ShowMessage("Game Over. No lives left.");
            MenuManager.Instance.ShowMenu(eMenuType.Lose);
        }
    }


}
