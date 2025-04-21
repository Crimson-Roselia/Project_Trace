using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VisualNovel.Mechanics;

public class DialogueTrigger : MonoBehaviour
{
    public string DialogueName;
    private bool _isDialogueTriggered = false; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isDialogueTriggered)
        {
            if (collision.TryGetComponent<PlayerController>(out PlayerController player))
            {
                DialogueSystem.Instance.StartDialogue(DialogueName);
                _isDialogueTriggered = true;
            }
        }
    }
}
