using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>抽线索的结果。</summary>
public enum ClueGrantResult
{
    /// <summary>拿到了一条没见过的线索。</summary>
    New,
    /// <summary>这条线索已经有了（不计入进度）。</summary>
    Duplicate,
    /// <summary>所有线索都已经集齐 / 参数无效。</summary>
    AllCollected
}

/// <summary>
/// 线索管理：线索是“挂”在气球上的，打爆带线索的气球才会调用这里。
/// 允许重复线索，只有“第一次拿到”的线索才算进度。
/// </summary>
public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance { get; private set; }

    [Header("线索文字（随机分配到气球上，可能重复）")]
    public string[] clueTexts;

    [Header("线索展示点（沿用原来的 Point，位置不变）")]
    public Transform cluePoint;

    [Header("线索文字组件（留空自动在展示点或本物体上找）")]
    public Text clueText;

    [Header("展示时长（秒）")]
    public float displayDuration = 5f;

    readonly HashSet<int> _collected = new HashSet<int>();
    readonly List<int> _all = new List<int>();

    /// <summary>已收集的不重复线索数量。</summary>
    public int WinCount { get; private set; }

    /// <summary>线索总条数。</summary>
    public int TotalCount => clueTexts != null ? clueTexts.Length : 0;

    /// <summary>拿到新线索时触发，参数是线索下标。</summary>
    public System.Action<int> OnNewClue;

    /// <summary>全部线索集齐时触发。</summary>
    public System.Action OnCleared;

    public bool IsCollectedAll => TotalCount > 0 && WinCount >= TotalCount;

    void Awake()
    {
        Instance = this;

        // 位置仍然取 LaucherManager 的 Point，取不到就退回到自己身上
        if (LaucherManager.Instance != null && LaucherManager.Instance.Point != null)
            cluePoint = LaucherManager.Instance.Point;
        if (cluePoint == null) cluePoint = transform;

        if (clueText == null && cluePoint != null)
            clueText = cluePoint.GetComponent<Text>();
        if (clueText == null)
            clueText = GetComponent<Text>();
        if (clueText == null)
            Debug.LogError("[ClueManager] 没找到 Text 组件，线索不会显示");

        ResetClues();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>重开一局：清空文字、重置线索池。</summary>
    public void ResetClues()
    {
        CancelInvoke(nameof(HideText));
        HideText();
        _collected.Clear();
        WinCount = 0;
    }

    /// <summary>这条线索是不是已经拿到过。</summary>
    public bool HasCollected(int index)
    {
        return _collected.Contains(index);
    }

    /// <summary>把还没收集到的线索下标填进 buffer（给生成器挑线索用）。</summary>
    public void FillUncollected(List<int> buffer)
    {
        if (buffer == null) return;
        buffer.Clear();

        for (int i = 0; i < TotalCount; i++)
        {
            if (!_collected.Contains(i)) buffer.Add(i);
        }
    }

    /// <summary>拿到指定下标的线索。</summary>
    public ClueGrantResult GrantClue(int index)
    {
        if (TotalCount == 0) return ClueGrantResult.AllCollected;
        if (index < 0 || index >= TotalCount) return ClueGrantResult.AllCollected;

        if (_collected.Contains(index))
        {
            return ClueGrantResult.Duplicate;
        }

        _collected.Add(index);
        WinCount = _collected.Count;

        ShowText(clueTexts[index]);
        OnNewClue?.Invoke(index);

        if (WinCount >= TotalCount)
        {
            OnCleared?.Invoke();
        }

        return ClueGrantResult.New;
    }

    /// <summary>随机拿一条（调试/兜底用；正常玩法用气球上的线索）。</summary>
    public ClueGrantResult GrantRandomClue()
    {
        if (TotalCount == 0) return ClueGrantResult.AllCollected;
        return GrantClue(Random.Range(0, TotalCount));
    }

    /// <summary>在线索点上临时显示一句话，到时自动清空（死亡提示用）。</summary>
    public void ShowTemporaryMessage(string message, float duration)
    {
        if (clueText == null) return;

        CancelInvoke(nameof(HideText));
        clueText.text = message;
        Invoke(nameof(HideText), Mathf.Max(0.1f, duration));
    }

    void ShowText(string content)
    {
        if (clueText == null) return;

        clueText.text = content;

        // 连续中奖时重置计时，避免上一条的隐藏把新的一条提前清掉
        CancelInvoke(nameof(HideText));
        Invoke(nameof(HideText), displayDuration);
    }

    void HideText()
    {
        if (clueText == null) return;
        clueText.text = "";
    }
}
