using HLH.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleRoom : MonoBehaviour
{
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private Transform _enemyParent;
    [SerializeField] public List<BattleWave> BattleWaves = new List<BattleWave>();
    [SerializeField] public bool BattleAutoStartsOnEnter = false;
    
    [Header("战斗事件")]
    public UnityEvent OnBattleStart;
    public UnityEvent OnBattleEnd;
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveEnd;
    
    private int _currentWaveIndex = -1;
    private List<GameObject> _currentWaveEnemies = new List<GameObject>();
    private bool _battleActive { get; set; }
    private Dictionary<string, GameObject> _enemyPrefabLookup = new Dictionary<string, GameObject>();
    private BoxCollider2D _roomTrigger;

    void Awake()
    {
        _roomTrigger = GetComponent<BoxCollider2D>();
        _battleActive = false;

        // 创建敌人预制体查询字典
        if (_enemyPrefabs != null)
        {
            foreach (var enemyPrefab in _enemyPrefabs)
            {
                if (enemyPrefab != null)
                {
                    _enemyPrefabLookup[enemyPrefab.name] = enemyPrefab;
                }
            }
        }
        
        if (_enemyParent == null)
        {
            _enemyParent = transform;
        }
    }

    private void Start()
    {
        GameManager.Instance.OnGameEnterBattleState += StartBattle;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameEnterBattleState -= StartBattle;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (BattleAutoStartsOnEnter)
        {
            StartBattle();
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle()
    {
        if (_battleActive) return;

        if (_roomTrigger.OverlapPoint(PlayerController.Instance.transform.position))
        {
            _battleActive = true;
            _currentWaveIndex = -1;

            // 触发战斗开始事件
            OnBattleStart?.Invoke();

            // 启动第一波敌人
            SpawnNextWave();
        }
    }
    
    /// <summary>
    /// 生成下一波敌人
    /// </summary>
    private void SpawnNextWave()
    {
        _currentWaveIndex++;
        
        // 检查是否还有下一波
        if (_currentWaveIndex >= BattleWaves.Count)
        {
            EndBattle();
            return;
        }
        
        // 触发波次开始事件
        OnWaveStart?.Invoke(_currentWaveIndex);
        
        // 生成当前波次的敌人
        _currentWaveEnemies.Clear();
        BattleWave currentWave = BattleWaves[_currentWaveIndex];
        
        foreach (var enemySpawn in currentWave.EnemiesThisWave)
        {
            if (string.IsNullOrEmpty(enemySpawn.EnemyName)) continue;
            
            // 检查敌人预制体是否存在
            if (_enemyPrefabLookup.TryGetValue(enemySpawn.EnemyName, out GameObject enemyPrefab))
            {
                // 在指定位置生成敌人
                GameObject enemy = Instantiate(enemyPrefab, transform.TransformPoint(enemySpawn.SpawnLocation), Quaternion.identity, _enemyParent);
                BattleManager.Instance.EnemiesInScene.Add(enemy.GetComponent<IEnemy>());
                _currentWaveEnemies.Add(enemy);
                
                // 监听敌人死亡（假设实现了IEnemy接口）
                IEnemy enemyInterface = enemy.GetComponent<IEnemy>();
                if (enemyInterface != null)
                {
                    StartCoroutine(MonitorEnemy(enemy, enemyInterface));
                }
            }
            else
            {
                Debug.LogWarning($"找不到敌人预制体: {enemySpawn.EnemyName}");
            }
        }
        
        // 如果没有敌人，直接进入下一波
        if (_currentWaveEnemies.Count == 0)
        {
            EndCurrentWave();
        }
    }
    
    /// <summary>
    /// 监控敌人状态
    /// </summary>
    private IEnumerator MonitorEnemy(GameObject enemyObject, IEnemy enemy)
    {
        // 确保敌人还存在
        while (enemyObject != null && _currentWaveEnemies.Contains(enemyObject))
        {
            yield return new WaitForSeconds(0.5f);
            
            // 如果敌人被销毁，从列表中移除
            if (enemyObject == null)
            {
                _currentWaveEnemies.Remove(enemyObject);
                CheckWaveCompletion();
                yield break;
            }
        }
    }
    
    /// <summary>
    /// 检查当前波次是否已完成
    /// </summary>
    private void CheckWaveCompletion()
    {
        // 移除已经被销毁的敌人
        _currentWaveEnemies.RemoveAll(e => e == null);
        
        // 如果所有敌人都被击败，开始下一波
        if (_currentWaveEnemies.Count == 0)
        {
            EndCurrentWave();
        }
    }
    
    /// <summary>
    /// 结束当前波次
    /// </summary>
    private void EndCurrentWave()
    {
        // 触发波次结束事件
        OnWaveEnd?.Invoke(_currentWaveIndex);
        
        // 延迟一会儿再生成下一波
        StartCoroutine(DelayNextWave());
    }
    
    /// <summary>
    /// 延迟生成下一波敌人
    /// </summary>
    private IEnumerator DelayNextWave()
    {
        yield return new WaitForSeconds(2f);
        SpawnNextWave();
    }
    
    /// <summary>
    /// 结束战斗
    /// </summary>
    private void EndBattle()
    {  
        // 触发战斗结束事件
        OnBattleEnd?.Invoke();
    }
    
    /// <summary>
    /// 手动结束战斗（可从外部调用）
    /// </summary>
    public void ForceEndBattle()
    {
        if (!_battleActive) return;
        
        // 清理所有敌人
        foreach (var enemy in _currentWaveEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        _currentWaveEnemies.Clear();
        
        EndBattle();
    }
    
    /// <summary>
    /// 可视化敌人生成点
    /// </summary>
    private void OnDrawGizmos()
    {
        if (BattleWaves == null) return;
        
        for (int waveIndex = 0; waveIndex < BattleWaves.Count; waveIndex++)
        {
            BattleWave wave = BattleWaves[waveIndex];
            if (wave.EnemiesThisWave == null) continue;
            
            // 不同波次使用不同颜色
            Color waveColor = new Color(
                Mathf.Sin(waveIndex * 0.7f) * 0.5f + 0.5f,
                Mathf.Sin(waveIndex * 0.4f) * 0.5f + 0.5f,
                Mathf.Sin(waveIndex * 0.3f) * 0.5f + 0.5f
            );
            
            // 绘制每个敌人生成点
            for (int spawnIndex = 0; spawnIndex < wave.EnemiesThisWave.Count; spawnIndex++)
            {
                EnemySpawn spawn = wave.EnemiesThisWave[spawnIndex];
                Vector3 worldPos = transform.TransformPoint(spawn.SpawnLocation);
                
                // 绘制敌人位置球体
                Gizmos.color = waveColor;
                Gizmos.DrawSphere(worldPos, 0.5f);
                
                // 绘制波次和索引编号
                #if UNITY_EDITOR
                UnityEditor.Handles.color = waveColor;
                UnityEditor.Handles.Label(worldPos + Vector3.up, $"Wave {waveIndex+1}\nEnemy {spawnIndex+1}\n{spawn.EnemyName}");
                #endif
            }
        }
    }
}

[Serializable]
public class BattleWave
{
    public List<EnemySpawn> EnemiesThisWave = new List<EnemySpawn>();
}

[Serializable]
public class EnemySpawn
{
    public string EnemyName;
    public Vector3 SpawnLocation;
}
