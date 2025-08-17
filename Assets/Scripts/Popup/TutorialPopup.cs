using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUI : Popup
{
    [SerializeField] private TextMeshProUGUI popupTitle;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button continueButton;

    private Action onContinue;
    public bool IsOpen { get; private set; }
    public override bool ShouldShowCloseButton => false;

    protected override void Awake()
    {
        base.Awake();

        continueButton.onClick.AddListener(() =>
        {
            Close();
            onContinue?.Invoke();
        });
    }

    public void Show(string title, string message, Action onContinueCallback)
    {
        if (IsOpen)
        {
            Debug.LogWarning($"[TutorialPopupUI] Show() called while already open — ignoring.");
            return;
        }
        popupTitle.text = title;
        popupText.text = message;
        onContinue = onContinueCallback;
        IsOpen = true;
        Open();
    }

    public override void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Debug.Log($"[TutorialPopupUI] Close() called");

        base.Close(); // Call base to trigger unregister
    }

    // public void Open() => gameObject.SetActive(true);

    // public void Close()
    // {
    //     Time.timeScale = 1f;
    //     IsOpen = false;
    //     gameObject.SetActive(false);
    // }
}
