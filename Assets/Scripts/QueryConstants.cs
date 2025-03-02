using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QueryConstants
{
    public const string Select = "SELECT ";
    public const string From = "\nFROM ";
    public const string Where = "\nWHERE ";
    public const string Comma = ", ";
    public const string Empty = "";
    public const string And = "\n AND ";
    public const string OR = "\n OR ";

    // private static readonly Dictionary<eOperator, string> OperatorMap = new Dictionary<eOperator, string>
    // {
    //     { eOperator.Equal, "=" },
    //     { eOperator.GreaterThan, ">" },
    //     { eOperator.LessThan, "<" },
    //     { eOperator.GreaterEqualThan, ">=" },
    //     { eOperator.LessEqualThan, "<=" },
    //     { eOperator.NotEqual, "!=" },
    //     { eOperator.Like, "LIKE" },
    //     { eOperator.Between, "BETWEEN" }
    // };

    // public static string GetOperatorString(eOperator op)
    // {
    //     return OperatorMap.TryGetValue(op, out string opString) ? opString : throw new ArgumentOutOfRangeException(nameof(op), op, null);
    // }

    public static string FormatValue(object value)
    {
        return value is string ? $"'{value}'" : value.ToString();
    }

    public static string FormatSupabaseValue(object value)
    {
        if (value is string)
            return $"\"{value}\""; // Strings need quotes

        if (value is int || value is float || value is double)
            return value.ToString(); // Numbers stay as they are

        return value.ToString(); // Default fallback
    }
}

