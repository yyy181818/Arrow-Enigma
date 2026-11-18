using DG.Tweening;
using UnityEngine;

/// <summary>
/// 射击靶点：十个点位之一。
/// 灰色 = 不可攻击（击中不加分），红色 = 可攻击（击中加 10 分）。
/// 用 Renderer 的材质颜色区分状态，不做显隐，这样灰色靶子依然能被箭矢击中并给出反馈。
/// </summary>
public class ScoreTarget : MonoBehaviour
{
    [Header("是否处于亮起（可攻击）状态")]
    public bool isActiveTarget = false;

    [Header("可攻击时的颜色")]
    public Color activeColor = new Color(1f, 0.15f, 0.1f, 1f);

    [Header("不可攻击时的颜色")]
    public Color inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("被击中时的闪光颜色")]
    public Color hitFlashColor = Color.white;

    [Header("击中可攻击靶子的得分")]
    public int scoreValue = 10;

    [Header("引用渲染器，留空则自动找")]
    public Renderer targetRenderer;

    protected Material mat;
    private bool isFlashing = false;

    protected virtual void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        // 用 material 而不是 sharedMaterial，保证每个靶子颜色独立
        if (targetRenderer != null)
        {
            mat = targetRenderer.material;
        }
    }

    protected virtual void OnEnable()
    {
        ApplyColor();
    }

    /// <summary>
    /// 设置为可攻击（亮红）或不可攻击（灰）
    /// </summary>
    public virtual void SetActive(bool active)
    {
        isActiveTarget = active;
        ApplyColor();
    }

    private void ApplyColor()
    {
        if (mat == null) return;
        mat.color = isActiveTarget ? activeColor : inactiveColor;
    }

    /// <summary>
    /// 被箭矢命中。返回是否得分。
    /// </summary>
      public virtual bool OnHit()
    {
        if (!isActiveTarget) return false;

        isActiveTarget = false;
        PlayFlash(inactiveColor);
        return true;
    }


    /// <summary>
    /// 击中闪光
    /// </summary>
    private void PlayFlash(Color backColor)
    {
        if (mat == null || isFlashing) return;

        isFlashing = true;
        Color flash = hitFlashColor;

        mat.color = flash;
        mat.DOKill();
        mat.DOFade(1f, 0.1f).OnComplete(() =>
        {
            mat.color = backColor;
            isFlashing = false;
        });
    }
}
