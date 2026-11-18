using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 箭矢：沿贝塞尔弹道飞行，命中靶点后回收。
/// 照搬 IslandBattle 的 PaoDan，命中判定改为走 TargetBoard / ScoreTarget。
///
/// ★ 与 InteractiveRoot.prefab / Arrow.prefab 的配合：
///   Arrow.prefab 里原本挂的是 PaoDan，字段名保持一致便于对照：
///     part_tuowei → trail（拖尾）
///     part_baozha → hitEffect（命中爆炸）
///     part_shuihua → splashEffect（落水水花）
///     aud_Enemys  → hitSound（命中音效）
///     aud_Planes  → splashSound（落水音效）
///   改造时只要把 PaoDan 删掉换成 Arrow，再把上面的引用拖进去即可。
/// </summary>
[RequireComponent(typeof(Collider))]
public class Arrow : MonoBehaviour
{
    [Header("落水判定高度，低于此值算落水")]
    public float waterY = -8.5f;

    [Header("命中/落水后延迟多久回收（秒）")]
    public float destroyDelay = 1f;

    [Header("拖尾特效（原版 part_tuowei）")]
    public ParticleSystem trail;

    [Header("命中特效（原版 part_baozha）")]
    public ParticleSystem hitEffect;

    [Header("落水特效（原版 part_shuihua）")]
    public ParticleSystem splashEffect;

    [Header("命中音效（原版 aud_Enemys）")]
    public AudioClip hitSound;

    [Header("落水音效（原版 aud_Planes）")]
    public AudioClip splashSound;

    [Header("打中灰靶（不得分）时的音效，留空则不播")]
    public AudioClip missSound;

    private AudioSource audioSource;
    private bool isDone;

    /// <summary>本次发射的回收回调</summary>
    private System.Action<Arrow> recycleCallback;

    /// <summary>回收倒计时（秒），-1 表示没在倒计时</summary>
    private float recycleTimer = -1f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    /// <summary>
    /// 发射。posList 是 BowPath 每帧算好的世界坐标弹道。
    /// 第一个点就是箭矢生成点（把它放在弩的弓弦位置）。
    /// </summary>
    public void Launch(List<Vector3> posList, float moveTime, System.Action<Arrow> onRecycle)
    {
        isDone = false;
        recycleCallback = onRecycle;
        recycleTimer = -1f;

        // 复位缩放（上一发命中时缩到 0.01 了）
        transform.localScale = Vector3.one;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (trail != null) trail.Play();
        if (hitEffect != null) hitEffect.gameObject.SetActive(false);
        if (splashEffect != null) splashEffect.gameObject.SetActive(false);

        // 起点 = 弹道第一个点，箭矢朝向 = 弹道切线方向
        transform.position = posList[0];
        Vector3 forward = posList[1] - posList[0];
        if (forward.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        transform.DOPath(posList.ToArray(), moveTime)
            .SetEase(Ease.Linear)
            .SetLookAt(0.01f)
            .OnComplete(() => { Recycle(); });
    }

    /// <summary>
    /// 落水判定 + 回收倒计时，每帧检测
    /// </summary>
    private void Update()
    {
        // 回收倒计时：命中/落水后延迟销毁
        if (recycleTimer >= 0f)
        {
            recycleTimer -= Time.deltaTime;
            if (recycleTimer <= 0f)
            {
                recycleTimer = -1f;
                Recycle();
            }
            return;
        }

        if (isDone) return;

        if (transform.position.y < waterY)
        {
            isDone = true;

            if (splashEffect != null) splashEffect.Play();
            if (splashSound != null && audioSource != null) audioSource.PlayOneShot(splashSound);

            StopVisual();

            // 延迟回收
            ScheduleRecycle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDone) return;

        // 判定打分靶点
        ScoreTarget boardTarget = other.GetComponentInParent<ScoreTarget>();
        if (boardTarget == null) return;

        isDone = true;

        // 优先找 TargetBoard 走它的逻辑；
        // 找不到（靶点不在 TargetBoard 层级下）就直接走通用逻辑
        TargetBoard board = boardTarget.GetComponentInParent<TargetBoard>();
        bool scored;
        if (board != null)
        {
            scored = board.OnTargetHit(boardTarget);
        }
        else
        {
            scored = boardTarget.OnHit();
            if (scored && GameManager.I != null)
            {
                GameManager.I.AddScore(boardTarget.scoreValue);
            }
        }

        // 打中红靶播命中特效音效，打中灰靶播打空音效
        if (scored)
        {
            if (hitEffect != null) hitEffect.Play();
            if (hitSound != null && audioSource != null) audioSource.PlayOneShot(hitSound);
        }
        else
        {
            if (missSound != null && audioSource != null) audioSource.PlayOneShot(missSound);
        }

        // 停掉飞行，箭插在命中点
        transform.DOKill();

        StopVisual();

        // 缩小到看不见（看起来像插进去了）
        transform.DOScale(Vector3.one * 0.01f, 0.15f);

        // 延迟回收
        ScheduleRecycle();
    }

    /// <summary>
    /// 停止箭矢的视觉表现：关碰撞体、停拖尾
    /// </summary>
    private void StopVisual()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (trail != null) trail.Stop();
    }

    /// <summary>
    /// 延迟回收。命中后停一会儿，让玩家看清命中的位置。
    /// </summary>
    private void ScheduleRecycle()
    {
        if (destroyDelay <= 0f)
        {
            Recycle();
            return;
        }

        // 用计时器实现，不依赖 DOTween 的延迟 API，
        // 也不会被 DOKill() 误伤。
        recycleTimer = destroyDelay;
    }

    /// <summary>
    /// 回收进对象池
    /// </summary>
    private void Recycle()
    {
        if (isDone && recycleCallback == null) return;

        isDone = true;
        recycleTimer = -1f;
        transform.DOKill();

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (trail != null) trail.Stop();
        if (hitEffect != null) hitEffect.gameObject.SetActive(false);
        if (splashEffect != null) splashEffect.gameObject.SetActive(false);

        var cb = recycleCallback;
        recycleCallback = null;
        cb?.Invoke(this);
    }
}
