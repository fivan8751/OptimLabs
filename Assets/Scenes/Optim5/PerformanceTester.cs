using UnityEngine;
using System.Diagnostics;

public class PerformanceTester : MonoBehaviour 
{
    [SerializeField] GameObject bulletPrefab;
    
    [ContextMenu("Test")]
    public void Test() 
    {
        PoolManager.Instance.CreatePool("Bullet", bulletPrefab, 1000);
        
        //Pooling
        var sw1 = Stopwatch.StartNew();
        int shots = 1000;
        for (int i = 0; i < shots; i++) 
        {
            var bullet = PoolManager.Instance.Spawn("Bullet", Vector3.zero, Quaternion.identity);
            PoolManager.Instance.Despawn(bullet);
        }
        sw1.Stop();
        UnityEngine.Debug.Log($"Pooling: {sw1.ElapsedMilliseconds}ms ({shots})");
        
        //Instantiate 
        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < shots; i++) 
        {
            var bullet = Instantiate(bulletPrefab);
            DestroyImmediate(bullet);
        }
        sw2.Stop();
        UnityEngine.Debug.Log($"Instantiate: {sw2.ElapsedMilliseconds}ms ({shots})");
        
        PoolManager.Instance.ShowStats();
    }
    [ContextMenu("🧠 Memory Test")]
public void MemoryTest() 
{
    UnityEngine.Debug.Log($"🧠 Memory BEFORE: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
    
    // Pooling тест
    PoolManager.Instance.CreatePool("Bullet", bulletPrefab, 2000);
    for (int i = 0; i < 1000; i++) {
        var b = PoolManager.Instance.Spawn("Bullet", Vector3.zero, Quaternion.identity);
        PoolManager.Instance.Despawn(b);
    }
    
    UnityEngine.Debug.Log($"🟢 Pooling AFTER: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
    
    // Instantiate тест
    for (int i = 0; i < 1000; i++) {
        var b = Instantiate(bulletPrefab);
        DestroyImmediate(b);
    }
    
    UnityEngine.Debug.Log($"🔴 Instantiate AFTER: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
}

}
