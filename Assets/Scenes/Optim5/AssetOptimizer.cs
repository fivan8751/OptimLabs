using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Profiling;

public class AssetOptimizer : MonoBehaviour 
{
    [SerializeField] Texture2D originalTexture;  // 2048x2048 
    
    [ContextMenu("Texture Compression Test")]
    public void TextureTest() 
    {
        Debug.Log($"Texture BEFORE: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
        
        // Тест 1: Без компрессии
        var mat1 = new Material(Shader.Find("Standard"));
        mat1.mainTexture = originalTexture;  
        Debug.Log($"NO COMPRESS: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
        
        // Тест 2: С компрессией 
        Destroy(mat1);
        Debug.Log($"COMPRESSED: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
    }
}
