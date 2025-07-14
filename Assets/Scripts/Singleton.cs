using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static bool HasInstance => _instance != null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    Debug.LogError($"❌ Singleton<{typeof(T).Name}> not found in scene! You must add it manually.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // DontDestroyOnLoad(gameObject); // Optional: only if you need it!
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"⚠️ Duplicate Singleton<{typeof(T).Name}> detected. Destroying extra one.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected static void SetInstance(T instance)
    {
        _instance = instance;
    }

}

// public abstract class Singleton <T>: MonoBehaviour where T: MonoBehaviour
// {

//     private static T _instance;

//     //property to access the instance
//     public static T Instance
//     {
//         get
//         {
//             if(_instance != null) return _instance;

//             // if the instance doesnt exit, find it in the scene:
//             _instance = FindObjectOfType<T>();

//             if(_instance == null)
//             {
//                 var SingletonObject = new GameObject(typeof(T).Name);
//                 _instance = SingletonObject.AddComponent<T>();
//             }
            
//             // DontDestroyOnLoad(_instance.gameObject);
//             return _instance;
//         }
//     }

//     protected void OnDestroy()
//     {
//         if(_instance == this)
//         {
//             _instance = null;
//         }
//     }
// }