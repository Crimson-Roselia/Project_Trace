using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathArrowsVisualizer
{
    private float _distanceBetweenArrows = 0.75f;
    private SpriteRenderer _spriteArrowPrefab;
    private List<Vector3> _pathwayAnchors;
    private List<SpriteRenderer> _spawnedArrows = new List<SpriteRenderer>();
    private Transform _arrowsParent;

    public PathArrowsVisualizer(SpriteRenderer spriteArrowPrefab, Transform arrowsParent, float distanceBetweenArrows = 0.5f)
    {
        _spriteArrowPrefab = spriteArrowPrefab;
        _arrowsParent = arrowsParent;
        _distanceBetweenArrows = distanceBetweenArrows;
        _pathwayAnchors = new List<Vector3>();
    }

    public void VisualizePathway(List<Vector3Int> pathPositions, Tilemap tilemap, MonoBehaviour coroutineStarter)
    {
        if (pathPositions == null || pathPositions.Count <= 1)
        {
            ClearArrows();
            return;
        }
        // 转换路径点为世界坐标
        _pathwayAnchors = Pathfinder.GetWorldPositionFromCoords(pathPositions, tilemap);
        // 生成箭头
        coroutineStarter.StartCoroutine(GenerateArrows());
    }

    public void VisualizePathway(List<Vector3> worldPositions, MonoBehaviour coroutineStarter)
    {
        if (worldPositions == null || worldPositions.Count <= 1)
        {
            ClearArrows();
            return;
        }

        _pathwayAnchors = new List<Vector3>(worldPositions);
        coroutineStarter.StartCoroutine(GenerateArrows());
    }

    private IEnumerator GenerateArrows()
    {
        ClearArrows();

        // 计算路径总长度
        float totalPathLength = 0f;
        for (int i = 0; i < _pathwayAnchors.Count - 1; i++)
        {
            totalPathLength += Vector3.Distance(_pathwayAnchors[i], _pathwayAnchors[i + 1]);
        }
        // 计算箭头数量
        int arrowCount = Mathf.FloorToInt(totalPathLength / _distanceBetweenArrows);
        if (arrowCount <= 0) yield break;

        float distanceCovered = 0f;
        int pathSegmentIndex = 0;
        Vector3 previousPosition = _pathwayAnchors[0];

        for (int i = 0; i < arrowCount; i++)
        {
            float targetDistance = (i + 1) * _distanceBetweenArrows;
            
            // 找到箭头应该放置的路径段
            while (pathSegmentIndex < _pathwayAnchors.Count - 1)
            {
                float segmentLength = Vector3.Distance(_pathwayAnchors[pathSegmentIndex], _pathwayAnchors[pathSegmentIndex + 1]);
                if (distanceCovered + segmentLength >= targetDistance)
                {
                    // 在这个路径段上放置箭头
                    float t = (targetDistance - distanceCovered) / segmentLength;
                    Vector3 arrowPosition = Vector3.Lerp(_pathwayAnchors[pathSegmentIndex], _pathwayAnchors[pathSegmentIndex + 1], t);
                    
                    // 计算箭头旋转方向
                    Vector3 direction = (_pathwayAnchors[pathSegmentIndex + 1] - _pathwayAnchors[pathSegmentIndex]).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    
                    // 创建箭头
                    SpriteRenderer arrow = Object.Instantiate(_spriteArrowPrefab, arrowPosition, Quaternion.Euler(0, 0, angle));
                    arrow.transform.SetParent(_arrowsParent);
                    _spawnedArrows.Add(arrow);
                    
                    break;
                }
                
                distanceCovered += segmentLength;
                pathSegmentIndex++;
            }
            
            // 如果已经到达路径末尾，退出循环
            if (pathSegmentIndex >= _pathwayAnchors.Count - 1)
            {
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ClearArrows()
    {
        foreach (SpriteRenderer arrow in _spawnedArrows)
        {
            if (arrow != null)
            {
                Object.Destroy(arrow.gameObject);
            }
        }
        _spawnedArrows.Clear();
    }

    public void SetDistanceBetweenArrows(float distance)
    {
        if (distance > 0)
        {
            _distanceBetweenArrows = distance;
        }
    }
}
