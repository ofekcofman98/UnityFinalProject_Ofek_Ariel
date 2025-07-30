using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataGridCell
{
    GameObject Create(Transform parent, float width);
}
