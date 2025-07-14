using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : Singleton<LocationManager>
{
    [SerializeField] private GameObject player;
    [SerializeField] private List<Location> allLocations;
    [SerializeField] private LocationsMenu locationsMenu;


    public void ShowMenu()
    {
        locationsMenu.Show(allLocations); // that's it
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
