using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QueryConstants
{
    public const string Select = "SELECT";
    public const string From = "FROM";
    public const string Join = "JOIN";
    public const string Where = "WHERE";
    public const string Comma = ", ";
    public const string Empty = "";
    public const string And = "AND";
    public const string OR = "OR";

    public static string FormatValue(object value)
    {
        return value is string ? $"'{value}'" : value.ToString();
    }

    public static string FormatSupabaseValue(object value)
    {
        if (value is string str)
            return str.ToLower();

        if (value is int || value is float || value is double)
            return value.ToString();

        if (value is DateTime dateTime)
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");

        return value?.ToString() ?? "null";

    }
}

