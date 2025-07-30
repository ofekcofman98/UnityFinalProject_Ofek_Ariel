using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionCell<T> : IDataGridCell
{
    private readonly T _rowData;
    private readonly IDataGridAction<T> _action;
    private readonly GameObject _buttonPrefab;

    public ActionCell(T rowData, IDataGridAction<T> action, GameObject buttonPrefab)
    {
        _rowData = rowData;
        _action = action;
        _buttonPrefab = buttonPrefab;
    }

    public GameObject Create(Transform parent, float width)
    {
        GameObject buttonGO = GameObject.Instantiate(_buttonPrefab, parent);

        LayoutElement layout = buttonGO.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.flexibleWidth = 0f;

        var button = buttonGO.GetComponent<Button>();
        var label = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
        label.text = _action.Label;
        label.alignment = TextAlignmentOptions.Center;

        button.onClick.AddListener(() => _action.Execute(_rowData));

        return buttonGO;
    }
}
