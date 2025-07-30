using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSuspectAction : IDataGridAction<PersonData>
{
    public string Label => "Remove";

    public void Execute(PersonData suspect)
    {
        SuspectsManager.Instance.RemoveSuspect(suspect);
        Debug.Log($"🗑️ Removed suspect: {suspect.name} ({suspect.id})");
    }
}
