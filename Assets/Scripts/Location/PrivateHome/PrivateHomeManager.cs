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

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }

        if (person.id == CaseManager.Instance.VictimId)
        {
            Debug.LogWarning("🚫 Cannot enter victims home.");
            return;
        }


        currentCharacter = Instantiate(person.characterPrefab, spawnPoint.position, spawnPoint.rotation);
        currentCharacter.name = person.name;

        PersonInteract interact = currentCharacter.GetComponent<PersonInteract>();
        if (interact == null)
        {
            interact = currentCharacter.AddComponent<PersonInteract>();
            interact.objectName = person.name;
            interact.InteractableId = person.id;
            interact.interactableType = eInteractableType.MessagePopup;
        }

        interact.Init(person);

        if (currentCharacter.GetComponent<BoxCollider>() == null)
        {
            BoxCollider collider = currentCharacter.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            Vector3 center = collider.center;
            center.y = 0.5f;
            collider.center = center;

            Vector3 size = collider.size;
            size.x = 5.1f;
            size.y = 1.75f;
            size.z = 6.83f;
            collider.size = size;
        }

        if (currentCharacter.GetComponent<PersonInteract>() == null)
        {
            PersonInteract interactable = currentCharacter.AddComponent<PersonInteract>();
            interactable.objectName = person.name;
            interactable.InteractableId = person.id;
            interactable.interactableType = eInteractableType.MessagePopup;
        }


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
