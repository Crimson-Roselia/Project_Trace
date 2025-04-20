using DG.Tweening;
using HLH.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Profiling;
using UnityEngine;

namespace HLH.Mechanics
{
    public class PlayerController : MonoBehaviour
    {
        #region References
        [SerializeField] private Animator _animatorNormal;
        [SerializeField] private SpriteRenderer _visualNomal;
        [SerializeField] private Animator _animatorSwordAttack;
        [SerializeField] private PlayerHealthBar _hpBar;
        [SerializeField] private ParticleSystem _slashParticle;
        [SerializeField] private List<AudioClip> _slashAudios;
        private Rigidbody2D _rb;
        private AudioSource _audioSource;
        #endregion

        #region Porperties & Logic Variables
        public Vector3 PlayerPosition { get { return transform.position; } }
        private Vector3 _interactPoint { get { return transform.position + _faceDirection; } }
        public float PlayerHealthPropotion { get { return Mathf.Clamp(_health / _maxHealth, 0f, 1f); } }
        private Vector3 _moveDirection;
        private Vector3 _faceDirection;
        private bool _isSwordAttacking { get; set; }
        #endregion

        #region Gameplay Variables
        public float MoveSpeed = 2f;
        private float _health = 100f;
        private float _maxHealth = 100f;
        private bool _isInvincible = false;
        private float _swordDamage = 25f;
        private float _dashTicker = 0f;
        private readonly float DASH_COOLDOWN = 1.0f;
        private readonly float DASH_DISTANCE = 2.0f;
        #endregion


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            ReadInput();
            TickFaceDirection();
        }

        private void TickFaceDirection()
        {
            if (_moveDirection != Vector3.zero)
            {
                _faceDirection = _moveDirection;
            }
        }

        private void ReadInput()
        {
            if (!_isSwordAttacking)
            {
                _moveDirection = Input.GetAxisRaw("Horizontal") * Vector3.right + Input.GetAxisRaw("Vertical") * Vector3.up;
            }
            else
            {
                _moveDirection = Vector3.zero;
            }

            if (_moveDirection != Vector3.zero)
            {
                _animatorNormal.SetFloat("verticalSpeed", _moveDirection.y);
                _animatorNormal.SetFloat("horizontalSpeed", _moveDirection.x);
                _animatorNormal.SetBool("isMoving", true);
                _moveDirection = _moveDirection.normalized;
                _faceDirection = _moveDirection;
            }
            else
            {
                _animatorNormal.SetBool("isMoving", false);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!_isSwordAttacking)
                {
                    PerformSwordFlashAttack();
                }
            }

            _dashTicker -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryDash();
            }
        }

        private void TryDash()
        {
            if (_dashTicker < 0f)
            {
                _dashTicker = DASH_COOLDOWN;

                transform.DOMove(transform.position + DASH_DISTANCE * _moveDirection, 0.1f);

                Vector3 startPoint = _visualNomal.transform.position;
                float distanceBetweenBlur = 0.2f;
                // 计算箭头数量
                int blurCount = Mathf.FloorToInt(DASH_DISTANCE / distanceBetweenBlur);
                if (blurCount <= 0) return;

                for (int i = 0; i < blurCount; i++)
                {
                    float targetDistance = (i + 1) * distanceBetweenBlur;

                    GameObject newBlurObj = new GameObject("Blur");
                    Vector3 arrowPosition = startPoint + _moveDirection * targetDistance;
                    newBlurObj.transform.position = arrowPosition;
                    SpriteRenderer newBlur = newBlurObj.AddComponent<SpriteRenderer>();
                    newBlur.sortingOrder = _visualNomal.sortingOrder;
                    newBlur.sprite = _visualNomal.sprite;
                    newBlur.DOFade(0f, 0.4f);
                    Destroy(newBlur.gameObject, 0.4f);
                }
            }
        }

        private void FixedUpdate()
        {
            _rb.MovePosition(_rb.position + (Vector2)_moveDirection * MoveSpeed * Time.fixedDeltaTime);
        }

        private void PerformSwordFlashAttack()
        {
            PlaySlashParticleEffect();
            _audioSource.PlayOneShot(_slashAudios[UnityEngine.Random.Range(0, _slashAudios.Count)]);

            float attackDirX; // x of the Vector2 representing direction from player to hitpoint
            float attackDirY; // y of the Vector2 representing direction from player to hitpoint
            if (TryGetNearbyEnemy(out IEnemy enemy, 2.5f))
            {
                attackDirX = enemy.GetEnemyPosition().x - transform.position.x;
                attackDirY = enemy.GetEnemyPosition().y - transform.position.y;
                StartCoroutine(AboutToHitEnemy(enemy, 0.15f));
            }
            else
            {
                attackDirX = _faceDirection.x;
                attackDirY = _faceDirection.y;
            }
            _animatorNormal.gameObject.SetActive(false);
            _animatorSwordAttack.gameObject.SetActive(true);
            _animatorSwordAttack.SetFloat("swordAttackX", attackDirX);
            _animatorSwordAttack.SetFloat("swordAttackY", attackDirY);
            StartCoroutine(SwitchToNormalVisualIn(0.45f));
        }

        private void PlaySlashParticleEffect()
        {
            if (_faceDirection.x > 0)
            {
                _slashParticle.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
            }
            else if (_faceDirection.x < 0)
            {
                _slashParticle.transform.rotation = Quaternion.AngleAxis(270, Vector3.forward);
            }
            else if (_faceDirection.y > 0)
            {
                _slashParticle.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
            }
            else if (_faceDirection.y < 0)
            {
                _slashParticle.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            }
            _slashParticle.Play();
        }

        private IEnumerator AboutToHitEnemy(IEnemy target, float delay)
        {
            yield return new WaitForSeconds(delay);
            target?.TakeHit(_swordDamage);

        }

        private void ActivateNormalVisual()
        {
            _animatorNormal.gameObject.SetActive(true);
            _animatorSwordAttack.gameObject.SetActive(false);
        }

        private IEnumerator SwitchToNormalVisualIn(float waitSecs)
        {
            _isSwordAttacking = true;
            yield return new WaitForSeconds(waitSecs);
            ActivateNormalVisual();
            _isSwordAttacking = false;
        }

        private bool TryGetNearbyEnemy(out IEnemy nearestEnemy, float detectDistance)
        {
            if (BattleManager.Instance.EnemiesInScene != null)
            {
                foreach (IEnemy enemy in BattleManager.Instance.EnemiesInScene)
                {
                    if (Vector3.Distance(transform.position, enemy.GetEnemyPosition()) < detectDistance)
                    {
                        nearestEnemy = enemy;
                        return true;
                    }
                }
            }
            nearestEnemy = null;
            return false;
        }

        public void ReduceHealthPoint(float reducedAmount)
        {
            if (_isInvincible)
            {
                return;
            }

            _health -= reducedAmount;
            if (_health <= 0)
            {
                Debug.Log("Game Over");
            }

            if (_hpBar != null)
            {
                _hpBar.StartCoroutine(_hpBar.StartReduceOrangeBar());
            }
            StartCoroutine(SetInvincible());
        }

        private IEnumerator SetInvincible()
        {
            _isInvincible = true;
            yield return new WaitForSeconds(1f);
            _isInvincible = false;
        }
    }
}