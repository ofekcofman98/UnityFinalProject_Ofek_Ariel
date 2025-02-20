using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool SqlMode {get; set;}
    public string CurrentQuery {get; private set;}
    [SerializeField] public GameObject QueryPanel;

    [SerializeField] private QuerySender querySender;
    [SerializeField] private QueryReceiver queryReceiver;

    void Awake()
    {
        if (querySender == null)
        {
            Debug.LogWarning("⚠️ QuerySender is not assigned in the Inspector! Trying to find it...");
            querySender = FindObjectOfType<QuerySender>(); // Try to find it in the scene
        }

        if (querySender == null)
        {
            Debug.LogError("❌ QuerySender is still missing! Make sure it's assigned in the Inspector.");
        }
    }
    void Start()
    {
        SqlMode = false;
        // querySender = GetComponent<QuerySender>(); 
    }

    public void SaveQuery(string i_Query)
    {
        CurrentQuery = i_Query;
        Debug.Log("Query saved in GameManager: " + i_Query);

        if (queryReceiver != null)
        {
            queryReceiver.StopListening();
        }

    }

    internal void SetSqlMode(bool i_Visible = true)
    {
        Debug.Log("query panel is ON");
        QueryPanel.SetActive(i_Visible);
    }

    public void RunQuery()
    {
        if (querySender == null)
        {
            Debug.LogError("QuerySender is null! Cannot send query.");
            return;
        }

        if (queryReceiver == null)
        {
            Debug.LogError("QueryReceiver is null! Cannot receive query.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(CurrentQuery))
        {
            Debug.Log("Sending Query: " + CurrentQuery);
            querySender.SendQueryToServer(CurrentQuery);

            queryReceiver.StartListening();
        }
        else
        {
            Debug.LogError("⚠️ No query to run.");
        }
    }

}
