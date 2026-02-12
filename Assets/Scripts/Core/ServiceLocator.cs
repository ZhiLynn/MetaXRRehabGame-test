using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator instance;
    public static ServiceLocator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ServiceLocator>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ServiceLocator");
                    instance = go.AddComponent<ServiceLocator>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private Dictionary<Type, object> services = new Dictionary<Type, object>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Registration Service
    /// </summary>
    public void Register<T>(T service) where T : class
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"the [ServiceLocator] service {type.Name} is already registered and will be overridden.");
            services[type] = service;
        }
        else
        {
            services.Add(type, service);
            Debug.Log($"[ServiceLocator] registration Service: {type.Name}");
        }
    }

    /// <summary>
    /// Get services
    /// </summary>
    public T Get<T>() where T : class
    {
        Type type = typeof(T);
        if (services.TryGetValue(type, out object service))
        {
            return service as T;
        }
        
        Debug.LogError($"[ServiceLocator] service not found: {type.Name}");
        return null;
    }

    /// <summary>
    /// Try to get service
    /// </summary>
    public bool TryGet<T>(out T service) where T : class
    {
        Type type = typeof(T);
        if (services.TryGetValue(type, out object obj))
        {
            service = obj as T;
            return service != null;
        }
        service = null;
        return false;
    }

    /// <summary>
    /// Cancel registration service
    /// </summary>
    public void Unregister<T>() where T : class
    {
        Type type = typeof(T);
        if (services.Remove(type))
        {
            Debug.Log($"[ServiceLocator] cancel registration service: {type.Name}");
        }
    }

    /// <summary>
    /// Clear all services
    /// </summary>
    public void Clear()
    {
        services.Clear();
        Debug.Log("[ServiceLocator] all services have been cleared.");
    }
}