using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]

public class Condition
{
    private Column m_Column;

    [JsonConverter(typeof(OperatorConverter))]
    private IOperatorStrategy m_Operator;
    [JsonProperty] private string m_OperatorId;
    private object m_Value;
    public event Action OnConditionUpdated;

    private string m_ConditionString;
    public string ConditionStringSupaBase { get; private set; }

    public string ColumnPart => Column?.Name ?? QueryConstants.Empty;
    public string OperatorPart => m_Operator != null ? m_Operator.GetSQLRepresentation() : QueryConstants.Empty;
    public string OperatorPartSupaBase => m_Operator != null ? m_Operator.FormatOperatorForSupaBase(m_Column) : QueryConstants.Empty;

    public string ValuePart => Value != null ? QueryConstants.FormatValue(Value) : QueryConstants.Empty;
    public string SupabaseValuePart => Value != null ? QueryConstants.FormatSupabaseValue(Value) : QueryConstants.Empty;

    public bool IsComplete => Column != null && Operator != null && Value != null;

    public string FormattedValueForOperator =>
        (m_Operator != null && SupabaseValuePart != null) ?
        m_Operator.FormatValueForSupabase(m_Column, SupabaseValuePart) :
        QueryConstants.Empty;

    public string ConditionString
    {
        get => m_ConditionString;
        set
        {
            if (m_ConditionString != value)
            {
                m_ConditionString = value;
                OnConditionUpdated?.Invoke();
            }
        }
    }

    public Column Column
    {
        get => m_Column;
        set
        {
            if (m_Column != value)
            {
                m_Column = value;
                updateConditionString();
            }
        }
    }
    [JsonIgnore]
    public IOperatorStrategy Operator
    {
        get => m_Operator;
        set
        {
            if (m_Operator != value)
            {
                m_Operator = value;
                m_OperatorId = value?.GetOperatorId(); // You’ll define this

                updateConditionString();
            }
        }
    }

    public object Value
    {
        get => m_Value;
        set
        {
            if (m_Value != value)
            {
                m_Value = value;
                updateConditionString();
            }
        }
    }


    private void updateConditionString()
    {
        if (!string.IsNullOrEmpty(ColumnPart) &&
            !string.IsNullOrEmpty(OperatorPart) &&
            !string.IsNullOrEmpty(ValuePart))
        {
            ConditionString = $"{ColumnPart} {OperatorPart} {ValuePart}";
        }
        else
        {
            ConditionString = QueryConstants.Empty;
        }

        // Build Supabase-facing string (column=op.value)
        if (!string.IsNullOrEmpty(ColumnPart) &&
            !string.IsNullOrEmpty(OperatorPartSupaBase) &&
            !string.IsNullOrEmpty(SupabaseValuePart))
        {
            ConditionStringSupaBase = $"{ColumnPart}={OperatorPartSupaBase}.{SupabaseValuePart}";
        }
        else
        {
            ConditionStringSupaBase = QueryConstants.Empty;
        }

    }

    private string GetCorrectSupabaseValue()
    {
        if (m_Value is int or float or double)
        {
            return FormattedValueForOperator; // just raw number, no quotes
        }
        else
        {
            return $"\"{FormattedValueForOperator}\""; // wrap string/boolean
        }
    }

    public void Refresh()
    {
        if (Operator == null && !string.IsNullOrEmpty(m_OperatorId))
        {
            Operator = OperatorFactory.GetOperatorById(m_OperatorId);
        }

        updateConditionString();
    }
}
