using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class ObjectPoolService<T> where T : Button
{
    private ObjectPool<T> m_ObjectPool;
    private Transform m_Parent;

    public ObjectPoolService(T i_Prefab, Transform i_Parent = null, int i_Capacity = 10, int i_MaxCapacity = 100)
    {
        m_Parent = i_Parent;

        m_ObjectPool = new ObjectPool<T>
        (
            createFunc: () =>
            {
                var newObject = Object.Instantiate(i_Prefab, m_Parent);
                newObject.gameObject.SetActive(false);
                return newObject;
            },
            actionOnGet: obj =>
            {
                obj.gameObject.SetActive(true);
                OnReset(obj);
            },
            actionOnRelease: obj =>
            {
                obj.gameObject.SetActive(false);
                OnReset(obj);
            },
            actionOnDestroy: obj => obj.gameObject.SetActive(false),
            defaultCapacity: i_Capacity,
            maxSize: i_MaxCapacity,
            collectionCheck: false
        );

        var poolObjects = new T[i_Capacity];
        for (int i = 0; i < i_Capacity; i++)
        {
            poolObjects[i] = m_ObjectPool.Get();
        }
        for (int i = 0; i < i_Capacity; i++)
        {
            m_ObjectPool.Release(poolObjects[i]);
        }

    }

    public T Get()
    {
        T obj = m_ObjectPool.Get();
        Debug.Log($"[POOL] Got object: {obj.name}, ActiveInHierarchy: {obj.gameObject.activeInHierarchy}");
        // obj.gameObject.SetActive(true);
        return obj;
    }
    public void Release(T obj)
    {
        if (obj == null || obj.gameObject == null) return;
        m_ObjectPool.Release(obj);
    }
    protected void OnReset(T obj)
    {
        obj.gameObject.SetActive(false);

        // 🧼 Clear label
        var label = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            // Debug.Log($"[RESET] Clearing text on {obj.name}");
            label.text = string.Empty;
        }

        // 🧼 Remove all listeners to avoid duplication
        obj.onClick.RemoveAllListeners();

        // 🧼 If you have any custom data (like AssignedSection), reset it here:
        var draggable = obj.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.AssignedSection = null;
            draggable.OnDropped = null;
        }
    }
    

public void ClearAll()
{
    while (m_ObjectPool.CountInactive > 0)
    {
        T obj = m_ObjectPool.Get();
        Object.Destroy(obj.gameObject);
    }
}

}

