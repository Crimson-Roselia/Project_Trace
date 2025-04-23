using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VisualNovel.Mechanics;

public class Door : MonoBehaviour
{
    private bool _isTriggered = false;
    private Transform _destination;

    private void Start()
    {
        _destination = GetComponentsInChildren<Transform>()[1];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isTriggered)
        {
            if (collision.TryGetComponent<PlayerController>(out PlayerController player))
            {
                DialogueSystem.Instance.CrossfadeIntoCombat();
                _isTriggered = true;
                Invoke("TeleportPlayer", 1f);
            }
        }
    }

    public void TeleportPlayer()
    {
        PlayerController.Instance.transform.position = _destination.position;
    }
}
