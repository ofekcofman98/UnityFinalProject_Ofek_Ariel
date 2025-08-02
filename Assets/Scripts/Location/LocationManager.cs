using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationManager : Singleton<LocationManager>
{
    [SerializeField] private GameObject player;
    [SerializeField] private List<Location> allLocations;
    [SerializeField] private LocationsMenu locationsMenu;

    [SerializeField] private Transform privateHomeSpawnPoint;
    [SerializeField] private Texture defaultHomePreview;
    private List<PrivateHomeLocation> privateHomes = new();


    private void Start()
    {
        StartCoroutine(InitializePrivateHomes());
    }

    private IEnumerator InitializePrivateHomes()
    {
        yield return PersonDataManager.Instance.WaitUntilReady();

        foreach (PersonData person in PersonDataManager.Instance.AllCharacters)
        {
            privateHomes.Add(new PrivateHomeLocation(person, privateHomeSpawnPoint, defaultHomePreview));
        }
    }

    public void ShowMenu()
    {
        locationsMenu.Show(allLocations); // that's it
    }

    public void ShowPrivateHomes()
    {
        locationsMenu.Show(privateHomes.Cast<Location>().ToList());
    }

    public void ShowCombinedMenu()
    {
        var combined = new List<Location>();
        combined.AddRange(allLocations);
        combined.AddRange(privateHomes);
        locationsMenu.Show(combined);
    }


    public void TeleportTo(Transform spawnPoint)
    {

        Debug.Log($"🟣 Teleporting player to {spawnPoint.position}");

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false; // ⛔ Stop physics movement
        }

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        if (controller != null)
        {
            controller.enabled = true; // ✅ Re-enable after setting position
        }
    }
}
