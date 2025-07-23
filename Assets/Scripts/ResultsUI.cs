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
    List<string> columnNames;
    List<float> columnWidths;

    bool isPersonsTable = tableName.ToLower() == "persons";

    if (isPersonsTable)
    {
        // Force correct column order: person_id, portrait, name, then the rest
        columnNames = new List<string> { "person_id", "portrait", "name" };

        // Add remaining columns (excluding person_id again and our added ones)
        columnNames.AddRange(columns
            .Select(c => c.Name)
            .Where(name => name != "person_id" && name != "__portrait" && name != "__name" && name != "portrait" && name != "name")
        );

        // Widths aligned with names
        columnWidths = new List<float> { 100f, 60f, 100f };
        columnWidths.AddRange(Enumerable.Repeat(100f, columnNames.Count - 3));
    }
    else
    {
        columnNames = columns.Select(c => c.Name).ToList();
        columnWidths = Enumerable.Repeat(100f, columnNames.Count).ToList();
    }

    // ✅ Display using correct order
    dataGridDisplayer.DisplayGrid<JObject>(
        columnNames,
        columnWidths,
        rowObjects,
        row =>
        {
            var values = new List<string>();
            foreach (string col in columnNames)
            {
                if (col == "portrait" || col == "name") continue; // handled visually
                values.Add(row[col]?.ToString() ?? "—");
            }
            return values;
        },
        isPersonsTable ? new List<IDataGridAction<JObject>> { new AddSuspectAction() } : null,
        isPersonsTable
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
