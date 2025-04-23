using DG.Tweening;
using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesTilemap : MonoBehaviour
{
    private float _playerEnterTime = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            _playerEnterTime = 0f;
            player.transform.DOJump(player.transform.position, 0f, 1, 0.1f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerController>(out PlayerController player))
        {
            _playerEnterTime += Time.fixedDeltaTime;
            if (_playerEnterTime < 0.1f)
            {
                player.transform.DOJump(player.transform.position, 0f, 1, 0.1f);
            }
        }
    }
}
