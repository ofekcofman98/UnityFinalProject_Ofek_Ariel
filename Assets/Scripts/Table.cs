using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Table
{
    public string Name { get; private set; }
    public List<Column> Columns1 { get; private set; }

    // public List<string> Columns { get; private set; }
    public bool IsUnlocked { get; private set; }

    public Table(string i_TableName, bool i_IsUnlocked = false)
    {
        Name = i_TableName;
        IsUnlocked = i_IsUnlocked;
        // Columns = new List<string>();
        Columns1 = new List<Column>();
    }

    public void SetColumns1(List<Column> i_Columns)
    {
        Columns1 = i_Columns;
    }

    // public void SetColumns(List<string> i_Columns)
    // {
    //     Columns = i_Columns;
    // }


    public void UnlockTable()
    {
        IsUnlocked = true;
    }

    public void LockTable()
    {
        IsUnlocked = false;
    }


}
