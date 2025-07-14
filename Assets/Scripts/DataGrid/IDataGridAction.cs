using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataGridAction<T>
{
    string Label { get; }
    void Execute(T rowData);
}
