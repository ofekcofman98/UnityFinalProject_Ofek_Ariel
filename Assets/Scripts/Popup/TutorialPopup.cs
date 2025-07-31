using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUI : MonoBehaviour, IPopup
{
    [SerializeField] private TextMeshProUGUI popupTitle;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button continueButton;

    private Action onContinue;
    public bool IsOpen { get; private set; }


    private void Awake()
    {
        continueButton.onClick.AddListener(() =>
        {
            Close();
            onContinue?.Invoke();
        });
    }

    public void Show(string title, string message, Action onContinueCallback)
    {
        Time.timeScale = 0f;
        popupTitle.text = title;
        popupText.text = message;
        onContinue = onContinueCallback;
        IsOpen = true;
        Open();
    }

    public void Open() => gameObject.SetActive(true);

    public void Close()
    {
        Time.timeScale = 1f;
        IsOpen = false;
        gameObject.SetActive(false);
    }
}
