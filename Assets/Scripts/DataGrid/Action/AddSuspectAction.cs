using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class AddSuspectAction : IDataGridAction<JObject>
{
    public string Label => "Add Suspect";
    public void Execute(JObject rowData)
    {
        string id = rowData["person_id"]?.ToString();
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        PersonData fullPerson = PersonDataManager.Instance.GetById(id);
        if (fullPerson == null)
        {
            Debug.LogWarning($"⚠️ No PersonData found for ID {id}");
            return;
        }

        SuspectsManager.Instance.AddSuspect(fullPerson);
    }
}
