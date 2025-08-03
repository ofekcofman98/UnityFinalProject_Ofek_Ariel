using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private MessagePopup messagePopup;

    public void Show(string message)
    {
        if (messagePopup != null)
        {
            messagePopup.ShowMessage(message);
        }
        else
        {
            Debug.LogError("MessagePopup not assigned in MessagePopupManager.");
        }
    }
}
