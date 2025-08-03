using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonInteract : InteractableObject
{
    private PersonData _personData;

    public void Init(PersonData personData)
    {
        _personData = personData;
    }

    public override void Interact()
    {
        base.Interact();

        if (_personData == null)
        {
            Debug.LogWarning("PersonData not assigned.");
            return;
        }

        string text = DialogueManager.Instance.GetBestDialogue(_personData);
        PopupManager.Instance.Show(text);
    }
}
