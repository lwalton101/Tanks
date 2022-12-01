using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonLoader : MonoBehaviour
{
    
    void Awake()
    {
        if (SteamManager.Instance == null)
        {
            CreateNewManager<SteamManager>();
        }    
    }

    private void CreateNewManager<T>() where T : MonoBehaviour
    {
        GameObject manager = new GameObject(typeof(T).Name);
        var component = manager.AddComponent<T>();
        component.enabled = true;
        DontDestroyOnLoad(manager);
    }
}
