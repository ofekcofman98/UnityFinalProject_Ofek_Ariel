using TMPro;
using UnityEngine;

public class ApplyCustomFont : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset customFont;

    void Start()
    {
        if (customFont == null)
        {
            Debug.LogError("Assign your TMP font asset in the inspector!");
            return;
        }

        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            text.font = customFont;
        }

        Debug.Log($"✅ Replaced fonts for {texts.Length} UI texts.");
    }
}
