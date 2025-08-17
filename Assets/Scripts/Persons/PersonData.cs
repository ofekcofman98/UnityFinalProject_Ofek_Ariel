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
