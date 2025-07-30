using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortraitCell : IDataGridCell
{
    private readonly Texture2D _portrait;
    private readonly GameObject _cellPrefab;

    public PortraitCell(Texture2D portrait, GameObject cellPrefab)
    {
        _portrait = portrait;
        _cellPrefab = cellPrefab;
    }

    public GameObject Create(Transform parent, float width)
    {
        GameObject cell = GameObject.Instantiate(_cellPrefab, parent);

        var text = cell.transform.Find("TextLabel")?.GetComponent<TextMeshProUGUI>();
        if (text != null) text.gameObject.SetActive(false);

        var image = cell.transform.Find("PortraitImage")?.GetComponent<RawImage>();
        if (image != null)
        {
            image.gameObject.SetActive(true);
            image.texture = _portrait;
            image.rectTransform.sizeDelta = new Vector2(width, width);
        }

        var layout = cell.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = width + 10f;
        layout.minHeight = width + 10f;

        return cell;
    }
}
