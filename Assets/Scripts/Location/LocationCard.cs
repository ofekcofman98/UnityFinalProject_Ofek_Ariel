using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationCard : MonoBehaviour
{
    [SerializeField] private RawImage buildingImage;
    [SerializeField] private TextMeshProUGUI locationName;
    [SerializeField] private GameObject lockOverlay;      // the lock icon/image
    [SerializeField] private CanvasGroup canvasGroup;     // for fade and interaction

    private Transform teleportTarget;

    public event Action<LocationCard> OnClicked;

    public void Init(Location location)
    {
        // Debug.Log($"📦 INIT: {location.LocationName}, Target: {location.SpawnPoint}");

        locationName.text = location.LocationName;
        buildingImage.texture = location.PreviewTexture;
        teleportTarget = location.SpawnPoint;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            // Debug.Log($"🟢 CLICKED card for location: {location.LocationName}");
            OnClicked?.Invoke(this);
        });
    }

    public void SetLocked(bool isLocked)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isLocked ? 0.5f : 1f;
            canvasGroup.interactable = !isLocked;
            canvasGroup.blocksRaycasts = !isLocked;
        }

        if (lockOverlay != null)
        {
            lockOverlay.SetActive(isLocked);

            var lockCanvasGroup = lockOverlay.GetComponent<CanvasGroup>();
            if (lockCanvasGroup != null)
            {
                lockCanvasGroup.alpha = 1f;
                lockCanvasGroup.interactable = false;
                lockCanvasGroup.blocksRaycasts = false;
            }
        }

        if (locationName != null)
        {
            if (isLocked)
            {
                locationName.text = "Locked";
            }
        }


    }

    public Transform GetTarget() => teleportTarget;
}
