using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 气球玩法总控：打爆带线索的气球 -> 拿到那条线索 -> 新线索才发简历。
///
/// 规则：
///   · 只有身上带线索的气球（Balloon.HasClue）才会给线索；
///   · 打爆时拿到的就是气球上挂着的那条：
///       - 没拿到过 => 显示线索 + 点亮一盏灯 + 生成一份简历；
///       - 拿到过   => 提示“重复”，不计数、不发简历；
///   · 6 条线索全部集齐 => 6 份简历全部出现 => ResumePickManager 解锁简历选择。
/// </summary>
public class BalloonGameManager : MonoBehaviour
{
    [Header("引用（留空会自动在本物体/场景里找）")]
    public BalloonSpawner spawner;
    public ClueManager clueManager;
    public WinBioManager winBioManager;

    [Header("进度文字（可留空）")]
    public Text progressText;
    public string progressFormat = "线索 {0}/{1}";

    [Header("线索集齐后要隐藏的弓弩（留空自动找场景里所有 NuJianCrossbow）")]
    public GameObject[] handCrossbows;

    [Header("音效（把 AudioClip 拖进来即可）")]
    [Tooltip("播放用的音源；留空则用 PlayClipAtPoint 在生成器位置播放")]
    public AudioSource sfxSource;

    [Tooltip("集齐全部线索时播放")]
    public AudioClip allCluesClip;

    [Header("重复线索提示")]
    public string duplicateMessage = "这条线索已经知道了";
    public float duplicateMessageDuration = 2f;

    [Header("诊断")]
    public bool debugLog = true;
    [Tooltip("没接弓弩时，按键盘上的键随机打爆一个气球，用来快速验证线索流程")]
    public bool useDebugKey = false;
    public KeyCode debugPopKey = KeyCode.B;

    /// <summary>累计抽到重复线索的次数。</summary>
    public int DuplicateCount { get; private set; }

    /// <summary>累计打爆的普通气球（没线索的）。</summary>
    public int EmptyPops { get; private set; }

    Coroutine _hintRoutine;

    void Awake()
    {
        if (spawner == null) spawner = GetComponentInChildren<BalloonSpawner>(true);
        if (clueManager == null) clueManager = FindAnyObjectByType<ClueManager>(FindObjectsInactive.Include);
        if (winBioManager == null) winBioManager = FindAnyObjectByType<WinBioManager>(FindObjectsInactive.Include);

        if (handCrossbows == null || handCrossbows.Length == 0)
        {
            NuJianCrossbow[] all = FindObjectsByType<NuJianCrossbow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            handCrossbows = new GameObject[all.Length];
            for (int i = 0; i < all.Length; i++)
            {
                handCrossbows[i] = all[i] != null ? all[i].gameObject : null;
            }
        }
    }

    void OnEnable()
    {
        if (spawner != null) spawner.BalloonPopped += HandleBalloonPopped;
        ShowCrossbow();          // 每次 GameScene 重载（重生）都把弓弩放回手上
        RefreshProgress();
    }

    void OnDisable()
    {
        if (spawner != null) spawner.BalloonPopped -= HandleBalloonPopped;
    }

    void Update()
    {
        if (!useDebugKey) return;
        if (spawner == null) return;
        if (Input.GetKeyDown(debugPopKey)) spawner.PopRandomBalloon();
    }

    void HandleBalloonPopped(Balloon balloon)
    {
        if (balloon == null) return;

        if (!balloon.HasClue)
        {
            EmptyPops++;
            if (debugLog) Debug.Log("[气球] 普通气球，没有线索");
            return;
        }

        if (clueManager == null)
        {
            Debug.LogWarning("[气球] 场景里没有 ClueManager，打爆气球不会给线索");
            return;
        }

        ClueGrantResult result = clueManager.GrantClue(balloon.clueIndex);

        switch (result)
        {
            case ClueGrantResult.New:
                if (winBioManager != null) winBioManager.OnWin();
                if (debugLog) Debug.Log($"[气球] 新线索 {clueManager.WinCount}/{clueManager.TotalCount}（#{balloon.clueIndex}）");

                if (clueManager.IsCollectedAll)
                {
                    OnAllCluesCollected();
                }
                break;

            case ClueGrantResult.Duplicate:
                DuplicateCount++;
                ShowDuplicateHint();
                if (debugLog) Debug.Log($"[气球] 重复线索 #{balloon.clueIndex}（累计 {DuplicateCount} 次）");
                break;

            case ClueGrantResult.AllCollected:
                // 已经集齐，剩下的气球当普通靶子打
                break;
        }

        RefreshProgress();
    }

    void RefreshProgress()
    {
        if (progressText == null || clueManager == null) return;
        progressText.text = string.Format(progressFormat, clueManager.WinCount, clueManager.TotalCount);
    }

    /// <summary>线索集齐：停止气球、收弓，进入简历选择阶段。</summary>
    void OnAllCluesCollected()
    {
        if (spawner != null) spawner.StopAndClear();
        HideCrossbow();
        PlayAllCluesSfx();

        if (debugLog) Debug.Log("[气球] 线索已集齐：停止生成气球并隐藏弓弩");
    }

    void PlayAllCluesSfx()
    {
        if (allCluesClip == null) return;

        if (sfxSource != null) sfxSource.PlayOneShot(allCluesClip);
        else AudioSource.PlayClipAtPoint(allCluesClip, transform.position);
    }

    public void ShowCrossbow()
    {
        if (handCrossbows == null) return;

        for (int i = 0; i < handCrossbows.Length; i++)
        {
            if (handCrossbows[i] != null) handCrossbows[i].SetActive(true);
        }
    }

    public void HideCrossbow()
    {
        if (handCrossbows == null) return;

        for (int i = 0; i < handCrossbows.Length; i++)
        {
            if (handCrossbows[i] != null) handCrossbows[i].SetActive(false);
        }
    }

    void ShowDuplicateHint()
    {
        if (string.IsNullOrEmpty(duplicateMessage)) return;

        if (_hintRoutine != null) StopCoroutine(_hintRoutine);
        _hintRoutine = StartCoroutine(DuplicateHintRoutine());
    }

    IEnumerator DuplicateHintRoutine()
    {
        if (LaucherManager.Instance != null)
        {
            LaucherManager.Instance.ShowText(duplicateMessage);
        }

        yield return new WaitForSeconds(Mathf.Max(0.2f, duplicateMessageDuration));

        if (LaucherManager.Instance != null)
        {
            LaucherManager.Instance.ShowText("");
        }

        _hintRoutine = null;
    }
}
