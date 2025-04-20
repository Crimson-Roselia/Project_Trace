using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField] private Tilemap _ground;
    [SerializeField] private Tilemap _obstacle;
    [SerializeField] private SpriteRenderer _arrowPrefab;
    [SerializeField] private Transform _arrowParent;
    private bool _hasClicked = false;
    private Vector3Int _origin;
    private Vector3Int _destination;
    private Pathfinder _pathfinder;
    private PathArrowsVisualizer _arrowsVisualizer;

    private void Awake()
    {
        _pathfinder = new Pathfinder(_ground, _obstacle);
        _arrowsVisualizer = new PathArrowsVisualizer(_arrowPrefab, _arrowParent);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = 0;
            if (!_hasClicked)
            {
                if (_ground.HasTile(_ground.WorldToCell(clickPos)))
                {
                    _origin = _ground.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    _hasClicked = true;
                }
            }
            else
            {
                if (_ground.HasTile(_ground.WorldToCell(clickPos)))
                {
                    _destination = _ground.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    _arrowsVisualizer.VisualizePathway(_pathfinder.FindPath(_origin, _destination), _ground, this);
                    _hasClicked = false;
                }
            }
        }
    }
}
