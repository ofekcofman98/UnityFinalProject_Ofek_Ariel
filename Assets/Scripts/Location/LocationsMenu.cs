using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationsMenu : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab; // LocationCard prefab
    [SerializeField] private Transform contentParent; // Horizontal layout group

    public void Show(List<Location> locations)
    {
        Time.timeScale = 0f;

        // Clear existing cards
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create new cards
        foreach (Location location in locations)
        {
            GameObject cardObj = Instantiate(cardPrefab, contentParent);
            LocationCard card = cardObj.GetComponent<LocationCard>();
            card.Init(location);

            // Attach event handler
            card.OnClicked += (clickedCard) =>
            {
                Location location = locations.FirstOrDefault(loc => loc.SpawnPoint == clickedCard.GetTarget());
                
                // Teleport as usual
                LocationManager.Instance.TeleportTo(clickedCard.GetTarget());

                // Handle PrivateHome
                if (location is PrivateHomeLocation privateHome)
                {
                    PrivateHomeManager.Instance.EnterPrivateHome(privateHome.person);
                }

                Hide();
            };
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
