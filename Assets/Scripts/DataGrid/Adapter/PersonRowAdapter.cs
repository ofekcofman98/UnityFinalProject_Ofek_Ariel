using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonRowAdapter : IDataGridRowAdapter<PersonData>
{
    private readonly GameObject cellPrefab;
    public List<string> GetColumnValues(PersonData person)
    {
        return new List<string>
        {
            person.id,
            "", // portrait handled separately
            person.name
        };
    }

    public Texture2D GetPortrait(PersonData person)
    {
        return person.portrait;
    }

    public string GetDisplayName(PersonData person)
    {
        return person.name;
    }

    public IDataGridCell CreateCell(PersonData person, string columnName)
    { 
        if (columnName == "portrait")
            return new PortraitCell(person.portrait, cellPrefab);  // You pass cellPrefab from outside

        string value = columnName switch
        {
            "person_id" => person.id,
            "name" => person.name,
            _ => "—"
        };

        return new TextCell(value, cellPrefab);
    }

}
