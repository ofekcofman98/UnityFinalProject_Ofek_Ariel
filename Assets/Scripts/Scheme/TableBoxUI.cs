using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TableBoxUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tableNameText;
    [SerializeField] private Transform columnListParent;
    [SerializeField] private GameObject columnTextPrefab;

    private Dictionary<string, RectTransform> columnLabelMap = new Dictionary<string, RectTransform>();
    private Dictionary<string, GameObject> columnGOMap = new();

    public void Init(Table i_Table)
    {
        tableNameText.text = i_Table.Name;
        foreach(Column column in i_Table.Columns)
        {
            var columnGO = Instantiate(columnTextPrefab, columnListParent);
            columnGO.GetComponentInChildren<TextMeshProUGUI>().text = column.Name;

            RectTransform rt = columnGO.GetComponentInChildren<TextMeshProUGUI>().transform.parent.GetComponent<RectTransform>();
            columnLabelMap[column.Name.ToLowerInvariant().Trim()] = rt;
        }

        
    }

    public RectTransform GetColumnRect(string i_ColumnName)
    {
        return columnLabelMap.TryGetValue(i_ColumnName.ToLowerInvariant().Trim(), out RectTransform o_Rect) ? o_Rect : null;
    }
}
