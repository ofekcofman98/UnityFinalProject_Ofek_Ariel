using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UniqueKeyMenu : MenuBase
{
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private GameObject waitingLabel;
    [SerializeField] private Button continueOnPcButton;
    private Action m_OnKeyAccepted;


    private void Awake()
    {
        continueOnPcButton.onClick.AddListener(() =>
        {
            Debug.Log("🧍‍♂️ Player chose to continue on PC (skip mobile)");
            GameManager.Instance.ForceStartGameFromPC();
        });
    }

    private void OnEnable()
    {
        waitingLabel.SetActive(true);
        keyLabel.text = "";

        StartCoroutine(WaitForKeyGeneration());
    }

    public void Show(Action onKeyAccepted)
    {
        m_OnKeyAccepted = onKeyAccepted;
        MenuManager.Instance.ShowMenu(eMenuType.Key);
    }


    private IEnumerator WaitForKeyGeneration()
    {
        UniqueKeyManager.Instance.GenerateGameKey();

        yield return new WaitUntil(() => !string.IsNullOrEmpty(UniqueKeyManager.Instance.gameKey));

        keyLabel.text = $"{UniqueKeyManager.Instance.gameKey}";

        // Wait for mobile to connect (you need to call OnMobileConnected externally)
        yield return new WaitUntil(() => GameManager.Instance.MobileConnected ||
                                         GameManager.Instance.SkipMobileWaiting); // or any other flag for mobile connection

        MenuManager.Instance.HideMenu(eMenuType.Key);
        GameManager.Instance.TurnOffSkipOnMobile();

        m_OnKeyAccepted?.Invoke();

    }
}
