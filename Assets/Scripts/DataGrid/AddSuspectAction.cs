using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class AddSuspectAction : IDataGridAction<JObject>
{
    public string Label => "Add Suspect";
    public void Execute(JObject rowData)
    {
        SuspectsManager.Instance.AddSuspectFromRow(rowData);
    }
}
