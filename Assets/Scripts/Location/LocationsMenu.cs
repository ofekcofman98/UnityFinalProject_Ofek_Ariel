using System.Collections;
using System.Collections.Generic;
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
                LocationManager.Instance.TeleportTo(clickedCard.GetTarget());
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
