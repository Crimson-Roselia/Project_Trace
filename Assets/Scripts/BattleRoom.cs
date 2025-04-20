using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoom : MonoBehaviour
{
    public List<BattleWave> BattleWaves;
}


[SerializeField]
public class BattleWave
{
    public List<EnemySpawn> EnemiesThisWave;
}

public class EnemySpawn
{
    public string EnemyName;
    public Vector3 SpawnLocation;
}
