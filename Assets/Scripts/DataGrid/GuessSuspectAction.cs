using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuessSuspectAction : IDataGridAction<SuspectData>
{
    public string Label => "Guess";
    public void Execute(SuspectData suspect)
    {
        SuspectsManager.Instance.GuessSuspect(suspect.Id);
    }
}
