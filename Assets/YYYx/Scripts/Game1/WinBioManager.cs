using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RewardSlot
{
    [Header("灯（默认常显）")]
    public GameObject light;

    [Header("简历展示点")]
    public Transform resumePoint;

    [HideInInspector] public Renderer lightRenderer;
}

/// <summary>
/// 简历墙：每拿到一条“新”线索点亮一盏灯、生成一份简历。
/// 六份简历全部出现后，ResumePickManager 才允许玩家长按选择。
/// </summary>
public class WinBioManager : MonoBehaviour
{
    public static WinBioManager Instance { get; private set; }

    [Header("简历槽位")]
    public RewardSlot[] slots;

    [Header("简历预制体")]
    public GameObject[] resumePrefabs;

    [Header("灯未亮时的颜色")]
    public string idleColorHex = "#434343";

    [Header("灯亮起后的颜色")]
    public string litColorHex = "#558C33";

    [Header("亮灯时的自发光强度（0 = 不改自发光）")]
    public float litEmission = 4f;

    [Header("六次简历全部出现后淡出的 Canvas（挂 CanvasFadeOut）")]
    public CanvasFadeOut finishCanvas;

    [Header("最后一份简历出现后，延迟多久开始淡出（秒）")]
    public float finishDelay = 0.6f;

    [Header("集齐六份简历后出现的物品（直接把物体拖进来）")]
    public GameObject rewardItem;

    [Header("物品出现前的延迟（秒）")]
    public float rewardItemDelay = 0.6f;

    List<int> _remaining;
    List<GameObject> _spawned;

    MaterialPropertyBlock _block;
    Color _idleColor = Color.gray;
    Color _litColor = Color.green;

    /// <summary>已经出现的简历数量（= 已收集的不重复线索数量）。</summary>
    public int WinCount { get; private set; }

    void Awake()
    {
        Instance = this;

        ColorUtility.TryParseHtmlString(idleColorHex, out Color idle);
        ColorUtility.TryParseHtmlString(litColorHex, out Color lit);
        _idleColor = idle;
        _litColor = lit;
        _block = new MaterialPropertyBlock();

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;

                if (slots[i].lightRenderer == null && slots[i].light != null)
                {
                    slots[i].lightRenderer = slots[i].light.GetComponentInChildren<Renderer>(true);
                }

                if (slots[i].light != null) slots[i].light.SetActive(true);

                ApplyColor(slots[i].lightRenderer, _idleColor, false);
            }
        }

        _remaining = new List<int>();
        if (resumePrefabs != null)
        {
            for (int i = 0; i < resumePrefabs.Length; i++)
            {
                _remaining.Add(i);
            }
        }

        _spawned = new List<GameObject>();

        // 先藏起来，等集齐六份简历再显示
        if (rewardItem != null) rewardItem.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>拿到一条新线索时调用：点亮下一盏灯 + 随机生成一份还没出现过的简历。</summary>
    public void OnWin()
    {
        if (slots == null || slots.Length == 0) return;
        if (WinCount >= slots.Length) return;

        RewardSlot slot = slots[WinCount];
        if (slot == null) return;

        if (slot.light != null) slot.light.SetActive(true);

        ApplyColor(slot.lightRenderer != null ? slot.lightRenderer : slot.light?.GetComponentInChildren<Renderer>(true),
                   _litColor, true);

        if (_remaining != null && _remaining.Count > 0 && resumePrefabs != null)
        {
            int pick = Random.Range(0, _remaining.Count);
            int resumeIdx = _remaining[pick];
            _remaining.RemoveAt(pick);

            if (slot.resumePoint != null && resumeIdx < resumePrefabs.Length && resumePrefabs[resumeIdx] != null)
            {
                GameObject go = Instantiate(
                    resumePrefabs[resumeIdx],
                    slot.resumePoint.position,
                    slot.resumePoint.rotation,
                    slot.resumePoint
                );

                _spawned.Add(go);
            }
        }

        WinCount++;

        // 集齐六份简历：出现物品
        if (WinCount >= slots.Length && rewardItem != null)
        {
            if (rewardItemDelay > 0f) StartCoroutine(ShowRewardItemAfterDelay());
            else rewardItem.SetActive(true);
        }

        if (WinCount >= slots.Length && finishCanvas != null)
        {
            if (finishDelay > 0f) StartCoroutine(FadeFinishCanvasAfterDelay());
            else finishCanvas.FadeOutAndDestroy();
        }
    }

    IEnumerator ShowRewardItemAfterDelay()
    {
        yield return new WaitForSeconds(rewardItemDelay);
        if (rewardItem != null) rewardItem.SetActive(true);
    }

    IEnumerator FadeFinishCanvasAfterDelay()
    {
        yield return new WaitForSeconds(finishDelay);
        if (finishCanvas != null) finishCanvas.FadeOutAndDestroy();
    }

    void ApplyColor(Renderer r, Color c, bool lit)
    {
        if (r == null) return;

        r.GetPropertyBlock(_block);

        _block.SetColor("_BaseColor", c);
        _block.SetColor("_Color", c);

        if (lit && litEmission > 0f)
        {
            _block.SetColor("_EmissionColor", new Color(c.r, c.g, c.b, 1f) * Mathf.Pow(2f, litEmission));
        }
        else
        {
            _block.SetColor("_EmissionColor", Color.black);
        }

        r.SetPropertyBlock(_block);
    }
}
