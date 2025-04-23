using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcSegment : MonoBehaviour
{
    private float _damage;
    private Tesla _ownerCoil;

    public void Initialize(float damage, Tesla owner)
    {
        _damage = damage;
        _ownerCoil = owner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.ReduceHealthPoint(_damage, true);
        }
    }
}
