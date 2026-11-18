using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ResumePickButton : MonoBehaviour
{
    [Tooltip("这份简历是不是正确答案")]
    public bool isCorrect;

    [Header("长按确认时长（秒）")]
    public float holdSeconds = 2f;

    [Tooltip("要填充的线框，留空则自动找 SquareFrame")]
    public Renderer frameRenderer;

    [Tooltip("玩家 Rig 根节点，留空自动找 XROrigin。只有它下面的碰撞体算手")]
    public Transform playerRoot;

    [Header("触碰判定")]
    [Tooltip("手/手柄离线框中心多近算触碰（米）")]
    public float touchRadius = 0.15f;

    [Tooltip("判定体积的最大半径（米）：防止线框 bounds 过大导致隔空触发")]
    public float maxTouchExtent = 0.3f;

    [Tooltip("除了碰撞体，再用左右手/手柄节点做距离判定（手柄没有碰撞体时也能选）")]
    public bool useProximityTouch = true;

    public bool IsHolding => _holdTimer > 0f;
    public bool HasFired => _fired;

    float _holdTimer;
    bool _fired;
    bool _isTouching = false;
    float _rootRetryTime;
    Material _frameMat;
    int _progressID;

    Transform[] _playerPoints;
    float _pointsRefreshAt;

    void Awake()
    {
        _progressID = Shader.PropertyToID("_Progress");

        if (frameRenderer == null)
        {
            Transform square = transform.Find("SquareFrame");
            if (square != null) frameRenderer = square.GetComponent<Renderer>();
        }

        if (frameRenderer != null)
        {
            _frameMat = frameRenderer.material;
            _frameMat.SetFloat(_progressID, 0f);
        }
        else
        {
            Debug.LogError($"🚨 {gameObject.name} 没有找到 frameRenderer！");
        }

        SetupPhysics();
        ResolvePlayerRoot();
    }

    void SetupPhysics()
    {
        // 只检查并补充刚体，绝不乱加 Collider
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            Debug.LogWarning($"⚠️ {gameObject.name} 缺少刚体，代码已自动为你补上。");
        }
    }

    void ResolvePlayerRoot()
    {
        if (playerRoot != null) return;
        if (Time.time < _rootRetryTime) return;

        _rootRetryTime = Time.time + 1f;

        XROrigin origin = FindObjectOfType<XROrigin>();
        if (origin != null) playerRoot = origin.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null || other.transform.IsChildOf(transform)) return;

        ResolvePlayerRoot();
        if (playerRoot != null && !other.transform.IsChildOf(playerRoot))
        {
            Debug.Log($"❌ 碰到了，但不是玩家: {other.name}");
            return;
        }

        Debug.Log($"✅ 玩家触碰进入: {other.name}");
        _isTouching = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other == null || other.transform.IsChildOf(transform)) return;

        ResolvePlayerRoot();
        if (playerRoot != null && !other.transform.IsChildOf(playerRoot)) return;

        _isTouching = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other == null || other.transform.IsChildOf(transform)) return;

        ResolvePlayerRoot();
        if (playerRoot != null && !other.transform.IsChildOf(playerRoot)) return;

        Debug.Log($"🚪 玩家离开: {other.name}");
        _isTouching = false;

        // 已选中：线框保持填满，手离开不清零
        if (_fired) return;

        ResetHold();
    }

    void OnEnable()
    {
        if (ResumePickManager.Instance != null) ResumePickManager.Instance.Register(this);
    }

    void OnDisable()
    {
        if (ResumePickManager.Instance != null) ResumePickManager.Instance.Unregister(this);

        // 已选中：保留填充状态，避免禁用/启用后丢失
        if (_fired) return;

        ResetHold();
    }

    public void ResetHold()
    {
        _holdTimer = 0f;
        if (_frameMat != null) _frameMat.SetFloat(_progressID, 0f);
    }

    /// <summary>手/手柄节点离线框够近就算触碰（手柄没有碰撞体时也能选）。</summary>
    bool IsNearPlayer()
    {
        if (!useProximityTouch) return false;

        if (playerRoot == null)
        {
            ResolvePlayerRoot();
            if (playerRoot == null) return false;
        }

        RefreshPlayerPoints();
        if (_playerPoints == null) return false;

        float r2 = touchRadius * touchRadius;

        Bounds raw = frameRenderer != null ? frameRenderer.bounds : new Bounds(transform.position, Vector3.one * 0.1f);
        Vector3 extents = raw.extents;
        float cap = Mathf.Max(0.02f, maxTouchExtent);
        extents.x = Mathf.Min(extents.x, cap);
        extents.y = Mathf.Min(extents.y, cap);
        extents.z = Mathf.Min(extents.z, cap);
        Bounds bounds = new Bounds(raw.center, extents * 2f);

        for (int i = 0; i < _playerPoints.Length; i++)
        {
            Transform t = _playerPoints[i];
            if (t == null) continue;
            if (bounds.SqrDistance(t.position) <= r2) return true;
        }

        return false;
    }

    void RefreshPlayerPoints()
    {
        if (_playerPoints != null && Time.time < _pointsRefreshAt) return;
        _pointsRefreshAt = Time.time + 2f;

        List<Transform> list = new List<Transform>();
        HashSet<Transform> seen = new HashSet<Transform>();
        Transform[] all = playerRoot.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null) continue;

            string n = t.name.ToLowerInvariant();
            bool isHandRoot = (n.Contains("hand") || n.Contains("controller") ||
                               n.Contains("finger") || n.Contains("palm") || n.Contains("wrist"))
                              && !n.Contains("origin") && !n.Contains("rig") && !n.Contains("camera");
            if (!isHandRoot) continue;

            // 手/手柄根节点下的所有骨骼都算“玩家”，这样手掌、手背、手指都能触发
            Transform[] children = t.GetComponentsInChildren<Transform>(true);
            for (int c = 0; c < children.Length; c++)
            {
                Transform child = children[c];
                if (child == null) continue;
                if (child.GetComponent<Camera>() != null) continue;
                if (seen.Add(child)) list.Add(child);
            }
        }

        _playerPoints = list.ToArray();
    }

    public void Tick(bool interactive)
    {
        // 拦截诊断日志
        if (_fired || !interactive || _frameMat == null)
        {
            if (Time.frameCount % 60 == 0) // 每60帧打印一次，防止刷屏
                Debug.Log($"[Tick被拦截] _fired:{_fired}, interactive:{interactive}, _frameMat不为空:{_frameMat != null}");
            return;
        }

        if (!_isTouching && !IsNearPlayer())
        {
            ResetHold();
            return;
        }

        _holdTimer += Time.deltaTime;

        _frameMat.SetFloat(_progressID, Mathf.Clamp01(_holdTimer / Mathf.Max(holdSeconds, 0.01f)));

        if (_holdTimer < holdSeconds) return;

        _fired = true;
        _frameMat.SetFloat(_progressID, 1f);

        Debug.Log(isCorrect ? "✅ 选中了正确简历" : "❌ 选中了错误简历");

        if (ResumePickManager.Instance != null) ResumePickManager.Instance.OnPick(this);
    }
}
