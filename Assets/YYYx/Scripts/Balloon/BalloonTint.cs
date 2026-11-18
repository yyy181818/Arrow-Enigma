using UnityEngine;

/// <summary>
/// 给气球随机上色。挂在气球预制体的根节点上即可。
///
/// 背景：BalloonSpawner 的 palette 随机配色只作用于「程序化占位气球」，
/// 一旦换成正式预制体，所有气球就会是同一个颜色。
/// 本组件在实例化时（Awake）给自己随机挑一个颜色，
/// 用 MaterialPropertyBlock 写入，不改动共享材质资源，也不会产生材质实例副本。
/// </summary>
public class BalloonTint : MonoBehaviour
{
    [Tooltip("可选颜色池；留空则使用默认的六色")]
    public Color[] palette =
    {
        new Color(0.85f, 0.12f, 0.12f),
        new Color(0.98f, 0.78f, 0.20f),
        new Color(0.30f, 0.60f, 0.95f),
        new Color(0.35f, 0.80f, 0.45f),
        new Color(0.70f, 0.45f, 0.95f),
        new Color(0.95f, 0.55f, 0.20f),
    };

    [Tooltip("是否随机上色；关掉就保持材质原来的颜色")]
    public bool randomize = true;

    [Tooltip("颜色变化的明暗范围，0 表示不变化")]
    [Range(0f, 0.3f)]
    public float brightnessJitter = 0.12f;

    [Tooltip("作用的渲染器；留空则自动取子物体上的全部 Renderer")]
    public Renderer[] targets;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    MaterialPropertyBlock _block;

    void Awake()
    {
        if (!randomize) return;
        if (palette == null || palette.Length == 0) return;

        Renderer[] renderers = targets != null && targets.Length > 0
            ? targets
            : GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0) return;

        Color color = palette[Random.Range(0, palette.Length)];

        if (brightnessJitter > 0f)
        {
            float factor = 1f + Random.Range(-brightnessJitter, brightnessJitter);
            color.r = Mathf.Clamp01(color.r * factor);
            color.g = Mathf.Clamp01(color.g * factor);
            color.b = Mathf.Clamp01(color.b * factor);
        }

        _block = new MaterialPropertyBlock();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            _block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(_block);
        }
    }
}
