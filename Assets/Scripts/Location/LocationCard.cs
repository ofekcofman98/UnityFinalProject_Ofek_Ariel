using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationCard : MonoBehaviour
{
    [SerializeField] private RawImage buildingImage;
    [SerializeField] private TextMeshProUGUI locationName;

    private Transform teleportTarget;

    public event Action<LocationCard> OnClicked;

    public void Init(Location location)
    {
        Debug.Log($"📦 INIT: {location.LocationName}, Target: {location.SpawnPoint}");

        locationName.text = location.LocationName;
        buildingImage.texture = location.PreviewTexture;
        teleportTarget = location.SpawnPoint;

GetComponent<Button>().onClick.AddListener(() => {
    Debug.Log($"🟢 CLICKED card for location: {location.LocationName}");
    OnClicked?.Invoke(this);
});
    }

    public Transform GetTarget() => teleportTarget;
}
