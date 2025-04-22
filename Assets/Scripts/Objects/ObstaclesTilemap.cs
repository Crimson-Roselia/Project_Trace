using DG.Tweening;
using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesTilemap : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            DOTween.Kill(player.transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            DOTween.Kill(player.transform);
        }
    }
}
