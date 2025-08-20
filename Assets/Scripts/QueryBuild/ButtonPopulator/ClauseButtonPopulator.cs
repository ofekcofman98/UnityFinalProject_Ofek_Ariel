using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClauseButtonPopulator<T> : IButtonPopulator<T>
{
    private readonly Transform _parent;
    private readonly Func<T, string> _getLabel;
    private readonly Func<T, Transform> _assignedSection;
    private readonly Action<T> _onItemDropped;
    private readonly Action<T> _onItemRemoved;
    private readonly Button _buttonPrefab;
    private readonly Dictionary<T, Button> _activeButtons;


    public ClauseButtonPopulator(
        Transform parent,
        Button buttonPrefab,
        Func<T, string> getLabel,
        Func<T, Transform> assignedSection,
        Action<T> onItemDropped,
        Action<T> onItemRemoved,
        Dictionary<T, Button> activeButtons)
    {
        _parent = parent;
        _buttonPrefab = buttonPrefab;
        _getLabel = getLabel;
        _assignedSection = assignedSection;
        _onItemDropped = onItemDropped;
        _onItemRemoved = onItemRemoved;
        _activeButtons = activeButtons;
    }

    public void PopulateButtons(IEnumerable<T> items)
    {
        foreach (var key in _activeButtons.Keys.ToList())
        {
            if (!items.Contains(key))
            {
                GameObject.Destroy(_activeButtons[key].gameObject);
                _activeButtons.Remove(key);
            }
        }


        foreach (var item in items)
        {
            if (_activeButtons.ContainsKey(item)) continue;

            Button button = GameObject.Instantiate(_buttonPrefab, _parent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = _getLabel(item);

            DraggableItem draggable = button.GetComponent<DraggableItem>() ?? button.gameObject.AddComponent<DraggableItem>();
            draggable.Reset();
            draggable.AssignedSection = _assignedSection(item);
            draggable.draggableType = eDraggableType.ClauseButton;
            draggable.OnDropped += _ => _onItemDropped(item);
            draggable.OnRemoved += () => _onItemRemoved?.Invoke(item);

            _activeButtons[item] = button;
        }

    }

}
