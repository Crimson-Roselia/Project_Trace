using DG.Tweening;
using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapsTilemap : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.ReduceHealthPoint(10f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.ReduceHealthPoint(10f);
        }
    }
}
