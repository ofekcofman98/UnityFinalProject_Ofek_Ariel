    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SelectClause : IQueryClause
    {
        public string DisplayName => QueryConstants.Select;
        public List<Column> Columns { get; set; }
        public string SelectPart { get; private set; } = QueryConstants.Empty;
        public bool isClicked { get; private set; } = false;
        public bool isAvailable { get; set; } = true;

        public SelectClause()
        {
            Columns = new List<Column>();
        }
        
        public void Toggle()
        {
            isClicked = !isClicked;

            if (!isClicked)
            {
                ClearColumns();
            }

            UpdateString();
            // onSelectChanged?.Invoke();
        }


        public void AddColumn(Column i_ColumnToAdd)
        {
            if (!Columns.Contains(i_ColumnToAdd))
            {
                Columns.Add(i_ColumnToAdd);
                UpdateString();
            }
        }
        public void RemoveColumn(Column i_ColumnToRemove)
        {
            if (Columns.Remove(i_ColumnToRemove))
            {
                Columns.Remove(i_ColumnToRemove);
                UpdateString();
            }
        }

        public void ClearColumns()
        {
            Columns.Clear();
            UpdateString();
        }
        
        public void UpdateString()
        {
            if (isClicked)
            {
                SelectPart = QueryConstants.Select;
                if (Columns.Count > 0)
                {
                    SelectPart += " " + string.Join(QueryConstants.Comma, Columns.Select(col => col.Name));
                }
            }
            else
            {
                SelectPart = QueryConstants.Empty;
            }
        }

        public string ToSQL()
        {
            return SelectPart;
        }

        public string ToSupabase()
        {
            return Columns.Count > 0 ? string.Join(QueryConstants.Comma , Columns.Select(col => col.Name)) : "*";
        }

        public bool NotEmpty()
        {
            return Columns.Count > 0;
        }

        public void OnQueryUpdated(Query query)
        {
            if (query.fromClause.GetTable() == null)
            {
                Reset();
            }

        }

    public void Reset()
    {
        // isClicked = false;
        SelectPart = QueryConstants.Select;
        ClearColumns();
    }
}
