using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Table
{
    public string Name { get; set; }
    public List<Column> Columns { get; set; }
    public List<ForeignKey> ForeignKeys { get; set; }
    public bool IsUnlocked { get; set; }
    public static event Action<Table> OnTableUnlocked;

    public Table(string i_TableName, bool i_IsUnlocked = false)
    {
        Name = i_TableName;
        IsUnlocked = i_IsUnlocked;
        Columns = new List<Column>();
        ForeignKeys = new List<ForeignKey>();
    }

    public void SetColumns(List<Column> i_Columns)
    {
        Columns = i_Columns;
    }
    public void SetForeignKeys(List<ForeignKey> i_ForeignKeys)
    {
        ForeignKeys = i_ForeignKeys;
    }

    public void AddForeignKey(ForeignKey i_ForeignKey)
    {
        ForeignKeys.Add(i_ForeignKey);
    } 
    
    public List<ForeignKey> GetForeignKeysTo(Table i_ToTable)
    {
        return ForeignKeys.Where(fk => fk.toTable == i_ToTable).ToList();
    }

    public void UnlockTable()
    {
        IsUnlocked = true;
        OnTableUnlocked?.Invoke(this);
    }

    public void LockTable()
    {
        IsUnlocked = false;
    }


}
