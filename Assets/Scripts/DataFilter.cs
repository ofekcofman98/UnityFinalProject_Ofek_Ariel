using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TableDataFilter
{
    private static readonly HashSet<string> HiddenColumnNames = new HashSet<string>
    {
        "photo_url",
        "prefab_id",
        "witness_id"
        // "id" // Add others as needed
    };

    public static IEnumerable<Column> GetVisibleColumns(IEnumerable<Column> columns)
    {
        return columns.Where(col => !HiddenColumnNames.Contains(col.Name.ToLower()));
    }

    public static bool IsVisible(Column column)
    {
        return !HiddenColumnNames.Contains(column.Name.ToLower());
    }
}
