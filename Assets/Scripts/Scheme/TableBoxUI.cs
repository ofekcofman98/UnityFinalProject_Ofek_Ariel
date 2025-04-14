using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TableBoxUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tableNameText;
    [SerializeField] private Transform columnListParent;
    [SerializeField] private TextMeshProUGUI columnTextPrefab;

    private Dictionary<string, RectTransform> columnLabelMap = new Dictionary<string, RectTransform>();

    public void Init(Table i_Table)
    {
        tableNameText.text = i_Table.Name;

        Debug.Log($"INIT to table: {i_Table.Name}, columns count: {i_Table.Columns.Count}");

        foreach(Column column in i_Table.Columns)
        {
            Debug.Log($"📦 Creating column: {column.Name} for table {i_Table.Name}");

            var columnText = Instantiate(columnTextPrefab, columnListParent);
            columnText.text = $"{column.Name}";

            columnLabelMap[column.Name] = columnText.GetComponent<RectTransform>();
        }
    }

    public RectTransform GetColumnRect(string i_ColumnName)
    {
        return columnLabelMap.TryGetValue(i_ColumnName, out var rect) ? rect : null;
    }
}
