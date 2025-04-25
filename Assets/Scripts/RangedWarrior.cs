using DG.Tweening;
using HLH.Mechanics;
using HLH.Objects;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RangedWarrior : MonoBehaviour, IEnemy
{
    [SerializeField] private float _detectDistance = 15f;
    [SerializeField] private float _attackDistance = 3f;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _visual;
    [SerializeField] public TextMeshProUGUI _aiDebugText;
    [SerializeField] private Medicine _medicinePrefab;
    
    public const float AI_EVALUATE_INTERVAL = 0.1f;
    private Node _aiTopNode;
    private Collider2D _wallsTilemapCollider;

    private Pathfinder _aStarPathfinder;
    private PlayerController _player;
    private List<Vector3> _navigationPath;
    private Coroutine _navigatingProcess;
    private Coroutine _tripleAttackProcess;
    private bool _playerSighted = false;
    private CharacterController _characterController;
    private Vector2 _playerDirection = Vector2.zero;
    private float _moveSpeed = 2f;
    private float _maxHealth = 100f;
    private float _health;
    private AudioSource _audioSource;
    private float rapidFireBulletSpeed = 5f;

    void Start()
    {
        _health = _maxHealth;
        _player = FindObjectOfType<PlayerController>();
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        
        // 初始化寻路系统
        _aStarPathfinder = new Pathfinder(BattleManager.Instance.GroundTilemap, BattleManager.Instance.WallsTilemap);
        
        // 构建行为树
        ConstructBehaviourTree();
        
        // 开始AI评估循环
        StartCoroutine(TickingBehaviourTree());
    }


    private void ConstructBehaviourTree()
    {
        // 创建基本条件节点
        IsPlayerInSightNode isPlayerInSightNode = new IsPlayerInSightNode(this, "isPlayerInSightNode");
        CanAttackNode canAttackNode = new CanAttackNode(this, "canAttackNode");
        HasDestinationNode hasDestinationNode = new HasDestinationNode(this, "hasDestinationNode");
        CasualNode casualNode = new CasualNode(this, "casualNode");
        
        // 创建序列和选择器节点
        Sequence maneuverSequence = new Sequence(this, "maneuverSequence");
        maneuverSequence.AddChild(isPlayerInSightNode);
        maneuverSequence.AddChild(hasDestinationNode);
        
        Sequence alertSequence = new Sequence(this, "alertSequence");
        alertSequence.AddChild(isPlayerInSightNode);
        alertSequence.AddChild(canAttackNode);

        ChaseNode chaseSequence = new ChaseNode(this, "chaseSequence");
        chaseSequence.AddChild(isPlayerInSightNode);

        Selector inFightSelector = new Selector(this, "inFightSelector");
        inFightSelector.AddChild(maneuverSequence);
        inFightSelector.AddChild(alertSequence);
        inFightSelector.AddChild(chaseSequence);
        
        // 创建顶层选择器
        Selector topNode = new Selector(this, "topNode");
        topNode.AddChild(inFightSelector);
        topNode.AddChild(casualNode);
        
        _aiTopNode = topNode;
    }
    
    private IEnumerator TickingBehaviourTree()
    {
        while (true)
        {
            // 尝试探测玩家
            _playerSighted = TryRaycastDetectPlayer();
            
            // 评估行为树
            _aiTopNode.Evaluate();

            // 打印调试信息
            PrintBehaviourTreePath();

            yield return new WaitForSeconds(AI_EVALUATE_INTERVAL);
        }
    }
    
    private bool TryRaycastDetectPlayer()
    {
        if (_player == null)
            return false;
            
        Vector2 directionToPlayer = (_player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        
        if (distanceToPlayer > _detectDistance)
            return false;
            
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, BattleManager.Instance.DetectableLayerM);
        
        if (hit.collider != null)
        {
            // 检查是否击中墙壁或其他障碍物
            if (hit.collider == _wallsTilemapCollider)
            {
                return false;
            }
            
            // 检查是否击中玩家
            PlayerController hitPlayer = hit.collider.GetComponent<PlayerController>();
            if (hitPlayer != null)
            {
                _playerDirection = directionToPlayer.normalized;
                return true;
            }
        }
        
        return false;
    }

    private void ChasePlayer()
    {
        // 计算路径
        List<Vector3Int> path = _aStarPathfinder.FindPath(_aStarPathfinder.WorldToCell(transform.position), _aStarPathfinder.WorldToCell((Vector2)_player.transform.position - _attackDistance * _playerDirection));
        // 转换为世界坐标
        _navigationPath = Pathfinder.GetWorldPositionFromCoords(path, BattleManager.Instance.GroundTilemap);

        if (_navigationPath != null && _navigationPath.Count > 0)
        {

            // 开始导航
            if (_navigatingProcess != null)
            {
                StopCoroutine(_navigatingProcess);
            }
            
            _navigatingProcess = StartCoroutine(SetNavigateThrough(_navigationPath));
        }
    }
    
    private void RandomManuever()
    {
        // 随机选择附近的一个可达点作为目标
        Vector3Int currentCell = _aStarPathfinder.WorldToCell(transform.position);
        List<Vector3Int> possibleDestinations = new List<Vector3Int>();
        
        // 遍历附近的单元格
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int targetCell = currentCell + new Vector3Int(x, y, 0);
                
                // 验证这个单元格可以走到
                List<Vector3Int> path = _aStarPathfinder.FindPath(currentCell, targetCell);
                if (path.Count > 0)
                {
                    possibleDestinations.Add(targetCell);
                }
            }
        }
        
        if (possibleDestinations.Count > 0)
        {
            // 随机选择一个目标
            Vector3Int randomDestination = possibleDestinations[Random.Range(0, possibleDestinations.Count)];
            
            // 计算路径
            List<Vector3Int> path = _aStarPathfinder.FindPath(currentCell, randomDestination);
          
            // 转换为世界坐标
            _navigationPath = Pathfinder.GetWorldPositionFromCoords(path, BattleManager.Instance.GroundTilemap);

            if (_navigationPath != null && _navigationPath.Count > 0)
            {

                // 开始导航
                if (_navigatingProcess != null)
                {
                    StopCoroutine(_navigatingProcess);
                }
                _navigatingProcess = StartCoroutine(SetNavigateThrough(_navigationPath));
            }
        }
    }

    private IEnumerator SetNavigateThrough(List<Vector3> pathway)
    {
        if (pathway == null || pathway.Count == 0)
        {
            yield break;
        }
        
        int currentIndex = 0;
        _animator.SetBool("isMoving", true);
        
        while (currentIndex < pathway.Count)
        {
            Vector3 targetPosition = pathway[currentIndex];
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            // 移动角色
            _characterController.Move(moveDirection * _moveSpeed * Time.deltaTime);
            
            // 检查是否到达当前路点
            if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
            {
                currentIndex++;
            }
            yield return null;
        }
        
        _animator.SetBool("isMoving", false);
        _navigatingProcess = null;
    }

    public IEnumerator TripleAttack()
    {
        _animator.SetTrigger("attack");

        // 发射子弹
        for (int i = 0; i < 3; i++)
        {
            Vector2 fireDirection = _playerDirection;
            Bullet bullet = Instantiate(_bulletPrefab, (Vector2)transform.position + fireDirection * 0.5f, Quaternion.identity);
            if (i == 0)
            {
                yield return new WaitForSeconds(0.4f);
                bullet.OnBulletFired(fireDirection, rapidFireBulletSpeed);
            }
            else
            {
                bullet.OnBulletFired(fireDirection, rapidFireBulletSpeed);
            }
            yield return new WaitForSeconds(0.7f);
        }

        _tripleAttackProcess = null;
    }
      
    public Vector3 GetEnemyPosition()
    {
        return transform.position;
    }
    
    public void TakeHit(float damageAmount, PlayerController dmgSource)
    {
        _health -= damageAmount;
        _visual.color = Color.red;
        _visual.DOBlendableColor(Color.white, 1f);

        Vector3Int currentCell = _aStarPathfinder.WorldToCell(transform.position);
        Vector3Int backCell = _aStarPathfinder.WorldToCell(transform.position - (Vector3)_playerDirection.normalized);
        List<Vector3Int> path = _aStarPathfinder.FindPath(currentCell, backCell);
        if (path.Count > 0)
        {
            transform.DOJump(transform.position - (Vector3)_playerDirection.normalized, 0.5f, 1, 0.4f);
        }
        else
        {
            transform.DOJump(transform.position, 0.5f, 1, 0.4f);
        }


        _animator.SetTrigger("hit");
        
        if (_health <= 0)
        {
            Instantiate(_medicinePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject, 0.2f);
        }
    }
    
    
    // 行为树节点类
    private class IsPlayerInSightNode : Node
    {
        public IsPlayerInSightNode(RangedWarrior owner, string name) : base(owner, name) { }
        
        public override bool Evaluate()
        {
            if (_owner._playerSighted)
            {
                _owner._animator.SetFloat("horizontalSpeed", _owner._playerDirection.x);
                _owner._animator.SetFloat("verticalSpeed", _owner._playerDirection.y);
                _owner._animator.SetFloat("swordAttackX", _owner._playerDirection.x);
                _owner._animator.SetFloat("swordAttackY", _owner._playerDirection.y);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    private class CanAttackNode : Node
    {
        private float _attackCooldown = 5f;
        private readonly float _attackInterval = 6f;

        public CanAttackNode(RangedWarrior owner, string name) : base(owner, name) { }
        
        public override bool Evaluate()
        {
            if (_owner._player == null || !_owner._playerSighted)
            {
                return false;
            }

            float distanceToPlayer = Vector3.Distance(_owner.transform.position, _owner._player.transform.position);
            bool canAttack = distanceToPlayer < _owner._attackDistance;
            
            if (canAttack)
            {
                _attackCooldown += RangedWarrior.AI_EVALUATE_INTERVAL;
                if (_attackCooldown > _attackInterval)
                {
                    _owner.StartCoroutine(_owner.TripleAttack());
                    _attackCooldown = 0f;
                }
            }
            
            return canAttack;
        }
    }
    
    private class HasDestinationNode : Node
    {
        public HasDestinationNode(RangedWarrior owner, string name) : base(owner, name) { }
        private readonly float MANUEVER_INTERVAL = 2f;
        private float _manueverCounter = 0f;
        
        public override bool Evaluate()
        {
            if (_owner._navigatingProcess == null)
            {
                _manueverCounter += RangedWarrior.AI_EVALUATE_INTERVAL;
                if (_manueverCounter > MANUEVER_INTERVAL)
                {
                    _manueverCounter = 0f;
                    if (Random.value > 0.6f)
                    {
                        _owner.RandomManuever();
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    
    private class CasualNode : Node
    {
        private float _casualDecisionTimer = 0f;
        private readonly float _casualDecisionInterval = 3f;
        
        public CasualNode(RangedWarrior owner, string name) : base(owner, name) { }
        
        public override bool Evaluate()
        {
            _casualDecisionTimer += RangedWarrior.AI_EVALUATE_INTERVAL;
            
            if (_casualDecisionTimer >= _casualDecisionInterval)
            {
                _casualDecisionTimer = 0f;
                
                // 随机决定是否漫游
                if (Random.value > 0.7f && _owner._navigatingProcess == null)
                {
                    
                }
            }
            
            return true;
        }
    }

    private class ChaseNode : Sequence
    {
        private float _chaseNavigationTimer = 0f;
        private readonly float _chaseNavigationInterval = 3f;

        public ChaseNode(RangedWarrior owner, string name) : base(owner, name) { }

        public override bool Evaluate()
        {
            bool isSuccess = base.Evaluate();
            
            if (isSuccess)
            {
                _chaseNavigationTimer += RangedWarrior.AI_EVALUATE_INTERVAL;
                if (_chaseNavigationTimer > _chaseNavigationInterval)
                {
                    _chaseNavigationTimer = 0f;
                    _owner.ChasePlayer();
                }
            }
            return isSuccess;
        }
    }

    private void PrintBehaviourTreePath()
    {
        List<string> path = _aiTopNode.GetDebugPath();
        string pathString = string.Join(" -> ", path);
        _aiDebugText.text = pathString;
    }

    private void OnDestroy()
    {
        BattleManager.Instance.RemoveEnemy(this);
    }
}
