using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;

public class Query : MonoBehaviour
{
    private string m_Table;
    private List<string> m_Columns;
    private Dictionary<string, object> m_Conditions;
    private bool m_IsValid;

    private const string k_Select = "SELECT ";
    private const string k_From = "\nFROM ";
    private const string k_Where = "";

    private string selectPart;
    private string fromPart;
    private string wherePart;
    private string sqlQueryStr;


    public string Table 
    {
        get { return m_Table;}
        private set 
        {
                m_Table = value;
                Validate();
        }
    }

    public List<string> Columns
    {
        get { return new List<string>(m_Columns);}
    }

    public Query(string i_Table, IEnumerable<string> i_Columns = null)
    {
        if (string.IsNullOrWhiteSpace(i_Table))
        {
            throw new ArgumentException("Table name cannot be null or empty.");
        }

        m_Table = i_Table;
        m_Columns = m_Columns ?? new List<string>();
        m_Conditions = new Dictionary<string, object>();

        selectPart = k_Select;
        fromPart = k_From;
        wherePart = k_Where;
        sqlQueryStr = "";

        Validate();
    }


    public bool IsValid()
    {
        return m_IsValid;
    }

    public Query Select(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
        {
            m_Columns.Clear();
            selectPart = k_Select + "*"; // Default to SELECT *
        }
        else
        {
            m_Columns = columns.ToList();
            selectPart = k_Select + string.Join(", ", m_Columns);
        }
        Validate();
        return this;
    }

    public Query Where(string column, object value)
    {
        if (string.IsNullOrWhiteSpace(column))
        {
            throw new ArgumentException("Column name cannot be null or whitespace.");
        }

        m_Conditions[column] = value;
        wherePart = "\nWHERE " + string.Join(" AND ", m_Conditions.Select(c => $"{c.Key} = @{c.Key}"));

        return this;
    }

    public bool HasWhereClause()
    {
        return m_Conditions.Count > 0;
    }


    public string BuildQuery()
    {
        if (!m_IsValid)
        {
            Debug.LogError("❌ BuildQuery() called but query is not valid!");
            return "Invalid Query"; 
        }

        fromPart = k_From + m_Table;
        sqlQueryStr = selectPart + fromPart + wherePart;

        Debug.Log("🔵 BuildQuery() Generated: " + sqlQueryStr); // Debugging output

        return sqlQueryStr;
    }


    private void Validate()
    {
        m_IsValid = !string.IsNullOrWhiteSpace(m_Table);
    }
}
