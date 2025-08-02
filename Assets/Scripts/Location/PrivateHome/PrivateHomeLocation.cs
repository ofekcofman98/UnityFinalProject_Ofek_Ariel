using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivateHomeLocation : Location
{
    public PersonData person;

    public PrivateHomeLocation(PersonData person, Transform sharedSpawn, Texture previewTexture)
    {
        this.person = person;
        this.LocationName = $"{person.name}'s Home";
        this.SpawnPoint = sharedSpawn;
        this.PreviewTexture = previewTexture;
    }
}
