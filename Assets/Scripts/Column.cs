using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eDataType
{
    Integer,
    String,
    Char,
}

[System.Serializable]
public class Column
{
    public string Name { get; private set; }
    // public IDataTypeStrategy<T> DataType { get; set; }
    public eDataType DataType { get; set; }

    public Column(string i_Name)
    {
        Name = i_Name;
    }
}
