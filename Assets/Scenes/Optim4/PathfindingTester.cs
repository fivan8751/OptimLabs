using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;

public class PathfindingTester : MonoBehaviour {
    public GridGenerator gridGen;
    public Transform player, target;
    public TextMeshProUGUI resultText;
    private Node[,] grid;
    [Header("ЛИНИИ ПУТИ")]
    public LineRenderer aStarLine;
    public LineRenderer dijkstraLine;
    
    
    
    
    void Start() {
        grid = new Node[gridGen.size, gridGen.size];
        
        if (gridGen != null && gridGen.onGridRegenerated != null) {
            gridGen.onGridRegenerated.AddListener(RegeneratePathfindingGrid);
        }
        
        SetupLines();
    }
    
    public void RegeneratePathfindingGrid() {
        if (player != null) player.position = gridGen.playerSpawn.position;
        if (target != null) target.position = gridGen.targetSpawn.position;
        GenerateGrid(); 
    }

    void GenerateGrid() {
        
        int obstacleCount = 0;
        for (int x = 0; x < gridGen.size; x++) {
            for (int z = 0; z < gridGen.size; z++) {
                bool walkable = gridGen.IsWalkable(x, z);
                grid[x, z] = new Node(walkable, new Vector3(x + 0.5f, 0.1f, z + 0.5f), new Vector2Int(x, z));
                
                if (!walkable) obstacleCount++;
            }
        }
        
        UnityEngine.Debug.Log($"PathfindingTester: {gridGen.size}x{gridGen.size}, препятствий: {obstacleCount}");
    }

    
    void SetupLines() {
        SetupLineRenderer(aStarLine, Color.green, "A* Path");
        SetupLineRenderer(dijkstraLine, Color.blue, "Dijkstra Path");
    }
    
    void SetupLineRenderer(LineRenderer lr, Color color, string name) {
        if (lr == null) return;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = color;
        lr.startWidth = 0.12f;
        lr.endWidth = 0.12f;
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.gameObject.name = name;
    }
    [ContextMenu("Test A*")]
    public void TestAStar() {
        
        if (grid == null) {
            
            resultText.text = "ОШИБКА: Сетка не готова";
            return;
        }
        
        Vector2Int start = WorldToGrid(player.position);
        Vector2Int goal = WorldToGrid(target.position);
        
        if (start.x < 0 || start.x >= gridGen.size || start.y < 0 || start.y >= gridGen.size) {
            return;
        }
        if (goal.x < 0 || goal.x >= gridGen.size || goal.y < 0 || goal.y >= gridGen.size) {
            return;
        }
        
        UnityEngine.Debug.Log($"A*: Start={start}, Goal={goal}");
        
        Stopwatch sw = Stopwatch.StartNew();
        PathResult result = AStarFindPath(start, goal);
        sw.Stop();
        
        resultText.text = $"A*: {sw.ElapsedMilliseconds}ms, Nodes: {result.nodesVisited}, Path: {result.path?.Count ?? 0}";
        
        if (result.path != null && result.path.Count > 0) {
            ShowPathLine(result, aStarLine);
            ShowResult("A*", sw.ElapsedMilliseconds, result);
        } else {
        }
    }


    [ContextMenu("Test Dijkstra*")]
    public void TestDijkstra() {
        Vector2Int start = WorldToGrid(player.position);
        Vector2Int goal = WorldToGrid(target.position);
        
        Stopwatch sw = Stopwatch.StartNew();
        PathResult result = DijkstraFindPath(start, goal);
        sw.Stop();
        
        ShowResult("Dijkstra", sw.ElapsedMilliseconds, result);
        ShowPathLine(result, dijkstraLine);
    }
    
    void ShowResult(string name, long timeMs, PathResult result) {
        resultText.text = $"{name}: {timeMs}ms, Nodes: {result.nodesVisited}, Path: {result.path?.Count ?? 0}";
        UnityEngine.Debug.Log($"{name}: {timeMs}ms, Nodes: {result.nodesVisited}, Path: {result.path?.Count ?? 0}");
    }
    
    void ShowPathLine(PathResult result, LineRenderer line) {
        if (result.path == null || result.path.Count == 0) {
            line.positionCount = 0;
            return;
        }
        
        List<Vector3> points = new List<Vector3>();
        
        foreach (Node node in result.path) {
            Vector3 center = node.worldPos + Vector3.up * 0.2f;
            
            float size = 0.3f;
            points.Add(center + new Vector3(-size, 0, -size));
            points.Add(center + new Vector3(size, 0, -size));
            points.Add(center + new Vector3(size, 0, size));
            points.Add(center + new Vector3(-size, 0, size));
            points.Add(center + new Vector3(-size, 0, -size)); // Замыкаем
        }
        
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }


    void OnDrawGizmos() {
        if (grid == null || gridGen == null || gridGen.size <= 0) return;
        
        Gizmos.color = Color.white;
        
        for (int x = 0; x < gridGen.size; x++) {
            for (int z = 0; z < gridGen.size; z++) {
                if (x >= grid.GetLength(0) || z >= grid.GetLength(1)) continue;
                
                Node node = grid[x, z];
                if (node == null) continue; // null node
                
                Vector3 center = new Vector3(x + 0.5f, 0, z + 0.5f);
                
                if (!node.walkable) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(center, Vector3.one * 0.95f);
                } else {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(center, Vector3.one * 0.8f);
                }
            }
        }
    }


    
    Vector2Int WorldToGrid(Vector3 worldPos) {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x - 0.5f), 
            Mathf.RoundToInt(worldPos.z - 0.5f)
        );
    }
    
    PathResult AStarFindPath(Vector2Int start, Vector2Int goal) {
        if (grid == null) {
            return new PathResult { path = null, nodesVisited = 0 };
        }
        
        if (start.x < 0 || start.x >= gridGen.size || start.y < 0 || start.y >= gridGen.size ||
            goal.x < 0 || goal.x >= gridGen.size || goal.y < 0 || goal.y >= gridGen.size) {
            return new PathResult { path = null, nodesVisited = 0 };
        }
        
        Node startNode = grid[start.x, start.y];
        Node goalNode = grid[goal.x, goal.y];
        if (startNode == null || !startNode.walkable || goalNode == null || !goalNode.walkable) {
            return new PathResult { path = null, nodesVisited = 0 };
        }
        
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };
        
        openSet.Add(startNode);
        
        int nodesVisited = 0;
        
        while (openSet.Count > 0) {
            Node current = GetLowestFScore(openSet, fScore);
            
            if (current.gridPos == goal) {
                return RetracePath(cameFrom, start, goal);
            }
            
            openSet.Remove(current);
            closedSet.Add(current.gridPos);
            nodesVisited++;
            
            foreach (Vector2Int neighborPos in GetNeighbors(current.gridPos)) {
                if (!grid[neighborPos.x, neighborPos.y].walkable || closedSet.Contains(neighborPos)) 
                    continue;
                
                int tentativeG = gScore[current.gridPos] + 1;
                if (!gScore.ContainsKey(neighborPos) || tentativeG < gScore[neighborPos]) {
                    cameFrom[neighborPos] = current.gridPos;
                    gScore[neighborPos] = tentativeG;
                    fScore[neighborPos] = tentativeG + Heuristic(neighborPos, goal);
                    
                    if (!openSet.Any(n => n.gridPos == neighborPos)) {
                        openSet.Add(grid[neighborPos.x, neighborPos.y]);
                    }
                }
            }
        }
        
        return new PathResult { path = null, nodesVisited = nodesVisited };
    }

    
    PathResult DijkstraFindPath(Vector2Int start, Vector2Int goal) {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> {{start, 0}};
        
        openSet.Add(grid[start.x, start.y]);
        
        while (openSet.Count > 0) {
            Node current = GetLowestGScore(openSet, gScore);
            
            if (current.gridPos == goal) {
                return RetracePath(cameFrom, start, goal);
            }
            
            openSet.Remove(current);
            closedSet.Add(current.gridPos);
            
            foreach (Vector2Int neighborPos in GetNeighbors(current.gridPos)) {
                if (!grid[neighborPos.x, neighborPos.y].walkable || closedSet.Contains(neighborPos)) 
                    continue;
                
                int tentativeG = gScore[current.gridPos] + 1;
                if (!gScore.ContainsKey(neighborPos) || tentativeG < gScore[neighborPos]) {
                    cameFrom[neighborPos] = current.gridPos;
                    gScore[neighborPos] = tentativeG;
                    
                    if (!openSet.Any(n => n.gridPos == neighborPos)) {
                        openSet.Add(grid[neighborPos.x, neighborPos.y]);
                    }
                }
            }
        }
        return new PathResult { path = null, nodesVisited = closedSet.Count };
    }
    
    Node GetLowestFScore(List<Node> openSet, Dictionary<Vector2Int, int> fScore) {
        Node lowest = openSet[0];
        foreach (Node node in openSet) {
            if (fScore[node.gridPos] < fScore[lowest.gridPos]) {
                lowest = node;
            }
        }
        return lowest;
    }
    
    Node GetLowestGScore(List<Node> openSet, Dictionary<Vector2Int, int> gScore) {
        Node lowest = openSet[0];
        foreach (Node node in openSet) {
            if (gScore.ContainsKey(node.gridPos) && gScore[node.gridPos] < gScore[lowest.gridPos]) {
                lowest = node;
            }
        }
        return lowest;
    }
    
    
    List<Vector2Int> GetNeighbors(Vector2Int pos) {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        // Только 4 направления (без диагоналей)
        Vector2Int[] directions = {
            new Vector2Int(0, 1),   //  z+1
            new Vector2Int(1, 0),   //  x+1  
            new Vector2Int(0, -1),  //  z-1
            new Vector2Int(-1, 0)   //  x-1
        };
        
        foreach (Vector2Int dir in directions) {
            Vector2Int neighbor = pos + dir; 
            
            if (neighbor.x >= 0 && neighbor.x < gridGen.size &&
                neighbor.y >= 0 && neighbor.y < gridGen.size &&
                grid[neighbor.x, neighbor.y].walkable) {
                
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    
    int Heuristic(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    PathResult RetracePath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int goal) {
        List<Node> path = new List<Node>();
        Vector2Int current = goal;
        
        while (current != start) {
            if (!grid[current.x, current.y].walkable) {
                UnityEngine.Debug.LogError($"НЕПРОХОДИМАЯ клетка в пути: {current}");
                return new PathResult { path = null };
            }
            path.Add(grid[current.x, current.y]);
            if (!cameFrom.ContainsKey(current)) break;
            current = cameFrom[current];
        }
        
        path.Add(grid[start.x, start.y]);
        path.Reverse();
        
        UnityEngine.Debug.Log($"Путь: {path.Count} клеток, все проходимы");
        return new PathResult { path = path, nodesVisited = cameFrom.Count };
    }
}
