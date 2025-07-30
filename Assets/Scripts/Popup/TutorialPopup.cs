using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupUI : MonoBehaviour, IPopup
{
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button continueButton;

    private Action onContinue;

    private void Awake()
    {
        continueButton.onClick.AddListener(() => {
            Close();
            onContinue?.Invoke();
        });
    }

    public void Show(string message, Action onContinueCallback)
    {
        Time.timeScale = 0f;
        popupText.text = message;
        onContinue = onContinueCallback;
        Open();
    }

    public void Open() => gameObject.SetActive(true);

    public void Close()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
