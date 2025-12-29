using UnityEngine;
using UnityEngine.Events;

public class GridGenerator : MonoBehaviour {
    [Header("Grid Settings")]
    public int size = 20;
    public float cellSize = 1f;
    
    [Header("Prefabs")]
    public GameObject obstaclePrefab; 
    
    [Header("Spawns")]
    public Transform playerSpawn;
    public Transform targetSpawn;
    
    [Header("Visualization")]
    
    public bool clearSceneFirst = true; 
    
    private GameObject gridParent;
    
    void Start() {
    GenerateGrid();
}

    int CountObstacles() {
        int count = 0;
        foreach (Transform child in gridParent.transform) {
            if (child.gameObject.layer == LayerMask.NameToLayer("Obstacle")) {
                count++;
            }
        }
        return count;
    }
    [HideInInspector] public UnityEvent onGridRegenerated; 

    void AutoPositionPlayerAndTarget() {
    
        if (playerSpawn == null || targetSpawn == null) {
            Debug.LogError("Player/Target Spawn пустые!");
            return;
        }
        
        Vector3 playerPos = new Vector3(0.5f, 0f, 0.5f);                    
        Vector3 targetPos = new Vector3(size - 0.5f, 0f, size - 0.5f);     
        
        Debug.Log($"size={size}, TargetPos={targetPos}");
        
        playerSpawn.position = playerPos;
        targetSpawn.position = targetPos;
        
        Debug.Log($"Player '{playerSpawn.name}' -> {playerSpawn.position}");
        Debug.Log($"Target '{targetSpawn.name}' -. {targetSpawn.position}");
    }




    [ContextMenu("Generate Grid")] // Правый клик в Inspector
    public void GenerateGrid() {
        if (clearSceneFirst) ClearGrid();
        
        gridParent = new GameObject("Grid");
        gridParent.transform.SetParent(transform);
        
        int obstacleCount = 0;
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                Vector3 pos = new Vector3(x * cellSize + cellSize * 0.5f, 0.05f, z * cellSize + cellSize * 0.5f);
                
                if (Random.value < 0.3f && obstaclePrefab != null) { 
                    GameObject obstacle = Instantiate(obstaclePrefab, pos, Quaternion.identity, gridParent.transform);
                    obstacle.name = $"Obstacle_{x}_{z}";
                    obstacle.layer = LayerMask.NameToLayer("Obstacle");
                    obstacleCount++;
                }
            }
        }
        AutoPositionPlayerAndTarget();
        
        onGridRegenerated?.Invoke();
        
        Debug.Log($"Grid {size}x{size}: {obstacleCount} препятствий");
    }
    
    public void ClearGrid() {
        if (gridParent != null) {
            DestroyImmediate(gridParent);
        }
        
    }
    
    public bool IsWalkable(int x, int z) {
        Vector3 worldPos = new Vector3(x * cellSize + cellSize * 0.5f, 0.5f, z * cellSize + cellSize * 0.5f);
        
        Collider[] hits = Physics.OverlapSphere(worldPos, cellSize * 0.4f, LayerMask.GetMask("Obstacle"));
        
        bool blocked = hits.Length > 0;
        
        return !blocked;
    }


}
