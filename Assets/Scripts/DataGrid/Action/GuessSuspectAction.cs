using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuessSuspectAction : IDataGridAction<PersonData>
{
    public string Label => "Guess";
    public void Execute(PersonData suspect)
    {
        SuspectsManager.Instance.GuessSuspect(suspect.id);
    }
}
