using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class SelectionButtonPopulator<T> : IButtonPopulator<T>
{
    private readonly Transform _parent;
    private readonly Button _buttonPrefab;
    private readonly Func<T, string> _getLabel;
    private readonly Func<T, Transform> _assignedSection;
    private readonly Action<T> _onItemDropped;
    private readonly Action<T> _onItemRemoved;
    private readonly Func<T, bool> _removalCondition;
    private readonly Func<T, int> _conditionIndexGetter;
    private readonly bool _clearFirst;
    private readonly Action<T, Button> _highlightIfNeeded;

    private readonly Dictionary<Button, (Func<bool>, Action)> _removalDict;

    private Dictionary<Type, object> _populators = new();

    public SelectionButtonPopulator(
        Transform parent,
        Button buttonPrefab,
        Func<T, string> getLabel,
        Func<T, Transform> assignedSection,
        Action<T> onItemDropped,
        Action<T> onItemRemoved = null,
        Func<T, bool> removalCondition = null,
        Func<T, int> conditionIndexGetter = null,
        bool clearFirst = true,
        Dictionary<Button, (Func<bool>, Action)> removalDict = null,
        Action<T, Button> highlightIfNeeded = null)
    {
        _parent = parent;
        _buttonPrefab = buttonPrefab;
        _getLabel = getLabel;
        _assignedSection = assignedSection;
        _onItemDropped = onItemDropped;
        _onItemRemoved = onItemRemoved;
        _removalCondition = removalCondition;
        _conditionIndexGetter = conditionIndexGetter;
        _clearFirst = clearFirst;
        _removalDict = removalDict;
        _highlightIfNeeded = highlightIfNeeded;
    }

    public void PopulateButtons(IEnumerable<T> items)
    {
        if (items == null || !items.Any())
        {
            Debug.LogWarning("No selection items to populate.");
            return;
        }

        if (_clearFirst)
        {
            foreach (Transform child in _parent)
                GameObject.Destroy(child.gameObject);
        }

        int index = 0;
        foreach (T item in items)
        {
            try
            {
                Button button = GameObject.Instantiate(_buttonPrefab, _parent);
                button.transform.SetSiblingIndex(index++);

                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                string labelText = _getLabel(item);
                if (label != null)
                {
                    label.text = labelText;
                    SetButtonPreferredSize(button, label);
                }
                else
                {
                    Debug.LogError($"Missing label for selection item: {labelText}");
                }

                _highlightIfNeeded?.Invoke(item, button);

                DraggableItem draggable = button.GetComponent<DraggableItem>() ?? button.gameObject.AddComponent<DraggableItem>();
                draggable.Reset();
                draggable.OriginalParent = _parent;
                draggable.AssignedSection = _assignedSection(item);
                draggable.draggableType = eDraggableType.SelectionButton;
                draggable.OnDropped += _ => _onItemDropped(item);
                if (_onItemRemoved != null)
                    draggable.OnRemoved += () => _onItemRemoved(item);

                if (_removalCondition != null && _removalDict != null)
                    _removalDict[button] = (() => _removalCondition(item), () => _onItemRemoved?.Invoke(item));

                if (_conditionIndexGetter != null)
                    draggable.ConditionIndex = _conditionIndexGetter(item);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating selection button: {ex.Message}");
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_parent);
    }

    private void SetButtonPreferredSize(Button button, TextMeshProUGUI label, float padding = 20f, float fixedHeight = 60f)
    {
        var layout = button.GetComponent<LayoutElement>();
        if (layout == null)
        {
            Debug.LogError("Button is missing LayoutElement.");
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(label.rectTransform);
        float preferredWidth = LayoutUtility.GetPreferredWidth(label.rectTransform);
        layout.preferredWidth = preferredWidth + padding;
        layout.preferredHeight = fixedHeight;
    }
}
