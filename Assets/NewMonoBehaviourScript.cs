using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LightingTester : MonoBehaviour {
    [Header("Свет")]
    public Light dirLight;
    public Light[] pointLights = new Light[4];
    
    [Header("UI")]
    public Button bakedBtn, realtimeBtn, mixedBtn, shadowsOffBtn;
    public TextMeshProUGUI fpsText, statsText;
    
    float fps;
    
    void Start() {
        // Подключение кнопок
        if (bakedBtn) bakedBtn.onClick.AddListener(() => SetLightingMode(LightmapBakeType.Baked));
        if (realtimeBtn) realtimeBtn.onClick.AddListener(() => SetLightingMode(LightmapBakeType.Realtime));
        if (mixedBtn) mixedBtn.onClick.AddListener(() => SetLightingMode(LightmapBakeType.Mixed));
        if (shadowsOffBtn) shadowsOffBtn.onClick.AddListener(() => ToggleShadows(false));
    }
    
    void Update() {
        fps = 1f / Time.unscaledDeltaTime;
        if (fpsText) fpsText.text = $"FPS: {fps:F0}";
    }
    
    void SetLightingMode(LightmapBakeType mode) {
        // Меняем режим только для статичных объектов (нужно пометить Static)
        if (dirLight) dirLight.lightmapBakeType = mode;
        for (int i = 0; i < pointLights.Length; i++) {
            if (pointLights[i]) pointLights[i].lightmapBakeType = mode;
        }
        
        string modeName = mode switch {
            LightmapBakeType.Baked => "Baked - Window>Rendering>Lighting>Generate",
            LightmapBakeType.Realtime => "Realtime - динамическое",
            LightmapBakeType.Mixed => "Mixed - запеките после смены",
            _ => "Unknown"
        };
        
        if (statsText) statsText.text = $"{mode} | {modeName}";
    }
    
    void ToggleShadows(bool enabled) {
        if (dirLight) dirLight.shadows = enabled ? LightShadows.Soft : LightShadows.None;
        for (int i = 0; i < pointLights.Length; i++) {
            if (pointLights[i]) pointLights[i].shadows = enabled ? LightShadows.Soft : LightShadows.None;
        }
        if (statsText) statsText.text += $"\nShadows: {(enabled ? "ON" : "OFF")}";
    }
}
