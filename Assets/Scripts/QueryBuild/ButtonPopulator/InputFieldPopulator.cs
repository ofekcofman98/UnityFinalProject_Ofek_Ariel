using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ValueInputPopulator<T>
{
    private readonly Transform selectionParent;
    private readonly GameObject inputFieldPrefab;
    private readonly GameObject confirmButtonPrefab;
    private readonly Button selectionButtonPrefab;

    private GameObject currentInputField;
    private GameObject currentConfirmButton;

    private readonly Func<string, bool> validateInput;
    private readonly Func<string, string> formatInput;
    private readonly Func<string, T> parseInput;
    private readonly Func<T, int> conditionIndexGetter;
    private readonly Dictionary<Button, (Func<bool>, Action)> removalDict;


    public ValueInputPopulator(
        Transform selectionParent,
        GameObject inputFieldPrefab,
        GameObject confirmButtonPrefab,
        GameObject selectionButtonPrefab,
        Func<string, bool> validateInput,
        Func<string, string> formatInput,
        Func<string, T> parseInput,
        Func<T, int> conditionIndexGetter,
        Dictionary<Button, (Func<bool>, Action)> removalDict
    )
    {
        this.selectionParent = selectionParent;
        this.inputFieldPrefab = inputFieldPrefab;
        this.confirmButtonPrefab = confirmButtonPrefab;
        this.selectionButtonPrefab = selectionButtonPrefab.GetComponent<Button>();
        this.validateInput = validateInput;
        this.formatInput = formatInput;
        this.parseInput = parseInput;
        this.conditionIndexGetter = conditionIndexGetter;
        this.removalDict = removalDict;
    }

    public void Show(
        List<T> predefinedValues,
        Action<T> onValueSelected,
        Func<T, bool> canRemove,
        Action<T> onRemoved,
        Func<T, Transform> assignedSection
    )
    {
        CreateInputUI(onValueSelected, canRemove, onRemoved, assignedSection);
        Action<T> wrappedOnValueSelected = val =>
        {
            Dispose();
            onValueSelected?.Invoke(val);
        };

        var populator = new SelectionButtonPopulator<T>(
            parent: selectionParent,
            buttonPrefab: selectionButtonPrefab,
            getLabel: val => val.ToString(),
            assignedSection: assignedSection,
            onItemDropped: wrappedOnValueSelected,
            onItemRemoved: onRemoved,
            removalCondition: canRemove,
            conditionIndexGetter: conditionIndexGetter,
            clearFirst: false,
            removalDict: removalDict
        );

        populator.PopulateButtons(predefinedValues);

        selectionParent.GetChild(selectionParent.childCount - 2).SetAsFirstSibling();
        selectionParent.GetChild(selectionParent.childCount - 1).SetSiblingIndex(1);
    }

    private void CreateInputUI(
        Action<T> onValueSelected,
        Func<T, bool> canRemove,
        Action<T> onRemoved,
        Func<T, Transform> assignedSection)
    {
        if (currentInputField) UnityEngine.Object.Destroy(currentInputField);
        if (currentConfirmButton) UnityEngine.Object.Destroy(currentConfirmButton);

        currentInputField = UnityEngine.Object.Instantiate(inputFieldPrefab, selectionParent);
        currentInputField.transform.localScale = Vector3.one;
        TMP_InputField inputField = currentInputField.GetComponent<TMP_InputField>();
        inputField.text = "";
        inputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter value...";


        if (typeof(T) == typeof(int))
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        else
            inputField.contentType = TMP_InputField.ContentType.Standard;

        // inputField.Select();
        // inputField.ActivateInputField();

        currentConfirmButton = UnityEngine.Object.Instantiate(confirmButtonPrefab, selectionParent);
        currentConfirmButton.transform.localScale = Vector3.one;

        Button confirmBtn = currentConfirmButton.GetComponent<Button>();
        confirmBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm";
        confirmBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.AddListener(() =>
        {
            string raw = inputField.text;
            if (string.IsNullOrWhiteSpace(raw) || !validateInput(raw)) return;

            string formatted = formatInput(raw);
            T parsed = parseInput(formatted);
            if (parsed == null) return;

            UnityEngine.Object.Destroy(currentInputField);
            UnityEngine.Object.Destroy(currentConfirmButton);

            var inputPopulator = new SelectionButtonPopulator<T>(
                parent: selectionParent,
                buttonPrefab: selectionButtonPrefab,
                getLabel: val => val.ToString(),
                assignedSection: assignedSection,
                onItemDropped: onValueSelected,
                onItemRemoved: onRemoved,
                removalCondition: canRemove,
                conditionIndexGetter: conditionIndexGetter,
                clearFirst: false,
                removalDict: removalDict
            );

            inputPopulator.PopulateButtons(new List<T> { parsed });
        });
    }

    public void Dispose()
    {
        if (currentInputField) UnityEngine.Object.Destroy(currentInputField);
        if (currentConfirmButton) UnityEngine.Object.Destroy(currentConfirmButton);
    }

}
