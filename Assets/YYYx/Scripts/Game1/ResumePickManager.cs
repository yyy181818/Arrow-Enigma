using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResumePickManager : MonoBehaviour
{
    public static ResumePickManager Instance { get; private set; }

    [Tooltip("六份简历全部出现（WinBioManager 走完）后才允许选择，留空则一直可选")]
    public WinBioManager winBioManager;

    [Tooltip("勾上就跳过“六份都出现”的判定，直接可选，调试用")]
    public bool forceInteractive = false;

    [Tooltip("选中一份后锁定，其余简历不再响应")]
    public bool lockAfterPick = true;

    [Header("选中正确简历后显示的文字（文字内容直接在 Text 上填）")]
    public Text correctText;

    [Header("逐字显示的间隔（秒）")]
    public float charInterval = 0.08f;

    [Header("选中正确简历后要隐藏的物品（直接把物体拖进来）")]
    public GameObject itemToHide;

    [Header("选错简历的提示")]
    public string wrongPickMessage = "你没有成功找到自己是谁，再来一次吧";
    public float wrongPickMessageDuration = 2f;

    [Header("选中简历那一刻的音效")]
    [Tooltip("播放用的音源；留空则用 PlayClipAtPoint 在本物体位置播放")]
    public AudioSource pickSfxSource;
    public AudioClip pickClip;

    [Header("逐字显示的打字音效")]
    [Tooltip("播放用的音源；留空则用 PlayClipAtPoint 在本物体位置播放")]
    public AudioSource typingSfxSource;
    public AudioClip typingClip;
    [Range(0f, 1f)] public float typingVolume = 0.5f;
    [Tooltip("每几个字符响一次（1 = 每个字都响）")]
    public int typingEveryNChars = 1;

    public bool Locked { get; private set; }

    readonly List<ResumePickButton> _buttons = new List<ResumePickButton>();

    string _correctFullText;
    Coroutine _typing;

    void Awake()
    {
        Instance = this;

        // 没手动拖打字音效时，用工程里自带的键盘敲击音
        if (typingClip == null) typingClip = Resources.Load<AudioClip>("Sound/KeyTap");

        if (correctText != null)
        {
            // 记住面板里填好的完整文字，运行时先清空并隐藏
            _correctFullText = correctText.text;
            correctText.text = "";
            correctText.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (Locked) return;

        bool interactive = forceInteractive || IsAllResumeShown();

        for (int i = _buttons.Count - 1; i >= 0; i--)
        {
            if (_buttons[i] == null)
            {
                _buttons.RemoveAt(i);
                continue;
            }

            _buttons[i].Tick(interactive);
        }
    }

    public void Register(ResumePickButton button)
    {
        if (button == null) return;
        if (_buttons.Contains(button)) return;

        button.ResetHold();
        _buttons.Add(button);
    }

    public void Unregister(ResumePickButton button)
    {
        if (button != null) _buttons.Remove(button);
    }

    public void OnPick(ResumePickButton button)
    {
        if (lockAfterPick) Locked = true;

        for (int i = 0; i < _buttons.Count; i++)
        {
            if (_buttons[i] == null) continue;
            if (_buttons[i] == button) continue;
            _buttons[i].ResetHold();
        }

        if (button == null) return;

        PlayPickSfx();

        if (button.isCorrect)
        {
            Debug.Log("选择正确简历");
            ShowCorrectResult();
            return;
        }

        Debug.Log("选择错误简历，玩家死亡");
        DoRespawn();
    }

    /// <summary>选对了：隐藏物品 + 文字逐字显示</summary>
    void ShowCorrectResult()
    {

        if (BombTimer.Instance != null)
        {
            BombTimer.Instance.StopPermanently();
            Debug.Log("[Resume] correct pick -> bomb timer stopped permanently");
        }

        if (ClockBgm.Instance != null)
        {
            ClockBgm.Instance.Stop();
        }
        if (itemToHide != null) itemToHide.SetActive(false);

        if (correctText == null) return;

        correctText.gameObject.SetActive(true);

        if (_typing != null) StopCoroutine(_typing);
        _typing = StartCoroutine(TypeRoutine(_correctFullText));
    }

    IEnumerator TypeRoutine(string full)
    {
        if (string.IsNullOrEmpty(full))
        {
            correctText.text = "";
            yield break;
        }

        correctText.text = "";

        for (int i = 0; i < full.Length; i++)
        {
            correctText.text = full.Substring(0, i + 1);

            if (!char.IsWhiteSpace(full[i]) &&
                typingClip != null &&
                (typingEveryNChars <= 1 || i % typingEveryNChars == 0))
            {
                PlayTypingSfx();
            }

            yield return new WaitForSeconds(charInterval);
        }

        _typing = null;
    }

    void PlayPickSfx()
    {
        if (pickClip == null) return;

        if (pickSfxSource != null) pickSfxSource.PlayOneShot(pickClip);
        else AudioSource.PlayClipAtPoint(pickClip, transform.position);
    }

    void PlayTypingSfx()
    {
        if (typingClip == null) return;

        if (typingSfxSource != null) typingSfxSource.PlayOneShot(typingClip, typingVolume);
        else AudioSource.PlayClipAtPoint(typingClip, transform.position, typingVolume);
    }

    void DoRespawn()
    {
        GameSceneManager gameSceneManager = GameSceneManager.Instance;
        if (gameSceneManager == null) return;

        // 在提示点显示“没找到自己是谁”，2 秒后清空再重置
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.ShowTemporaryMessage(wrongPickMessage, wrongPickMessageDuration);
        }
        else if (LaucherManager.Instance != null)
        {
            LaucherManager.Instance.ShowText(wrongPickMessage);
        }

        // 提示展示完再走重置流程
        float delay = Mathf.Max(gameSceneManager.respawnDelay, wrongPickMessageDuration);
        gameSceneManager.Invoke(nameof(GameSceneManager.Respawn), delay);
    }

    public void ResetPick()
    {
        Locked = false;

        if (_typing != null)
        {
            StopCoroutine(_typing);
            _typing = null;
        }

        if (correctText != null)
        {
            correctText.text = "";
            correctText.gameObject.SetActive(false);
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            if (_buttons[i] == null) continue;
            _buttons[i].ResetHold();
        }
    }

    bool IsAllResumeShown()
    {
        if (winBioManager == null) return true;
        if (winBioManager.slots == null || winBioManager.slots.Length == 0) return true;

        return winBioManager.WinCount >= winBioManager.slots.Length;
    }
}
