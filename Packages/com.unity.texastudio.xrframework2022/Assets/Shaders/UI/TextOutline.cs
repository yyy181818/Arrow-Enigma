using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

[RequireComponent(typeof(Text))]
public class TextOutline : BaseMeshEffect
{
    public Material material;
    private static readonly string SOutlineShader = "TA Shaders/UI/UI(Outline)";
    private static readonly int SOutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int SOutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int SShadowOutlineColor = Shader.PropertyToID("_ShadowOutlineColor");
    private static readonly int SShadowOutlineWidth = Shader.PropertyToID("_ShadowOutlineWidth");
    private static readonly int SShadowOutlineOffset = Shader.PropertyToID("_ShadowOutlineOffset");
    [Header("使用渐变")] [SerializeField] private bool useTextGradient;

    public bool BUseTextGradient
    {
        get { return useTextGradient; }
        set
        {
            useTextGradient = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("文字颜色")] [SerializeField] private Gradient textGradient;

    public Gradient TextGradient
    {
        get { return textGradient; }
        set
        {
            textGradient = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("描边颜色")] [SerializeField] private Color outlineColor = Color.white;

    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("描边宽度"), Range(0, 8)] [SerializeField]
    private float outlineWidth;

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("===== 阴影 =====")] [SerializeField]
    private bool useShadow;

    public bool BUseShadow
    {
        get { return useShadow; }
        set
        {
            useShadow = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("使用独立透明度")] [SerializeField] private bool useShadowAlphaStand;

    public bool BShadowAlphaStand
    {
        get { return useShadowAlphaStand; }
        set
        {
            useShadowAlphaStand = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("阴影文字过渡")] [SerializeField] private bool useShadowGradient;

    public bool BUseShadowGradient
    {
        get { return useShadowGradient; }
        set
        {
            useShadowGradient = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("阴影文字颜色")] [SerializeField] private Gradient shadowGradient;

    public Gradient ShadowGradient
    {
        get { return shadowGradient; }
        set
        {
            shadowGradient = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [HideInInspector] [Header("阴影描边颜色")] [SerializeField]
    private Color shadowOutlineColor = Color.white;

    public Color ShadowOutlineColor
    {
        get { return shadowOutlineColor; }
        set
        {
            shadowOutlineColor = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [Header("阴影偏移")] [SerializeField] private Vector2 shadowOutlineOffset = new Vector2(0, 0);

    public Vector2 ShadowOutlineOffset
    {
        get { return shadowOutlineOffset; }
        set
        {
            shadowOutlineOffset = value;
            if (graphic != null)
                _Refresh();
        }
    }

    [HideInInspector] [Header("阴影描边宽度"), Range(0, 8)] [SerializeField]
    private float shadowOutlineWidth;

    public float ShadowOutlineWidth
    {
        get { return shadowOutlineWidth; }
        set
        {
            shadowOutlineWidth = value;
            if (graphic != null)
                _Refresh();
        }
    }

    int _iMatHash;
    bool _bSetPreviewCanvas;

    protected override void Awake()
    {
        base.Awake();
        //if (Application.isPlaying)
        //{
        //    if (graphic.material.GetHashCode() != _iMatHash)
        //    {
        //        graphic.material = CoreUtils.CreateEngineMaterial(SOutlineShader);
        //        _iMatHash = graphic.material.GetHashCode();
        //    }
        //}
        if (CheckShader())
        {
            SetShaderChannels();
            SetShaderParams();
            _Refresh();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        graphic.material = null;
    }

    public override void ModifyMesh(Mesh mesh)
    {
        base.ModifyMesh(mesh);
    }

    bool CheckShader()
    {
        if (graphic == null)
        {
            Debug.LogError("No Graphic Component !");
            return false;
        }

        if (graphic.material == null)
        {
            Debug.LogError("No Material !");
            return false;
        }

        return true;
    }

    void SetShaderParams()
    {
        if (graphic.material != null)
        {
            graphic.material.SetColor(SOutlineColor, OutlineColor);
            graphic.material.SetFloat(SOutlineWidth, OutlineWidth);
            graphic.material.SetVector(SShadowOutlineOffset, ShadowOutlineOffset);

            graphic.material.SetColor(SShadowOutlineColor, ShadowOutlineColor);
            graphic.material.SetFloat(SShadowOutlineWidth, ShadowOutlineWidth);
        }
    }

    void SetShaderChannels()
    {
        if (graphic.canvas)
        {
            var v1 = graphic.canvas.additionalShaderChannels;
            var v2 = AdditionalCanvasShaderChannels.TexCoord1;
            if ((v1 & v2) != v2)
            {
                graphic.canvas.additionalShaderChannels |= v2;
            }

            v2 = AdditionalCanvasShaderChannels.TexCoord2;
            if ((v1 & v2) != v2)
            {
                graphic.canvas.additionalShaderChannels |= v2;
            }

            v2 = AdditionalCanvasShaderChannels.TexCoord3;
            if ((v1 & v2) != v2)
            {
                graphic.canvas.additionalShaderChannels |= v2;
            }
        }
    }
#if UNITY_EDITOR
    protected void OnValidate()
    {
        if (!_bSetPreviewCanvas && Application.isEditor && gameObject.activeInHierarchy)
        {
            var can = GetComponentInParent<Canvas>();
            if (can != null)
            {
                if ((can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0 ||
                    (can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord2) == 0)
                {
                    if (can.name.Contains("(Environment)"))
                    {
                        // 处于Prefab预览场景中(需要打开TexCoord1,2,3通道，否则Scene场景上会有显示问题(因为我们有用到uv1,uv2.uv3))
                        can.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 |
                                                        AdditionalCanvasShaderChannels.TexCoord2 |
                                                        AdditionalCanvasShaderChannels.TexCoord3;
                    }
                    else
                    {
                        // 不是处于Prefab预览场景中，但父级Canvas的TexCoord1和TexCoord2通道没打开
                        Debug.LogWarning(
                            "Text2DOutline may display incorrect if TexCoord1, TexCoord2 and TexCoord3 Channels are not open at Canvas where Text2DOutline object in.");
                    }
                }

                _bSetPreviewCanvas = true;
            }
        }

        Invoke(nameof(OnValidate), 0);
        if (CheckShader())
        {
            SetShaderParams();
            _Refresh();
        }

        //if (graphic.material.GetHashCode() != _iMatHash)
        //{
        //    graphic.material = new Material(Shader.Find(SOutlineShader));
        //    _iMatHash = graphic.material.GetHashCode();
        //}
    }
#endif
#if UNITY_EDITOR
    protected void Reset()
    {
        // base.Reset();
        Invoke(nameof(Reset), 0);
        if (graphic.material.shader.name != SOutlineShader)
        {
            graphic.material = material;
            _iMatHash = graphic.material.GetHashCode();
        }
    }
#endif

    public override void ModifyMesh(VertexHelper vh)
    {
        var lVetexList = new List<UIVertex>();
        vh.GetUIVertexStream(lVetexList);
        _ProcessVertices(lVetexList, OutlineWidth);

        List<UIVertex> lShadowVerts = new List<UIVertex>();
        if (useShadow)
        {
            vh.GetUIVertexStream(lShadowVerts);
            ApplayShadow(lShadowVerts, shadowGradient.colorKeys[0].color);
            _ProcessVertices(lShadowVerts, ShadowOutlineWidth);


            if (useShadowGradient)
                ApplyGradient(lShadowVerts, shadowGradient);
        }

        if (useTextGradient)
        {
            ApplyGradient(lVetexList, textGradient);
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(lShadowVerts.Concat(lVetexList).ToList());
        lShadowVerts.Clear();
        lVetexList.Clear();
    }

    private void ApplayShadow(List<UIVertex> lShadowVerts, Color32 color)
    {
        for (var i = 0; i < lShadowVerts.Count; i++)
        {
            UIVertex vt = lShadowVerts[i];
            vt.position += new Vector3(ShadowOutlineOffset.x, ShadowOutlineOffset.y, 0);
            var newColor = color;
            if (!useShadowAlphaStand)
                newColor.a = (byte)((newColor.a * vt.color.a) / 255);
            vt.color = newColor;
            // uv3.x = -1用来传给Shader判断是文字还是阴影
            vt.uv3.x = -1;
            lShadowVerts[i] = vt;
        }
    }

    private void ApplyGradient(List<UIVertex> verts, Gradient gradient)
    {
        if (verts.Count == 0) return;
        float topY = verts[0].position.y;
        float bottomY = verts[0].position.y;
        for (var i = 0; i < verts.Count; i++)
        {
            var y = verts[i].position.y;
            if (y > topY)
                topY = y;
            else if (y < bottomY)
                bottomY = y;
        }

        float height = topY - bottomY;
        for (var i = 0; i < verts.Count; i++)
        {
            var vt = verts[i];
            Color32 color = gradient.Evaluate((topY - vt.position.y) / height);
            // var color = Color32.Lerp (new Color(1, 1, 1, 0), new Color(1, 1, 1, 0), (vt.position.y - bottomY) / height);
            color.a = (byte)((color.a * vt.color.a) / 255);
            vt.color = color;
            verts[i] = vt;
        }
    }

    // 添加描边后，为防止描边被网格边框裁切，需要将顶点外扩，同时保持UV不变
    private void _ProcessVertices(List<UIVertex> lVerts, float width)
    {
        for (int i = 0, count = lVerts.Count - 3; i <= count; i += 3)
        {
            var v1 = lVerts[i];
            var v2 = lVerts[i + 1];
            var v3 = lVerts[i + 2];
            // 计算原顶点坐标中心点
            //
            var minX = Min(v1.position.x, v2.position.x, v3.position.x);
            var minY = Min(v1.position.y, v2.position.y, v3.position.y);
            var maxX = Max(v1.position.x, v2.position.x, v3.position.x);
            var maxY = Max(v1.position.y, v2.position.y, v3.position.y);
            var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
            // 计算原始顶点坐标和UV的方向
            //
            Vector2 triX, triY, uvX, uvY;
            Vector2 pos1 = v1.position;
            Vector2 pos2 = v2.position;
            Vector2 pos3 = v3.position;
            if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
            {
                triX = pos2 - pos1;
                triY = pos3 - pos2;
                uvX = v2.uv0 - v1.uv0;
                uvY = v3.uv0 - v2.uv0;
            }
            else
            {
                triX = pos3 - pos2;
                triY = pos2 - pos1;
                uvX = v3.uv0 - v2.uv0;
                uvY = v2.uv0 - v1.uv0;
            }

            // 计算原始UV框
            var uvMin = Min(v1.uv0, v2.uv0, v3.uv0);
            var uvMax = Max(v1.uv0, v2.uv0, v3.uv0);

            // 为每个顶点设置新的Position和UV，并传入原始UV框
            v1 = _SetNewPosAndUV(v1, width, posCenter, triX, triY, uvX, uvY, uvMin, uvMax, ShadowOutlineOffset);
            v2 = _SetNewPosAndUV(v2, width, posCenter, triX, triY, uvX, uvY, uvMin, uvMax, ShadowOutlineOffset);
            v3 = _SetNewPosAndUV(v3, width, posCenter, triX, triY, uvX, uvY, uvMin, uvMax, ShadowOutlineOffset);

            // 应用设置后的UIVertex
            //
            lVerts[i] = v1;
            lVerts[i + 1] = v2;
            lVerts[i + 2] = v3;
        }
    }


    private static UIVertex _SetNewPosAndUV(UIVertex pVertex, float pOutLineWidth,
        Vector2 pPosCenter,
        Vector2 pTriangleX, Vector2 pTriangleY,
        Vector2 pUVx, Vector2 pUVy,
        Vector2 pUVOriginMin, Vector2 pUVOriginMax, Vector2 offset)
    {
        // Position
        var pos = pVertex.position;
        var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
        var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
        pos.x += posXOffset;
        pos.y += posYOffset;
        pVertex.position = pos;
        // UV
        var uv = pVertex.uv0;
        var uvOffsetX = pUVx / pTriangleX.magnitude * posXOffset *
                        (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
        var uvOffsetY = pUVy / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
        uv.x += (uvOffsetX.x + uvOffsetY.x);
        uv.y += (uvOffsetX.y + uvOffsetY.y);
        pVertex.uv0 = uv;

        pVertex.uv1 = pUVOriginMin; //uv1 uv2 可用  tangent  normal 在缩放情况 会有问题
        pVertex.uv2 = pUVOriginMax;

        return pVertex;
    }

    private void _Refresh()
    {
        SetShaderParams();
        graphic.SetVerticesDirty();
    }

    private static float Min(float pA, float pB, float pC)
    {
        return Mathf.Min(Mathf.Min(pA, pB), pC);
    }


    private static float Max(float pA, float pB, float pC)
    {
        return Mathf.Max(Mathf.Max(pA, pB), pC);
    }


    private static Vector2 Min(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(Min(pA.x, pB.x, pC.x), Min(pA.y, pB.y, pC.y));
    }


    private static Vector2 Max(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(Max(pA.x, pB.x, pC.x), Max(pA.y, pB.y, pC.y));
    }
}