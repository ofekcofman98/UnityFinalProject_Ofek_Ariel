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
    [SerializeField] private Transform officeSpawnPoint;
    [SerializeField] private Transform privateHomeSpawnPoint;
    [SerializeField] private Texture defaultHomePreview;
    private List<PrivateHomeLocation> privateHomes = new();
    public Transform OfficeSpawnPoint => officeSpawnPoint;



    private IEnumerator Start()
    {
        yield return new WaitUntil(() => MissionsManager.Instance != null && MissionsManager.Instance.MissionSequence != null);

        yield return PersonDataManager.Instance.WaitUntilReady();

        string caseId = MissionsManager.Instance.MissionSequence.case_id;

        var loadCaseTask = CaseManager.Instance.LoadCaseData(caseId);
        while (!loadCaseTask.IsCompleted)
            yield return null;

        if (loadCaseTask.IsFaulted)
        {
            Debug.LogError("Failed to load case data.");
            yield break;
        }

        yield return InitializePrivateHomes();
    }

    private IEnumerator InitializePrivateHomes()
    {
        yield return PersonDataManager.Instance.WaitUntilReady();

        string victimId = CaseManager.Instance.VictimId;

        foreach (PersonData person in PersonDataManager.Instance.AllCharacters)
        {
            if (person.id == victimId)
            {
                Debug.Log($"🛑 Skipping victim: {person.name}");
                continue;
            }

            privateHomes.Add(new PrivateHomeLocation(person, privateHomeSpawnPoint, defaultHomePreview));
        }
    }

    public void ShowMenu()
    {
        ShowCombinedMenu();
        // locationsMenu.Show(allLocations); // that's it
    }


    public void ShowCombinedMenu()
    {
            // Debug.Log($"🟢 Showing combined menu. allLocations: {allLocations.Count}, privateHomes: {privateHomes.Count}");

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
        
        FindObjectOfType<MouseLook>()?.ResetLook(spawnPoint.rotation);
    }
}
