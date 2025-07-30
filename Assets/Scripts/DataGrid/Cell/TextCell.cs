using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextCell : IDataGridCell
{
    private readonly string _text;
    private readonly GameObject _cellPrefab;

    public TextCell(string text, GameObject cellPrefab)
    {
        _text = text;
        _cellPrefab = cellPrefab;
    }

    public GameObject Create(Transform parent, float width)
    {
        GameObject cell = GameObject.Instantiate(_cellPrefab, parent);
        var tmp = cell.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = _text;
        tmp.alignment = TextAlignmentOptions.Center;

        var layout = cell.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.flexibleWidth = 0f;

        return cell;
    }
}
