using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HLH.Mechanics
{
    public class BattleManager : MonoBehaviour
    {
        public Tilemap GroundTilemap;
        public Tilemap WallsTilemap;
        public HashSet<IEnemy> EnemiesInScene = new HashSet<IEnemy>();
        public LayerMask DetectableLayerM;
        [SerializeField] private List<BoxCollider2D> _battleAreas;
        
        

        public static BattleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                if (Instance != this)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Instance = this;
            }


        }

        private void Start()
        {
            RangedWarrior[] rangedWarriors = FindObjectsByType<RangedWarrior>(FindObjectsSortMode.None);
            for (int i = 0; i < rangedWarriors.Length; i++)
            {
                if (rangedWarriors[i] is IEnemy)
                {
                    EnemiesInScene.Add((IEnemy)rangedWarriors[i]);
                }
            }
        }

        public HashSet<RangedWarrior> GetRangedWarriors()
        {
            HashSet<RangedWarrior> result = new HashSet<RangedWarrior>();
            foreach (IEnemy enemy in EnemiesInScene)
            {
                if (enemy is RangedWarrior)
                {
                    result.Add((RangedWarrior)enemy);
                }
            }
            return result;
        }

        public void RemoveEnemy(IEnemy enemy)
        {
            if (EnemiesInScene.Contains(enemy))
            {
                EnemiesInScene.Remove(enemy);
            }
        }
    }


    [SerializeField]
    public class BattleWave
    {
        public List<EnemySpawn> _enemiesThisWave;
    }

    public class EnemySpawn
    {
        public string EnemyName;
        public Vector3 SpawnLocation;
    }
}