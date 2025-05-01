using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SQL Detective/Schema/TableSchema")]
public class TableStructure : ScriptableObject
{
    public string TableName;
    public List<ColumnStructure> Columns = new List<ColumnStructure>();
}
