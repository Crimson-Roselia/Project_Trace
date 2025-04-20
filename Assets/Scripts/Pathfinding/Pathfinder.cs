using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Pathfinder
{
    private Tilemap _groundTilemap;
    private Tilemap _obstaclesTilemap;
    
    private Dictionary<Vector3Int, PathNode> _nodes = new Dictionary<Vector3Int, PathNode>();
    private List<PathNode> _openSet = new List<PathNode>();
    private HashSet<PathNode> _closedSet = new HashSet<PathNode>();
    
    // 构造函数，初始化地图
    public Pathfinder(Tilemap groundTilemap, Tilemap obstaclesTilemap)
    {
        _groundTilemap = groundTilemap;
        _obstaclesTilemap = obstaclesTilemap;
        InitializeNodes();
    }
    
    // 初始化所有可行走的节点
    private void InitializeNodes()
    {
        _nodes.Clear();
        
        // 遍历所有地面瓦片
        BoundsInt bounds = _groundTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                
                // 如果是地面瓦片且不是障碍物
                if (_groundTilemap.HasTile(cellPos) && 
                    (_obstaclesTilemap == null || !_obstaclesTilemap.HasTile(cellPos)))
                {
                    PathNode node = new PathNode(cellPos);
                    _nodes.Add(cellPos, node);
                }
            }
        }
        
        // 为每个节点设置邻居
        foreach (PathNode node in _nodes.Values)
        {
            SetNeighbors(node);
        }
    }
    
    // 设置节点的邻居
    private void SetNeighbors(PathNode node)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),   // 上
            new Vector3Int(1, 0, 0),   // 右
            new Vector3Int(0, -1, 0),  // 下
            new Vector3Int(-1, 0, 0),  // 左
            
            new Vector3Int(1, 1, 0),   // 右上
            new Vector3Int(1, -1, 0),  // 右下
            new Vector3Int(-1, -1, 0), // 左下
            new Vector3Int(-1, 1, 0)   // 左上
        };
        
        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = node.Position + dir;
            if (_nodes.TryGetValue(neighborPos, out PathNode neighbor))
            {
                node.Neighbors.Add(neighbor);
            }
        }
    }
    
    // 计算两点间的曼哈顿距离
    private float CalculateHeuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static List<Vector3> GetWorldPositionFromCoords(List<Vector3Int> coords, Tilemap tilemap)
    {
        if (coords == null || coords.Count == 0)
        {
            return null;
        }
        List<Vector3> result = new List<Vector3>();
        // 转换路径点为世界坐标
        foreach (Vector3Int pos in coords)
        {
            Vector3 worldPos = tilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
            result.Add(worldPos);
        }
        return result;
    }
    
    // 执行 A* 寻路算法
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int target)
    {
        // 如果起点或终点不在可行走区域内，返回空路径
        if (!_nodes.TryGetValue(start, out PathNode startNode) || 
            !_nodes.TryGetValue(target, out PathNode targetNode))
        {
            return new List<Vector3Int>();
        }
        
        // 重置所有节点
        foreach (PathNode node in _nodes.Values)
        {
            node.GCost = float.MaxValue;
            node.HCost = 0;
            node.Parent = null;
        }
        
        _openSet.Clear();
        _closedSet.Clear();
        
        startNode.GCost = 0;
        startNode.HCost = CalculateHeuristic(start, target);
        _openSet.Add(startNode);
        
        while (_openSet.Count > 0)
        {
            // 获取 F 值最小的节点
            PathNode currentNode = _openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
            
            // 如果到达目标节点，重建路径并返回
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }
            
            _openSet.Remove(currentNode);
            _closedSet.Add(currentNode);
            
            // 检查所有邻居
            foreach (PathNode neighbor in currentNode.Neighbors)
            {
                if (_closedSet.Contains(neighbor))
                {
                    continue;
                }
                float tentativeGCost = currentNode.GCost + 1; // 假设每步代价为1
                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.Parent = currentNode;
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateHeuristic(neighbor.Position, target);
                    if (!_openSet.Contains(neighbor))
                    {
                        _openSet.Add(neighbor);
                    }
                }
            }
        }
        // 如果没有找到路径，返回空列表
        return new List<Vector3Int>();
    }
    
    // 重建从起点到终点的路径
    private List<Vector3Int> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        PathNode currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        
        path.Reverse(); // 反转路径，使其从起点到终点
        return path;
    }
    
    // 更新地图（如果地图发生变化）
    public void UpdateMap()
    {
        InitializeNodes();
    }
    
    // 获取世界坐标对应的单元格位置
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return _groundTilemap.WorldToCell(worldPosition);
    }
    
    // 获取单元格位置对应的世界坐标（居中）
    public Vector3 CellToWorld(Vector3Int cellPosition)
    {
        return _groundTilemap.GetCellCenterWorld(cellPosition);
    }
}

// 路径节点类
public class PathNode
{
    public Vector3Int Position { get; private set; }
    public float GCost { get; set; } // 从起点到当前节点的代价
    public float HCost { get; set; } // 从当前节点到终点的估计代价
    public float FCost => GCost + HCost; // 总代价
    public PathNode Parent { get; set; }
    public List<PathNode> Neighbors { get; private set; }
    
    public PathNode(Vector3Int position)
    {
        Position = position;
        GCost = float.MaxValue;
        HCost = 0;
        Parent = null;
        Neighbors = new List<PathNode>();
    }
}
