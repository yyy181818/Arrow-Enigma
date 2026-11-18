using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 计分管理。只负责分数显示和分数变化广播，具体玩法（十靶等）自行订阅事件。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("当前分数")]
    public int score = 0;

    [Header("分数显示（可选）")]
    public Text scoreText;

    [Header("文本网格分数显示（可选）")]
    public TMPro.TextMeshProUGUI scoreTextPro;

    private void Awake()
    {
        I = this;
    }

    private void Update()
    {
        if (scoreText != null) scoreText.text = score.ToString();
        if (scoreTextPro != null) scoreTextPro.text = score.ToString();
    }

    /// <summary>
    /// 加分
    /// </summary>
    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// 重置分数
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// 分数变化事件，靶面管理器靠它刷新亮起数量
    /// </summary>
    public event System.Action<int> OnScoreChanged;
}
