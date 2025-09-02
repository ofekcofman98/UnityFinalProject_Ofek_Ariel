using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TableBoxUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tableNameText;
    [SerializeField] private Transform columnListParent;
    [SerializeField] private GameObject columnTextPrefab;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private CanvasGroup canvasGroup;


    private Dictionary<string, RectTransform> columnLabelMap = new Dictionary<string, RectTransform>();
    private Dictionary<string, GameObject> columnGOMap = new();

    public void Init(Table i_Table)
    {
        foreach (Transform child in columnListParent)
        {
            Destroy(child.gameObject);
        }

        columnLabelMap.Clear();

        tableNameText.text = i_Table.Name;

        List<Column> visibleColumns = TableDataFilter.GetVisibleColumns(i_Table.Columns).ToList();

        foreach (Column column in visibleColumns)
        {
            GameObject columnGO = Instantiate(columnTextPrefab, columnListParent);
            columnGO.GetComponentInChildren<TextMeshProUGUI>().text = column.Name;

            RectTransform rt = columnGO.GetComponentInChildren<TextMeshProUGUI>().transform.parent.GetComponent<RectTransform>();
            columnLabelMap[column.Name.ToLowerInvariant().Trim()] = rt;
        }

        bool isUnlocked = i_Table.IsUnlocked;
        canvasGroup.alpha = isUnlocked ? 1f : 0.4f;
        lockOverlay.SetActive(!isUnlocked);
    }

    public RectTransform GetColumnRect(string i_ColumnName)
    {
        return columnLabelMap.TryGetValue(i_ColumnName.ToLowerInvariant().Trim(), out RectTransform o_Rect) ? o_Rect : null;
    }
}
