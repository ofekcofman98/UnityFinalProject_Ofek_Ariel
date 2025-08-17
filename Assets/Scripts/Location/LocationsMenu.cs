using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationsMenu : Popup//MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab; // LocationCard prefab
    [SerializeField] private Transform contentParent; // Horizontal layout group


    protected override void Awake()
    {
        base.Awake();
        Table.OnTableUnlocked += HandleTableUnlocked;
    }

    private void OnDestroy()
    {
        Table.OnTableUnlocked -= HandleTableUnlocked;
    }

    private void HandleTableUnlocked(Table unlockedTable)
    {
        if (unlockedTable.Name == "Persons")
        {
            if (gameObject.activeSelf)
            {
                LocationManager.Instance.ShowMenu(); // or ShowCombinedMenu()
            }
        }
    }

    public void Show(List<Location> locations)
    {
        // Time.timeScale = 0f;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Location location in locations)
        {
            GameObject cardObj = Instantiate(cardPrefab, contentParent);
            LocationCard card = cardObj.GetComponent<LocationCard>();
            card.Init(location);

            bool isPersonsUnlocked = SupabaseManager.Instance.IsTableUnlocked("Persons");
            bool isLocked = (location is PrivateHomeLocation) && !isPersonsUnlocked;
            card.SetLocked(isLocked);

            if (!isLocked)
            {
                card.OnClicked += (clickedCard) =>
                {
                    LocationManager.Instance.TeleportTo(location.SpawnPoint);

                    if (location is PrivateHomeLocation privateHome)
                    {
                        PrivateHomeManager.Instance.EnterPrivateHome(privateHome.person);
                    }

                    Hide();
                };
            }
        }

        Open(); 
        // gameObject.SetActive(true);
    }

    public void Hide()
    {
        Close();
        // Time.timeScale = 1f;
        // gameObject.SetActive(false);
    }
}
