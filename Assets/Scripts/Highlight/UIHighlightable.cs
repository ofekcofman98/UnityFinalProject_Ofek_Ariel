using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHighlightable : MonoBehaviour, IHighlightable
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Outline outline;

    [SerializeField] private Color highlightColor = Color.yellow;
    private Color originalColor;

    private void Awake()
    {
        // if (targetGraphic != null)
        //     originalColor = targetGraphic.color;
                if (outline == null)
            outline = GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;

    }

    public void Highlight(bool state)
    {
        if (outline != null)
            outline.enabled = state;

        // if (targetGraphic != null)
        //     targetGraphic.color = state ? highlightColor : originalColor;
    }

}
