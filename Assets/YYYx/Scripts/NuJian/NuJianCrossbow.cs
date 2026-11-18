using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[DisallowMultipleComponent]
public class NuJianCrossbow : MonoBehaviour
{
    [Header("场景限制")]
    public bool onlyInRequiredScene = true;
    public string requiredScene = "WallArchery";

    [Tooltip("进入目标场景时要隐藏）")]
    public GameObject[] hideInRequiredScene;

    [Tooltip("弩的模型根节点")]
    public GameObject bowVisualRoot;

    [Header("弩管朝向")]
    public Vector3 barrelAxisLocal = Vector3.forward;

    [Tooltip("射向参考节点")]
    public Transform aimReference;

    [Header("发射点")]
    public Transform firePoint;

    [Header("箭预制件")]
    public Arrow arrowPrefab;

    [Header("弹道：初速（米/秒）")]
    public float speed = 60f;

    [Header("重力加速度（米/秒²）")]
    public float gravity = 9.8f;

    [Header("地面高度（Y）")]
    public float groundY = -5f;

    [Header("弹道采样点数）")]
    public int resolution = 24;

    [Header("飞行播放加速倍率")]
    public float flightSpeedup = 1f;

    [Header("朝向平滑）")]
    public float directionSmoothing = 12f;

    [Header("全自动发射")]
    public bool autoFire = true;

    [Header("射速（秒/发）")]
    public float fireInterval = 0.08f;

    [Header("箭与箭之间的最小间距（米）")]
    public float spawnSpacing = 3f;

    [Header("自动保证间距 ≥ 箭长 + 这个余量（米）")]
    public float autoSpacingMargin = 0.4f;

    [Header("出膛闸门最长等待（秒）：防止近距离命中卡死射速")]
    public float clearanceTimeout = 0.5f;

    [Header("同时在飞的最大箭数（0 = 不限）")]
    public int maxArrowsInFlight = 0;

    [Header("发射音效")]
    public AudioClip fireSound;

    [Header("瞄准线")]
    public bool showLine = true;
    public Color lineColor = new Color(1f, 0.85f, 0.4f, 1f);
    public Color onTargetLineColor = new Color(0.25f, 1f, 0.35f, 1f);
    [Range(0f, 1f)]
    public float lineAlpha = 0.4f;
    public float lineWidth = 0.035f;

    [Header("命中截断")]
    public bool clampToHit = true;
    public LayerMask hitLayers = ~0;

    [Header("落点圆环")]
    public bool showRing = true;
    public float ringRadius = 0.6f;
    public float ringWidth = 0.03f;
    public int ringSegments = 40;
    public float ringPulse = 0.05f;
    public Color ringColor = new Color(1f, 0.15f, 0.1f, 1f);
    [Range(0f, 1f)]
    public float ringAlpha = 1f;

    [Header("屏幕调试")]
    public bool showDebug = true;

    [Header("命中过滤）")]
    public Transform ignoreRoot;

    [Header("目标场景里隐藏握弩那只手的手柄模型（只留弩），其它场景自动恢复")]
    public bool hideControllerModelInScene = false;

    [Tooltip("XRI 运行时生成的手柄模型节点名")]
    public string controllerModelName = "Model";

    [Header("拦截敌人扔来的石头：箭命中后石头闪红消失")]
    public bool interceptRocks = false;

    [Header("闪红时长（秒）")]
    public float rockRedFlashTime = 0.12f;

    [Header("闪红的颜色")]
    public Color rockFlashColor = new Color(1f, 0.08f, 0.05f, 1f);

    [Header("发射音效音量")]
    [Range(0f, 1f)]
    public float fireVolume = 0.6f;

    /// <summary>当前场景是否允许出现这把弩</summary>
    private bool sceneAllowed = true;

    // 弹道
    private readonly List<Vector3> points = new List<Vector3>(64);
    private readonly List<Vector3> drawPoints = new List<Vector3>(64);
    private float totalTime = 1f;
    private float usedTime = 1f;
    private Vector3 aimDir = Vector3.forward;

    // 命中
    private bool hasHit = false;
    private Vector3 hitPoint = Vector3.zero;
    private Vector3 hitNormal = Vector3.up;
    private bool hitIsLitTarget = false;
    private RaycastHit hitInfo;

    // 发射
    private readonly List<Arrow> pool = new List<Arrow>();
    private Arrow lastArrow = null;
    private int arrowsInFlight = 0;
    private float fireTimer = 0f;
    private float lastFireTime = -10f;
    private float arrowLength = 1f;

    // 视觉
    private LineRenderer aimLine;
    private LineRenderer ring;
    private Material lineMaterial;
    private Material ringMaterial;
    private AudioSource audioSource;
    private bool audioDiagLogged = false;
    private Renderer[] ownRenderers;

    // 手柄模型守护
    private Transform controllerModel;
    private float controllerModelScanTimer = 0f;

    // 石头拦截
    private readonly List<Arrow> flyingArrows = new List<Arrow>(16);
    private readonly List<Vector3> flyingPos = new List<Vector3>(16);
    private readonly HashSet<GameObject> flashingRocks = new HashSet<GameObject>();

    private void Awake()
    {
        audioSource = CreateFireSource();
        ownRenderers = GetComponentsInChildren<Renderer>(true);

        if (showLine) CreateLine();
        if (showRing) CreateRing();

        arrowLength = MeasureArrowLength();

        if (barrelAxisLocal.sqrMagnitude < 0.0001f) barrelAxisLocal = Vector3.forward;
        aimDir = AimRef.TransformDirection(barrelAxisLocal.normalized);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneGate();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (lineMaterial != null) Destroy(lineMaterial);
        if (ringMaterial != null) Destroy(ringMaterial);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneGate();
    }

    /// <summary>
    /// 场景门控：不在目标场景就不显示弩、不发射、不画线，并把隐藏的手柄模型还回来
    /// </summary>
    private void ApplySceneGate()
    {
        // 本项目 GameScene 是叠加加载的，GetActiveScene() 仍然是 Laucher，
        // 所以这里改成“目标场景是否已加载”（单场景模式同样适用）。
        bool allowed = !onlyInRequiredScene;
        if (!allowed)
        {
            UnityEngine.SceneManagement.Scene target = SceneManager.GetSceneByName(requiredScene);
            allowed = target.IsValid() && target.isLoaded;
        }

        sceneAllowed = allowed;

        // 弩的模型
        if (bowVisualRoot != null && bowVisualRoot != gameObject)
        {
            bowVisualRoot.SetActive(allowed);
        }
        else if (ownRenderers != null)
        {
            for (int i = 0; i < ownRenderers.Length; i++)
            {
                if (ownRenderers[i] != null) ownRenderers[i].enabled = allowed;
            }
        }

        // 手柄模型：目标场景里藏起来，别的场景还回去
        if (hideInRequiredScene != null)
        {
            for (int i = 0; i < hideInRequiredScene.Length; i++)
            {
                if (hideInRequiredScene[i] != null) hideInRequiredScene[i].SetActive(!allowed);
            }
        }

        if (aimLine != null) aimLine.enabled = allowed && showLine;
        if (ring != null) ring.enabled = allowed && showRing;

        // 离开目标场景：把手柄模型还回去
        if (!allowed && hideControllerModelInScene && controllerModel != null)
        {
            if (!controllerModel.gameObject.activeSelf) controllerModel.gameObject.SetActive(true);
        }

        if (!allowed) ClearHit();
    }

    private void Update()
    {
        if (!sceneAllowed) return;

        UpdateControllerModelVisibility();
        UpdateAimDirection();
        ComputePath();
        UpdateLine();
        UpdateRing();
        if (autoFire) UpdateFire();
        UpdateRockIntercept();
    }

    /// <summary>
    /// 目标场景里隐藏手柄模型：XRI 的 XRController 在 tracking 变化时会自己把 Model 重新打开，
    /// 所以这里每帧守护一次；离开目标场景时在 ApplySceneGate 里主动恢复。
    /// </summary>
    private void UpdateControllerModelVisibility()
    {
        if (!hideControllerModelInScene) return;

        if (controllerModel == null)
        {
            controllerModelScanTimer -= Time.deltaTime;
            if (controllerModelScanTimer > 0f) return;
            controllerModelScanTimer = 0.5f;
            controllerModel = FindControllerModel();
            if (controllerModel == null) return;
        }

        if (controllerModel.gameObject.activeSelf) controllerModel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 从弩往上最多找 4 层，定位名字里带 Controller 的节点，再取它下面名为 Model 的子物体
    /// </summary>
    private Transform FindControllerModel()
    {
        Transform t = transform;
        for (int i = 0; i < 4 && t != null; i++)
        {
            if (t.name.IndexOf("Controller", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (int c = 0; c < t.childCount; c++)
                {
                    Transform ch = t.GetChild(c);
                    if (ch.name == controllerModelName) return ch;
                }
            }
            t = t.parent;
        }
        return null;
    }

    /// <summary>射向与出膛位置的参考节点</summary>
    private Transform AimRef
    {
        get { return aimReference != null ? aimReference : transform; }
    }

    private Vector3 MuzzlePosition
    {
        get { return firePoint != null ? firePoint.position : AimRef.position; }
    }

    /// <summary>
    /// 射向 = 参考节点自身的朝向（可加低通平滑压手柄抖动）
    /// </summary>
    private void UpdateAimDirection()
    {
        Vector3 dir = AimRef.TransformDirection(barrelAxisLocal.normalized);
        if (dir.sqrMagnitude < 0.0001f) dir = AimRef.forward;
        dir.Normalize();

        if (directionSmoothing > 0f)
        {
            float k = 1f - Mathf.Exp(-directionSmoothing * Time.deltaTime);
            dir = Vector3.Lerp(aimDir, dir, k).normalized;
        }

        aimDir = dir;
    }

    /// <summary>
    /// 算真实抛物线，并沿轨迹找到第一个命中的表面
    /// </summary>
    private void ComputePath()
    {
        Vector3 origin = MuzzlePosition;

        float v = Mathf.Max(1f, speed);
        float g = Mathf.Max(0.01f, gravity);
        Vector3 vel = aimDir * v;

        // 落地时间： y(t) = origin.y + vel.y*t - 0.5*g*t² = groundY
        float h = origin.y - groundY;
        float disc = vel.y * vel.y + 2f * g * h;

        totalTime = disc > 0f ? (vel.y + Mathf.Sqrt(disc)) / g : 1f;
        if (totalTime <= 0.02f) totalTime = 1f;

        int count = Mathf.Max(2, resolution);
        points.Clear();
        for (int i = 0; i < count; i++)
        {
            float ti = totalTime * i / (float)(count - 1);
            Vector3 p = origin + vel * ti;
            p.y -= 0.5f * g * ti * ti;
            points.Add(p);
        }

        // 终点贴到地面，避免浮点误差
        Vector3 last = points[points.Count - 1];
        last.y = groundY;
        points[points.Count - 1] = last;

        usedTime = totalTime;
        ClearHit();

        if (!clampToHit)
        {
            drawPoints.Clear();
            drawPoints.AddRange(points);
            return;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

            if (!Physics.Linecast(a, b, out hitInfo, hitLayers, QueryTriggerInteraction.Collide)) continue;
            if (!IsValidHit(hitInfo)) continue;

            hasHit = true;
            hitPoint = hitInfo.point;
            hitNormal = hitInfo.normal.sqrMagnitude > 0.0001f ? hitInfo.normal.normalized : Vector3.up;

            ScoreTarget target = hitInfo.collider.GetComponentInParent<ScoreTarget>();
            hitIsLitTarget = target != null && target.isActiveTarget;

            // 命中点在采样段内的比例，用来算这支箭要飞多久
            float segLen = (b - a).magnitude;
            float segFrac = segLen > 0.0001f ? Mathf.Clamp01((hitPoint - a).magnitude / segLen) : 0f;
            usedTime = totalTime * (i + segFrac) / (count - 1);
            usedTime = Mathf.Max(0.03f, usedTime);

            // 截断：到命中段为止，再补上精确接触点
            drawPoints.Clear();
            for (int j = 0; j <= i; j++) drawPoints.Add(points[j]);
            drawPoints.Add(hitPoint);
            return;
        }

        drawPoints.Clear();
        drawPoints.AddRange(points);
    }

    /// <summary>
    /// 这个命中算不算“真打到东西”
    /// </summary>
    private bool IsValidHit(RaycastHit h)
    {
        if (h.collider == null) return false;
        if (h.collider.transform == transform) return false;
        if (h.collider.transform.IsChildOf(transform)) return false;
        if (ignoreRoot != null && h.collider.transform.IsChildOf(ignoreRoot)) return false;

        // 靶子即使是 trigger 也算命中
        ScoreTarget target = h.collider.GetComponentInParent<ScoreTarget>();
        if (target != null) return true;

        // 飞行中的箭不算障碍
        if (h.collider.GetComponentInParent<Arrow>() != null) return false;

        if (h.collider.isTrigger) return false;
        return true;
    }

    private void ClearHit()
    {
        hasHit = false;
        hitIsLitTarget = false;
        hitNormal = Vector3.up;
    }

    #region 瞄准线与落点圆环

    private Material CreateTransparentMaterial(string matName, Color c)
    {
        Shader s = Shader.Find("Sprites/Default");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Unlit/Transparent");
        if (s == null) return null;

        Material m = new Material(s);
        m.name = matName;

        Color rgb = new Color(c.r, c.g, c.b, 1f);
        if (m.HasProperty("_Color")) m.SetColor("_Color", rgb);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", rgb);
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
        if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
        if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);

        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        return m;
    }

    private void CreateLine()
    {
        GameObject go = new GameObject("NuJianAimLine");
        go.transform.SetParent(transform, false);

        aimLine = go.AddComponent<LineRenderer>();
        aimLine.useWorldSpace = true;
        aimLine.loop = false;
        aimLine.positionCount = 0;
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.numCapVertices = 2;
        aimLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        aimLine.receiveShadows = false;
        aimLine.alignment = LineAlignment.View;

        lineMaterial = CreateTransparentMaterial("NuJianAimLineMat", lineColor);
        if (lineMaterial != null) aimLine.sharedMaterial = lineMaterial;
    }

    private void CreateRing()
    {
        GameObject go = new GameObject("NuJianLandingRing");
        go.transform.SetParent(transform, false);

        ring = go.AddComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = Mathf.Max(8, ringSegments);
        ring.startWidth = ringWidth;
        ring.endWidth = ringWidth;
        ring.numCapVertices = 2;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        ring.alignment = LineAlignment.View;
        ring.enabled = false;

        ringMaterial = CreateTransparentMaterial("NuJianRingMat", ringColor);
        if (ringMaterial != null) ring.sharedMaterial = ringMaterial;
    }

    private void UpdateLine()
    {
        if (aimLine == null || !showLine) return;

        int count = drawPoints.Count;
        aimLine.positionCount = count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = drawPoints[i];
            // 线头退 1cm，避免扎进表面里闪烁
            if (hasHit && i == count - 1) p -= hitNormal * 0.01f;
            aimLine.SetPosition(i, p);
        }

        Color c = hitIsLitTarget ? onTargetLineColor : lineColor;
        c.a = Mathf.Clamp01(lineAlpha);
        aimLine.startColor = c;
        aimLine.endColor = c;
    }

    private void UpdateRing()
    {
        if (ring == null || !showRing)
        {
            if (ring != null) ring.enabled = false;
            return;
        }

        Vector3 p;
        Vector3 n;

        if (hasHit)
        {
            p = hitPoint;
            n = hitNormal;
        }
        else if (points.Count > 0)
        {
            p = points[points.Count - 1];
            n = Vector3.up;
        }
        else
        {
            ring.enabled = false;
            return;
        }

        if (n.sqrMagnitude < 0.0001f) n = Vector3.up;
        n.Normalize();

        Vector3 t1 = Vector3.Cross(n, Mathf.Abs(n.y) > 0.9f ? Vector3.right : Vector3.up);
        if (t1.sqrMagnitude < 0.0001f) t1 = Vector3.right;
        t1.Normalize();
        Vector3 t2 = Vector3.Cross(n, t1).normalized;

        float r = Mathf.Max(0.02f, ringRadius + Mathf.Sin(Time.time * 6f) * ringPulse);
        Vector3 center = p + n * 0.012f;

        int segments = Mathf.Max(8, ringSegments);
        if (ring.positionCount != segments) ring.positionCount = segments;
        ring.startWidth = ringWidth;
        ring.endWidth = ringWidth;

        for (int i = 0; i < segments; i++)
        {
            float ang = (i / (float)segments) * Mathf.PI * 2f;
            ring.SetPosition(i, center + (t1 * Mathf.Cos(ang) + t2 * Mathf.Sin(ang)) * r);
        }

        Color c = ringColor;
        c.a = Mathf.Clamp01(ringAlpha);
        ring.startColor = c;
        ring.endColor = c;

        ring.enabled = true;
    }

    #endregion

    #region 发射

    private void UpdateFire()
    {
        if (arrowPrefab == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer < fireInterval) return;

        // 闸门一：上一支箭还没飞出足够间距，不出下一支
        if (lastArrow != null && lastArrow.isActiveAndEnabled)
        {
            if (Time.time - lastFireTime < clearanceTimeout)
            {
                Vector3 muzzle = MuzzlePosition;
                float spacing = GetSpawnSpacing();
                if ((lastArrow.transform.position - muzzle).sqrMagnitude < spacing * spacing) return;
            }
        }

        // 闸门二：在飞箭数上限
        if (maxArrowsInFlight > 0 && arrowsInFlight >= maxArrowsInFlight) return;

        fireTimer = 0f;
        Fire();
    }

    /// <summary>
    /// 发一支箭。箭沿截断后的轨迹飞，命中点就是这条线的终点。
    /// </summary>
    private void Fire()
    {
        if (arrowPrefab == null) return;
        if (drawPoints.Count < 2) return;

        Arrow arrow = GetArrowFromPool();
        if (arrow == null) return;

        arrow.transform.SetParent(null);

        float moveTime = Mathf.Max(0.03f, usedTime / Mathf.Max(0.1f, flightSpeedup));
        arrow.Launch(drawPoints, moveTime, OnArrowFinished);

        lastArrow = arrow;
        arrowsInFlight++;
        lastFireTime = Time.time;

        flyingArrows.Add(arrow);
        flyingPos.Add(arrow.transform.position);

        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound, Mathf.Clamp01(fireVolume));
        }

        if (!audioDiagLogged)
        {
            audioDiagLogged = true;
            AudioListener[] listeners = FindObjectsOfType<AudioListener>();
            Debug.Log("[NuJian] 音源=" + audioSource.gameObject.name +
                      " enabled=" + audioSource.enabled +
                      " volume=" + audioSource.volume +
                      " mute=" + audioSource.mute +
                      " | clip=" + (fireSound == null ? "空" : fireSound.name + " " + fireSound.length.ToString("F2") + "s") +
                      " | Listener=" + listeners.Length +
                      " | AudioSettings=" + AudioSettings.outputSampleRate);
        }
    }

    /// <summary>
    /// 专用于发射音的播放源：新建独立子节点，不复用场景里任何已有 AudioSource，
    /// 免得被别人的 mute / volume 0 / 已经占用的 loop 影响。
    /// </summary>
    private AudioSource CreateFireSource()
    {
        GameObject go = new GameObject("FireAudio");
        go.transform.SetParent(transform, false);

        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.volume = 1f;
        src.mute = false;
        src.spatialBlend = 0f;
        src.bypassEffects = true;
        src.bypassListenerEffects = true;
        src.bypassReverbZones = true;
        return src;
    }

    private Arrow GetArrowFromPool()
    {
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (pool[i] == null) { pool.RemoveAt(i); continue; }
            Arrow a = pool[i];
            pool.RemoveAt(i);
            a.gameObject.SetActive(true);
            return a;
        }

        Arrow created = Instantiate(arrowPrefab);
        created.gameObject.SetActive(true);
        return created;
    }

    private void OnArrowFinished(Arrow arrow)
    {
        if (arrow == null) return;

        arrowsInFlight = Mathf.Max(0, arrowsInFlight - 1);
        if (lastArrow == arrow) lastArrow = null;

        int idx = flyingArrows.IndexOf(arrow);
        if (idx >= 0)
        {
            flyingArrows.RemoveAt(idx);
            flyingPos.RemoveAt(idx);
        }

        arrow.transform.SetParent(null);
        arrow.gameObject.SetActive(false);
        pool.Add(arrow);
    }

    #region 石头拦截

    /// <summary>
    /// 箭的飞行速度高，一帧能走一米多，所以要按“上一帧位置 → 这一帧位置”这条线段去扫，
    
    /// </summary>
    private void UpdateRockIntercept()
    {
        if (!interceptRocks) return;

        for (int i = flyingArrows.Count - 1; i >= 0; i--)
        {
            Arrow a = flyingArrows[i];
            if (a == null || !a.gameObject.activeSelf)
            {
                flyingArrows.RemoveAt(i);
                flyingPos.RemoveAt(i);
                continue;
            }

            Vector3 prev = flyingPos[i];
            Vector3 cur = a.transform.position;
            flyingPos[i] = cur;

            Vector3 dir = cur - prev;
            float len = dir.magnitude;
            if (len < 0.0001f) continue;

            // 箭的碰撞体是往前 2 米多的长条，箭尖才是真正接触点，
           
            Vector3 dirN = dir / len;
            Vector3 prevTip = prev + dirN * arrowLength;
            Vector3 curTip = cur + dirN * arrowLength;

            RaycastHit hit;
            if (!Physics.Linecast(prevTip, curTip, out hit, hitLayers, QueryTriggerInteraction.Ignore)) continue;

            Rock rock = hit.collider != null ? hit.collider.GetComponentInParent<Rock>() : null;
            if (rock == null) continue;

            
            flyingArrows.RemoveAt(i);
            flyingPos.RemoveAt(i);
            a.gameObject.SetActive(false);

            HitRock(rock);
        }
    }

    private void HitRock(Rock rock)
    {
        GameObject go = rock.gameObject;
        if (go == null) return;
        if (flashingRocks.Contains(go)) return;

        flashingRocks.Add(go);

        
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        for (int i = 0; i < rs.Length; i++)
        {
            if (rs[i] == null) continue;

            mpb.Clear();
            Material mat = rs[i].sharedMaterial;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor")) mpb.SetColor("_BaseColor", rockFlashColor);
                if (mat.HasProperty("_Color")) mpb.SetColor("_Color", rockFlashColor);
                if (mat.HasProperty("_EmissionColor")) mpb.SetColor("_EmissionColor", rockFlashColor * 1.5f);
            }
            rs[i].SetPropertyBlock(mpb);
        }

        // onRecycle 是 Rock 上公开的池回调，走它才能回到 SiegeEnemy 的池里
        System.Action<GameObject> recycle = rock.onRecycle;

        StartCoroutine(FlashThenRecycle(go, rs, recycle));
    }

    private System.Collections.IEnumerator FlashThenRecycle(GameObject go, Renderer[] rs, System.Action<GameObject> recycle)
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, rockRedFlashTime));

        MaterialPropertyBlock empty = new MaterialPropertyBlock();
        for (int i = 0; i < rs.Length; i++)
        {
            if (rs[i] != null) rs[i].SetPropertyBlock(empty);
        }

        flashingRocks.Remove(go);

        if (go == null) yield break;

        if (recycle != null) recycle(go);
        else go.SetActive(false);
    }

    #endregion

    private float GetSpawnSpacing()
    {
        float auto = arrowLength + Mathf.Max(0f, autoSpacingMargin);
        return Mathf.Max(0.1f, Mathf.Max(spawnSpacing, auto));
    }

    private float MeasureArrowLength()
    {
        if (arrowPrefab == null) return 1f;

        Renderer[] rs = arrowPrefab.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return 1f;

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
        {
            if (rs[i] != null) b.Encapsulate(rs[i].bounds);
        }

        return Mathf.Max(0.2f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
    }

    #endregion

    #region 调试

    private void OnGUI()
    {
        if (!showDebug) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = sceneAllowed ? Color.yellow : Color.gray;

        string msg = "手持弩\n" +
                     "场景: " + SceneManager.GetActiveScene().name +
                     (sceneAllowed ? "" : "（不在 " + requiredScene + "，已停用）") + "\n" +
                     "初速: " + speed.ToString("F1") + " m/s  弹道时间: " + totalTime.ToString("F2") + " s\n" +
                     "命中: " + (hasHit ? hitPoint.ToString("F2") : "无") +
                     (hitIsLitTarget ? "（可打靶）" : "") + "\n" +
                     "在飞箭数: " + arrowsInFlight +
                     "  箭长: " + arrowLength.ToString("F2") + " m" +
                     "  间距: " + GetSpawnSpacing().ToString("F2") + " m\n" +
                     "音源: " + (audioSource == null ? "无" : (audioSource.enabled ? "有" : "已禁用")) +
                     "  音效: " + (fireSound == null ? "未拖" : fireSound.name) +
                     "  音量: " + fireVolume.ToString("F2") +
                     "  听者: " + (FindObjectOfType<AudioListener>() == null ? "场景无 AudioListener" : "有");

        GUI.Label(new Rect(20, 20, 900, 200), msg, style);
    }

    #endregion
}
