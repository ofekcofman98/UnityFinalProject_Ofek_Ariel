using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateHomeManager : Singleton<PrivateHomeManager>
{
    [SerializeField] private Transform spawnPoint;
    private GameObject currentCharacter;

    public void EnterPrivateHome(PersonData person)
    {
        ClearPrevious();

        if (person.characterPrefab == null)
        {
            Debug.LogError($"No prefab found for person {person.name}");
            return;
        }

        currentCharacter = Instantiate(person.characterPrefab, spawnPoint.position, Quaternion.identity);

        Debug.Log($"🏠 Entered private home of: {person.name}");
    }

    private void ClearPrevious()
    {
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
            currentCharacter = null;
        }
    }
}
