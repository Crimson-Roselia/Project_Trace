using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _visual;

    private void Start()
    {
        _visual.DOFade(0f, 2.5f);
    }
}
