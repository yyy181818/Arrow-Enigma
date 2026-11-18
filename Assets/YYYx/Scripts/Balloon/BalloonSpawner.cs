using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 气球生成器：在指定矩形区域内源源不断地生成五颜六色的气球（统一叫 Balloon），气球自行上升。
///
/// 线索规则：
///   · 每个气球生成时按 <see cref="clueChance"/> 的概率带一条线索；
///   · 带线索时优先挑“还没拿到过”的线索（<see cref="preferMissingChance"/>），
///     但也会随机到重复线索，保证会重复；
///   · 打爆带线索的气球才给线索，普通气球只负责热闹。
/// </summary>
public class BalloonSpawner : MonoBehaviour
{
    [Header("生成区域（以本物体为原点，XZ 平面随机）")]
    public Vector3 areaSize = new Vector3(8f, 0f, 8f);

    [Tooltip("气球升到多高算飞走（相对本物体的世界 Y 高度）")]
    public float flightHeight = 4f;

    [Header("生成节奏")]
    [Tooltip("两次生成之间的间隔范围（秒）")]
    public Vector2 intervalRange = new Vector2(0.35f, 0.8f);

    [Tooltip("同时存在的最大气球数")]
    public int maxAlive = 18;

    [Tooltip("开始时立刻生成几个")]
    public int initialBurst = 8;

    [Header("上升速度（米/秒）")]
    public Vector2 riseSpeedRange = new Vector2(0.25f, 0.55f);

    [Header("线索分布")]
    [Tooltip("每个气球带线索的概率（只在“随机模式”下生效）")]
    [Range(0f, 1f)] public float clueChance = 0.25f;

    [Tooltip("带线索时，优先选“还没拿到过”的线索的概率")]
    [Range(0f, 1f)] public float preferMissingChance = 0.45f;

    [Header("线索气球节奏（推荐）")]
    [Tooltip("开启后线索气球按冷却时间出现，而不是每个气球随机判定——直接控制一局时长")]
    public bool useClueCooldown = true;

    [Tooltip("两个线索气球之间的间隔（秒）。6 条线索大概要打 15 个线索气球，按这个间隔就能算出最短局时")]
    public Vector2 clueCooldownRange = new Vector2(10f, 16f);

    [Tooltip("同屏最多几个线索气球")]
    public int maxClueAlive = 1;

    [Tooltip("线索气球比普通气球快多少倍（越高越难打）")]
    public float clueRiseSpeedMultiplier = 1.6f;

    [Tooltip("普通气球也显示假标记的概率（干扰用，0 = 关闭）")]
    [Range(0f, 1f)] public float fakeMarkerChance = 0f;

    [Tooltip("线索管理器，留空自动找")]
    public ClueManager clueManager;

    [Header("线索标记")]
    [Tooltip("带线索的气球是否在头顶显示一个小圆点")]
    public bool showClueMarker = true;

    [Tooltip("线索标记的颜色")]
    public Color clueMarkerColor = new Color(1f, 0.92f, 0.35f);

    [Header("外观")]
    [Tooltip("气球预制体；留空则使用程序化占位气球")]
    public GameObject balloonPrefab;

    [Tooltip("程序化气球的颜色池（每次随机抽一个）")]
    public Color[] palette =
    {
        new Color(0.95f, 0.30f, 0.30f),
        new Color(0.98f, 0.78f, 0.20f),
        new Color(0.30f, 0.60f, 0.95f),
        new Color(0.35f, 0.80f, 0.45f),
        new Color(0.70f, 0.45f, 0.95f),
        new Color(0.95f, 0.55f, 0.20f),
    };

    [Tooltip("用气球预制体时，是否也用颜色池随机染色（需要材质支持 _BaseColor）")]
    public bool tintPrefabWithPalette = false;

    [Tooltip("程序化气球的整体缩放")]
    public float balloonScale = 1.7f;

    [Header("爆裂音效")]
    [Tooltip("留空则自动用 Resources/Sound/BalloonPop")]
    public AudioClip popSound;
    [Range(0f, 1f)] public float popVolume = 0.6f;

    [Header("开关")]
    public bool autoStart = true;

    /// <summary>有气球被打爆。</summary>
    public event Action<Balloon> BalloonPopped;

    /// <summary>有气球飞走（没打中）。</summary>
    public event Action<Balloon> BalloonMissed;

    /// <summary>累计打爆数量。</summary>
    public int PoppedCount { get; private set; }

    /// <summary>当前存活数量。</summary>
    public int AliveCount => _alive.Count;

    readonly List<Balloon> _alive = new List<Balloon>();
    readonly List<int> _missing = new List<int>();
    readonly Dictionary<Color, Material> _materialCache = new Dictionary<Color, Material>();

    float _timer;
    bool _running;
    float _nextClueTime;

    void Start()
    {
        if (clueManager == null) clueManager = ClueManager.Instance;
        if (autoStart) StartSpawning();
    }

    /// <summary>开始持续生成（初始爆发也在此时发生）。</summary>
    public void StartSpawning()
    {
        if (_running) return;
        _running = true;
        _timer = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
        _nextClueTime = Time.time + UnityEngine.Random.Range(1.5f, 3.5f);

        for (int i = 0; i < initialBurst; i++)
        {
            SpawnOne();
        }
    }

    /// <summary>停止生成（已经在飞的气球不受影响）。</summary>
    public void StopSpawning()
    {
        _running = false;
    }

    /// <summary>线索集齐后：停止生成并清掉场上所有气球。</summary>
    public void StopAndClear()
    {
        _running = false;

        for (int i = _alive.Count - 1; i >= 0; i--)
        {
            if (_alive[i] != null) Destroy(_alive[i].gameObject);
        }

        _alive.Clear();
    }

    void Update()
    {
        if (!_running) return;

        _alive.RemoveAll(b => b == null);

        if (_alive.Count >= maxAlive)
        {
            _timer = 0.25f;
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        SpawnOne();
        _timer = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
    }

    void SpawnOne()
    {
        Vector3 position = transform.position + new Vector3(
            UnityEngine.Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            0f,
            UnityEngine.Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f));

        GameObject go = balloonPrefab != null
            ? Instantiate(balloonPrefab, position, Quaternion.identity)
            : BuildPlaceholder(position);

        go.name = "Balloon";
        go.transform.SetParent(transform, true);

        if (balloonPrefab != null && tintPrefabWithPalette)
        {
            TintRenderers(go, PickColor());
        }

        Balloon balloon = go.GetComponent<Balloon>();
        if (balloon == null) balloon = go.AddComponent<Balloon>();

        balloon.popSound = popSound;     // 为空时 Balloon 会自动从 Resources 加载
        balloon.popVolume = popVolume;

        balloon.Configure(
            this,
            transform.position.y + flightHeight,
            UnityEngine.Random.Range(riseSpeedRange.x, riseSpeedRange.y),
            UnityEngine.Random.Range(0f, Mathf.PI * 2f));

        // 决定这个气球带不带线索
        int clue = -1;
        bool isClueBalloon = false;

        if (useClueCooldown)
        {
            bool slotFree = CountClueBalloons() < Mathf.Max(1, maxClueAlive);
            if (slotFree && Time.time >= _nextClueTime)
            {
                clue = PickClueIndex();
                isClueBalloon = clue >= 0;

                if (isClueBalloon)
                {
                    _nextClueTime = Time.time + UnityEngine.Random.Range(clueCooldownRange.x, clueCooldownRange.y);
                }
            }
        }
        else if (UnityEngine.Random.value < clueChance)
        {
            clue = PickClueIndex();
            isClueBalloon = clue >= 0;
        }

        // 线索气球飞得更快，更难打
        if (isClueBalloon)
        {
            balloon.riseSpeed *= Mathf.Max(1f, clueRiseSpeedMultiplier);
        }

        balloon.SetClue(clue);

        // 假标记干扰
        if (!isClueBalloon && fakeMarkerChance > 0f && UnityEngine.Random.value < fakeMarkerChance)
        {
            balloon.ShowMarker(true);
        }

        _alive.Add(balloon);
    }

    int CountClueBalloons()
    {
        int count = 0;
        for (int i = 0; i < _alive.Count; i++)
        {
            if (_alive[i] != null && _alive[i].HasClue) count++;
        }
        return count;
    }

    /// <summary>挑一条线索给新气球：优先没拿到过的，也可能重复。</summary>
    int PickClueIndex()
    {
        if (clueManager == null) clueManager = ClueManager.Instance;
        if (clueManager == null || clueManager.TotalCount == 0) return -1;

        if (UnityEngine.Random.value < preferMissingChance)
        {
            _missing.Clear();
            clueManager.FillUncollected(_missing);
            if (_missing.Count > 0)
            {
                return _missing[UnityEngine.Random.Range(0, _missing.Count)];
            }
        }

        return UnityEngine.Random.Range(0, clueManager.TotalCount);
    }

    /// <summary>调试用：随机打爆一个正在飞的气球。</summary>
    public bool PopRandomBalloon()
    {
        _alive.RemoveAll(b => b == null);
        if (_alive.Count == 0) return false;

        _alive[UnityEngine.Random.Range(0, _alive.Count)].Pop();
        return true;
    }

    /// <summary>程序化生成一个占位气球：球体 + 绳结 + 绳子（每次随机颜色）。</summary>
    GameObject BuildPlaceholder(Vector3 position)
    {
        GameObject root = new GameObject("Balloon");
        root.transform.position = position;
        root.transform.localScale = Vector3.one * balloonScale;

        Color color = PickColor();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = Vector3.one * 0.24f;
        DestroyImmediateSafe(body.GetComponent<Collider>());
        ApplyColor(body, color);

        GameObject knot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knot.name = "Knot";
        knot.transform.SetParent(root.transform, false);
        knot.transform.localPosition = new Vector3(0f, -0.125f, 0f);
        knot.transform.localScale = new Vector3(0.025f, 0.03f, 0.025f);
        DestroyImmediateSafe(knot.GetComponent<Collider>());
        ApplyColor(knot, color * 0.8f);

        var line = root.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(0f, -0.14f, 0f));
        line.SetPosition(1, new Vector3(0f, -0.55f, 0f));
        line.startWidth = 0.006f;
        line.endWidth = 0.004f;
        line.startColor = new Color(0.85f, 0.85f, 0.8f, 0.9f);
        line.endColor = new Color(0.85f, 0.85f, 0.8f, 0.2f);
        line.material = GetMaterial(new Color(0.9f, 0.9f, 0.85f));
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.radius = 0.14f;

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.None;

        if (showClueMarker)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "ClueMarker";
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            marker.transform.localScale = Vector3.one * 0.055f;
            DestroyImmediateSafe(marker.GetComponent<Collider>());
            ApplyColor(marker, clueMarkerColor);
            marker.SetActive(false);
        }

        return root;
    }

    Color PickColor()
    {
        if (palette == null || palette.Length == 0) return Color.red;
        return palette[UnityEngine.Random.Range(0, palette.Length)];
    }

    void ApplyColor(GameObject go, Color color)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        Material material = GetMaterial(color);
        renderer.material = material;
    }

    /// <summary>给预制体上的所有渲染器染色（不影响共享材质）。</summary>
    void TintRenderers(GameObject root, Color color)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            renderers[i].SetPropertyBlock(block);
        }
    }

    Material GetMaterial(Color color)
    {
        if (_materialCache.TryGetValue(color, out Material cached) && cached != null)
            return cached;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);

        _materialCache[color] = material;
        return material;
    }

    /// <summary>程序化生成时用；运行时 Destroy，编辑器里 DestroyImmediate。</summary>
    static void DestroyImmediateSafe(UnityEngine.Object obj)
    {
        if (obj == null) return;

        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    // ---------- 供 Balloon 回调 ----------

    internal void NotifyPopped(Balloon balloon)
    {
        PoppedCount++;
        _alive.Remove(balloon);
        BalloonPopped?.Invoke(balloon);
    }

    internal void NotifyMissed(Balloon balloon)
    {
        _alive.Remove(balloon);
        BalloonMissed?.Invoke(balloon);
    }
}
