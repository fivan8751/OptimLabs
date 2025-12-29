using UnityEngine;

[System.Serializable]
public class Node {
    public bool walkable;
    public Vector3 worldPos;
    public Vector2Int gridPos;
    public int gCost, hCost, fCost;
    public Node parent;
    
    public Node(bool _walkable, Vector3 _worldPos, Vector2Int _gridPos) {
        walkable = _walkable;
        worldPos = _worldPos;
        gridPos = _gridPos;
        gCost = hCost = fCost = 0;
        parent = null;
    }
}