using System;
using UnityEngine;

/// <summary>
/// 巨石：攻城靶点扔向城墙的石头。
///
/// ★ 石头是真实物理体：受重力下坠、撞到东西会弹开、落地会滚动。
///   不做「插在墙上不动」那种假效果——砸中墙的瞬间加进度，
///   然后石头继续弹开飞走，最后落到地上，滚一会儿，被回收。
///
/// ★ 判定方式：物理碰撞 + Tag。
///   撞到带指定 Tag（默认 Wall）的物体 → 攻城进度条增加。
///   撞到别的物体 → 就是普通碰撞，弹开，不加进度。
///   石头落在城墙的哪一段、偏多少，全由物理决定，不是写死的点。
///
/// ★ 石头预制体由你自己做（模型 + 材质），
///   把预制体拖到 SiegeEnemy 的 Rock Prefab 槽里就行，
///   这个脚本是运行时自动挂上去的，你不用手动加。
///   Collider / Rigidbody 带不带都行，脚本运行时会处理好。
/// </summary>
public class Rock : MonoBehaviour
{
    [Header("城墙 Tag。石头撞到这个 tag 的物体就算砸中城墙")]
    public string wallTag = "Wall";

    [Header("落地时的音效（可选）")]
    public AudioClip landSound;

    [Header("落地时的粒子特效（可选）")]
    public ParticleSystem landEffect;

    [Header("石头碰撞体半径（米），预制体没有 Collider 时自动加一个")]
    public float colliderRadius = 0.5f;

    [Header("总存活时间（秒），超时自动回收")]
    public float lifeTime = 10f;

    [Header("真正落地后多久回收（秒）。石头躺在地上滚一会儿就消失")]
    public float landRecycleDelay = 3f;

    [Header("回收回调（对象池用），可选")]
    public System.Action<GameObject> onRecycle;

    private Rigidbody body = null;

    private float lifeTimer = 0f;
    private float landTimer = -1f;

    /// <summary>是否已经撞到地面（真正落地）</summary>
    private bool hasLanded = false;

    /// <summary>是否已经砸中过城墙（防止一次投掷加多次进度）</summary>
    private bool hasHitWall = false;

    private Action<Vector3> onHitWall;

    private void Awake()
    {
        SetupPhysics();
    }

    /// <summary>
    /// 从对象池重新启用时：先保持「静止待发」状态，等 Launch 被调用后才开始飞。
    /// （否则复用时残留上一次的计时，会原地闪一下就消失）
    /// </summary>
    private void OnEnable()
    {
        lifeTimer = 0f;
        landTimer = -1f;
        hasLanded = false;
        hasHitWall = false;
        onHitWall = null;

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 把石头配置成一个正常的物理抛体
    /// </summary>
    private void SetupPhysics()
    {
        // 碰撞体：没有就自动补一个球
        if (GetComponent<Collider>() == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = colliderRadius;
        }

        // 刚体：动态模式，受重力影响，会滚动会弹开
        body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = false;
        body.useGravity = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        // 石头飞得快，防止穿墙
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// 发射
    /// </summary>
    public void Launch(Vector3 launchVelocity, float gravityValue, Action<Vector3> hitWallCallback)
    {
        if (body == null) SetupPhysics();

        onHitWall = hitWallCallback;
        lifeTimer = 0f;
        landTimer = -1f;
        hasLanded = false;
        hasHitWall = false;

        // 物理引擎的重力必须跟 Throw Gravity 接近，
        // 否则自动算出来的初速跟实际轨迹对不上，石头会砸偏。
        float gDiff = Mathf.Abs(Physics.gravity.magnitude - gravityValue);
        if (gDiff > 0.1f)
        {
            Debug.LogWarning("[攻城] Physics.gravity(" + Physics.gravity.magnitude +
                             ") 跟 Throw Gravity(" + gravityValue +
                             ") 差太多，石头轨迹会跟自动算的初速对不上");
        }

        // 给一个初速度，之后完全交给物理引擎
        body.linearVelocity = launchVelocity;
        // 随机一点旋转，看起来像石头在空中翻着飞
        body.angularVelocity = UnityEngine.Random.onUnitSphere * 3f;
    }

    /// <summary>
    /// 发射前摆位置。动态刚体要用刚体的位置接口，否则会跟物理状态脱节。
    /// </summary>
    public void SetSpawnPosition(Vector3 pos)
    {
        if (body == null) SetupPhysics();
        body.position = pos;
        transform.position = pos;
    }

    private void Update()
    {
        if (body == null) return;

        // 落地之后滚一会儿就回收
        if (landTimer >= 0f)
        {
            landTimer -= Time.deltaTime;
            if (landTimer <= 0f)
            {
                Recycle();
            }
            return;
        }

        lifeTimer += Time.deltaTime;

        // 超时兜底：一路飞出场外没落地，也得回收
        if (lifeTimer >= lifeTime)
        {
            Recycle();
        }
    }

    /// <summary>
    /// 撞到东西。
    /// 撞到城墙 → 加进度（石头继续弹开飞走，不插住）。
    /// 撞到别的 → 普通碰撞，弹开；如果不是城墙就算真正落地。
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null) return;

        bool isWall = IsWall(collision.collider);

        // 第一次撞到东西：播落地音效 + 特效
        if (!hasLanded)
        {
            hasLanded = true;

            if (landSound != null)
            {
                AudioSource src = GetComponent<AudioSource>();
                if (src == null) src = gameObject.AddComponent<AudioSource>();
                src.PlayOneShot(landSound);
            }

            if (landEffect != null) landEffect.Play();
        }

        // 撞到非城墙的东西 → 石头真的落地了，躺一会儿就回收
        if (!isWall && landTimer < 0f)
        {
            landTimer = landRecycleDelay;
        }

        // 砸中城墙 → 进度条增加（只加一次）
        if (isWall && !hasHitWall)
        {
            hasHitWall = true;

            Vector3 point = collision.contactCount > 0
                ? collision.GetContact(0).point
                : collision.collider.ClosestPoint(transform.position);

            if (onHitWall != null) onHitWall.Invoke(point);
        }
    }

    /// <summary>
    /// 撞到 Trigger 物体（万一路城墙用的是 Trigger）
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!IsWall(other)) return;
        if (hasHitWall) return;

        hasHitWall = true;

        if (onHitWall != null) onHitWall.Invoke(other.ClosestPoint(transform.position));
    }

    /// <summary>
    /// 这个碰撞体是不是城墙
    /// </summary>
    private bool IsWall(Collider c)
    {
        if (c == null) return false;

        // 自身或父级带这个 tag 都算
        Transform t = c.transform;
        for (int i = 0; i < 10 && t != null; i++)
        {
            if (t.tag == wallTag) return true;
            t = t.parent;
        }

        return false;
    }

    /// <summary>
    /// 回收
    /// </summary>
    private void Recycle()
    {
        ResetState();

        if (landEffect != null) landEffect.Stop();

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        onHitWall = null;

        if (onRecycle != null) onRecycle.Invoke(gameObject);
        onRecycle = null;
    }

    /// <summary>
    /// 重置所有状态
    /// </summary>
    private void ResetState()
    {
        landTimer = -1f;
        lifeTimer = 0f;
        hasLanded = false;
        hasHitWall = false;
    }
}
