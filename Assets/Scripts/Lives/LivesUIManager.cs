using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesUIManager : MonoBehaviour
{
    [SerializeField] private List<Image> heartImages; // assign in Inspector
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private void Start()
    {
        UpdateHearts(LivesManager.Instance.Lives);
        LivesManager.Instance.OnLivesChanged += UpdateHearts;
    }

    private void OnDestroy()
    {
        if (LivesManager.HasInstance)
        {
            LivesManager.Instance.OnLivesChanged -= UpdateHearts;
        }

    }

    private void UpdateHearts(int lives)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].sprite = i < lives ? fullHeartSprite : emptyHeartSprite;
        }
    }

}
