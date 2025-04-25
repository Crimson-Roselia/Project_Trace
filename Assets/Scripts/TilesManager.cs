using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesManager : MonoBehaviour
{
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _obstacleTilemap;
    private Pathfinder _pathfinder;
    private PathArrowsVisualizer _arrowsVisualizer;

    [SerializeField] private Tilemap _trapsTilemap;
    [SerializeField] private Tilemap _destroyableTilemap;
    [SerializeField] private Tilemap _teslaTilemap;
    [SerializeField] private Tilemap _turretTilemap;
    [SerializeField] private Tilemap _bombTilemap;

    [SerializeField] private Tesla _teslaPrefab;
    [SerializeField] private Turret _turretPrefab;
    [SerializeField] private Bomb _bombPrefab;
    [SerializeField] private Transform _objectsParent;

    private void Start()
    {
        InitTeslaTiles();
        InitTurretTiles();
        InitBombTiles();
    }

    private void InitTeslaTiles()
    {
        BoundsInt bounds = _teslaTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (_teslaTilemap.HasTile(cellPos))
                {
                    Vector3 worldPosition = _teslaTilemap.CellToWorld(cellPos);
                    Tesla tesla =  Instantiate(_teslaPrefab, worldPosition + new Vector3(0.5f, 0.5f), Quaternion.identity);
                    tesla.transform.SetParent(_objectsParent);
                }
            }
        }

        _teslaTilemap.gameObject.SetActive(false);
    }

    private void InitTurretTiles()
    {
        BoundsInt bounds = _turretTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (_turretTilemap.HasTile(cellPos))
                {
                    Vector3 worldPosition = _turretTilemap.CellToWorld(cellPos);
                    Turret turret = Instantiate(_turretPrefab, worldPosition + new Vector3(0.5f, 0.5f), Quaternion.identity);
                    turret.transform.SetParent(_objectsParent);
                }
            }
        }

        _turretTilemap.gameObject.SetActive(false);
    }

    private void InitBombTiles()
    {
        BoundsInt bounds = _bombTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (_bombTilemap.HasTile(cellPos))
                {
                    Vector3 worldPosition = _bombTilemap.CellToWorld(cellPos);
                    Bomb bomb = Instantiate(_bombPrefab, worldPosition + new Vector3(0.5f, 0.5f), Quaternion.identity);
                    bomb.transform.SetParent(_objectsParent);
                }
            }
        }

        _bombTilemap.gameObject.SetActive(false);
    }


    public void ShowNavigationToNextRoom()
    {
        //_destination = _ground.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        //_arrowsVisualizer.VisualizePathway(_pathfinder.FindPath(_origin, _destination), _ground, this);
    }
}
