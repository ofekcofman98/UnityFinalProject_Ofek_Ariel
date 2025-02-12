using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton <T>: MonoBehaviour where T: MonoBehaviour
{

    private static T _instance;

    //property to access the instance
    public static T Instance
    {
        get
        {
            if(_instance != null) return _instance;

            // if the instance doesnt exit, find it in the scene:
            _instance = FindObjectOfType<T>();

            if(_instance == null)
            {
                var SingletonObject = new GameObject(typeof(T).Name);
                _instance = SingletonObject.AddComponent<T>();
            }
            
            // DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }

    protected void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }
}