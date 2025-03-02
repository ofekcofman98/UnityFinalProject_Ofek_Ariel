using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]

public class Condition
{
    private Column m_Column;
    private IOperatorStrategy m_Operator;
    private object m_Value;
    public event Action OnConditionUpdated;  

    private string m_ConditionString;
    public string ConditionStringSupaBase {get; set;}
    
    public string ColumnPart => Column?.Name ?? QueryConstants.Empty;
    public string OperatorPart => m_Operator != null ? m_Operator.GetSQLRepresentation() : QueryConstants.Empty;
    public string OperatorPartSupaBase => m_Operator != null ? m_Operator.GetSupabaseFormat() : QueryConstants.Empty;

    public string ValuePart => Value != null ? QueryConstants.FormatValue(Value) : QueryConstants.Empty;
    public string SupabaseValuePart => Value != null ? QueryConstants.FormatSupabaseValue(Value) : QueryConstants.Empty;

    public string ConditionString
    {
        get => m_ConditionString;
        set
        {
            m_ConditionString = value;
            OnConditionUpdated?.Invoke();
        }
    }

    public Column Column
    {
        get => m_Column;
        set
        {
            m_Column = value;
            updateConditionString();
        }
    }

    public IOperatorStrategy Operator
    {
        get => m_Operator;
        set
        {
            m_Operator = value;
            updateConditionString();
        }
    }

    public object Value
    {
        get => m_Value;
        set
        {
            m_Value = value;
            updateConditionString();
        }
    }


    private void updateConditionString()
    {
        ConditionStringSupaBase = $"{ColumnPart}={OperatorPartSupaBase}.{SupabaseValuePart}";
        ConditionString = $"{ColumnPart} {OperatorPart} {ValuePart}";
        
        Debug.Log($"current condition is {ConditionString}");
        Debug.Log($"current SupaBase formatted condition is {ConditionStringSupaBase}");
    }
}
