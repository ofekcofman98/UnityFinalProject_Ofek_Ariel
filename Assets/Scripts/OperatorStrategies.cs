using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOperatorStrategy
{
    string GetSQLRepresentation();
    string GetSupabaseFormat();
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
    public string GetSupabaseFormat() => "eq";
}


public class GreaterThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => ">";
    public string GetSupabaseFormat() => "gt";
}


public class LessThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "<";
    public string GetSupabaseFormat() => "lt";
}


public class GreaterEqualThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => ">=";
    public string GetSupabaseFormat() => "gte";
}


public class LessEqualThanOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "<=";
    public string GetSupabaseFormat() => "lte";
}


public class NotEqualOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "!=";
    public string GetSupabaseFormat() => "neq";
}


public class LikeOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "LIKE";
    public string GetSupabaseFormat() => "like";
}
public class BetweenOperator : IOperatorStrategy
{
    public string GetSQLRepresentation() => "BETWEEN";
    public string GetSupabaseFormat() => "between";
}


