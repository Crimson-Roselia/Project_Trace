using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector3 _moveDirection;
    private Rigidbody _rb;
    private Animator _animator;
    private Vector3 _interactPoint { get { return transform.position + (Vector3)_faceDirection; } }
    private Vector3 _faceDirection;
    private readonly float _moveSpeed = 10f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // ʹ�ô�ͳ��Inputϵͳ����ȡ�û���ֱ��ˮƽ�����ϵ����룬��ֵ��moveDirection�ƶ�����
        _moveDirection = Input.GetAxisRaw("Horizontal") * Vector3.left + Input.GetAxisRaw("Vertical") * Vector3.back;
        // ��moveDirection������һ����ʹ�����������
        if (_moveDirection != Vector3.zero)
        {
            _moveDirection = _moveDirection.normalized;
            _animator.SetBool("isMoving", true);
            _animator.SetFloat("horizontal", _moveDirection.x);
            _animator.SetFloat("vertical", _moveDirection.y);
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }
    }


    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);
    }

    public void FaceLeft()
    {
        _faceDirection = Vector2.left;
    }

    public void FaceRight()
    {
        _faceDirection = Vector2.right;
    }

    public void FaceUp()
    {
        _faceDirection = Vector2.up;
    }

    public void FaceDown()
    {
        _faceDirection = Vector2.down;
    }
}
