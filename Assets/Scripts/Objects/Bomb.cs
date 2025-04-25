using HLH.Mechanics;
using HLH.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Bomb : MonoBehaviour
{
    [SerializeField] private BombOrientation orientation;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private List<Sprite> _explodeSprites;
    private Rigidbody2D _rb2D;
    private bool _isTriggered = false;
    private Vector2 flyDirection { get
        {
            if (orientation == BombOrientation.Left)
            {
                return Vector2.left;
            }
            else if (orientation == BombOrientation.Up)
            {
                return Vector2.up;
            }
            else if (orientation == BombOrientation.Down)
            {
                return Vector2.down;
            }
            else if (orientation == BombOrientation.Right)
            {
                return Vector2.right;
            }
            return Vector2.zero;
        } }
    private float _flySpeed = 10f;

    private void Awake()
    {
        _rb2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_isTriggered)
        {
            _rb2D.MovePosition(_rb2D.position + _flySpeed * flyDirection * Time.fixedDeltaTime);
        }
    }

    private void OnRenderObject()
    {
        if (orientation == BombOrientation.Left)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (orientation == BombOrientation.Up)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
        }
        else if (orientation == BombOrientation.Down)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f));
        }
        else if (orientation == BombOrientation.Right)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out PlayerController player))
        {
            _isTriggered = true;
        }
        else if (_isTriggered)
        {
            Debug.Log(collision.gameObject);

            if (collision.gameObject.GetComponentInParent<RangedWarrior>() != null)
            {
                collision.gameObject.GetComponentInParent<RangedWarrior>().TakeHit(1000, PlayerController.Instance);
                StartCoroutine(Explode());
            }
            else if (collision.TryGetComponent<Bullet>(out Bullet bullet))
            {

            }
            else
            {
                StartCoroutine(Explode());
            }
        }
    }

    private IEnumerator Explode()
    {
        _isTriggered = false;

        for (int i = 0; i < _explodeSprites.Count; i++)
        {
            visual.sprite = _explodeSprites[i];
            yield return new WaitForSeconds(0.15f);
        }

        Destroy(gameObject);
    }
}

public enum BombOrientation
{
    Left, Up, Right, Down
}
