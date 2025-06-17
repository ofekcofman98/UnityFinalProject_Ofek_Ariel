using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QueryUIRenderer : MonoBehaviour
{

    [Header("Selection")]
    public GameObject selectionButtonPrefab;
    public Transform selectionParent;
    public ObjectPoolService<Button> selectionButtonPool;

    [Header("Clauses")]
    public GameObject ClausesButtonPrefab;
    public Transform clausesParent;
    public ObjectPoolService<Button> clauseButtonPool;
    private Dictionary<IQueryClause, Button> activeClauseButtons = new Dictionary<IQueryClause, Button>();


    [SerializeField] private GameObject inputFieldPrefab;
    [SerializeField] private GameObject confirmButtonPrefab;
    public Button executeButton;


    public TextMeshProUGUI queryPreviewText;
    private Dictionary<Button, (Func<bool> condition, Action removeAction)> removalConditions
        = new Dictionary<Button, (Func<bool>, Action)>();



    void Awake()
    {
        executeButton.onClick.AddListener(ExecuteQuery);  
        selectionButtonPool = new ObjectPoolService<Button>(selectionButtonPrefab.GetComponent<Button>(), selectionParent);
        clauseButtonPool = new ObjectPoolService<Button>(ClausesButtonPrefab.GetComponent<Button>(), clausesParent, 5, 20);
    }


    public void populateClauseButtons<T>(
        IEnumerable<T> i_Items,
        Action<T> i_OnItemDropped,
        Func<T, string> i_GetLabel,
        Transform i_ParentTransform,
        Func<T, Transform> i_AssignedSection,
        ObjectPoolService<Button> i_ButtonPool,
        Dictionary<T, Button> i_ActiveButtons,

        Func<T, bool> i_RemovalCondition = null,
        Action<T> i_OnItemRemoved = null)

    {

        foreach (var key in i_ActiveButtons.Keys.ToList()) // Loop through all stored buttons
        {
            if (!i_Items.Contains(key)) // If the clause is no longer available
            {
                i_ButtonPool.Release(i_ActiveButtons[key]); // Release the button back to the pool
                i_ActiveButtons.Remove(key); // Remove the entry from the dictionary
            }
        }

        int index = 0;
        foreach (T item in i_Items)
        {
            if (!i_ActiveButtons.ContainsKey(item))
            {
                Button button = i_ButtonPool.Get();
                // button.transform.SetParent(i_ParentTransform, false);
                InsertButtonInSection(i_ParentTransform, button, eDraggableType.ClauseButton);

                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TextMeshProUGUI>().text = i_GetLabel(item);

                // button.onClick.RemoveAllListeners();
                // button.onClick.AddListener(() => i_OnItemSelected(item));

                DraggableItem draggableItem = button.GetComponent<DraggableItem>();
                if (draggableItem == null)
                {
                    draggableItem = button.gameObject.AddComponent<DraggableItem>();
                }

                draggableItem.AssignedSection = i_AssignedSection(item);
                draggableItem.draggableType = eDraggableType.ClauseButton;
                draggableItem.OnDropped += (droppedItem) => i_OnItemDropped(item);
                draggableItem.OnRemoved += () => i_OnItemRemoved(item);


                i_ActiveButtons[item] = button;
            }
            // i_ActiveButtons[item].transform.SetSiblingIndex(index);
            index++;

        }

    }

    public void populateSelectionButtons<T>(
        IEnumerable<T> i_Items,
        Action<T> i_OnItemDropped,
        Func<T, string> i_GetLabel,
        Transform i_ParentTransform,
        Func<T, Transform> i_AssignedSection,
        ObjectPoolService<Button> i_ButtonPool,
        bool i_ClearSelectionPanel = true,
        Func<T, bool> i_RemovalCondition = null,
        Action<T> i_OnItemRemoved = null)
    {

        if (i_Items == null || !i_Items.Any())
        {
            Debug.LogWarning("No items available for selection.");
            return;
        }

        if (i_ClearSelectionPanel)
        {
            foreach (Transform child in i_ParentTransform)
            {
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    i_ButtonPool.Release(child.GetComponent<Button>());
                }
            }
        }

        int index = 0;
        foreach (T item in i_Items)
        {

            if (i_ButtonPool == null)
            {
                Debug.LogError("populateSelectionButtons: Button pool is null!");
                return;
            }

            Button button = i_ButtonPool.Get();
            if (button == null || button.gameObject == null)
            {
                Debug.LogError("[populateSelection] Button from pool is NULL!");
                continue;
            }

            InsertButtonInSection(i_ParentTransform, button, eDraggableType.SelectionButton);

            button.transform.SetSiblingIndex(index);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TextMeshProUGUI>().text = i_GetLabel(item);

            DraggableItem draggableItem = button.GetComponent<DraggableItem>();
            if (draggableItem == null)
            {
                draggableItem = button.gameObject.AddComponent<DraggableItem>();
            }
            draggableItem.ResetEvents();
            draggableItem.AssignedSection = i_AssignedSection(item);

            draggableItem.draggableType = eDraggableType.SelectionButton;

            draggableItem.OnDropped += (droppedItem) => i_OnItemDropped(item);
            draggableItem.OnRemoved += () => i_OnItemRemoved(item);

            if (i_RemovalCondition != null)
            {
                Action removeAction = () => i_OnItemRemoved?.Invoke(item);
                removalConditions[button] = (() => i_RemovalCondition(item), removeAction);
            }

            index++;
        }
    }

    private void InsertButtonInSection(Transform section, Button button, eDraggableType type)
    {
        int insertIndex = 0;

        for (int i = 0; i < section.childCount; i++)
        {
            DraggableItem existingDraggable = section.GetChild(i).GetComponent<DraggableItem>();
            if (existingDraggable == null) continue;

            if (type == eDraggableType.SelectionButton && existingDraggable.draggableType == eDraggableType.SelectionButton)
            {
                insertIndex = i + 1;
            }
        }

        button.transform.SetParent(section, false);
        button.transform.SetSiblingIndex(insertIndex);
    }

    // public void ShowInputField()
    // {
    //     GameObject inputFieldObject = Instantiate(inputFieldPrefab, selectionParent);
    //     inputFieldObject.transform.localScale = Vector3.one;
    //     TMP_InputField inputField = inputFieldObject.GetComponent<TMP_InputField>();

    //     if (inputField == null)
    //     {
    //         Debug.LogError("InputFieldPrefab is missing a TMP_InputField component!");
    //         return;
    //     }

    //     inputField.text = "";
    //     inputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter value...";
    //     inputField.Select();
    //     inputField.ActivateInputField();

    //     GameObject confirmButtonObject = Instantiate(confirmButtonPrefab, selectionParent);
    //     confirmButtonObject.transform.localScale = Vector3.one;

    //     Button confirmButton = confirmButtonObject.GetComponent<Button>();
    //     if (confirmButton == null)
    //     {
    //         Debug.LogError("ConfirmButtonPrefab is missing a Button component!");
    //         return;
    //     }

    //     confirmButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm";

    //     confirmButton.onClick.RemoveAllListeners();
    //     confirmButton.onClick.AddListener(() => OnConditionValueEntered(inputField.text));
    // }


public void ShowInputField(
    Func<string, bool> validateInput,
    Func<string, string> formatInput,
    Action<string> onConfirm,
    Func<string, bool> canRemove,
    Action<string> onRemove,
    Transform clauseSection)
    {
        GameObject inputFieldObject = Instantiate(inputFieldPrefab, selectionParent);
        inputFieldObject.transform.localScale = Vector3.one;
        TMP_InputField inputField = inputFieldObject.GetComponent<TMP_InputField>();

        if (inputField == null)
        {
            Debug.LogError("InputFieldPrefab is missing a TMP_InputField component!");
            return;
        }

        inputField.text = "";
        inputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter value...";
        inputField.Select();
        inputField.ActivateInputField();

        GameObject confirmButtonObject = Instantiate(confirmButtonPrefab, selectionParent);
        confirmButtonObject.transform.localScale = Vector3.one;

        Button confirmButton = confirmButtonObject.GetComponent<Button>();
        if (confirmButton == null)
        {
            Debug.LogError("ConfirmButtonPrefab is missing a Button component!");
            return;
        }

        confirmButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            string rawInput = inputField.text;
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                Debug.LogWarning("No input provided.");
                return;
            }

            if (!validateInput(rawInput))
            {
                return;
            }

            string formatted = formatInput(rawInput);
            onConfirm(formatted);

            populateSelectionButtons(
                i_Items: new List<string> { formatted },
                i_OnItemDropped: val => onConfirm(val),
                i_GetLabel: val => formatted,
                i_ParentTransform: selectionParent,
                i_ButtonPool: selectionButtonPool,
                i_AssignedSection: val => clauseSection,
                i_ClearSelectionPanel: false,
                i_RemovalCondition: canRemove,
                i_OnItemRemoved: onRemove
            );
        });
    }


    public void ShowNumberInputOptions<T>(
        List<T> values,
        Action<T> onValueSelected,
        Func<T, bool> canRemove,
        Transform clauseSection
    )
    {
        ShowInputField(
            validateInput: raw => {
                if (string.IsNullOrWhiteSpace(raw)) return false;
                return int.TryParse(raw, out _);
            },
            formatInput: raw => raw.Trim(),
            onConfirm: formatted => {
                if (!int.TryParse(formatted, out int parsed)) return;

                // add custom entered number as if it was part of the list
                populateSelectionButtons(
                    i_Items: new List<int> { parsed },
                    i_OnItemDropped: val => onValueSelected((T)(object)parsed),
                    i_GetLabel: val => val.ToString(),
                    i_ParentTransform: selectionParent,
                    i_ButtonPool: selectionButtonPool,
                    i_AssignedSection: val => clauseSection,
                    i_ClearSelectionPanel: false,
                    i_RemovalCondition: val => canRemove((T)(object)parsed),
                    i_OnItemRemoved: val => { /* optional cleanup */ }
                );
            },
            canRemove: val => canRemove((T)(object)val),
            onRemove: val => { /* optional removal logic */ },
            clauseSection: clauseSection);


        populateSelectionButtons(
            i_Items: values,
            i_OnItemDropped: onValueSelected,
            i_GetLabel: val => val.ToString(),
            i_ParentTransform: selectionParent,
            i_ButtonPool: selectionButtonPool,
            i_AssignedSection: val => clauseSection,
            i_ClearSelectionPanel: false,
            i_RemovalCondition: canRemove
        );

        selectionParent.GetChild(selectionParent.childCount - 2).SetAsFirstSibling();
        selectionParent.GetChild(selectionParent.childCount - 1).SetSiblingIndex(1);
    }


    public void RefreshPanelButtons()
    {
        EvaluateQueryPanelButtons();
    }

    public void SetExecuteButtonInteractable(bool interactable)
    {
        executeButton.interactable = interactable;
    }

    // public void syncQueryUI()
    // {
    //     RefreshPanelButtons();
    //     UpdateQueryPreview();
    // }

    public void RenderQueryPreview(string queryString, bool isValid)
    {
        queryPreviewText.text = queryString;
        executeButton.interactable = isValid;
    }

    private void EvaluateQueryPanelButtons()
    {
        foreach (var pair in removalConditions.ToList())
        {
            Button button = pair.Key;
            var (condition, removeAction) = pair.Value;

            if (condition())
            {
                // removeAction?.Invoke();
                removalConditions.Remove(button);
                selectionButtonPool.Release(button);
            }
        }
    }


    private void ExecuteQuery()
    {
        GameManager.Instance.ExecuteQuery();
    }

    
    private string FormatString(string i_InputValue)
    {
        return i_InputValue.Trim('"');
    }

    public void PickDateTime()
    {
        throw new NotImplementedException();
    }



}
