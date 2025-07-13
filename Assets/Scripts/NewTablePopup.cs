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


    public void Open(Table table)
    {
        if (currentTableBox != null)
        {
            Destroy(currentTableBox.gameObject);
            currentTableBox = null;
        }

        messageText.text = $"New table unlocked: {table.Name}";
        currentTableBox = Instantiate(tableBoxPrefab, tableContainer);
        currentTableBox.Init(table);

        messageText.text = $"New Table Unlocked: {table.Name}";
        popup.Open();
    }

    public void Close()
    {
        popup.Close();
    }

}
