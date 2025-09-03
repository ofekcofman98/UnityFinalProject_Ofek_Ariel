using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavedGameMenu : MenuBase
{
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private GameObject blockerPanel;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnContinueClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.SavedGame);
        MenuManager.Instance.ShowMenu(eMenuType.Pause);
    }

    public void SaveGame()
    {
        GameSaver.Instance.SaveGame(key => keyLabel.text = key);

        // int lastValidMissionIndex = MissionsManager.Instance.GetLastValidMissionIndex();
        // int sequenceIndex = SequenceManager.Instance.CurrentSequenceIndex;
        // int lives = LivesManager.Instance.Lives;

        // GameProgressContainer gpc = new GameProgressContainer(sequenceIndex, lastValidMissionIndex, lives);

        // if (GameProgressSender.Instance.IsSameAsLastSave(gpc))
        // {
        //     Debug.Log("🛑 No changes since last save. Skipping.");
        //     keyLabel.text = UniqueKeyManager.Instance.gameKey;
        //     return;
        // }

        // StartCoroutine(GameProgressSender.Instance.SendGameProgressToServer(gpc));
    }

    public override void Show()
    {
        base.Show();
        blockerPanel.SetActive(true);

        SaveGame();  
        keyLabel.text = UniqueKeyManager.Instance.gameKey; 
    }
    
    public override void Hide()
    {
        base.Hide();
        blockerPanel.SetActive(false);
    }
}
