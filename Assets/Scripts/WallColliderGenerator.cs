using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallColliderGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap _groundTilemap;// this is a 2d rectangular tilemap where 2d ground tiles are placed
    
    [SerializeField] private Transform _collidersParent;
    [SerializeField] private float _colliderHeight = 4f;
    [SerializeField] private float _colliderSize = 1;

    private HashSet<Vector3Int> _visitedCells = new HashSet<Vector3Int>();
    private List<Vector3Int> _currentChunk = new List<Vector3Int>();

    private void Start()
    {
        GenerateWallColliders();
    }

    // The character is a 3d object yet it walks around 2d tiles, so I need some 3d colliders to prevent character from walking into nowhere
    // This method spawns 3d box colliders according to following rules:
    // 1. detect non-empty cells in _groundTilemap
    // 2. recognize complete chunks of tiles and mark them as a whole piece of ground (can be islands or room floors)
    // 3. for every piece of ground, generate box colliders above their every adjacent empty cells so that player won't walk into void
    private void GenerateWallColliders()
    {
        // 清除现有的碰撞体
        foreach (Transform child in _collidersParent)
        {
            Destroy(child.gameObject);
        }
        _visitedCells.Clear();

        // 遍历所有瓦片
        for (int x = _groundTilemap.cellBounds.xMin; x <= _groundTilemap.cellBounds.xMax; x++)
        {
            for (int y = _groundTilemap.cellBounds.yMin; y <= _groundTilemap.cellBounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (!_visitedCells.Contains(cellPosition) && _groundTilemap.HasTile(cellPosition))
                {
                    _currentChunk.Clear();
                    FindConnectedTiles(cellPosition);
                    GenerateCollidersForChunk();
                }
            }
        }
    }

    private void FindConnectedTiles(Vector3Int startCell)
    {
        if (_visitedCells.Contains(startCell) || !_groundTilemap.HasTile(startCell))
            return;

        _visitedCells.Add(startCell);
        _currentChunk.Add(startCell);

        // 检查四个方向
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.left
        };

        foreach (Vector3Int dir in directions)
        {
            FindConnectedTiles(startCell + dir);
        }
    }

    private void GenerateCollidersForChunk()
    {
        if (_currentChunk.Count == 0) return;

        // 计算当前块的边界
        Vector3Int min = _currentChunk[0];
        Vector3Int max = _currentChunk[0];
        foreach (Vector3Int cell in _currentChunk)
        {
            min = Vector3Int.Min(min, cell);
            max = Vector3Int.Max(max, cell);
        }

        // 为每个边界生成碰撞体
        for (int x = min.x - 1; x <= max.x + 1; x++)
        {
            for (int y = min.y - 1; y <= max.y + 1; y++)
            {
                Vector3Int currentCell = new Vector3Int(x, y, 0);
                if (!_currentChunk.Contains(currentCell))
                {
                    // 检查是否与当前块相邻
                    bool isAdjacent = false;
                    foreach (Vector3Int dir in new Vector3Int[] { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left })
                    {
                        if (_currentChunk.Contains(currentCell + dir))
                        {
                            isAdjacent = true;
                            break;
                        }
                    }

                    if (isAdjacent)
                    {
                        CreateWallCollider(currentCell);
                    }
                }
            }
        }
    }

    private void CreateWallCollider(Vector3Int cellPosition)
    {
        GameObject wallObject = new GameObject($"Wall_{cellPosition.x}_{cellPosition.y}");
        wallObject.transform.SetParent(_collidersParent);
        wallObject.transform.position = 0.5f * (_groundTilemap.CellToWorld(cellPosition) + _groundTilemap.CellToWorld(cellPosition + new Vector3Int(1,1,0)));

        BoxCollider collider = wallObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(_colliderSize, _colliderHeight, _colliderSize);
        collider.center = new Vector3(0, _colliderHeight / 2f, 0);
    }
}
