using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    Vector3 GetEnemyPosition();
    void TakeHit(float damageAmount);
} 