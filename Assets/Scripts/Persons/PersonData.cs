using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class PersonData // or PersonData
{
    public string id;
    public string first_name;
    public string last_name;
    public string name => $"{first_name} {last_name}".Trim();
    public string description;
    public string photo_url;
    public string prefab_id;

    [NonSerialized] public GameObject characterPrefab;
    [NonSerialized] public Texture2D portrait;
}

public static class PersonFactory
{
    public static PersonData FromRow(JObject row)
    {
        return new PersonData
        {
            id = row["person_id"]?.ToString(),
            first_name = row["first_name"]?.ToString(),
            last_name = row["last_name"]?.ToString(),
            description = row["description"]?.ToString(),
            photo_url = row["photo_url"]?.ToString(),
            prefab_id = row["prefab_id"]?.ToString()
        };
    }
}
