using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class QueryResultDecorator
{
    public static void Enrich(JArray rows, string tableName, List<Column> columnList)
    {
        string currentCaseId = MissionsManager.Instance.MissionSequence.case_id;

        if (tableName == "Witnesses" || tableName == "CrimeEvidence")
        {
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var row = rows[i];
                string rowCaseId = row["case_id"]?.ToString();
                if (rowCaseId != currentCaseId)
                {
                    rows.RemoveAt(i);
                }
            }
        }


        if (tableName != "Persons")
            return;

        foreach (JObject obj in rows)
        {
            string id = obj["person_id"]?.ToString();
            if (string.IsNullOrEmpty(id)) continue;

            var person = PersonDataManager.Instance.GetById(id);
            if (person == null) continue;

            obj["__personId"] = person.id;

            if (!obj.ContainsKey("__portrait"))
                obj["__portrait"] = "[portrait]";

            if (!obj.ContainsKey("__name"))
                obj["__name"] = person.name;
        }
    }
}
