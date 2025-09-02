using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QRMenu : MenuBase
{
    [SerializeField] private Button continueButton;
    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);    
    }

    private void OnContinueClicked()
    {
        MenuManager.Instance.HideMenu(eMenuType.QR);
    }
}
