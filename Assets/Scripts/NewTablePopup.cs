using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewTablePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Popup popup;
    [SerializeField] private TableBoxUI tableBoxPrefab;
    [SerializeField] private Transform tableContainer;

    private TableBoxUI currentTableBox;
    public Action onCloseCallback;
    public bool IsOpen { get; private set; }



    public void Open(Table table)
    {
        if (currentTableBox != null)
        {
            Destroy(currentTableBox.gameObject);
            currentTableBox = null;
        }

        currentTableBox = Instantiate(tableBoxPrefab, tableContainer);
        currentTableBox.Init(table);

        messageText.text = $"New Table Unlocked:\n {table.Name}";
        IsOpen = true;

        popup.OnPopupClosed -= HandlePopupClosed; // Prevent duplicate hook
        popup.OnPopupClosed += HandlePopupClosed;

        popup.Open();
    }


    private void HandlePopupClosed()
    {
        if (!IsOpen) return; // Prevent double firing

        IsOpen = false;

        Debug.Log("📦 NewTablePopup closed via Popup.Close()");
        onCloseCallback?.Invoke();
        onCloseCallback = null;

        MissionsManager.Instance.ReportTutorialStep("CloseNewTablePopup");

        // Unsubscribe to avoid memory leaks
        popup.OnPopupClosed -= HandlePopupClosed;
    }

    public void Close()
    {
        IsOpen = false;
        popup.Close();

        // onCloseCallback?.Invoke();
        // onCloseCallback = null;
        // Debug.Log("closing popup");
        // MissionsManager.Instance.ReportTutorialStep("CloseNewTablePopup");
    }

}
