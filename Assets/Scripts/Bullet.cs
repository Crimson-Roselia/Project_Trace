using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HLH.Objects
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] protected ParticleSystem _bulletScatterEffectPrefab;
        [SerializeField] protected Transform _visual;
        protected Rigidbody2D _rb2D;
        protected Vector3 flyDir;
        protected float maxFlyDistance = 50f;
        protected Vector3 spawnPosition;
        protected float flySpd;
        protected float damage = 25f;
        protected AudioSource AudioSource;
        public BulletType type;
        private bool isActivated = false;

        protected virtual void Awake()
        {
            _rb2D = GetComponent<Rigidbody2D>();
            AudioSource = GetComponent<AudioSource>();
            _visual.gameObject.SetActive(false);
        }


        private void FixedUpdate()
        {
            if (isActivated)
            {
                _rb2D.MovePosition(transform.position + flyDir * flySpd * Time.fixedDeltaTime);
                _visual.gameObject.SetActive(true);

                if (Vector3.Distance(transform.position, spawnPosition) > maxFlyDistance)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                _visual.gameObject.SetActive(false);
            }
        }

        public virtual void OnBulletFired(Vector2 flyDirection, float flySpeed, float delayedTime)
        {
            flyDir = flyDirection.normalized;
            flySpd = flySpeed;
            //transform.rotation = Quaternion.LookRotation(Vector3.down, flyDirection);
            if (delayedTime == 0f)
            {
                isActivated = true;
            }
            else
            {
                isActivated = false;
                Invoke("SetBackToActive", delayedTime);
            }

            spawnPosition = transform.position;
        }

        public virtual void OnBulletFired(Vector2 flyDirection, float flySpeed)
        {
            flyDir = flyDirection.normalized;
            flySpd = flySpeed;
            //transform.rotation = Quaternion.LookRotation(Vector3.down, flyDirection);
            isActivated = true;

            spawnPosition = transform.position;
        }

        public void SetBackToActive()
        {
            isActivated = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated)
            {

                if (other.TryGetComponent<PlayerController>(out PlayerController player))
                {
                    player.ReduceHealthPoint(damage, true);
                }
                if (other.tag == "Enemy")
                {
                    if (type == BulletType.MonsterBullet || other.TryGetComponent<BattleRoom>(out BattleRoom room))
                    {
                        return;
                    }
                }
                if (other.tag == "Trigger")
                {
                    return;
                }

                if (type == BulletType.MonsterBullet)
                {
                    //Instantiate(_bulletScatterEffectPrefab, transform.position, _bulletScatterEffectPrefab.transform.rotation);
                }

                Debug.Log(other.gameObject);
                _visual.gameObject.SetActive(false);
                Destroy(gameObject, 0);
            }
        }

    }

    public enum BulletType
    {
        MonsterBullet, PlayerBullet
    }
}