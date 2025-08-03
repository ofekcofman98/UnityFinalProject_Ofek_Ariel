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


    // private void Start()
    // {
    //     StartCoroutine(InitializePrivateHomes());
    // }

    private IEnumerator Start()
    {
        yield return PersonDataManager.Instance.WaitUntilReady(); // wait here early

        yield return InitializePrivateHomes(); // proceed after ready
    }

    private IEnumerator InitializePrivateHomes()
    {
        yield return PersonDataManager.Instance.WaitUntilReady();

        foreach (PersonData person in PersonDataManager.Instance.AllCharacters)
        {
            privateHomes.Add(new PrivateHomeLocation(person, privateHomeSpawnPoint, defaultHomePreview));
            // Debug.Log($"🏠 Added private home for: {person.name}");
        }

        // Debug.Log($"✅ Found {PersonDataManager.Instance.AllCharacters.Count} characters");
    }

    public void ShowMenu()
    {
        ShowCombinedMenu();
        // locationsMenu.Show(allLocations); // that's it
    }


    public void ShowCombinedMenu()
    {
            Debug.Log($"🟢 Showing combined menu. allLocations: {allLocations.Count}, privateHomes: {privateHomes.Count}");

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
