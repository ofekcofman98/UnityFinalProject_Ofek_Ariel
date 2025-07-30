using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlightable : MonoBehaviour, IHighlightable
{
    [SerializeField] private GameObject markerObject;
    public void Highlight(bool state)
    {
        if (markerObject != null)
        {
            markerObject.SetActive(state);
        }
    }

    public void SetMarkerLabel(string label)
    {
        if (markerObject == null) return;

        TMPro.TextMeshProUGUI text = markerObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
            text.text = label;
    }


}
