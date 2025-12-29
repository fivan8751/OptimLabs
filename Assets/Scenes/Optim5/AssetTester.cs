using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Profiling;
using System.Threading.Tasks;

public class AssetTester : MonoBehaviour 
{
    [Header("Материалы")]
    [SerializeField] Material testMaterial;  
    
    [Header("Аудио")]
    [SerializeField] AudioSource audioSource;  
    
    [ContextMenu("Load Material")]
    async void LoadMaterial() 
    {
        Debug.Log($"BEFORE: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:F1} MB");
        
        var matHandle = Addressables.LoadAssetAsync<Material>("Material1");
        var skyMat = await matHandle.Task;
        
        RenderSettings.skybox = skyMat;  
        
        Debug.Log($"Material1 LOADED: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:F1} MB");
        Addressables.Release(matHandle);  //  Освобождаем память
    }
    
    [ContextMenu("Load Audio")]
    async void LoadAudio() 
    {
        Debug.Log($"BEFORE: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:F1} MB");
        
        var audioHandle = Addressables.LoadAssetAsync<AudioClip>("Music1");
        var musicClip = await audioHandle.Task;
        
        audioSource.clip = musicClip;
        audioSource.Play();
        
        Debug.Log($"Music1 LOADED: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:F1} MB");
        // Addressables.Release(audioHandle);  // Не освобождаем, т.к. играет музыка
    }
    
    [ContextMenu("Load ALL ")]
    async void LoadAll() 
    {
        Debug.Log($"START: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:F1} MB");
        
        // Параллельно!
        var matTask = Addressables.LoadAssetAsync<Material>("Material1");
        var audioTask = Addressables.LoadAssetAsync<AudioClip>("Music1");
        
        await Task.WhenAll(matTask.Task, audioTask.Task);
        
        RenderSettings.skybox = matTask.Result;
        audioSource.clip = audioTask.Result;
        audioSource.Play();
        
        Debug.Log($"ALL LOADED: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024:F1} MB");
    }
    
    [ContextMenu("Cleanup")]
    void Cleanup() 
    {
        RenderSettings.skybox = null;
        audioSource.Stop();
        audioSource.clip = null;
        Debug.Log($"Cleaned");
    }
}
