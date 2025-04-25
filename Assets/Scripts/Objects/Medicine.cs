using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medicine : MonoBehaviour
{
    bool _isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isTriggered)
        {
            if (collision.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.AddHealth(10.0f);
                _isTriggered = true;
                Destroy(gameObject);
            }
        }
    }

}
