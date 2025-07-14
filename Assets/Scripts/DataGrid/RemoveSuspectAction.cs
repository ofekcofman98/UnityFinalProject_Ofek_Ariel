using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSuspectAction : IDataGridAction<SuspectData>
{
    public string Label => "Remove";

    public void Execute(SuspectData suspect)
    {
        SuspectsManager.Instance.RemoveSuspect(suspect);
        Debug.Log($"🗑️ Removed suspect: {suspect.FullName} ({suspect.Id})");
    }
}
