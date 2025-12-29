using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour {
    [SerializeField] private PathResult currentPath;
    private int pathIndex;
    public float moveSpeed = 5f; // Увеличил скорость
    private float lastMoveTime;
    private Vector3 lastPos;
    
    void Update() {
        if (currentPath?.path == null || pathIndex >= currentPath.path.Count) return;
        
        Vector3 targetPos = currentPath.path[pathIndex].worldPos;
        
        // Движение
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        
        // Застревание: не сдвинулся 0.5 сек
        if (Vector3.Distance(transform.position, lastPos) < 0.01f && Time.time - lastMoveTime > 0.5f) {
            Debug.LogWarning($"❌ ЗАСТРЯЛ на узле {pathIndex}! Пропускаю");
            pathIndex++;
        }
        
        lastPos = transform.position;
        lastMoveTime = Time.time;
        
        if (Vector3.Distance(transform.position, targetPos) < 0.4f) {
            pathIndex++;
            Debug.Log($"✅ Узел {pathIndex}/{currentPath.path.Count}");
        }
    }
    
    public void FollowPath(PathResult path) {
        Debug.Log($"🚀 Получен путь: {path?.path?.Count ?? 0} узлов");
        currentPath = path;
        pathIndex = 1; // ✅ НАЧИНАЕМ С ПЕРВОГО УЗЛА (0 = стартовая позиция)
    }
}
