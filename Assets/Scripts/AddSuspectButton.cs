using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddSuspectButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI label;

    public void Setup(string id, string name)
    {
        label.text = "Add Suspect";
        label.alignment = TextAlignmentOptions.Center;
        button.onClick.RemoveAllListeners(); // just in case

        button.onClick.AddListener(() =>
        {
            var suspect = new SuspectData { Id = id, Name = name };
            SuspectsManager.Instance.AddSuspect(suspect);
            Debug.Log($"🕵️ Added suspect: {name} ({id})");
        });
    }

}
