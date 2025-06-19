using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinEntry
{
    public Table TableToJoin;
    public Column FromColumn;
    public Column ToColumn;


    public JoinEntry(Table table, Column fromCol, Column toCol)
    {
        TableToJoin = table;
        FromColumn = fromCol;
        ToColumn = toCol;
    }

}
