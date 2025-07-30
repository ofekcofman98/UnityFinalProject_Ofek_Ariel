using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JObjectRowAdapter : IDataGridRowAdapter<JObject>
{

    private readonly GameObject cellPrefab;
        private readonly List<string> _columns;

        public JObjectRowAdapter(List<string> columnNames)
        {
            _columns = columnNames;
        }

        public List<string> GetColumnValues(JObject row)
        {
            return _columns
                .Select(col =>
                    (col == "portrait" || col == "name") ? "" :
                    row[col]?.ToString() ?? "—"
                )
                .ToList();
        }

        public Texture2D GetPortrait(JObject row)
        {
            if (row.TryGetValue("__personId", out var idToken))
            {
                var person = PersonDataManager.Instance.GetById(idToken.ToString());
                return person?.portrait;
            }

            return null;
        }

        public string GetDisplayName(JObject row)
        {
            if (row.TryGetValue("__name", out var nameToken))
            {
                return nameToken.ToString();
            }

            return null;
        }


        public IDataGridCell CreateCell(JObject row, string columnName)
    {
        if (columnName == "portrait")
        {
            var portrait = GetPortrait(row);
            return new PortraitCell(portrait, cellPrefab);
        }

        string value = columnName switch
        {
            "name" => GetDisplayName(row),
            _ => row[columnName]?.ToString() ?? "—"
        };

        return new TextCell(value, cellPrefab);
    }

}
