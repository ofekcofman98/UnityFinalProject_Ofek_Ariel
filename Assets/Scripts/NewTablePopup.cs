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

        currentTableBox = Instantiate(tableBoxPrefab, tableContainer);
        currentTableBox.Init(table);

        messageText.text = $"New Table Unlocked:\n {table.Name}";
        popup.Open();
    }

    public void Close()
    {
        popup.Close();
    }

}
