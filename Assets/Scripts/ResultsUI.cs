using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    [SerializeField] private DataGridDisplayer dataGridDisplayer;
    [SerializeField] private Popup popup;

    public void ShowResults(JArray rows, List<Column> columns, string tableName)
    {
        if (rows == null || columns == null || columns.Count == 0)
        {
            Debug.LogWarning("⚠️ Cannot display results: rows or columns are null/empty");
            return;
        }

        List<JObject> rowObjects = rows.Select(token => (JObject)token).ToList();

        dataGridDisplayer.DisplayGrid<JObject>(
            columns.Select(c => c.Name).ToList(),
            Enumerable.Repeat(100f, columns.Count).ToList(),
            rowObjects,
            row => columns.Select(c => row[c.Name]?.ToString() ?? "—").ToList(),
            tableName.ToLower() == "persons" ? new List<IDataGridAction<JObject>> { new AddSuspectAction() } : null
        );

        Open();
    }

    public void Open()
    {
        popup.Open();
    }

    public void Close()
    {
        popup.Close();
    }
}
