using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOperatorStrategy
{
    string GetSQLRepresentation();
    string FormatOperatorForSupaBase(Column i_Column);
    string FormatValueForSupabase(Column i_Column, string i_Value);
}

public static class OperatorFactory
{
    public static List<IOperatorStrategy> GetAllOperators()
    {
        return new List<IOperatorStrategy>
        {
            new EqualOperator(),
            new GreaterThanOperator(),
            new LessThanOperator(),
            new GreaterEqualThanOperator(),
            new LessEqualThanOperator(),
            new NotEqualOperator(),
            new LikeOperator(),
            new BetweenOperator()
        };
    }
}


public class EqualOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "=";
    public string FormatOperatorForSupaBase(Column i_Column)
    {
        return i_Column.DataType == eDataType.String ? "ilike" : "eq";
    }
    public string FormatValueForSupabase(Column i_Column, string i_Value)
    {
        return i_Column.DataType == eDataType.String ? i_Value.ToLower() : i_Value;
    }
}


public class GreaterThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => ">";
    public string FormatOperatorForSupaBase(Column i_Column) => "gt";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value;
}


public class LessThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "<";
    public string FormatOperatorForSupaBase(Column i_Column) => "lt";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value;

}


public class GreaterEqualThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => ">=";
    public string FormatOperatorForSupaBase(Column i_Column) => "gte";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value;

}


public class LessEqualThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "<=";
    public string FormatOperatorForSupaBase(Column i_Column) => "lte";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value;

}


public class NotEqualOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "!=";
    public string FormatOperatorForSupaBase(Column i_Column) => "neq";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value;
}


public class LikeOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "LIKE";
    public string FormatOperatorForSupaBase(Column i_Column) => "ilike";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value.Replace("%", "%25");
}

public class BetweenOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "BETWEEN";
    public string FormatOperatorForSupaBase(Column i_Column) => "between";
    public string FormatValueForSupabase(Column i_Column, string i_Value) => i_Value;
}


