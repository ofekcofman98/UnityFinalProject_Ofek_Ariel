using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private MessagePopup messagePopup;
    [SerializeField] private GameObject PopupBlockerPanel;

    private List<Popup> activePopups = new();
    private int freezeCounter = 0;


    public void Register(Popup popup)
    {
        if (!activePopups.Contains(popup))
            activePopups.Add(popup);

        if (popup.ShouldFreezeTime)
        {
            freezeCounter++;
            Time.timeScale = 0f;
            PopupBlockerPanel.SetActive(true);
            PopupBlockerPanel.transform.SetSiblingIndex(0);
        }

        Debug.Log($"[PopupManager] ✅ Registered: {popup.name}, FreezeCounter = {freezeCounter}");
    }

    public void Unregister(Popup popup)
    {
        activePopups.Remove(popup);

        if (popup.ShouldFreezeTime)
        {
            freezeCounter = Mathf.Max(0, freezeCounter - 1);

            if (freezeCounter == 0)
            {
                Time.timeScale = 1f;
                PopupBlockerPanel.SetActive(false);
            }
        }

        Debug.Log($"[PopupManager] ✅ Unregistered: {popup.name}, FreezeCounter = {freezeCounter}");

    }

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

    public void CloseAllPopups()
    {
        foreach (Popup popup in new List<Popup>(activePopups))
        {
            popup.Close();
        }
    }

    public bool IsEmpty()
    {
        return activePopups.Count == 0;
    }
}
