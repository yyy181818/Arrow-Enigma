using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 炸弹倒计时。到 0 播爆炸特效，然后让 GameSceneManager 重生。
/// 轮盘赌移除后，倒计时是唯一的“时间压力”来源。
/// </summary>
public class BombTimer : MonoBehaviour
{
    public static BombTimer Instance { get; private set; }

    [Header("炸弹倒计时")]
    [Tooltip("总时长（秒）")]
    [SerializeField] private float totalTime = 600f;

    public Text timerText;

    [Tooltip("爆炸特效，拖那个装了所有特效的父物体")]
    public ExplosionGroup explosion;

    [Header("倒计时音频")]
    [Tooltip("剩余多少秒开始播")]
    [SerializeField] private int warningSeconds = 9;

    [Tooltip("音频源")]
    public AudioSource warningAudio;

    [Tooltip("音频片段")]
    public AudioClip warningClip;

    [Header("Win pause (seconds; <=0 = pause forever)")]
    public float winPauseSeconds = 5f;

    public bool isRunning { get; private set; }

    public System.Action OnExploded;

    float currentTime;
    int lastShownSecond = -1;

    bool _warningPlayed;

    bool _pausedForWin;
    float _resumeAt = -1f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCountdown();
    }

    /// <summary>开始倒计时。</summary>
    public void StartCountdown()
    {
        _pausedForWin = false;
        _resumeAt = -1f;
        currentTime = totalTime;
        isRunning = true;
        lastShownSecond = -1;
        _warningPlayed = false;

        RefreshText();
    }

    public void StopCountdown()
    {
        isRunning = false;
    }

    /// <summary>
    /// 玩家获胜后暂停倒计时。winPauseSeconds > 0 时到点自动恢复；
    /// <= 0 表示永久暂停（直到手动 ResumeFromWinPause 或重新开始）。
    /// </summary>
    public void PauseOnWin()
    {
        _pausedForWin = true;
        isRunning = false;
        _resumeAt = winPauseSeconds > 0f ? Time.time + winPauseSeconds : -1f;
    }

    /// <summary>提前恢复倒计时（例如玩家开始下一轮）。</summary>
    public void ResumeFromWinPause()
    {
        if (!_pausedForWin) return;
        _pausedForWin = false;
        _resumeAt = -1f;
        isRunning = true;
    }

    /// <summary>永久停止倒计时（例如选对简历）。只有重新开始倒计时才会恢复。</summary>
    public void StopPermanently()
    {
        _pausedForWin = true;
        _resumeAt = -1f;
        isRunning = false;
    }

    public bool IsPausedForWin => _pausedForWin;

    void Update()
    {
        if (_pausedForWin)
        {
            if (_resumeAt > 0f && Time.time >= _resumeAt)
            {
                _pausedForWin = false;
                _resumeAt = -1f;
                isRunning = true;
            }
            else
            {
                return;
            }
        }

        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;

            if (explosion != null) explosion.Play();

            OnExploded?.Invoke();

            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.Invoke(nameof(GameSceneManager.Respawn), 1f);
            }

            return;
        }

        RefreshText();
        CheckWarning();
    }

    void CheckWarning()
    {
        if (_warningPlayed) return;
        if (currentTime > warningSeconds) return;

        _warningPlayed = true;

        if (warningAudio != null && warningClip != null)
        {
            warningAudio.clip = warningClip;
            warningAudio.Play();
        }

        Debug.Log("播放爆炸倒计时");
    }

    void RefreshText()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(currentTime);
        if (totalSeconds == lastShownSecond) return;

        lastShownSecond = totalSeconds;
        int min = totalSeconds / 60;
        int sec = totalSeconds % 60;
        timerText.text = $"{min}:{sec:00}";
    }

    void OnDestroy()
    {
        if (warningAudio != null) warningAudio.Stop();
    }
}
