using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspectsManager : Singleton<SuspectsManager>
{
    public List<SuspectData> Suspects = new();
    public string FinalAnswerSuspectId;

    public void AddSuspect(SuspectData suspect)
    {
        if (!Suspects.Contains(suspect))
            Suspects.Add(suspect);
    }

    public void RemoveSuspect(SuspectData suspect)
    {
        Suspects.Remove(suspect);
    }
}

public class SuspectData {
    public string Id;
    public string Name;
    public string Description;
}

