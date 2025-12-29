using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PoolManager : MonoBehaviour 
{
    public static PoolManager Instance { get; private set; }
    
    private Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
    private Dictionary<GameObject, string> _instanceToKey = new Dictionary<GameObject, string>();
    
    private void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else 
        {
            Destroy(gameObject);
        }
    }
    
    public void CreatePool(string key, GameObject prefab, int initialSize, Transform parent = null, bool autoExpand = true) 
    {
        if (string.IsNullOrEmpty(key)) key = prefab.name;
        if (_pools.ContainsKey(key)) return;
        
        ObjectPool pool = new ObjectPool(prefab, initialSize, parent ?? this.transform, autoExpand, 
            (go) => {
                if (!_instanceToKey.ContainsKey(go)) _instanceToKey.Add(go, key);
            });
        
        _pools[key] = pool;
        Debug.Log($"Pool '{key}': {pool.TotalCreated} created, {pool.AvailableCount} available");
    }
    
    
    
    public GameObject Spawn(string key, Vector3 pos, Quaternion rot) 
    {
        if (!_pools.TryGetValue(key, out ObjectPool pool)) 
        {
            Debug.LogWarning($"PoolManager: pool '{key}' not found.");
            return null;
        }
        return pool.Spawn(pos, rot);
    }
    
    
    
    public void Despawn(GameObject instance) 
    {
        if (instance == null) return;
        
        if (!_instanceToKey.TryGetValue(instance, out string key)) 
        {
            Debug.LogWarning($"PoolManager: {instance.name} not managed. Destroying.");
            Destroy(instance);
            return;
        }
        
        if (!_pools.TryGetValue(key, out ObjectPool pool)) 
        {
            Debug.LogWarning($"PoolManager: pool '{key}' missing. Destroying.");
            Destroy(instance);
            _instanceToKey.Remove(instance);
            return;
        }
        
        pool.Despawn(instance);
    }
    
    public (int total, int available) GetStats(string key) 
    {
        if (!_pools.TryGetValue(key, out ObjectPool pool)) return (0, 0);
        return (pool.TotalCreated, pool.AvailableCount);
    }
    
    [ContextMenu("Show All Stats")]
    public void ShowStats() 
    {
        Debug.Log("POOL STATS");
        foreach (var kv in _pools) 
        {
            var stats = GetStats(kv.Key);
            Debug.Log($"📈 {kv.Key}: {stats.total} total, {stats.available} available");
        }
    }
    
    [ContextMenu("Clear All Pools")]
    public void ClearAllPools() 
    {
        foreach (var pool in _pools.Values) 
        {
            pool.DespawnAll();
        }
        Debug.Log("All pools cleared");
    }
}
