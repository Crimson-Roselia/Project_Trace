using HLH.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesla : MonoBehaviour
{
    [Header("链接设置")]
    [SerializeField] private float _linkRadius = 10f;
    [SerializeField] private float _linkAngle = 45f;
    [SerializeField] private LayerMask _teslaCoilLayer;
    [SerializeField] private Color _linkColor = Color.cyan;
    [SerializeField] private float _lineWidth = 0.1f;

    [Header("电弧设置")]
    [SerializeField] private float _arcActivationInterval = 6f;
    [SerializeField] private float _arcDuration = 3f;
    [SerializeField] private float _arcDamage = 10f;
    [SerializeField] private float _laserWidth = 0.5f;
    [SerializeField] private Sprite _arcSprite;

    private List<Tesla> _linkedCoils = new List<Tesla>();
    private List<LineRenderer> _linkRenderers = new List<LineRenderer>();
    private List<GameObject> _arcObjects = new List<GameObject>();
    private bool _areArcsActive = false;
    private float _arcTimer = 0f;

    private void Start()
    {
        Invoke("InitializeLinks", 0.05f);
    }

    private void InitializeLinks()
    {
        // 尝试在四个方向链接其他特斯拉线圈
        TryLinkInDirection(Vector2.up);
        TryLinkInDirection(Vector2.right);
        TryLinkInDirection(Vector2.down);
        TryLinkInDirection(Vector2.left);
    }

    private void TryLinkInDirection(Vector2 direction)
    {
        // 创建扇形区域检测
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _linkRadius, _teslaCoilLayer);
        Tesla nearestCoil = null;
        float nearestDistance = _linkRadius;

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.gameObject == gameObject) continue;

            Tesla otherCoil = collider.GetComponent<Tesla>();
            if (otherCoil != null)
            {
                Vector2 directionToCoil = (otherCoil.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(direction, directionToCoil);

                // 检查是否在扇区内
                if (angle <= _linkAngle / 2)
                {
                    float distance = Vector2.Distance(transform.position, otherCoil.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCoil = otherCoil;
                    }
                }
            }
        }

        if (nearestCoil != null && !_linkedCoils.Contains(nearestCoil))
        {
            // 添加到链接列表
            _linkedCoils.Add(nearestCoil);
            
            // 创建连接线
            CreateLinkRenderer(nearestCoil);
        }
    }

    private void CreateLinkRenderer(Tesla targetCoil)
    {
        GameObject lineObj = new GameObject("LinkLine");
        lineObj.transform.SetParent(transform);
        
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = _linkColor;
        lineRenderer.endColor = _linkColor;
        
        _linkRenderers.Add(lineRenderer);
        
        // 设置线条端点
        UpdateLinkRenderer(lineRenderer, targetCoil);
    }

    private void UpdateLinkRenderer(LineRenderer lineRenderer, Tesla targetCoil)
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetCoil.transform.position);
    }

    private void Update()
    {
        // 更新链接线位置
        for (int i = 0; i < _linkedCoils.Count; i++)
        {
            if (i < _linkRenderers.Count && _linkedCoils[i] != null)
            {
                UpdateLinkRenderer(_linkRenderers[i], _linkedCoils[i]);
            }
        }

        // 电弧计时器
        _arcTimer += Time.deltaTime;
        if (_arcTimer >= _arcActivationInterval)
        {
            _arcTimer = 0;
            if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) < 30.0f)
            {
                StartCoroutine(ActivateArcs());
            }
        }
    }

    private IEnumerator ActivateArcs()
    {
        // 创建电弧
        CreateArcs();
        _areArcsActive = true;

        yield return new WaitForSeconds(_arcDuration);

        // 销毁电弧
        DestroyArcs();
        _areArcsActive = false;
    }

    private void CreateArcs()
    {
        for (int i = 0; i < _linkedCoils.Count; i++)
        {
            if (_linkedCoils[i] != null)
            {
                // 创建激光电弧父对象
                GameObject arcParent = new GameObject("LaserArc");
                arcParent.transform.SetParent(transform);
                _arcObjects.Add(arcParent);

                Vector3 startPos = transform.position;
                Vector3 endPos = _linkedCoils[i].transform.position;
                Vector3 direction = (endPos - startPos).normalized;
                float distance = Vector3.Distance(startPos, endPos);

                // 使用LineRenderer创建激光
                LineRenderer laserRenderer = arcParent.AddComponent<LineRenderer>();
                laserRenderer.positionCount = 2;
                laserRenderer.SetPosition(0, startPos);
                laserRenderer.SetPosition(1, endPos);
                laserRenderer.startWidth = _laserWidth;
                laserRenderer.endWidth = _laserWidth;
                laserRenderer.material = new Material(Shader.Find("Sprites/Default"));
                laserRenderer.startColor = new Color(_linkColor.r, _linkColor.g, _linkColor.b, 0.8f);
                laserRenderer.endColor = new Color(_linkColor.r, _linkColor.g, _linkColor.b, 0.8f);
                laserRenderer.sortingOrder = 5;

                // 为激光添加碰撞检测
                GameObject colliderObj = new GameObject("LaserCollider");
                colliderObj.transform.SetParent(arcParent.transform);
                
                // 创建Box碰撞器来检测玩家
                BoxCollider2D collider = colliderObj.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                
                // 设置碰撞器的大小和位置
                collider.size = new Vector2(distance, _laserWidth);
                colliderObj.transform.position = (startPos + endPos) / 2;
                
                // 设置碰撞器的旋转以匹配激光方向
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                colliderObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                // 添加电弧脚本组件
                ArcSegment arcScript = colliderObj.AddComponent<ArcSegment>();
                arcScript.Initialize(_arcDamage, this);
                
                // 可选：添加闪烁效果
                StartCoroutine(FlickerLaser(laserRenderer));
            }
        }
    }

    // 激光闪烁效果
    private IEnumerator FlickerLaser(LineRenderer laser)
    {
        float flickerTime = 0.1f;
        Color originalColor = laser.startColor;
        Color dimColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
        
        while (_areArcsActive)
        {
            laser.startColor = originalColor;
            laser.endColor = originalColor;
            yield return new WaitForSeconds(flickerTime);
            
            laser.startColor = dimColor;
            laser.endColor = dimColor;
            yield return new WaitForSeconds(flickerTime * 0.5f);
        }
    }

    private void DestroyArcs()
    {
        foreach (GameObject arcObj in _arcObjects)
        {
            Destroy(arcObj);
        }
        _arcObjects.Clear();
    }
}
