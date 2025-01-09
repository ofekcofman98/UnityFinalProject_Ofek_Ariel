using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using SQLite4Unity3d;
using System;

public class CrimeEvidenceTest : MonoBehaviour
{
    private SQLiteConnection _connection;

    void Start()
    {
        string dbPath = Application.streamingAssetsPath + "/CrimeEvidence.db";
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
    
        Debug.Log("Connected to SQLite database!");

        QueryCrimeEvidence();
    }

    private void QueryCrimeEvidence()
    {
        try
        {
            var evidenceList = _connection.Query<CrimeEvidence>("SELECT * FROM CrimeEvidence");
        
            foreach (var evidence in evidenceList)
            {
                Debug.Log($"EvidenceID: {evidence.EvidenceID}, VictimID: {evidence.VictimID}, " +
                $"LocationID: {evidence.LocationID}, Object: {evidence.Object}, " +
                $"WitnessID: {evidence.WitnessID}");

            }
        }
                catch (System.Exception ex)
        {
            Debug.LogError("Error querying CrimeEvidence: " + ex.Message);
        }

    }

public class CrimeEvidence
{
    public int EvidenceID { get; set; }
    public int VictimID { get; set; }
    public int LocationID { get; set; }
    public string Object { get; set; }
    public int WitnessID { get; set; }
}
}
