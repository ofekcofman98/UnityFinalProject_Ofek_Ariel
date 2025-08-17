using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBase : MonoBehaviour, IMenu
{
    [SerializeField] private eMenuType menuType;
    public eMenuType MenuType => menuType;

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
