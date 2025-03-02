using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public interface IDataTypeStrategy<T>
{
    string FormatValue(T i_Value);
    List<T> GetInputOptions(); 
    // bool SupportsManualInput(); 
}

public class IntegerDataType : IDataTypeStrategy<int>
{
    private static readonly List<int> predefinedValues = new List<int> { 10, 20, 30, 40, 50, 100 };

    public string FormatValue(int i_Value)
    {
        return i_Value.ToString();
    }

    public List<int> GetInputOptions()
    {
        return predefinedValues;
    }
}

public class StringDataType : IDataTypeStrategy<string>
{
    public string FormatValue(string i_Value)
    {
        return $"'{i_Value}'";
    }

    public List<string> GetInputOptions()
    {
        return new List<string>(); 
    }
}




