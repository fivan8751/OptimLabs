using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;

public class TestMemoryLeaks : MonoBehaviour 
{
    [SerializeField] GameObject bulletPrefab;
    
    [ContextMenu("Memory Leak Test")]
    public void MemoryLeakTest() 
    {
        StartCoroutine(TestLeaks());
    }
    
    IEnumerator TestLeaks() 
    {
        Debug.Log($"START: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
        
        // Итерация 1 не очищает Mono GC и создаёт ~8кб за объект
        for (int i = 0; i < 1000; i++) 
        {
            var bullet = Instantiate(bulletPrefab);
            DestroyImmediate(bullet);
            if (i % 100 == 0) yield return null;
        }
        Debug.Log($"LEAK 1: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB ");
        
        // Итерация 2 пулинг, утечек нет
        PoolManager.Instance.CreatePool("Bullet", bulletPrefab, 1000);
        for (int i = 0; i < 1000; i++) 
        {
            var bullet = PoolManager.Instance.Spawn("Bullet", Vector3.zero, Quaternion.identity);
            PoolManager.Instance.Despawn(bullet);
            if (i % 100 == 0) yield return null;
        }
        Debug.Log($"FIX 2: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB ");
    }
}
