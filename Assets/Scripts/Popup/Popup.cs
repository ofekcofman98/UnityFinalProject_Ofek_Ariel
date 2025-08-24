using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour, IPopup
{

    [SerializeField] private GameObject closeButtonPrefab;
    [SerializeField] private GameObject HeaderPanel;
    [SerializeField] protected bool shouldFreezeTime = true;
    private GameObject closeButtonInstance;

    public string tutorialStepIdOnClose;
    public Action OnPopupOpened;
    public Action OnPopupClosed;
    public virtual bool ShouldFreezeTime => shouldFreezeTime;
    public virtual bool ShouldShowCloseButton => true; 
    private void OnEnable() => EnsureCloseButton();

    protected virtual void Awake() {}

    public void Open()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            EnsureCloseButton();
            OnPopupOpened?.Invoke();
            PopupManager.Instance.Register(this);
        }
        else
        {
            Debug.LogWarning($"[Popup] Tried to Open() an already active popup: {name}");
        }

    }

    private void EnsureCloseButton()
    {
        if (!ShouldShowCloseButton || closeButtonPrefab == null || closeButtonInstance != null)
            return;

        if (HeaderPanel != null)
            closeButtonInstance = Instantiate(closeButtonPrefab, HeaderPanel.transform);
        else
            closeButtonInstance = Instantiate(closeButtonPrefab, transform);
    
        RectTransform rt = closeButtonInstance.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-30, -30);

        Button btn = closeButtonInstance.GetComponent<Button>();
        btn.onClick.AddListener(Close);
    }

    public virtual void Close()
    {
        OnPopupClosed?.Invoke();
        gameObject.SetActive(false);
        PopupManager.Instance.Unregister(this);

        // if (!string.IsNullOrEmpty(tutorialStepIdOnClose))
        // {
        //     Debug.Log($"📘 Popup closed: reporting tutorial step '{tutorialStepIdOnClose}'");
        //     MissionsManager.Instance.ReportTutorialStep(tutorialStepIdOnClose);
        // }
        if (!string.IsNullOrEmpty(tutorialStepIdOnClose)
            && MissionsManager.HasInstance
            && MissionsManager.Instance.CurrentMission is CustomTutorialMissionData)
        {
            Debug.Log($"📘 Popup closed: reporting tutorial step '{tutorialStepIdOnClose}'");
            MissionsManager.Instance.ReportTutorialStep(tutorialStepIdOnClose);
        }


    }
    
}
