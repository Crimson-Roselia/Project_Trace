using HLH.Mechanics;
using HLH.Objects;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private Bullet _bulletPrefab;
    private float _attackCooldown = 3f;
    private readonly float _attackInterval = 4f;
    private readonly float _attackDistance = 10f;
    private readonly float _bulletFireSpeed = 10f;

    private void Update()
    {
        _attackCooldown += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        bool canAttack = distanceToPlayer < _attackDistance;

        if (canAttack && _attackCooldown > _attackInterval)
        {
            Vector2 fireDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
            Bullet bullet = Instantiate(_bulletPrefab, (Vector2)transform.position + fireDirection * 0.5f, Quaternion.identity);
            bullet.OnBulletFired(fireDirection, _bulletFireSpeed, 0.35f);
            _attackCooldown = 0f;
        }
    }
}
