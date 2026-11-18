using System;
using UnityEngine;

/// <summary>
/// 会持续上升的气球（物体名统一叫 Balloon）。
///
/// 每个气球在生成时会被随机分配一种颜色；部分气球身上带着一条线索
/// （<see cref="clueIndex"/> >= 0），打爆它就能拿到那条线索。
/// 打爆没有线索的气球则什么都不给。
///
/// 命中判定刻意做成“零依赖”：
///   · 箭矢/子弹带 Tag（默认 Arrow）即可；
///   · 或者箭矢上挂着名为 Arrow / ArrowController / PaoDan / Bullet 的脚本即可。
/// 如果都不满足，也可以在箭矢命中时直接调用 <see cref="Pop"/>。
/// </summary>
[DisallowMultipleComponent]
public class Balloon : MonoBehaviour
{
    [Header("上升")]
    [Tooltip("上升速度（米/秒），由 Spawner 生成时覆盖")]
    public float riseSpeed = 0.35f;
    [Tooltip("左右摇摆幅度（米）")]
    public float swayAmplitude = 0.12f;
    [Tooltip("摇摆频率")]
    public float swayFrequency = 0.7f;
    [Tooltip("飞行自转速度（度/秒）")]
    public float spinSpeed = 25f;

    [Header("命中判定")]
    [Tooltip("箭矢/子弹的 Tag；留空表示不按 Tag 判断")]
    public string[] arrowTags = { "Arrow" };
    [Tooltip("箭矢/子弹脚本的类名（精确匹配）")]
    public string[] arrowScriptNames = { "Arrow", "ArrowController", "PaoDan", "Bullet", "Projectile" };

    [Header("线索")]
    [Tooltip(">= 0 表示这个气球带着第几条线索；-1 表示普通气球")]
    public int clueIndex = -1;
    [Tooltip("可选：线索标记（气球上那个提示点），没有就留空")]
    public GameObject clueMarker;

    [Header("爆开表现")]
    [Tooltip("爆开粒子特效，可留空")]
    public ParticleSystem popEffect;
    [Tooltip("爆开音效，可留空（留空自动用 Resources/Sound/BalloonPop）")]
    public AudioClip popSound;
    [Range(0f, 1f)] public float popVolume = 0.6f;
    [Tooltip("爆开后多久销毁本体（秒）")]
    public float destroyDelay = 0.6f;

    static AudioClip _defaultPop;

    /// <summary>被击中爆开时触发（参数是自己）。</summary>
    public event Action<Balloon> Popped;

    public bool IsPopped { get; private set; }
    public bool HasClue => clueIndex >= 0;

    BalloonSpawner _spawner;
    Transform _body;
    float _baseX;
    float _baseZ;
    float _phase;
    float _topY;

    /// <summary>由 Spawner 调用，注入本次飞行的参数。</summary>
    public void Configure(BalloonSpawner spawner, float topY, float speed, float phase)
    {
        _spawner = spawner;
        _topY = topY;
        _phase = phase;
        riseSpeed = speed;

        _baseX = transform.position.x;
        _baseZ = transform.position.z;
        _body = transform.Find("Body");

        if (popSound == null)
        {
            if (_defaultPop == null) _defaultPop = Resources.Load<AudioClip>("Sound/BalloonPop");
            popSound = _defaultPop;
        }

        if (clueMarker == null)
        {
            Transform marker = transform.Find("ClueMarker");
            if (marker != null) clueMarker = marker.gameObject;
        }
    }

    /// <summary>设置这个气球携带的线索（-1 = 普通气球）。</summary>
    public void SetClue(int index)
    {
        clueIndex = index;
        if (clueMarker != null) clueMarker.SetActive(HasClue);
    }

    /// <summary>只控制头顶标记的显隐（假线索气球用）。</summary>
    public void ShowMarker(bool show)
    {
        if (clueMarker != null) clueMarker.SetActive(show);
    }

    void Update()
    {
        if (IsPopped) return;

        float y = transform.position.y + riseSpeed * Time.deltaTime;
        if (y >= _topY)
        {
            FlyAway();
            return;
        }

        float t = Time.time * swayFrequency + _phase;
        transform.position = new Vector3(
            _baseX + Mathf.Sin(t) * swayAmplitude,
            y,
            _baseZ + Mathf.Cos(t * 0.8f) * swayAmplitude * 0.6f);

        if (_body != null)
        {
            _body.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsArrow(other)) Pop();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null && IsArrow(collision.collider)) Pop();
    }

    /// <summary>判断碰到的东西是不是箭矢/子弹。</summary>
    bool IsArrow(Collider other)
    {
        if (other == null || IsPopped) return false;

        if (arrowTags != null)
        {
            for (int i = 0; i < arrowTags.Length; i++)
            {
                if (!string.IsNullOrEmpty(arrowTags[i]) && other.CompareTag(arrowTags[i]))
                    return true;
            }
        }

        if (arrowScriptNames != null && arrowScriptNames.Length > 0)
        {
            MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null) continue;
                string typeName = behaviours[i].GetType().Name;
                for (int j = 0; j < arrowScriptNames.Length; j++)
                {
                    if (typeName == arrowScriptNames[j]) return true;
                }
            }
        }

        return false;
    }

    /// <summary>打爆气球。外部射击逻辑可以直接调用它。</summary>
    public void Pop()
    {
        if (IsPopped) return;
        IsPopped = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (popEffect != null)
        {
            // 特效脱离气球，免得气球销毁时把特效一起带走
            popEffect.transform.SetParent(null, true);
            popEffect.Play();
            Destroy(popEffect.gameObject, 2f);
        }

        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, transform.position, popVolume);
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = false;

        Popped?.Invoke(this);
        if (_spawner != null) _spawner.NotifyPopped(this);

        Destroy(gameObject, destroyDelay);
    }

    /// <summary>升到顶还没被打中，算飞走了（不产生线索）。</summary>
    void FlyAway()
    {
        if (IsPopped) return;
        IsPopped = true;

        if (_spawner != null) _spawner.NotifyMissed(this);
        Destroy(gameObject);
    }
}
