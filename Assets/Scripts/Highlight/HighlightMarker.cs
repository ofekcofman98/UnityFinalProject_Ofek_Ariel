using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighlightMarker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject container;

    public void SetLabel(string text) => label.text = text;
    public void Show() => container.SetActive(true);
    public void Hide() => container.SetActive(false);
}
