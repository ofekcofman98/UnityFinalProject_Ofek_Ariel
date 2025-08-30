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

    [SerializeField] private GameObject inputFieldPrefab;
    [SerializeField] private GameObject confirmButtonPrefab;
    public Button executeButton;

    [SerializeField] private Transform selectSection;
    [SerializeField] private Transform fromSection;
    [SerializeField] private Transform whereSection;

    private IButtonPopulator<IQueryClause> clauseButtonPopulator;
    private IButtonPopulator<Table> tableSelectionPopulator;
    private IButtonPopulator<Column> columnSelectionPopulator;
    private IButtonPopulator<Column> conditionColumnSelectionPopulator;
    private IButtonPopulator<IOperatorStrategy> operatorSelectionPopulator;
    private IButtonPopulator<object> valueSelectionPopulator;
    private ValueInputPopulator<object> valueInputPopulator;

    public GameObject currentInputField;
    public GameObject currentConfirmButton;

    public TextMeshProUGUI queryPreviewText;
    private Dictionary<Button, (Func<bool> condition, Action removeAction)> removalConditions
        = new Dictionary<Button, (Func<bool>, Action)>();

    void Awake()
    {
        executeButton.onClick.AddListener(ExecuteQuery);
        selectionButtonPool = new ObjectPoolService<Button>(selectionButtonPrefab.GetComponent<Button>(), selectionParent, i_Capacity: 30);
        clauseButtonPool = new ObjectPoolService<Button>(ClausesButtonPrefab.GetComponent<Button>(), clausesParent, 5, 20);

    }

    public void SetClausePopulator(
        Action<IQueryClause> onDropped,
        Action<IQueryClause> onRemoved,
        Func<IQueryClause, Transform> assignedSection,
        Dictionary<IQueryClause, Button> activeButtons)
    {
        clauseButtonPopulator = new ClauseButtonPopulator<IQueryClause>(
            parent: clausesParent,
            buttonPrefab: ClausesButtonPrefab.GetComponent<Button>(),
            getLabel: clause => clause.DisplayName,
            assignedSection: assignedSection,
            onItemDropped: onDropped,
            onItemRemoved: onRemoved,
            activeButtons: activeButtons
        );
    }

    public void RenderClauseButtons(IEnumerable<IQueryClause> clauses)
    {
        clauseButtonPopulator?.PopulateButtons(clauses);
    }



    public void SetTablePopulator(
    Action<Table> onDropped,
    Action<Table> onRemoved,
    Func<Table, Transform> assignedSection,
    Func<Table, bool> removalCondition)
    {
        tableSelectionPopulator = new SelectionButtonPopulator<Table>(
            parent: selectionParent,
            buttonPrefab: selectionButtonPrefab.GetComponent<Button>(),
            getLabel: table => table.Name,
            assignedSection: assignedSection,
            onItemDropped: onDropped,
            onItemRemoved: onRemoved,
            removalCondition: removalCondition,
            removalDict: removalConditions
        );
    }

    public void RenderTableButtons(IEnumerable<Table> tables)
    {
        tableSelectionPopulator?.PopulateButtons(tables);
    }

    public void SetColumnPopulator(
        Action<Column> onDropped,
        Action<Column> onRemoved,
        Func<Column, Transform> assignedSection,
        Func<Column, bool> removalCondition)
    {
        columnSelectionPopulator = new SelectionButtonPopulator<Column>(
            parent: selectionParent,
            buttonPrefab: selectionButtonPrefab.GetComponent<Button>(),
            getLabel: column => column.Name,
            assignedSection: assignedSection,
            onItemDropped: onDropped,
            onItemRemoved: onRemoved,
            removalCondition: removalCondition,
            removalDict: removalConditions 
        );
    }
    public void RenderColumnButtons(IEnumerable<Column> columns)
    {
        columnSelectionPopulator?.PopulateButtons(columns);
    }

    public void SetConditionColumnPopulator(
        Action<Column> onDropped,
        Action<Column> onRemoved,
        Func<Column, Transform> assignedSection,
        Func<Column, bool> removalCondition,
        Func<Column, int> conditionIndexGetter)
    {
        conditionColumnSelectionPopulator = new SelectionButtonPopulator<Column>(
            parent: selectionParent,
            buttonPrefab: selectionButtonPrefab.GetComponent<Button>(),
            getLabel: column => column.Name,
            assignedSection: assignedSection,
            onItemDropped: onDropped,
            onItemRemoved: onRemoved,
            removalCondition: removalCondition,
            conditionIndexGetter: conditionIndexGetter,
            removalDict: removalConditions 
        );
    }

    public void RenderConditionColumnButtons(IEnumerable<Column> columns)
    {
        conditionColumnSelectionPopulator?.PopulateButtons(columns);
    }


    public void SetOperatorPopulator(
        Action<IOperatorStrategy> onDropped,
        Action<IOperatorStrategy> onRemoved,
        Func<IOperatorStrategy, Transform> assignedSection,
        Func<IOperatorStrategy, bool> removalCondition,
        Func<IOperatorStrategy, int> conditionIndexGetter)
    {
        operatorSelectionPopulator = new SelectionButtonPopulator<IOperatorStrategy>(
            parent: selectionParent,
            buttonPrefab: selectionButtonPrefab.GetComponent<Button>(),
            getLabel: op => op.GetSQLRepresentation(),
            assignedSection: assignedSection,
            onItemDropped: onDropped,
            onItemRemoved: onRemoved,
            removalCondition: removalCondition,
            conditionIndexGetter: conditionIndexGetter,
            removalDict: removalConditions
        );
    }

    public void RenderOperatorButtons(IEnumerable<IOperatorStrategy> operators)
    {
        operatorSelectionPopulator?.PopulateButtons(operators);
    }


    public void ShowValueInputOptions<T>(
        List<T> predefinedValues,
        Func<string, bool> validateInput,
        Func<string, string> formatInput,
        Func<string, T> parseInput,
        Action<T> onValueSelected,
        Func<T, bool> canRemove,
        Action<T> onRemoved,
        Transform clauseSection)
    {
        var valueInputPopulator = new ValueInputPopulator<T>(
            selectionParent,
            inputFieldPrefab,
            confirmButtonPrefab,
            selectionButtonPrefab,
            validateInput,
            formatInput,
            parseInput,
            conditionIndexGetter: _ => GameManager.Instance.CurrentQuery.whereClause.NewConditionIndex,
            removalDict: removalConditions
        );

        valueInputPopulator.Show(
            predefinedValues,
            onValueSelected,
            canRemove,
            onRemoved,
            assignedSection: _ => clauseSection
        );
    }

    public void RenderValueButtons(IEnumerable<object> values)
    {
        valueSelectionPopulator?.PopulateButtons(values);
    }



    // public void populateSelectionButtons<T>(
    //     IEnumerable<T> i_Items,
    //     Action<T> i_OnItemDropped,
    //     Func<T, string> i_GetLabel,
    //     Transform i_ParentTransform,
    //     Func<T, Transform> i_AssignedSection,
    //     // ObjectPoolService<Button> i_ButtonPool,
    //     Button i_ButtonPrefab,
    //     bool i_ClearSelectionPanel = true,
    //     Func<T, bool> i_RemovalCondition = null,
    //     Action<T> i_OnItemRemoved = null,
    //     Func<T, int> i_ConditionIndexGetter = null) //! ofek 15.8 
    // {
    //     if (i_Items == null || !i_Items.Any())
    //     {
    //         Debug.LogWarning("No items available for selection.");
    //         return;
    //     }

    //     if (i_ClearSelectionPanel)
    //     {
    //         foreach (Transform child in i_ParentTransform)
    //         {
    //             GameObject.Destroy(child.gameObject);
    //         }
    //     }

    //     int index = 0;
    //     foreach (T item in i_Items)
    //     {
    //         try
    //         {
    //             Button button = GameObject.Instantiate(i_ButtonPrefab, i_ParentTransform);
    //             button.transform.SetSiblingIndex(index++);

    //             // ✅ Assign label
    //             var label = button.GetComponentInChildren<TextMeshProUGUI>();
    //             var labelText = i_GetLabel(item);
    //             if (label != null)
    //             {
    //                 label.text = labelText;
    //                 SetButtonPreferredSize(button);
    //             }
    //             else
    //             {
    //                 Debug.LogError($"Missing label on button for: {labelText}");
    //             }

    //             CheckForHighlight(item, button);

    //             // ✅ Setup draggable
    //             var draggableItem = button.GetComponent<DraggableItem>();
    //             if (draggableItem == null)
    //                 draggableItem = button.gameObject.AddComponent<DraggableItem>();
    //             else
    //                 draggableItem.Reset();

    //             draggableItem.OriginalParent = i_ParentTransform;
    //             draggableItem.AssignedSection = i_AssignedSection(item);
    //             draggableItem.draggableType = eDraggableType.SelectionButton;

    //             draggableItem.OnDropped = null;
    //             draggableItem.OnRemoved = null;

    //             draggableItem.OnDropped += _ => i_OnItemDropped(item);
    //             if (i_OnItemRemoved != null)
    //                 draggableItem.OnRemoved += () => i_OnItemRemoved(item);

    //             if (i_RemovalCondition != null)
    //                 removalConditions[button] = (() => i_RemovalCondition(item), () => i_OnItemRemoved?.Invoke(item));

    //             if (i_ConditionIndexGetter != null)                             //! ofek 15.8 
    //                 draggableItem.ConditionIndex = i_ConditionIndexGetter(item);//! ofek 15.8 
    //         }
    //         catch (Exception ex)
    //         {
    //             Debug.LogError($"❌ Error creating button for {i_GetLabel(item)}: {ex.Message}");
    //         }
    //     }

    //     LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)i_ParentTransform);
    // }


    // private void InsertButtonInSection(Transform section, Button button, eDraggableType type)
    // {
    //     int insertIndex = 0;

    //     for (int i = 0; i < section.childCount; i++)
    //     {
    //         DraggableItem existingDraggable = section.GetChild(i).GetComponent<DraggableItem>();
    //         if (existingDraggable == null) continue;

    //         if (type == eDraggableType.SelectionButton && existingDraggable.draggableType == eDraggableType.SelectionButton)
    //         {
    //             insertIndex = i + 1;
    //         }
    //     }

    //     button.transform.SetParent(section, false);
    //     button.transform.SetSiblingIndex(insertIndex);
    // }

    // //TODO pass to another class 
    // private void SetButtonPreferredSize(Button button, float padding = 20f, float fixedHeight = 60f)
    // {
    //     var label = button.GetComponentInChildren<TextMeshProUGUI>();
    //     var layout = button.GetComponent<LayoutElement>();

    //     if (label == null || layout == null)
    //     {
    //         Debug.LogError("Button is missing either TMP label or LayoutElement.");
    //         return;
    //     }

    //     // Force update so preferred width is valid
    //     LayoutRebuilder.ForceRebuildLayoutImmediate(label.rectTransform);

    //     float preferredWidth = LayoutUtility.GetPreferredWidth(label.rectTransform);
    //     layout.preferredWidth = preferredWidth + padding;
    //     layout.preferredHeight = fixedHeight; // Optional: keep button heights aligned
    // }


    // public void ShowInputField(
    //     Func<string, bool> validateInput,
    //     Func<string, string> formatInput,
    //     Action<string> onValueSelected,
    //     Func<string, bool> canRemove,
    //     Action<string> onRemove,
    //     Transform clauseSection)
    // {

    //     if (currentInputField != null) Destroy(currentInputField);
    //     if (currentConfirmButton != null) Destroy(currentConfirmButton);

    //     currentInputField = Instantiate(inputFieldPrefab, selectionParent);
    //     currentInputField.transform.localScale = Vector3.one;
    //     TMP_InputField inputField = currentInputField.GetComponent<TMP_InputField>();

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
    //     confirmButton.onClick.AddListener(() =>
    //     {
    //         string rawInput = inputField.text;
    //         if (string.IsNullOrWhiteSpace(rawInput))
    //             return;

    //         if (!validateInput(rawInput))
    //             return;

    //         string formatted = formatInput(rawInput);

    //         Column column = GameManager.Instance.CurrentQuery?.whereClause.newCondition?.Column;
    //         if (column == null)
    //         {
    //             Debug.LogWarning("No column assigned to condition.");
    //             return;
    //         }

    //         object parsedValue = ParseToCorrectType(formatted, column.DataType);
    //         if (parsedValue == null)
    //         {
    //             Debug.LogWarning("Parsed value is null.");
    //             return;
    //         }

    //         Destroy(currentInputField);
    //         Destroy(confirmButtonObject);

    //         populateSelectionButtons(
    //             i_Items: new List<string> { formatted },
    //             i_OnItemDropped: val => onValueSelected?.Invoke(formatted),
    //             i_GetLabel: val => formatted,
    //             i_ParentTransform: selectionParent,
    //             i_ButtonPrefab: selectionButtonPrefab.GetComponent<Button>(),
    //             i_AssignedSection: val => clauseSection,
    //             i_ClearSelectionPanel: false,
    //             i_RemovalCondition: canRemove,
    //             i_OnItemRemoved: onRemove
    //         );
    //     });
    // }


    // public void ShowNumberInputOptions<T>(
    //     List<T> values,
    //     Action<T> onValueSelected,
    //     Func<T, bool> canRemove,
    //     Transform clauseSection,
    //     Action<T> onRemove = null
    // )
    // {
    //     ShowInputField
    //     (
    //         validateInput: raw =>
    //         {
    //             if (string.IsNullOrWhiteSpace(raw)) return false;
    //             return int.TryParse(raw, out _);
    //         },
    //         formatInput: raw => raw.Trim(),
    //         onValueSelected: formatted =>
    //         {
    //             if (!int.TryParse(formatted, out int parsed)) return;
    //             T typedValue = (T)(object)parsed;

    //             onValueSelected(typedValue);

    //             // add custom entered number as if it was part of the list
    //             populateSelectionButtons(
    //                 i_Items: new List<T> { typedValue },
    //                 i_OnItemDropped: onValueSelected, //val => onValueSelected((T)(object)parsed),
    //                 i_GetLabel: val => val.ToString(),
    //                 i_ParentTransform: selectionParent,
    //                 i_ButtonPrefab: selectionButtonPrefab.GetComponent<Button>(),
    //                 i_AssignedSection: val => clauseSection,
    //                 i_ClearSelectionPanel: false,
    //                 i_RemovalCondition: canRemove,//val => canRemove((T)(object)parsed),
    //                 i_OnItemRemoved: onRemove //val => { /* optional cleanup */ }
    //             );
    //         },
    //         canRemove: raw => int.TryParse(raw, out int parsed) && canRemove((T)(object)parsed),
    //         onRemove: raw =>
    //         {
    //             if (int.TryParse(raw, out int parsed))
    //                 onRemove((T)(object)parsed);
    //         },
    //         clauseSection: clauseSection
    //     );


    //     populateSelectionButtons(
    //         i_Items: values,
    //         i_OnItemDropped: onValueSelected,
    //         i_GetLabel: val => val.ToString(),
    //         i_ParentTransform: selectionParent,
    //         i_ButtonPrefab: selectionButtonPrefab.GetComponent<Button>(),
    //         i_AssignedSection: val => clauseSection,
    //         i_ClearSelectionPanel: false,
    //         i_RemovalCondition: canRemove,
    //         i_OnItemRemoved: onRemove
    //     );

    //     selectionParent.GetChild(selectionParent.childCount - 2).SetAsFirstSibling();
    //     selectionParent.GetChild(selectionParent.childCount - 1).SetSiblingIndex(1);
    // }


    public void RefreshPanelButtons()
    {
        EvaluateQueryPanelButtons();
    }

    // public void SetExecuteButtonInteractable(bool interactable)
    // {
    //     executeButton.interactable = interactable;
    // }

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

    private void CheckForHighlight<T>(T item, Button button)
    {
        SQLMissionData currentMission = MissionsManager.Instance.CurrentMission as SQLMissionData;
        if (currentMission == null || !currentMission.isTutorial)
        {
            return;
        }
        Debug.Log("[CheckForHighlight] currentMission != null && currentMission.isTutorial");

        if (typeof(T) == typeof(Column))
        {
            Column col = item as Column;
            if (col != null && currentMission.requiredColumns.Contains(col.Name))
            {
                Debug.Log("[CheckForHighlight] its col!");
                if (button.TryGetComponent<UIHighlightable>(out var hl))
                {
                    HighlightManager.Instance.RegisterHighlight(hl);
                }
            }
        }
        else if (typeof(T) == typeof(Table))
        {
            Table table = item as Table;
            if (table != null && table.Name == currentMission.requiredTable)
            {
                Debug.Log("[CheckForHighlight] its table!");
                if (button.TryGetComponent<UIHighlightable>(out var hl))
                {
                    HighlightManager.Instance?.RegisterHighlight(hl);
                }
            }
        }
    }

    private void ExecuteQuery()
    {
        GameManager.Instance.ExecuteQuery();
    }

    private object ParseToCorrectType(string input, eDataType dataType)
    {
        switch (dataType)
        {
            case eDataType.Integer:
                if (int.TryParse(input, out int intVal))
                    return intVal;
                break;

            case eDataType.String:
                return input; // No parsing needed

            case eDataType.DateTime:
                if (DateTime.TryParse(input, out DateTime dateVal))
                    return dateVal;
                break;
        }

        Debug.LogWarning($"Could not parse input: {input} for type {dataType}");
        return null;
    }

    // private string FormatString(string i_InputValue)
    // {
    //     return i_InputValue.Trim('"');
    // }

    public void PickDateTime()
    {
        throw new NotImplementedException();
    }

    public Transform MatchClauseToSection(IQueryClause i_Clause)
    {
        switch (i_Clause.DisplayName)
        {
            case QueryConstants.Select: return selectSection;
            case QueryConstants.From: return fromSection;
            case QueryConstants.Where:
            case QueryConstants.And: return whereSection;
            default: return selectSection; // Fallback
        }
    }

    public void DisposeValueInputPopulator()
    {
        valueInputPopulator?.Dispose();
        valueInputPopulator = null;

        if (currentInputField)
        {
            Destroy(currentInputField);
            currentInputField = null;
        }

        if (currentConfirmButton)
        {
            Destroy(currentConfirmButton);
            currentConfirmButton = null;
        }
    }


    public void ClearClauseSections(Transform[] clauseSections)
    {
        foreach (Transform section in clauseSections)
        {
            foreach (Transform child in section)
            {
                if (child.TryGetComponent<Button>(out var btn))
                {
                    // This ensures we're only destroying buttons that are not part of the pool
                    Destroy(btn.gameObject);
                }
            }
        }

        DisposeValueInputPopulator();

        // valueInputPopulator?.Dispose();
        // valueInputPopulator = null;

        // if (currentInputField != null)
        // {
        //     Destroy(currentInputField);
        //     currentInputField = null;
        // }

        // if (currentConfirmButton != null)
        // {
        //     Destroy(currentConfirmButton);
        //     currentConfirmButton = null;
        // }
    }



}

