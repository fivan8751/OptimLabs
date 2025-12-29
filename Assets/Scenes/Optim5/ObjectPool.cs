using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool 
{
    private readonly GameObject _prefab;
    private readonly Transform _root;
    private readonly Stack<GameObject> _available = new Stack<GameObject>();
    private readonly List<GameObject> _all = new List<GameObject>();
    private readonly bool _autoExpand;
    private readonly Action<GameObject> _onCreateCallback;

    public int TotalCreated => _all.Count;
    public int AvailableCount => _available.Count;

    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null, bool autoExpand = true, Action<GameObject> onCreateCallback = null) 
    {
        _prefab = prefab;
        _root = parent;
        _autoExpand = autoExpand;
        _onCreateCallback = onCreateCallback;
        
        for (int i = 0; i < initialSize; i++) 
        {
            CreateNew();
        }
    }

    private GameObject CreateNew() 
    {
        GameObject go = UnityEngine.Object.Instantiate(_prefab, _root);
        go.name = _prefab.name;
        go.SetActive(false);
        _available.Push(go);
        _all.Add(go);
        _onCreateCallback?.Invoke(go);
        return go;
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation) 
    {
        if (_available.Count == 0 && _autoExpand) CreateNew();
        if (_available.Count == 0) return null;
        
        var go = _available.Pop();
        
        
        go.SetActive(true);
        return go;
    }

    public void Despawn(GameObject go) 
    {
        go.SetActive(false);
        _available.Push(go);
    }

    public void DespawnAll() 
    {
        foreach (var go in _all.ToArray()) 
        {
            if (go == null) continue;
            Despawn(go);
        }
    }

    public void DestroyPool() 
    {
        foreach (var go in _all) 
        {
            if (go != null) 
            {
                UnityEngine.Object.Destroy(go);
            }
        }
        _all.Clear();
        _available.Clear();
    }
}
