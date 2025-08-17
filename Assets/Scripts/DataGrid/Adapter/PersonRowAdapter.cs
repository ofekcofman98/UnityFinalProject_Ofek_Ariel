using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonRowAdapter : IDataGridRowAdapter<PersonData>
{
    private readonly GameObject cellPrefab;
    private readonly List<string> _columnNames;

    public PersonRowAdapter(List<string> columnNames)
    {
        _columnNames = columnNames;
    }


    public List<string> GetColumnValues(PersonData person)
    {
        return _columnNames.Select(col =>
        {
            return col switch
            {
                "person_id" => person.id,
                "name" => person.name,
                "portrait" => "", // handled separately
                _ => "—"
            };
        }).ToList();
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
            return new PortraitCell(person.portrait, cellPrefab);

        string value = columnName switch
        {
            "person_id" => person.id,
            "name" => person.name,
            _ => "—"
        };

        return new TextCell(value, cellPrefab);
    }

}
