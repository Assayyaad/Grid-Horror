using System;

using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException($"{_instance.GetType()} instance is not initialized");
            }

            return _instance;
        }
    }
    private static T _instance;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            throw new InvalidOperationException($"Multiple instances of {_instance.GetType()} are not allowed");
        }

        _instance = (T)this;
        DontDestroyOnLoad(_instance.gameObject);
    }

    protected virtual void OnDestroy()
    {
        _instance = null;
    }
}
