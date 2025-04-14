using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForeignKey
{
    public Column fromColumn;
    public Table toTable;
    public Column toColumn;

    public ForeignKey(Column i_FromColumn, Table i_ToTable, Column i_ToColumn)
    {
        fromColumn = i_FromColumn;
        toTable = i_ToTable;
        toColumn = i_ToColumn;
    }
}
