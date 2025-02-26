using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Column
{
    public string Name { get; private set; }
    // public string DataType { get; private set; }

    public Column(string i_Name)
    {
        Name = i_Name;
    }
}
