using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eDataType
{
    Integer,
    String,
    DateTime,
}

[System.Serializable]
public class Column
{

    [JsonProperty] public string Name { get; set; }
    public eDataType DataType { get; set; }
    
    [NonSerialized]
    public Table ParentTable;

    public Column(string i_Name, eDataType i_DataType)
    {
        Name = i_Name;
        DataType = i_DataType;
    }
}
