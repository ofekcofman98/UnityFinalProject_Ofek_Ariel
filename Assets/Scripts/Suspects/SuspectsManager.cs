using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SuspectsManager : Singleton<SuspectsManager>
{
    private int m_Lives = 3;
    public int Lives => m_Lives;
    public event Action<int> OnLivesChanged;
    public event Action<bool> OnGuessResult;


    public List<SuspectData> Suspects = new();
    public string FinalAnswerSuspectId;

    public void AddSuspect(SuspectData suspect)
    {
        if (!Suspects.Contains(suspect))
        {
            Suspects.Add(suspect);
            Debug.Log($"🟢 Suspects count: {Suspects.Count}");
        }
        else
        {
            Debug.Log("⚠️ Suspect already added.");
        }
    }

    public void AddSuspectFromRow(JObject row)
    {
        if (!row.TryGetValue("person_id", out var idToken)) return;

        var id = idToken?.ToString();
        if (string.IsNullOrEmpty(id)) return;

        // Check if already added
        if (Suspects.Any(s => s.Id == id)) return;

        var firstName = row.TryGetValue("first_name", out var f) ? f?.ToString() : "";
        var lastName = row.TryGetValue("last_name", out var l) ? l?.ToString() : "";
        var pictureUrl = row.TryGetValue("profile_picture_url", out var p) ? p?.ToString() : null;
        var description = row.TryGetValue("description", out var d) ? d?.ToString() : null;

        var suspect = new SuspectData
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            // ProfilePictureUrl = pictureUrl,
            Description = description
        };

        Suspects.Add(suspect);
        Debug.Log($"🕵️ Added suspect: {firstName} {lastName} ({id})");
    }


    public void RemoveSuspect(SuspectData suspect)
    {
        Suspects.Remove(suspect);
    }

    public void GuessSuspect(String suspectId)
    {

    }
    // public JArray GetSuspectsAsJArray()
    // {
    //     var array = new JArray();

    //     foreach (var suspect in Suspects)
    //     {
    //         JObject row = new JObject
    //         {
    //             ["person_id"] = suspect.Id,
    //             ["first_name"] = suspect.FirstName,
    //             ["last_name"] = suspect.LastName,
    //             ["description"] = suspect.Description,
    //             // ["profile_picture_url"] = suspect.ProfilePictureUrl
    //         };

    //         array.Add(row);
    //     }

    //     return array;
    // }

}

public class SuspectData
{
    public string Id;
    public string FirstName;
    public string LastName;
    public string Name;
    public string Description;
    public string FullName => $"{FirstName} {LastName}".Trim();

}

