using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataGridRowAdapter<T>
{
    List<string> GetColumnValues(T item);       // Data to show in text cells
    Texture2D GetPortrait(T item);              // Optional portrait
    string GetDisplayName(T item);              // Optional display name

    // IDataGridCell CreateCell(T item, string columnName);

}
