using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessagePopup : Popup
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void ShowMessage(string message)
    {
        messageText.text = message;
        Open(); // from base Popup
    }
}
