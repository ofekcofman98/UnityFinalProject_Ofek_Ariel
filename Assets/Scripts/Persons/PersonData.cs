using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterData // or PersonData
{
    public string id;
    public string name;
    public string photo_url;
    public string prefab_id;

    [NonSerialized] public GameObject characterPrefab;
    [NonSerialized] public Texture2D portrait;
}
