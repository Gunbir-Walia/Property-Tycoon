using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonScriptObj<T> : ScriptableObject where T : ScriptableObject
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            if (_instance == null)
            {
                Object[] objects = Resources.LoadAll("SingletonScriptObjects", typeof(T));
                _instance = objects[0] as T;
            }
            // does not have instant in scene
            if (_instance == null)
            {
                Debug.Log($"Cannot find the singleton instant of type{typeof(T).Name}, trying to finding one with tag");
                ScriptableObject.CreateInstance<T>();
            }
            if (_instance == null)
            {
                Debug.Log($"Warning, singleton instant of type{typeof(T).Name} cannot be found or create");
                return null;
            }
            else
            {
                return _instance;
            }

        }
        set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else
            {
                Debug.Log($"Destroying {value.name}");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        InitialzeSingleton();
    }

    void InitialzeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        //if (_autoUnparentObjOnAwake)
        //{
        //    transform.SetParent(null);
        //}
        Instance = this as T;
    }
}
