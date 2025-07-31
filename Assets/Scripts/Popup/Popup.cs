using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour, IPopup
{

    [SerializeField] private GameObject closeButtonPrefab;
    private GameObject closeButtonInstance;
    public string tutorialStepIdOnClose; 
    public Action OnPopupOpened; 
    public Action OnPopupClosed;

    private void OnEnable()
    {
        EnsureCloseButton();
    }



    public void Open()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);
        EnsureCloseButton();
        OnPopupOpened?.Invoke(); 
    }

    private void EnsureCloseButton()
    {
        if (closeButtonPrefab == null || closeButtonInstance != null)
            return;

        closeButtonInstance = Instantiate(closeButtonPrefab, transform);
        RectTransform rt = closeButtonInstance.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-30, -30);

        Button btn = closeButtonInstance.GetComponent<Button>();
        btn.onClick.AddListener(Close);

    }
    public void Close()
    {
        Time.timeScale = 1f;
        OnPopupClosed?.Invoke();
        gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(tutorialStepIdOnClose))
        {
            Debug.Log($"📘 Popup closed: reporting tutorial step '{tutorialStepIdOnClose}'");
            MissionsManager.Instance.ReportTutorialStep(tutorialStepIdOnClose);
        }

    }
}
