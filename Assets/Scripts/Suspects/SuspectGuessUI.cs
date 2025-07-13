using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectGuessUI : MonoBehaviour
{
    [SerializeField] private Transform suspectListContainer; // set this to Content inside Scroll View
    [SerializeField] private GameObject suspectRowPrefab;    // assign your prefab
    [SerializeField] private DataGridDisplayer dataGridDisplayer;

    [SerializeField] private Popup popup;

    private void OnEnable()
    {
        PopulateSuspects();
    }

    public void Open()
    {
        popup.Open();
    }

    public void Close()
    {
        popup.Close();
    }

    private void PopulateSuspects()
    {
        dataGridDisplayer.DisplaySuspects(SuspectsManager.Instance.Suspects);
    }


}
