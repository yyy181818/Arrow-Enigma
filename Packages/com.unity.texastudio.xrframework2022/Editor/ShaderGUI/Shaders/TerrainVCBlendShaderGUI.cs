#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IKASAMADUS
{
    // 配套 Terrain_VCBlend_Standalone.shader 的自定义面板。
    // 规范参考 TexaXRFramework/Editor/ShaderGUI（URPPlus 风格）：
    //   - 自带 Foldout 助手，helpBox 分组
    //   - FindProperty 缓存全部 MaterialProperty
    //   - 只用 ShaderProperty 绘制（[Toggle]/[KeywordEnum] 属性由 Unity 自动托管 keyword）
    //   - 不调用 base.OnGUI，彻底替换默认平铺面板
    public class TerrainVCBlendShaderGUI : ShaderGUI
    {
        private GUIStyle style = new GUIStyle();
        public GUILayoutOption[] shortButtonStyle = { GUILayout.Width(100) };

        MaterialEditor m_MaterialEditor;

        private static bool Foldout(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);
            style.fontSize = 11;
            style.normal.textColor = new Color(0.7f, 0.8f, 0.9f);

            var rect = GUILayoutUtility.GetRect(16f, 25f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        // ---- Surface ----
        MaterialProperty _CullMode = null;
        MaterialProperty _BlendContrast = null;
        MaterialProperty _Sample = null;
        MaterialProperty _WorldUVScale = null;
        MaterialProperty _Debug = null;

        // ---- Layer 1 (顶点色 R) ----
        MaterialProperty _Splat0 = null;
        MaterialProperty _Layer1_BaseColor = null;
        MaterialProperty _Layer1_addColor = null;
        MaterialProperty _Layer1_Normal = null;
        MaterialProperty _Layer1_NormalIntensity = null;
        MaterialProperty _Layer1_Tilling = null;
        MaterialProperty _Layer1_HeightContrast = null;
        MaterialProperty _Layer1_Mask = null;
        MaterialProperty _Layer1_Metallic = null;
        MaterialProperty _Layer1_Smoothness = null;

        // ---- Layer 2 (顶点色 G) ----
        MaterialProperty _Splat1 = null;
        MaterialProperty _Layer2_BaseColor = null;
        MaterialProperty _Layer2_addColor = null;
        MaterialProperty _Layer2_Normal = null;
        MaterialProperty _Layer2_NormalIntensity = null;
        MaterialProperty _Layer2_Tilling = null;
        MaterialProperty _Layer2_HeightContrast = null;
        MaterialProperty _Layer2_Mask = null;
        MaterialProperty _Layer2_Metallic = null;
        MaterialProperty _Layer2_Smoothness = null;

        // ---- Layer 3 (顶点色 B) ----
        MaterialProperty _Layer3Enable = null;
        MaterialProperty _Splat2 = null;
        MaterialProperty _Layer3_BaseColor = null;
        MaterialProperty _Layer3_addColor = null;
        MaterialProperty _Layer3_Normal = null;
        MaterialProperty _Layer3_NormalIntensity = null;
        MaterialProperty _Layer3_Tilling = null;
        MaterialProperty _Layer3_HeightContrast = null;
        MaterialProperty _Layer3_Mask = null;
        MaterialProperty _Layer3_Metallic = null;
        MaterialProperty _Layer3_Smoothness = null;

        private static bool _Surface_Foldout = true;
        private static bool _Layer1_Foldout = true;
        private static bool _Layer2_Foldout = true;
        private static bool _Layer3_Foldout = true;

        private void FindProperty(MaterialProperty[] properties)
        {
            _CullMode = FindProperty("_CullMode", properties);
            _BlendContrast = FindProperty("_BlendContrast", properties);
            _Sample = FindProperty("_Sample", properties);
            _WorldUVScale = FindProperty("_WorldUVScale", properties);
            _Debug = FindProperty("_Debug", properties);

            _Splat0 = FindProperty("_Splat0", properties);
            _Layer1_BaseColor = FindProperty("_Layer1_BaseColor", properties);
            _Layer1_addColor = FindProperty("_Layer1_addColor", properties);
            _Layer1_Normal = FindProperty("_Layer1_Normal", properties);
            _Layer1_NormalIntensity = FindProperty("_Layer1_NormalIntensity", properties);
            _Layer1_Tilling = FindProperty("_Layer1_Tilling", properties);
            _Layer1_HeightContrast = FindProperty("_Layer1_HeightContrast", properties);
            _Layer1_Mask = FindProperty("_Layer1_Mask", properties);
            _Layer1_Metallic = FindProperty("_Layer1_Metallic", properties);
            _Layer1_Smoothness = FindProperty("_Layer1_Smoothness", properties);

            _Splat1 = FindProperty("_Splat1", properties);
            _Layer2_BaseColor = FindProperty("_Layer2_BaseColor", properties);
            _Layer2_addColor = FindProperty("_Layer2_addColor", properties);
            _Layer2_Normal = FindProperty("_Layer2_Normal", properties);
            _Layer2_NormalIntensity = FindProperty("_Layer2_NormalIntensity", properties);
            _Layer2_Tilling = FindProperty("_Layer2_Tilling", properties);
            _Layer2_HeightContrast = FindProperty("_Layer2_HeightContrast", properties);
            _Layer2_Mask = FindProperty("_Layer2_Mask", properties);
            _Layer2_Metallic = FindProperty("_Layer2_Metallic", properties);
            _Layer2_Smoothness = FindProperty("_Layer2_Smoothness", properties);

            _Layer3Enable = FindProperty("_Layer3Enable", properties);
            _Splat2 = FindProperty("_Splat2", properties);
            _Layer3_BaseColor = FindProperty("_Layer3_BaseColor", properties);
            _Layer3_addColor = FindProperty("_Layer3_addColor", properties);
            _Layer3_Normal = FindProperty("_Layer3_Normal", properties);
            _Layer3_NormalIntensity = FindProperty("_Layer3_NormalIntensity", properties);
            _Layer3_Tilling = FindProperty("_Layer3_Tilling", properties);
            _Layer3_HeightContrast = FindProperty("_Layer3_HeightContrast", properties);
            _Layer3_Mask = FindProperty("_Layer3_Mask", properties);
            _Layer3_Metallic = FindProperty("_Layer3_Metallic", properties);
            _Layer3_Smoothness = FindProperty("_Layer3_Smoothness", properties);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperty(properties);
            m_MaterialEditor = materialEditor;

            var material = materialEditor.target as Material;
            if (material == null)
                throw new ArgumentNullException("material");

            // ---------------- Surface Settings ----------------
            _Surface_Foldout = Foldout(_Surface_Foldout, "Surface Settings(表面设置)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Surface_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_CullMode, new GUIContent("Cull Mode", "剔除模式"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_BlendContrast, new GUIContent("Blend Contrast", "混合过渡宽度，越小边界越锐利"));
                GUILayout.Space(5);
                // UV 采样方式：自定义下拉 + 手动托管 _SAMPLE_UV / _SAMPLE_WORLD 关键字
                int sampleIndex = Mathf.RoundToInt(_Sample.floatValue);
                int newSample = EditorGUILayout.IntPopup(
                    new GUIContent("UV Sample Mode", "UV：用网格 UV；World：用世界 XZ 平面投影（地形默认）"),
                    sampleIndex,
                    new GUIContent[] { new GUIContent("UV"), new GUIContent("World (默认)") },
                    new int[] { 0, 1 });
                if (newSample != sampleIndex) _Sample.floatValue = newSample;
                CoreUtils.SetKeyword(material, "_SAMPLE_UV", newSample == 0);
                CoreUtils.SetKeyword(material, "_SAMPLE_WORLD", newSample == 1);
                // World 采样专属参数，仅 World 模式显示
                if (newSample == 1)
                {
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_WorldUVScale, new GUIContent("World UV Scale", "世界平面采样缩放"));
                }
                GUILayout.Space(5);
                // Debug 模式：自定义下拉 + 手动托管 _DEBUG_* 关键字
                int debugIndex = Mathf.RoundToInt(_Debug.floatValue);
                int newDebug = EditorGUILayout.IntPopup(
                    new GUIContent("Debug Mode", "调试显示：关 / 顶点色 / 混合权重 / 世界法线"),
                    debugIndex,
                    new GUIContent[] {
                        new GUIContent("Off (默认)"),
                        new GUIContent("Vertex Color (顶点色)"),
                        new GUIContent("Weight (混合权重)"),
                        new GUIContent("Normal (世界法线)") },
                    new int[] { 0, 1, 2, 3 });
                if (newDebug != debugIndex) _Debug.floatValue = newDebug;
                CoreUtils.SetKeyword(material, "_DEBUG_OFF", newDebug == 0);
                CoreUtils.SetKeyword(material, "_DEBUG_VERTEXCOLOR", newDebug == 1);
                CoreUtils.SetKeyword(material, "_DEBUG_WEIGHT", newDebug == 2);
                CoreUtils.SetKeyword(material, "_DEBUG_NORMAL", newDebug == 3);
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // ---------------- Layer 1 ----------------
            _Layer1_Foldout = Foldout(_Layer1_Foldout, "Layer 1 (顶点色 R)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Layer1_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L1 Albedo (RGB)", "反照率 RGB"), _Splat0, _Layer1_BaseColor);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer1_addColor, new GUIContent("L1 Add Color", "叠加色"));
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L1 Normal Map", "法线贴图"), _Layer1_Normal, _Layer1_NormalIntensity);
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L1 Mask (R=M G=AO B=H A=Smooth)", "R=金属 G=遮蔽 B=高度 A=光滑度"), _Layer1_Mask);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer1_Tilling, new GUIContent("L1 Tilling", "平铺"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer1_HeightContrast, new GUIContent("L1 Height Contrast", "高度混合锐度(高度取自 Mask.B)"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer1_Metallic, new GUIContent("L1 Metallic", "该层金属度；未拖 Mask 时直接生效，有 Mask 时 × Mask.R"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer1_Smoothness, new GUIContent("L1 Smoothness", "该层光滑度；未拖 Mask 时直接生效，有 Mask 时 × Mask.A × 顶点色A(白=光滑)"));
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // ---------------- Layer 2 ----------------
            _Layer2_Foldout = Foldout(_Layer2_Foldout, "Layer 2 (顶点色 G)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Layer2_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L2 Albedo (RGB)", "反照率 RGB"), _Splat1, _Layer2_BaseColor);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer2_addColor, new GUIContent("L2 Add Color", "叠加色"));
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L2 Normal Map", "法线贴图"), _Layer2_Normal, _Layer2_NormalIntensity);
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L2 Mask (R=M G=AO B=H A=Smooth)", "R=金属 G=遮蔽 B=高度 A=光滑度"), _Layer2_Mask);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer2_Tilling, new GUIContent("L2 Tilling", "平铺"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer2_HeightContrast, new GUIContent("L2 Height Contrast", "高度混合锐度(高度取自 Mask.B)"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer2_Metallic, new GUIContent("L2 Metallic", "该层金属度；未拖 Mask 时直接生效，有 Mask 时 × Mask.R"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_Layer2_Smoothness, new GUIContent("L2 Smoothness", "该层光滑度；未拖 Mask 时直接生效，有 Mask 时 × Mask.A × 顶点色A(白=光滑)"));
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // ---------------- Layer 3 ----------------
            _Layer3_Foldout = Foldout(_Layer3_Foldout, "Layer 3 (顶点色 B)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Layer3_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                // [Toggle(_LAYER3)] 属性，ShaderProperty 自动托管 _LAYER3 keyword
                m_MaterialEditor.ShaderProperty(_Layer3Enable, new GUIContent("Enable Layer 3", "启用后读取顶点色 B 通道作为第三层权重"));
                if (_Layer3Enable.floatValue > 0.5f)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L3 Albedo (RGB)", "反照率 RGB"), _Splat2, _Layer3_BaseColor);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Layer3_addColor, new GUIContent("L3 Add Color", "叠加色"));
                    GUILayout.Space(5);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L3 Normal Map", "法线贴图"), _Layer3_Normal, _Layer3_NormalIntensity);
                    GUILayout.Space(5);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("L3 Mask (R=M G=AO B=H A=Smooth)", "R=金属 G=遮蔽 B=高度 A=光滑度"), _Layer3_Mask);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Layer3_Tilling, new GUIContent("L3 Tilling", "平铺"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Layer3_HeightContrast, new GUIContent("L3 Height Contrast", "高度混合锐度(高度取自 Mask.B)"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Layer3_Metallic, new GUIContent("L3 Metallic", "该层金属度；未拖 Mask 时直接生效，有 Mask 时 × Mask.R"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Layer3_Smoothness, new GUIContent("L3 Smoothness", "该层光滑度；未拖 Mask 时直接生效，有 Mask 时 × Mask.A × 顶点色A(白=光滑)"));
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                }
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
