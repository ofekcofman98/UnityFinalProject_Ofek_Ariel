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
    [SerializeField] private GameObject imageContainer; // parent object to enable/disable
    [SerializeField] private Image popupContentImage;    // actual image component


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

    public void Show(string title, string message, Sprite optionalImage, Action onContinueCallback)
    {
        if (IsOpen)
        {
            Debug.LogWarning($"[TutorialPopupUI] Show() called while already open — ignoring.");
            return;
        }
        popupTitle.text = title;
        popupText.text = message;
        onContinue = onContinueCallback;


    if (optionalImage != null)
    {
        imageContainer.SetActive(true);
        popupContentImage.sprite = optionalImage;
    }
    else
    {
        imageContainer.SetActive(false); // Hide the container if no image
    }




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
