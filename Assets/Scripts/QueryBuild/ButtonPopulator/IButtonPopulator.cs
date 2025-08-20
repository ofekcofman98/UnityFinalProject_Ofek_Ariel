using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IButtonPopulator<T>
{
    void PopulateButtons(IEnumerable<T> items);
}
