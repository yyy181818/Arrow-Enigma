using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 十个靶点的管理器：控制哪些靶子亮起（红）、哪些熄灭（灰）。
/// 亮起数量随机，且随分数提高而减少，逼迫玩家提高命中率。
///
/// 分数档位（按当前总分）：
///   0   ~ 100  → 亮起 8 ~ 10 个
///   100 ~ 150  → 亮起 6 ~ 8 个
///   150 ~ 200  → 亮起 4 ~ 6 个
///   200 以上   → 亮起 2 ~ 4 个
/// </summary>
public class TargetBoard : MonoBehaviour
{
    [Header("所有靶点（应为 10 个）。攻城靶点也放这里，它们是 ScoreTarget 的子类")]
    public List<ScoreTarget> targets = new List<ScoreTarget>();

    [Header("分数档位配置")]
    public int tier1Score = 0;
    public int tier1Min = 8;
    public int tier1Max = 10;

    public int tier2Score = 100;
    public int tier2Min = 6;
    public int tier2Max = 8;

    public int tier3Score = 150;
    public int tier3Min = 4;
    public int tier3Max = 6;

    public int tier4Score = 200;
    public int tier4Min = 2;
    public int tier4Max = 4;

    [Header("命中可攻击靶子后的刷新延迟（秒）")]
    public float refreshDelay = 0.3f;

    private bool isRefreshing = false;
    private bool isSubscribed = false;

    private void Start()
    {
        CollectTargets();
        RefreshAll();
        Subscribe();
    }

    /// <summary>
    /// 收集所有靶点。
    ///
    /// 为什么要自动收集：
    ///   在靶点上把 ScoreTarget 换成 SiegeEnemy（子类）时，
    ///   Unity 会把列表里的旧引用置空（显示 Missing），
    ///   手动重拖 10 个太麻烦，这里自动修复。
    ///
    /// 规则：
    ///   1. 先清掉列表里失效（null）的引用
    ///   2. 把场景里所有还没在列表里的靶点补进来
    ///      （SiegeEnemy 是 ScoreTarget 的子类，会被一起收进来）
    ///   3. 被禁用的组件一律不要 —— 同一个靶点上如果同时挂着基类和 SiegeEnemy，
    ///      随机点亮时名额会被基类白白占掉一半，亮红数量和实际开火数量就对不上。
    /// </summary>
    private void CollectTargets()
    {
        if (targets == null) targets = new List<ScoreTarget>();

        // 1. 清掉失效引用（换脚本组件后旧引用会变 null）和被禁用的引用
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null || !targets[i].enabled)
            {
                targets.RemoveAt(i);
            }
        }

        // 2. 自动补上场景里所有还没在列表里的、且处于启用状态的靶点
        ScoreTarget[] all = FindObjectsByType<ScoreTarget>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            if (!all[i].enabled) continue;
            if (!targets.Contains(all[i])) targets.Add(all[i]);
        }
    }

    private void OnEnable()
    {
        // GameManager 的 Awake 可能还没跑，这里先试一次，Start 里会再兜一次
        Subscribe();
    }

    private void OnDisable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnScoreChanged -= OnScoreChanged;
        }
        isSubscribed = false;
    }

    private void Subscribe()
    {
        if (isSubscribed) return;
        if (GameManager.I == null) return;

        GameManager.I.OnScoreChanged += OnScoreChanged;
        isSubscribed = true;
    }

    /// <summary>
    /// 分数变化后按新档位刷新亮起数量
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        if (!isRefreshing)
        {
            isRefreshing = true;
            Invoke(nameof(DoRefresh), refreshDelay);
        }
    }

    /// <summary>
    /// 根据当前分数重新随机一批亮起的靶点
    /// </summary>
    public void RefreshAll()
    {
        if (targets == null || targets.Count == 0) return;

        int score = GameManager.I != null ? GameManager.I.score : 0;
        int count = GetLitCount(score);

        // 先全部熄灭
        foreach (var t in targets)
        {
            if (t != null) t.SetActive(false);
        }

        // 随机挑 count 个亮起
        List<ScoreTarget> pool = new List<ScoreTarget>();
        foreach (var t in targets)
        {
            if (t != null) pool.Add(t);
        }

        // 洗牌
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            ScoreTarget tmp = pool[i];
            pool[i] = pool[j];
            pool[j] = tmp;
        }

        int lit = Mathf.Min(count, pool.Count);
        for (int i = 0; i < lit; i++)
        {
            pool[i].SetActive(true);
        }
    }

    /// <summary>
    /// 按分数算当前应该亮起几个靶子
    /// </summary>
    private int GetLitCount(int score)
    {
        int min, max;

        if (score < tier2Score)
        {
            min = tier1Min; max = tier1Max;
        }
        else if (score < tier3Score)
        {
            min = tier2Min; max = tier2Max;
        }
        else if (score < tier4Score)
        {
            min = tier3Min; max = tier3Max;
        }
        else
        {
            min = tier4Min; max = tier4Max;
        }

        if (min > max)
        {
            int t = min; min = max; max = t;
        }

        return Random.Range(min, max + 1);
    }

    /// <summary>
    /// 某个靶点被命中。返回是否得分。
    /// </summary>
    public bool OnTargetHit(ScoreTarget target)
    {
        if (target == null) return false;

        // OnHit 是 virtual，攻城敌人（继承 ScoreTarget）override 后能正确走它的逻辑
        bool scored = target.OnHit();

        if (scored)
        {
            if (GameManager.I != null)
            {
                // AddScore 会触发 OnScoreChanged，由事件统一负责刷新，这里不再重复调
                GameManager.I.AddScore(target.scoreValue);
            }
        }

        return scored;
    }

    private void DoRefresh()
    {
        isRefreshing = false;
        RefreshAll();
    }
}
