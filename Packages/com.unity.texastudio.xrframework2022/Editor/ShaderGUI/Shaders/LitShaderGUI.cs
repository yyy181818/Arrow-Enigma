#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IKASAMADUS
{
    public class LitShaderGUI : ShaderGUI
    {
        private GUIStyle style = new GUIStyle();
        public GUILayoutOption[] shortButtonStyle = { GUILayout.Width(100) };
        private bool isAddedComponent = false;
        public bool setPlanaerShadowCaster = false;

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

        MaterialProperty _TransparentShadows = null;
        MaterialProperty _SpecularHighlights = null;
        MaterialProperty _EnvironmentReflections = null;
        MaterialProperty _ShadingModel = null;
        MaterialProperty _EnableSpecular = null;
        MaterialProperty _SurfaceType = null;
        MaterialProperty _SrcBlend = null;
        MaterialProperty _DstBlend = null;
        MaterialProperty _ZWrite = null;
        MaterialProperty _CullMode = null;
        MaterialProperty _AlphaClip = null;
        MaterialProperty _ZTestAddValue = null;
        MaterialProperty _ZTest = null;
        MaterialProperty _QueueOffset = null;

        MaterialProperty _StencilRef = null;
        MaterialProperty _StencilComp = null;
        MaterialProperty _StencilPass = null;
        MaterialProperty _StencilFail = null;
        MaterialProperty _StencilZFail = null;

        MaterialProperty _TextureMode = null;
        MaterialProperty _BaseMap = null;
        MaterialProperty _BaseColor = null;
        MaterialProperty _BaseColorStrength = null;
        MaterialProperty _Cutoff = null;
        private MaterialProperty _AlphaChannel = null;
        MaterialProperty _DirectStrength = null;
        MaterialProperty _InDirectStrength = null;
        private MaterialProperty _LightMapIntensity = null;
        MaterialProperty _Alpha = null;

        MaterialProperty _SpecularColor = null;
        MaterialProperty _SpecularIntensity = null;
        MaterialProperty _SpecularPower = null;
        MaterialProperty _CombineMap = null;
        MaterialProperty _MetallicTex = null;
        MaterialProperty _RoughnessTex = null;
        MaterialProperty _OcclusionMap = null;
        MaterialProperty _OcclusionStrength = null;
        MaterialProperty _Metallic = null;

        MaterialProperty _Roughness = null;

        MaterialProperty _BumpMap = null;
        MaterialProperty _BumpScale = null;

        MaterialProperty _EmissionMap = null;
        MaterialProperty _EmissionColor = null;
        MaterialProperty _EmissionChannel = null;
        MaterialProperty _EnableSampleRGB = null;

        MaterialProperty _PlanarShadowMode = null;
        MaterialProperty _ShadowColor = null;
        MaterialProperty _ShadowIntensity = null;
        MaterialProperty _ShadowFalloff = null;
        MaterialProperty _GlobalHeight = null;
        MaterialProperty _LocalHeight = null;
        MaterialProperty _ShadowOffset = null;

        MaterialProperty _DissolveEnable = null;
        MaterialProperty _DissolveMap = null;
        private MaterialProperty _DissolveColor = null;
        private MaterialProperty _DissolveAmount = null;
        private MaterialProperty _DissolveMinAmount = null;
        private MaterialProperty _DissolveMaxAmount = null;
        private MaterialProperty _DissolveAxis = null;
        private MaterialProperty _DissolveEdgeWidth = null;
        private MaterialProperty _GuideStrength = null;
        private MaterialProperty _UseTriplanar = null;
        private MaterialProperty _UVTilingTriplanar = null;
        private MaterialProperty _SampleMode = null;
        MaterialProperty _InvertDissolveMinMax = null;

        MaterialProperty _EdgesAffect = null;

        private MaterialProperty _EnableMainEdgePattern = null;
        private MaterialProperty _GuideAffectsEdgesBlending = null;
        private MaterialProperty _MainEdgePattern = null;
        private MaterialProperty _MainEdgePatternTiling = null;

        private MaterialProperty _MainEdgeColor2 = null;
        private MaterialProperty _MainEdgePatternSpeed = null;
        private MaterialProperty _MainEdgePatternStrength = null;
        private MaterialProperty _MainColorMode = null;
        private MaterialProperty _2SidesSecondEdge = null;
        private MaterialProperty _EnableSecondEdgePattern = null;
        private MaterialProperty _SecondColorMode = null;
        private MaterialProperty _SecondEdgePatternSpeed = null;
        private MaterialProperty _SecondEdgePatternStrength = null;
        private MaterialProperty _SecondEdgePattern = null;
        private MaterialProperty _SecondEdgePatternTiling = null;
        private MaterialProperty _SecondEdgeWidth = null;
        private MaterialProperty _SecondEdgeColor1 = null;
        private MaterialProperty _SecondEdgeColor2 = null;


        MaterialProperty _RimEnable = null;
        private MaterialProperty _RimColor = null;
        private MaterialProperty _RimPower = null;
        private MaterialProperty _RimScale = null;


        MaterialProperty _PlanarShadowEnable = null;

        MaterialProperty _SphereMaskEnable = null;
        MaterialProperty _MaskType = null;
        MaterialProperty _UseTextureMask = null;
        //MaterialProperty _CutoutBaseMap = null;
        //MaterialProperty _CutoutMap = null;
        //MaterialProperty _NoiseStrength = null;
        MaterialProperty _Invert = null;
        // MaterialProperty _Radius = null;
        // MaterialProperty _SmoothEdge = null;
        // MaterialProperty _EdgeSize = null;
        // MaterialProperty _EdgeIntensity = null;
        // MaterialProperty _NoiseWidth = null;
        // MaterialProperty _EdgeColor = null;
        // MaterialProperty _NoiseSpeed = null;

        MaterialProperty _ToonEnable = null;
        MaterialProperty _SmoothstepMin = null;
        MaterialProperty _SmoothstepMax = null;
        MaterialProperty _SmoothstepEdge = null;
        MaterialProperty _SmoothstepIntensity = null;
        MaterialProperty _OutlineColor = null;
        MaterialProperty _OutlineWidth = null;
        MaterialProperty _EnableSmoothNormals = null;

        MaterialProperty _EnableAcesToneMapping = null;
        MaterialProperty _EnableColorProcessing = null;
        MaterialProperty _Brightness = null;
        MaterialProperty _Contrast = null;
        MaterialProperty _Saturation = null;
        MaterialProperty _EnableHue = null;
        MaterialProperty _Hue = null;

        MaterialProperty _ReceiveShadows = null;

        MaterialProperty _EnableClipShadow = null;

        MaterialProperty _EnableClearCoat = null;
        MaterialProperty _ClearCoat = null;
        MaterialProperty _Iridescence = null;
        MaterialProperty _IridescenceThickness = null;

        MaterialProperty _TillingOffset = null;
        private MaterialProperty _AlphaToMask = null;

        private void FindProperty(MaterialProperty[] properties)
        {
            _TillingOffset = FindProperty("_TillingOffset", properties);
            _SpecularHighlights = FindProperty("_SpecularHighlights", properties);
            _EnvironmentReflections = FindProperty("_EnvironmentReflections", properties);
            _TransparentShadows = FindProperty("_TransparentShadows", properties);
            _ShadingModel = FindProperty("_ShadingModel", properties);
            _EnableSpecular = FindProperty("_EnableSpecular", properties);
            _SurfaceType = FindProperty("_Surface", properties);
            _SrcBlend = FindProperty("_SrcBlend", properties);
            _DstBlend = FindProperty("_DstBlend", properties);
            _ZWrite = FindProperty("_ZWrite", properties);
            _CullMode = FindProperty("_CullMode", properties);
            _AlphaClip = FindProperty("_AlphaClip", properties);
            _AlphaChannel = FindProperty("_AlphaChannel", properties);
            _Alpha = FindProperty("_Alpha", properties);
            _ZTest = FindProperty("_ZTest", properties);
            _AlphaToMask = FindProperty("_AlphaToMask", properties);
            _QueueOffset = FindProperty("_QueueOffset", properties);

            _StencilRef = FindProperty("_StencilRef", properties);
            _StencilComp = FindProperty("_StencilComp", properties);
            _StencilPass = FindProperty("_StencilPass", properties);
            _StencilFail = FindProperty("_StencilFail", properties);
            _StencilZFail = FindProperty("_StencilZFail", properties);

            _BaseMap = FindProperty("_BaseMap", properties);
            _BaseColor = FindProperty("_BaseColor", properties);
            _BaseColorStrength = FindProperty("_BaseColorStrength", properties);
            _Cutoff = FindProperty("_Cutoff", properties);
            _DirectStrength = FindProperty("_DirectStrength", properties);
            _InDirectStrength = FindProperty("_InDirectStrength", properties);
            _LightMapIntensity = FindProperty("_LightMapIntensity", properties);

            _SpecularColor = FindProperty("_SpecularColor", properties);
            _SpecularIntensity = FindProperty("_SpecularIntensity", properties);
            _SpecularPower = FindProperty("_SpecularPower", properties);
            _BumpMap = FindProperty("_BumpMap", properties);
            _TextureMode = FindProperty("_TextureMode", properties);
            _CombineMap = FindProperty("_CombineMap", properties);
            _MetallicTex = FindProperty("_MetallicTex", properties);
            _RoughnessTex = FindProperty("_RoughnessTex", properties);
            _OcclusionMap = FindProperty("_OcclusionMap", properties);
            _OcclusionStrength = FindProperty("_OcclusionStrength", properties);
            _Metallic = FindProperty("_Metallic", properties);
            _Roughness = FindProperty("_Roughness", properties);
            _BumpScale = FindProperty("_BumpScale", properties);

            _EmissionMap = FindProperty("_EmissionMap", properties);
            _EmissionColor = FindProperty("_EmissionColor", properties);
            _EmissionChannel = FindProperty("_EmissionChannel", properties);
            _EnableSampleRGB = FindProperty("_EnableSampleRGB", properties);

            _PlanarShadowMode = FindProperty("_PlanarShadowMode", properties);
            _ShadowColor = FindProperty("_ShadowColor", properties);
            _ShadowIntensity = FindProperty("_ShadowIntensity", properties);
            _ShadowFalloff = FindProperty("_ShadowFalloff", properties);
            _GlobalHeight = FindProperty("_GlobalHeight", properties);
            _LocalHeight = FindProperty("_LocalHeight", properties);
            _ShadowOffset = FindProperty("_ShadowOffset", properties);

            //ClearCoat
            _EnableClearCoat = FindProperty("_EnableClearCoat", properties);
            _ClearCoat = FindProperty("_ClearCoat", properties);
            _Iridescence = FindProperty("_Iridescence", properties);
            _IridescenceThickness = FindProperty("_IridescenceThickness", properties);

            //Features
            _DissolveEnable = FindProperty("_DissolveEnable", properties);
            _DissolveMap = FindProperty("_DissolveMap", properties);
            _DissolveColor = FindProperty("_DissolveColor", properties);
            _DissolveAmount = FindProperty("_DissolveAmount", properties);
            _DissolveMinAmount = FindProperty("_DissolveMinAmount", properties);
            _DissolveMaxAmount = FindProperty("_DissolveMaxAmount", properties);
            _DissolveEdgeWidth = FindProperty("_DissolveEdgeWidth", properties);
            _GuideStrength = FindProperty("_GuideStrength", properties);
            _UseTriplanar = FindProperty("_UseTriplanar", properties);
            _UVTilingTriplanar = FindProperty("_UVTilingTriplanar", properties);
            _DissolveAxis = FindProperty("_DissolveAxis", properties);
            _SampleMode = FindProperty("_SampleMode", properties);
            _InvertDissolveMinMax = FindProperty("_InvertDissolveMinMax", properties);
            _EdgesAffect = FindProperty("_EdgesAffect", properties);
            _EnableMainEdgePattern = FindProperty("_EnableMainEdgePattern", properties);
            _GuideAffectsEdgesBlending = FindProperty("_GuideAffectsEdgesBlending", properties);
            _MainEdgePattern = FindProperty("_MainEdgePattern", properties);
            _MainEdgePatternTiling = FindProperty("_MainEdgePatternTiling", properties);
            _MainEdgeColor2 = FindProperty("_MainEdgeColor2", properties);
            _MainColorMode = FindProperty("_MainColorMode", properties);
            _MainEdgePatternStrength = FindProperty("_MainEdgePatternStrength", properties);
            _MainEdgePatternSpeed = FindProperty("_MainEdgePatternSpeed", properties);
            _2SidesSecondEdge = FindProperty("_2SidesSecondEdge", properties);
            _EnableSecondEdgePattern = FindProperty("_EnableSecondEdgePattern", properties);
            _SecondColorMode = FindProperty("_SecondColorMode", properties);
            _SecondEdgePatternSpeed = FindProperty("_SecondEdgePatternSpeed", properties);
            _SecondEdgePatternStrength = FindProperty("_SecondEdgePatternStrength", properties);
            _SecondEdgePattern = FindProperty("_SecondEdgePattern", properties);
            _SecondEdgeWidth = FindProperty("_SecondEdgeWidth", properties);
            _SecondEdgePatternTiling = FindProperty("_SecondEdgePatternTiling", properties);
            _SecondEdgeColor1 = FindProperty("_SecondEdgeColor1", properties);
            _SecondEdgeColor2 = FindProperty("_SecondEdgeColor2", properties);

            _RimEnable = FindProperty("_RimEnable", properties);
            _RimColor = FindProperty("_RimColor", properties);
            _RimPower = FindProperty("_RimPower", properties);
            _RimScale = FindProperty("_RimScale", properties);

            _SphereMaskEnable = FindProperty("_SphereMaskEnable", properties);
            _PlanarShadowEnable = FindProperty("_PlanarShadowEnable", properties);
            _MaskType = FindProperty("_MaskType", properties);
            _UseTextureMask = FindProperty("_UseTextureMask", properties);
            //_CutoutBaseMap = FindProperty("_CutoutBaseMap", properties);
            //_CutoutMap = FindProperty("_CutoutMap", properties);
            //_NoiseStrength = FindProperty("_NoiseStrength", properties);
            _Invert = FindProperty("_Invert", properties);
            // _Radius = FindProperty("_Radius", properties);
            // _SmoothEdge = FindProperty("_SmoothEdge", properties);
            // _EdgeSize = FindProperty("_EdgeSize", properties);
            // _EdgeIntensity = FindProperty("_EdgeIntensity", properties);
            // _NoiseWidth = FindProperty("_NoiseWidth", properties);
            // _EdgeColor = FindProperty("_EdgeColor", properties);
            // _NoiseSpeed = FindProperty("_NoiseSpeed", properties);

            _ToonEnable = FindProperty("_ToonEnable", properties);
            _SmoothstepMin = FindProperty("_SmoothstepMin", properties);
            _SmoothstepMax = FindProperty("_SmoothstepMax", properties);
            _SmoothstepEdge = FindProperty("_SmoothstepEdge", properties);
            _SmoothstepIntensity = FindProperty("_SmoothstepIntensity", properties);
            _OutlineColor = FindProperty("_OutlineColor", properties);
            _OutlineWidth = FindProperty("_OutlineWidth", properties);
            _EnableSmoothNormals = FindProperty("_EnableSmoothNormals", properties);

            _EnableAcesToneMapping = FindProperty("_EnableAcesToneMapping", properties);
            _EnableColorProcessing = FindProperty("_EnableColorProcessing", properties);
            _Brightness = FindProperty("_Brightness", properties);
            _Contrast = FindProperty("_Contrast", properties);
            _Saturation = FindProperty("_Saturation", properties);
            _EnableHue = FindProperty("_EnableHue", properties);
            _Hue = FindProperty("_Hue", properties);

            _ReceiveShadows = FindProperty("_ReceiveShadows", properties);

            _EnableClipShadow = FindProperty("_EnableClipShadow", properties);
        }

        private static bool _Feature_Foldout = true;
        private static bool _Dissolve_Foldout = true;
        private static bool _Rim_Foldout = true;
        private static bool _SphereMask_Foldout = true;
        private static bool _PlanarShadow_Foldout = true;

        private static bool _Basic_Foldout = true;
        private static bool _Advanced_Foldout = true;
        private static bool _Emission_Foldout = true;
        private static bool _Matter_Foldout = true;
        private static bool _Base_Foldout = true;
        private static bool _Toon_Foldout = true;
        private static bool _Stencil_Foldout = true;
        private static int[] SurfaceValues = new int[] { 0, 1 };
        private static int[] MaskTypeValues = new int[] { 0, 1 };
        private bool isTransparent;
        private static GUIContent[] SurfaceContents = new GUIContent[]
            { new GUIContent("Opaque", "不透明"), new GUIContent("Transparent", "半透明") };

        private static GUIContent[] MaskTypeContents = new GUIContent[]
            { new GUIContent("Sphere", "球形"), new GUIContent("Plane", "平面") };

        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
        private static readonly int CombineMap = Shader.PropertyToID("_CombineMap");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

        private static int GetSurfaceValue(MaterialProperty prop)
        {
            var index = -1;
            if (prop == null) return index;
            for (var i = 0; i < SurfaceValues.Length; i++)
            {
                if (Math.Abs(prop.floatValue - SurfaceValues[i]) < 1)
                {
                    index = i;
                }
            }

            if (index != -1) return index;
            prop.floatValue = SurfaceValues[1];
            index = 1;

            return index;
        }

        private static int GetMaskValue(MaterialProperty prop)
        {
            var index = -1;
            if (prop == null) return index;
            for (var i = 0; i < MaskTypeValues.Length; i++)
            {
                if (Math.Abs(prop.floatValue - MaskTypeValues[i]) < 1)
                {
                    index = i;
                }
            }

            if (index != -1) return index;
            prop.floatValue = MaskTypeValues[1];
            index = 1;

            return index;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperty(properties);

            m_MaterialEditor = materialEditor;

            var material = materialEditor.target as Material;
            if (material == null)
                throw new ArgumentNullException("material");

            _Feature_Foldout = Foldout(_Feature_Foldout, "Feature(功能定制)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Feature_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_DissolveEnable, "Dissolve(溶解)");
                CoreUtils.SetKeyword(material, "_DISSOLVE_ENABLE", _DissolveEnable.floatValue > 0);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_RimEnable, "Rim(边缘光)");
                CoreUtils.SetKeyword(material, "_RIM_ENABLE", _RimEnable.floatValue > 0);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_SphereMaskEnable, "Sphere Mask(球形遮罩)");
                CoreUtils.SetKeyword(material, "_SPHEREMASK_ENABLE", _SphereMaskEnable.floatValue > 0);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_PlanarShadowEnable, "Planar Shadow(平面阴影)");
                if (_PlanarShadowEnable.floatValue == 0)
                {
                    if (material) material.SetShaderPassEnabled("UniversalForwardOnly", false);
                    //if (material) material.SetShaderPassEnabled("PlanarShadowPass", false);
                    if (material) CoreUtils.SetKeyword(material, "_PLANARSHADOW_ENABLE", false);
                }
                else
                {
                    if (material) material.SetShaderPassEnabled("UniversalForwardOnly", true);
                    //if (material != null) material.SetShaderPassEnabled("PlanarShadowPass", true);
                    if (material) CoreUtils.SetKeyword(material, "_PLANARSHADOW_ENABLE", true);
                }

                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_ToonEnable, "ToonShading(卡通着色)");
                if (_ToonEnable.floatValue == 0)
                {
                    if (material) material.SetShaderPassEnabled("SRPDefaultUnlit", false);
                    //if (material != null) material.SetShaderPassEnabled("ToonPass", false);
                    CoreUtils.SetKeyword(material, "_TOON_ENABLE", false);
                }
                else
                {
                    if (material) material.SetShaderPassEnabled("SRPDefaultUnlit", true);
                    //if (material != null) material.SetShaderPassEnabled("ToonPass", true);
                    CoreUtils.SetKeyword(material, "_TOON_ENABLE", true);
                }

                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            _Basic_Foldout = Foldout(_Basic_Foldout, "Basic(基础设置)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Basic_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_ShadingModel, new GUIContent("Shading Model", "着色模式"));

                if (Math.Abs(_ShadingModel.floatValue - 1) < 1)
                {
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_EnableSpecular, new GUIContent("Enable Specular", "开启高光"));
                    CoreUtils.SetKeyword(material, "_ENABLE_SPECULAR", _EnableSpecular.floatValue > 0);
                }

                GUILayout.Space(5);
                // var index = GetSurfaceValue(_SurfaceType);
                // var num = EditorGUILayout.IntPopup(new GUIContent("Surface Type", "表面类型"), index, SurfaceContents,
                //     SurfaceValues);
                // if (num != index)
                // {
                //     _SurfaceType.floatValue = SurfaceValues[num];
                // }
                // if (_SurfaceType.floatValue == 0)
                // {
                //     _SrcBlend.floatValue = 1;
                //     _DstBlend.floatValue = 0;
                //     if (material != null)
                //     {
                //         material.SetOverrideTag("RenderType", "Opaque");
                //         material.renderQueue = (int)RenderQueue.Geometry + (int)_QueueOffset.floatValue;
                //         material.SetShaderPassEnabled("ShadowCaster", true);
                //         material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                //         material.DisableKeyword("_ALPHAMODULATE_ON");
                //     }
                // }
                // else
                // {
                //     _SrcBlend.floatValue = 5;
                //     _DstBlend.floatValue = 10;
                //     if (material != null)
                //     {
                //         material.SetOverrideTag("RenderType", "Transparent");
                //         material.renderQueue = (int)RenderQueue.Transparent + (int)_QueueOffset.floatValue;
                //         material.SetShaderPassEnabled("ShadowCaster",
                //             Math.Abs(material.GetFloat("_TransparentShadows") - 1) < 1);
                //         material.SetFloat("_AlphaToMask", 0);
                //     }
                // }
                bool alphaClip = false;
                if (material.HasProperty("_AlphaClip"))
                    alphaClip = material.GetFloat("_AlphaClip") >= 0.5;
                CoreUtils.SetKeyword(material, "_ALPHATEST_ON", alphaClip);
                if (material.HasProperty("_Surface"))
                {
                    var index = GetSurfaceValue(_SurfaceType);
                    var num = EditorGUILayout.IntPopup(new GUIContent("Surface Type", "表面类型"), index, SurfaceContents,
                        SurfaceValues);
                    // float surfaceType = material.GetFloat(propertyID);
                    // if (surfaceType >= 1.0f)
                    // {
                    //     material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    // }
                    // else
                    // {
                    //     material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    // }
                    if (num != index)
                    {
                        _SurfaceType.floatValue = SurfaceValues[num];
                    }

                    //SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
                    CoreUtils.SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT",
                        Mathf.Approximately(_SurfaceType.floatValue, (int)SurfaceType.Transparent));
                    bool alphaToMask = false;
                    if (num == (int)SurfaceType.Opaque)
                    {
                        if (alphaClip)
                        {
                            material.renderQueue = (int)RenderQueue.AlphaTest + + (int)_QueueOffset.floatValue;
                            material.SetOverrideTag("RenderType", "TransparentCutout");
                            alphaToMask = true;
                        }
                        else
                        {
                            material.renderQueue = (int)RenderQueue.Geometry + + (int)_QueueOffset.floatValue;
                            material.SetOverrideTag("RenderType", "Opaque");
                        }

                        _SrcBlend.floatValue = 1;
                        _DstBlend.floatValue = 0;
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.DisableKeyword("_ALPHAMODULATE_ON");
                        isTransparent = false;
                    }
                    else
                    {
                        _SrcBlend.floatValue = 5;
                        _DstBlend.floatValue = 10;

                        material.SetOverrideTag("RenderType", "Transparent");
                        material.renderQueue = (int)RenderQueue.Transparent + (int)_QueueOffset.floatValue;
                        material.SetShaderPassEnabled("ShadowCaster",
                            Math.Abs(material.GetFloat("_TransparentShadows") - 1) < 1);
                        //material.SetFloat("_AlphaToMask", 0);
                        isTransparent = true;
                    }
                    if (material.HasProperty("AlphaToMask"))
                    {
                        material.SetFloat("AlphaToMask", alphaToMask ? 1.0f : 0.0f);
                    }
                }
                else
                {
                    // no surface type property -- must be hard-coded by the shadergraph,
                    // so ensure the pass is enabled at the material level
                    material.SetShaderPassEnabled("DepthOnly", true);
                }

                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_CullMode, new GUIContent("Cull Mode", "剔除模式"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_ZWrite, new GUIContent("ZWrite", "深度写入"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_ZTest, new GUIContent("ZTest", "深度测试"));
                GUILayout.Space(5);
                if (Math.Abs(_SurfaceType.floatValue - 1) < 1)
                {
                    m_MaterialEditor.ShaderProperty(_Alpha, new GUIContent("Alpha", "透明度"));
                    GUILayout.Space(5);
                }

                if (isTransparent)
                {
                    m_MaterialEditor.ShaderProperty(_AlphaChannel,new GUIContent("Alpha Channel","透明通道"));
                    GUILayout.Space(5);
                }
                m_MaterialEditor.ShaderProperty(_AlphaClip, new GUIContent("Alpha Clip", "透明度裁剪"));
                if (_AlphaClip.floatValue > 0)
                {
                    CoreUtils.SetKeyword(material, "_ALPHATEST_ON", _AlphaClip.floatValue > 0);
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.renderQueue = (int)RenderQueue.AlphaTest + (int)_QueueOffset.floatValue;
                    material.SetFloat("_AlphaToMask", 1);
                    material.SetShaderPassEnabled("ShadowCaster", true);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Cutoff, new GUIContent("Cut off", "裁剪值"));
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                }

                //GUILayout.Space(5);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                m_MaterialEditor.VectorProperty(_TillingOffset, new GUIContent("Tilling Offset"));
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();


            _Base_Foldout = Foldout(_Base_Foldout, "Base Color(基础颜色) && Specular(高光)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Base_Foldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Base Map"), _BaseMap, _BaseColor);
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_SpecularColor, new GUIContent("Specular Color", "高光颜色"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_SpecularPower, new GUIContent("Specular Power", "高光边缘范围"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_SpecularIntensity, new GUIContent("Specular Intensity", "高光强度"));
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(_BaseColorStrength, new GUIContent("BaseColor Strength", "颜色强度"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_DirectStrength, new GUIContent("Direct Strength", "直接光强度"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_InDirectStrength, new GUIContent("InDirectLight Strength", "间接光强度"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_LightMapIntensity, new GUIContent("LightMap Strength", "烘焙光照强度"));
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            _Matter_Foldout = Foldout(_Matter_Foldout, "Normal & Metallic & Roughness(法线&金属度&粗糙度)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Matter_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_TextureMode, new GUIContent("贴图采样模式", "0:使用合并贴图 1:使用单个贴图 2:不采样"));
                switch (_TextureMode.floatValue)
                {
                    case 2:
                    {
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), _BumpMap);
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map"), _MetallicTex);
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Roughness Map"), _RoughnessTex);
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Occlusion Map"),
                            _OcclusionMap);
                        if (material != null && material.HasProperty("_BumpMap"))
                            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture(BumpMap));
                        if (material != null && material.HasProperty("_OcclusionMap"))
                        {
                            CoreUtils.SetKeyword(material, "_OCCLUSIONMAP",
                                material.GetTexture("_OcclusionMap"));
                        }

                        _CombineMap.textureValue = null;
                        break;
                    }
                    case 0:
                    {
                        GUILayout.Space(5);
                        m_MaterialEditor.TexturePropertySingleLine(
                            new GUIContent("Combine Map", "xy法线 z金属度 w粗糙度(AO=BaseMap.a)"),
                            _CombineMap);
                        if (material && material.HasProperty("_CombineMap"))
                            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture(CombineMap));
                        if (material != null && material.HasProperty("_BaseMap"))
                        {
                            CoreUtils.SetKeyword(material, "_OCCLUSIONMAP",
                                material.GetTexture("_BaseMap"));
                        }

                        _MetallicTex.textureValue = null;
                        _RoughnessTex.textureValue = null;
                        _OcclusionMap.textureValue = null;
                        GUILayout.Space(5);
                        break;
                    }
                    case 1:
                    {
                        GUILayout.Space(5);
                        m_MaterialEditor.TexturePropertySingleLine(
                            new GUIContent("Combine Map", "x金属度 y粗糙度 zAO(关闭SRGB！！)"),
                            _CombineMap);
                        GUILayout.Space(5);
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), _BumpMap);
                        if (material != null && material.HasProperty("_BumpMap"))
                            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture(BumpMap));
                        if (material != null && material.HasProperty("_CombineMap"))
                        {
                            CoreUtils.SetKeyword(material, "_OCCLUSIONMAP",
                                material.GetTexture("_CombineMap"));
                        }

                        _MetallicTex.textureValue = null;
                        _RoughnessTex.textureValue = null;
                        _OcclusionMap.textureValue = null;
                        GUILayout.Space(5);
                        break;
                    }
                    case 3:
                        _CombineMap.textureValue = null;
                        _BumpMap.textureValue = null;
                        _MetallicTex.textureValue = null;
                        _RoughnessTex.textureValue = null;
                        _OcclusionMap.textureValue = null;
                        CoreUtils.SetKeyword(material, "_NORMALMAP", false);
                        break;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(_Metallic, new GUIContent("Metallic", "金属度"));
                m_MaterialEditor.ShaderProperty(_Roughness, new GUIContent("Roughness", "粗糙度"));
                m_MaterialEditor.ShaderProperty(_BumpScale, new GUIContent("Normal Scale", "法线强度"));
                m_MaterialEditor.ShaderProperty(_OcclusionStrength, new GUIContent("Occlusion Strength", "AO强度"));
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();


            _Emission_Foldout = Foldout(_Emission_Foldout, "Emission(自发光)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Emission_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_EnableSampleRGB, new GUIContent("Enable Sample RGB", "开启采样RGB通道"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_EmissionChannel, new GUIContent("Emission Channel", "自发光通道"));
                GUILayout.Space(5);
                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), _EmissionMap,
                    _EmissionColor);
                if (material != null && material.HasProperty("_EmissionMap"))
                    CoreUtils.SetKeyword(material, "_EMISSION", material.GetTexture(EmissionMap));
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            if (Math.Abs(_PlanarShadowEnable.floatValue - 1) < 1)
            {
                _PlanarShadow_Foldout = Foldout(_PlanarShadow_Foldout, "Planar Shadow(平面阴影)");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_PlanarShadow_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_PlanarShadowMode, new GUIContent("Planar Shadow Mode", "平面阴影模式"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_ShadowColor, new GUIContent("Shadow Color", "阴影颜色"));
                    GUILayout.Space(5);
                    switch (_PlanarShadowMode.floatValue)
                    {
                        case 0:
                            setPlanaerShadowCaster = false;
                            m_MaterialEditor.ShaderProperty(_ShadowFalloff, new GUIContent("Shadow Falloff", "阴影衰减"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_GlobalHeight, new GUIContent("Ground Height", "地面高度"));
                            GUILayout.Space(5);
                            break;
                        case 1:
                            setPlanaerShadowCaster = false;
                            m_MaterialEditor.ShaderProperty(_LocalHeight, new GUIContent("Local Height", "阴影高度"));
                            GUILayout.Space(5);
                            // m_MaterialEditor.ShaderProperty(_GlobalHeight, new GUIContent("Ground Height", "地面高度"));
                            // GUILayout.Space(5);
                            break;
                        case 2:
                            setPlanaerShadowCaster = true;
                            m_MaterialEditor.ShaderProperty(_ShadowIntensity,
                                new GUIContent("Shadow Intensity", "阴影强度"));
                            GUILayout.Space(5);
                            break;
                    }

                    if (true)
                    {
                        var obj = Selection.activeGameObject;
                        if (obj != null)
                        {
                            if (_PlanarShadowMode.floatValue is 2 && !isAddedComponent)
                            {
                                var objs = obj.GetComponents<PlanaerShadowCaster>();
                                if (objs.Length > 0)
                                {
                                    for (var i = 0; i < objs.Length - 1; i++)
                                    {
                                        UnityEngine.Object.DestroyImmediate(objs[i]);
                                    }

                                    isAddedComponent = true;
                                }
                                else
                                {
                                    obj.AddComponent<PlanaerShadowCaster>();
                                    isAddedComponent = true;
                                }
                            }

                            if (_PlanarShadowMode.floatValue is not 2 && isAddedComponent)
                            {
                                var objs = obj.GetComponents<PlanaerShadowCaster>();
                                foreach (var Object in objs)
                                {
                                    UnityEngine.Object.DestroyImmediate(Object);
                                }

                                isAddedComponent = false;
                            }
                        }
                    }

                    m_MaterialEditor.ShaderProperty(_ShadowOffset, new GUIContent("Shadow Offset", "阴影偏移"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_EnableClipShadow,
                        new GUIContent("Enable Clip Shadow", "开启裁剪阴影(当相机高度低于阴影高度时,阴影将被裁剪)"));
                    GUILayout.Space(5);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }

            if (Math.Abs(_DissolveEnable.floatValue - 1) < 1)
            {
                _Dissolve_Foldout = Foldout(_Dissolve_Foldout, "Dissolve(溶解)");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_Dissolve_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Dissolve Map"), _DissolveMap,
                        _DissolveColor);
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.TextureScaleOffsetProperty(_DissolveMap);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(_EdgesAffect, new GUIContent("Edges Affection", "边缘影响"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_DissolveAxis, new GUIContent("Dissolve Axis", "溶解轴"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_SampleMode, new GUIContent("Sample Mode", "溶解UV采样模式"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_DissolveAmount, new GUIContent("Dissolve Amount", "溶解强度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_InvertDissolveMinMax,
                        new GUIContent("Invert Dissolve MinMax", "反转溶解最小最大值"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_DissolveMinAmount,
                        new GUIContent("Dissolve Min Amount", "溶解最小强度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_DissolveMaxAmount,
                        new GUIContent("Dissolve Max Amount", "溶解最大强度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_DissolveEdgeWidth,
                        new GUIContent("Dissolve Edge Width", "溶解边缘宽度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_GuideStrength, new GUIContent("Guide Adjuster", "溶解边缘调整"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_UseTriplanar, new GUIContent("Use Triplanar", "使用三轴投影"));
                    GUILayout.Space(5);
                    if (Math.Abs(_UseTriplanar.floatValue - 1) < 1)
                    {
                        m_MaterialEditor.ShaderProperty(_UVTilingTriplanar,
                            new GUIContent("UV Tiling Triplanar", "三轴投影UV缩放"));
                        GUILayout.Space(5);
                    }

                    m_MaterialEditor.ShaderProperty(_EnableMainEdgePattern,
                        new GUIContent("Enable MainEdgePattern", "开启采样遮罩图并混合颜色"));
                    GUILayout.Space(5);
                    if (Math.Abs(_EnableMainEdgePattern.floatValue - 1) < 1)
                    {
                        if (material != null) material.EnableKeyword("_ENALBE_MAINEDGEPATTERN");
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("MainEdge Pattern"),
                            _MainEdgePattern);
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_MainColorMode, new GUIContent("MainColor Mode", "颜色模式"));
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_MainEdgePatternTiling, new GUIContent("Main Tiling"));
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_MainEdgePatternSpeed, new GUIContent("Speed", "TilingSpeed"));
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_MainEdgeColor2,
                            new GUIContent("MainEdge Color2", "溶解颜色2(混合主溶解颜色)"));
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_MainEdgePatternStrength,
                            new GUIContent("Pattern Power", "遮罩强度"));
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_EnableSecondEdgePattern,
                            new GUIContent("Enable SecondEdgePattern", "开启采样第二层遮罩图并混合颜色"));
                        GUILayout.Space(5);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                        if (Math.Abs(_EnableSecondEdgePattern.floatValue - 1) < 1)
                        {
                            if (material != null) material.EnableKeyword("_ENALBE_SECONDEDGEPATTERN");
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("SecondEdge Pattern"),
                                _SecondEdgePattern);
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_GuideAffectsEdgesBlending,
                                new GUIContent("Edges Blending", "边缘混合"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_2SidesSecondEdge, new GUIContent("two Sides", "双面边缘"));
                            CoreUtils.SetKeyword(material, "_2SIDESSECONDEDGE_ON", _2SidesSecondEdge.floatValue > 0);
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondColorMode,
                                new GUIContent("SecondColor Mode", "第二层颜色模式"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondEdgePatternTiling, new GUIContent("Second Tiling"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondEdgePatternSpeed,
                                new GUIContent("Speed", "TilingSpeed"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondEdgeWidth,
                                new GUIContent("SecondEdge Width", "第二层溶解宽度"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondEdgeColor1,
                                new GUIContent("SecondEdge Color1", "溶解颜色1"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondEdgeColor2,
                                new GUIContent("SecondEdge Color2", "溶解颜色2"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_SecondEdgePatternStrength,
                                new GUIContent("Pattern Power", "遮罩强度"));
                            GUILayout.Space(5);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);
                        }
                        else
                        {
                            material.DisableKeyword("_ENALBE_SECONDEDGEPATTERN");
                        }
                    }
                    else
                    {
                        if (material != null) material.DisableKeyword("_ENALBE_MAINEDGEPATTERN");
                        material.DisableKeyword("_ENALBE_SECONDEDGEPATTERN");
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            // else
            // {
            //     _DissolveMap.textureValue = null;
            //     _DissolveColor.colorValue = Color.white;
            //     _DissolveAmount.floatValue = 0;
            //     _DissolveEdgeWidth.floatValue = 0.1f;
            //     _GuideAdjuster.floatValue = 1f;
            //     _UseTriplanar.floatValue = 0;
            //     _UVTilingTriplanar.floatValue = 1f;
            // }
            if (Mathf.Approximately(_RimEnable.floatValue, 1))
            {
                _Rim_Foldout = Foldout(_Rim_Foldout, "Rim(边缘光)");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_Dissolve_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_RimColor, new GUIContent("Rim Color", "边缘光颜色"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_RimPower, new GUIContent("Rim Power", "边缘光强度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_RimScale, new GUIContent("Rim Scale", "边缘光大小"));
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            if (Mathf.Approximately(_SphereMaskEnable.floatValue, 1))
            {
                _SphereMask_Foldout = Foldout(_SphereMask_Foldout, "Sphere Mask(球面遮罩)");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_SphereMask_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    int maskindex = GetSurfaceValue(_MaskType);
                    int maskNum = EditorGUILayout.IntPopup(new GUIContent("SphereMask Type", "球形遮罩类型"), maskindex,
                        MaskTypeContents,
                        MaskTypeValues);
                    if (maskNum != maskindex)
                    {
                        _MaskType.floatValue = MaskTypeValues[maskNum];
                    }

                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_UseTextureMask,
                        new GUIContent("Use Texture Mask", "使用第二张贴图作为Cutout后的BaseMap"));
                    //GUILayout.Space(5);
                    // if (Mathf.Approximately(_UseTextureMask.floatValue, 1))
                    // {
                    //     m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Cutout BaseMap"), _CutoutBaseMap);
                    // }

                    GUILayout.Space(5);
                    
                    m_MaterialEditor.ShaderProperty(_Invert, new GUIContent("Invert", "反转"));

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            if (Math.Abs(_ToonEnable.floatValue - 1) < 1)
            {
                _Toon_Foldout = Foldout(_Toon_Foldout, "ToonShading(卡通着色)");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (_Toon_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_SmoothstepMin, new GUIContent("Smoothstep Min", "Smoothstep最小值"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_SmoothstepMax, new GUIContent("Smoothstep Max", "Smoothstep最大值"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_SmoothstepEdge, new GUIContent("Smoothstep Edge", "Smoothstep边缘"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_SmoothstepIntensity,
                        new GUIContent("Smoothstep Intensity", "Smoothstep强度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_EnableSmoothNormals,
                        new GUIContent("Enable Smooth Normals", "开启平滑法线"));
                    //material.EnableKeyword("_ENABLE_SMOOTHNORMALS");
                    CoreUtils.SetKeyword(material, "_ENABLE_SMOOTHNORMALS",
                        Math.Abs(_EnableSmoothNormals.floatValue - 1) < 1);

                    //material.DisableKeyword("_ENABLE_SMOOTHNORMALS");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_OutlineWidth, new GUIContent("Outline Width", "描边宽度"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_OutlineColor, new GUIContent("Outline Color", "描边颜色"));
                    GUILayout.Space(5);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            _Stencil_Foldout = Foldout(_Stencil_Foldout, "Stencil(模板测试)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Stencil_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_StencilRef, new GUIContent("Stencil Ref", "模板参考值"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_StencilComp, new GUIContent("Stencil Compare Function", "模板比较函数"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_StencilPass, new GUIContent("Stencil Pass Op", "模板通过操作"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_StencilFail, new GUIContent("Stencil Fail Op", "模板失败操作"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_StencilZFail, new GUIContent("Stencil ZFail Op", "模板深度失败操作"));
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _Advanced_Foldout = Foldout(_Advanced_Foldout, "Advanced Settings");
            if (_Advanced_Foldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_QueueOffset, new GUIContent("RenderQueue Offset", "渲染队列偏移"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_SpecularHighlights, new GUIContent("Specular Highlights", "高光"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_EnvironmentReflections,
                    new GUIContent("Environment Reflections", "环境反射"));
                if (material.HasProperty("_EnvironmentReflections"))
                    CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
                        material.GetFloat("_EnvironmentReflections") == 0.0f);
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_ReceiveShadows, new GUIContent("Receive Shadows", "接收阴影"));
                GUILayout.Space(5);
                // Receive Shadows
                if (material.HasProperty("_ReceiveShadows"))
                    CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", _ReceiveShadows.floatValue == 0.0f);
                m_MaterialEditor.ShaderProperty(_EnableAcesToneMapping,
                    new GUIContent("Enable ACES ToneMapping", "开启ACES色调映射"));
                //material.EnableKeyword("_ENABLE_TONEMAP");
                CoreUtils.SetKeyword(material, "_ENABLE_TONEMAP", Math.Abs(_EnableAcesToneMapping.floatValue - 1) < 1);

                //material.DisableKeyword("_ENABLE_TONEMAP");
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_EnableColorProcessing,
                    new GUIContent("Enable Color Processing", "开启色彩处理"));
                GUILayout.Space(5);
                if (Math.Abs(_EnableColorProcessing.floatValue - 1) < 1)
                {
                    //material.EnableKeyword("_ENABLE_COLOR_PROCESSING");
                    CoreUtils.SetKeyword(material, "_ENABLE_COLOR_PROCESSING", true);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(_Brightness, new GUIContent("Brightness", "亮度"));
                    m_MaterialEditor.ShaderProperty(_Contrast, new GUIContent("Contrast", "对比度"));
                    m_MaterialEditor.ShaderProperty(_Saturation, new GUIContent("Saturation", "饱和度"));
                    m_MaterialEditor.ShaderProperty(_EnableHue, new GUIContent("Enable Hue", "开启色调"));
                    if (Math.Abs(_EnableHue.floatValue - 1) < 1)
                    {
                        m_MaterialEditor.ShaderProperty(_Hue, new GUIContent("Hue", "色调"));
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    //material.DisableKeyword("_ENABLE_COLOR_PROCESSING");
                    CoreUtils.SetKeyword(material, "_ENABLE_COLOR_PROCESSING", false);
                }

                m_MaterialEditor.ShaderProperty(_TransparentShadows, new GUIContent("Transparent Shadows", "开启半透明阴影"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_EnableClearCoat, new GUIContent("Enable ClearCoat", "开启ClearCoat"));
                CoreUtils.SetKeyword(material, "_ENABLE_CLEARCOAT_ON", _EnableClearCoat.floatValue > 0);
                if (Math.Abs(_EnableClearCoat.floatValue - 1) < 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_ClearCoat, new GUIContent("ClearCoat", "薄膜反射率"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_Iridescence, new GUIContent("Iridescence", "虹光"));
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_IridescenceThickness,
                        new GUIContent("Iridescence Thickness", "虹光厚度"));
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);
                Common(material);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            return;


            void Common(Material mat)
            {
                EditorGUI.BeginChangeCheck();
                {
                    MaterialProperty[] props = { };
                    base.OnGUI(m_MaterialEditor, props);
                }
            }
        }
    }
}
#endif