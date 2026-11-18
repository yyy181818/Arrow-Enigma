#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace IKASAMADUS
{
    public class FX_ShaderGUI : ShaderGUI
    {
        public GUIStyle style = new GUIStyle();
        public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[] { GUILayout.Width(100) };

        static bool Foldout(bool display, string title)
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

        MaterialEditor m_MaterialEditor;


        private static bool _Base_Foldout = true;
        private static bool _Main_Foldout = true;

        private static bool _Feature_Foldout = true;
        private static bool _Mask_Foldout = true;
        private static bool _Dissolve_Foldout = true;
        private static bool _Fresnel_Foldout = true;
        private static bool _Distortion_Foldout = true;
        private static bool _VertexOffset_Foldout = true;
        private static bool _Stencil_Foldout = true;
        private static bool _Common_Foldout = true;
        private static bool _SoftParticle_Foldout = true;
        private static bool _WorldMask_Foldout = true;

        #region MaterialProperties

        //Base
        MaterialProperty BlendMode = null;
        MaterialProperty CullMode = null;
        MaterialProperty ZTest = null;
        MaterialProperty ZWrite = null;
        MaterialProperty _BlendOp = null;
        MaterialProperty _SrcBlend = null;
        MaterialProperty _DstBlend = null;
        MaterialProperty _AlphaTest = null;
        MaterialProperty _AlphaCutoff = null;

        //
        MaterialProperty _ColorProcessEnable = null;
        MaterialProperty _DissolveEnable = null;
        MaterialProperty _DistortionEnable = null;
        MaterialProperty _VertexOffsetEnable = null;
        MaterialProperty _VertexMaskEnable = null;
        MaterialProperty _FresnelEnable = null;
        MaterialProperty _MaskEnable = null;
        MaterialProperty _MainPolarEnable = null;
        MaterialProperty _SoftParticles = null;
        MaterialProperty _SoftParticleValue = null;

        //Toggle
        MaterialProperty _BaseDetailToggle = null;
        MaterialProperty _MaskDetailToggle = null;
        MaterialProperty _BaseRtoAToggle = null;
        MaterialProperty _MaskRtoAToggle = null;
        MaterialProperty _ColorMask = null;
        MaterialProperty _DissolveDetailToggle = null;
        MaterialProperty _DistortionDetailToggle = null;
        MaterialProperty _DissolveRtoAToggle = null;
        MaterialProperty _DistortionRtoAToggle = null;
        MaterialProperty _RemapToggle = null;
        MaterialProperty _VertexOffsetRtoAToggle = null;
        MaterialProperty _VertexMaskRtoAToggle = null;
        MaterialProperty _VertexOffsetUMirrorToggle = null;
        MaterialProperty _VertexOffsetVMirrorToggle = null;
        MaterialProperty _VertexMaskUMirrorToggle = null;
        MaterialProperty _VertexMaskVMirrorToggle = null;
        MaterialProperty _VertexOffsetUClampToggle = null;
        MaterialProperty _VertexOffsetVClampToggle = null;
        MaterialProperty _VertexMaskUClampToggle = null;
        MaterialProperty _VertexMaskVClampToggle = null;
        MaterialProperty _VertexOffsetToggle = null;
        MaterialProperty _VertexMaskToggle = null;
        MaterialProperty _FresneltoAlphaToggle = null;
        MaterialProperty _InvFresnelToggle = null;
        MaterialProperty _StencilToggle = null;

        MaterialProperty _DissolveColorToggle = null;

        //
        MaterialProperty _Stencil = null;
        MaterialProperty _StencilComp = null;
        MaterialProperty _StencilFail = null;

        MaterialProperty _StencilPass = null;

        //
        MaterialProperty _CustomMainOffset = null;
        MaterialProperty _CustomDissolveValue = null;
        MaterialProperty _CustomDistortionStrength = null;
        MaterialProperty _CustomMaskOffset = null;
        MaterialProperty _CustomPolarOffset = null;

        //Properties
        //Main
        MaterialProperty _BaseMap = null;
        MaterialProperty _BaseColor = null;
        MaterialProperty _Rotation = null;
        MaterialProperty _Saturation = null;
        MaterialProperty _GrayScale = null;
        MaterialProperty _Offset = null;
        MaterialProperty _Brightness = null;
        MaterialProperty _Dispersion = null;
        MaterialProperty _Contrast = null;
        MaterialProperty _HueOffset = null;
        MaterialProperty _BaseUVX = null;
        MaterialProperty _BaseUVY = null;
        MaterialProperty _Alpha = null;
        MaterialProperty _ClampRepeat = null;
        MaterialProperty _UClamp = null;
        MaterialProperty _VClamp = null;
        MaterialProperty _UMirror = null;

        MaterialProperty _VMirror = null;

        //Mask
        MaterialProperty _MaskMap = null;
        MaterialProperty _MaskColor = null;
        MaterialProperty _MaskUVX = null;
        MaterialProperty _MaskUVY = null;
        MaterialProperty _MaskUMirror = null;
        MaterialProperty _MaskVMirror = null;
        MaterialProperty _MaskRotation = null;
        MaterialProperty _MaskStrength = null;

        //Dissolve
        MaterialProperty _DissolveMap = null;
        MaterialProperty _DissolveValue = null;
        MaterialProperty _DissolveColor = null;
        MaterialProperty _DissolveUVX = null;
        MaterialProperty _DissolveUVY = null;
        MaterialProperty _DissolveWidth = null;
        MaterialProperty _DissolveHardness = null;
        MaterialProperty _DissolveRotation = null;
        MaterialProperty _DissolveUMirror = null;
        MaterialProperty _DissolveVMirror = null;

        //Distortion
        MaterialProperty _DistortionMap = null;
        MaterialProperty _DistortionUStrength = null;
        MaterialProperty _DistortionVStrength = null;
        MaterialProperty _DistortionUVX = null;
        MaterialProperty _DistortionUVY = null;
        MaterialProperty _DistortionRotation = null;
        MaterialProperty _DistortionUMirror = null;
        MaterialProperty _DistortionVMirror = null;

        //
        MaterialProperty _VertexOffsetMap = null;
        MaterialProperty _VertexOffsetStrength = null;
        MaterialProperty _VetexOffsetPower = null;
        MaterialProperty _VertexOffsetUVX = null;
        MaterialProperty _VertexOffsetUVY = null;
        MaterialProperty _VertexOffsetRotation = null;
        MaterialProperty _VertexMaskMap = null;
        MaterialProperty _VertexMaskRotation = null;

        //Fresnel
        MaterialProperty _FresnelColor = null;
        MaterialProperty _FresnelPower = null;
        MaterialProperty _FresnelScale = null;
        MaterialProperty _InvFresnelScale = null;


        MaterialProperty _PolarParameters = null;
        MaterialProperty _PolarUOffset = null;
        MaterialProperty _PolarVOffset = null;

        MaterialProperty _DissolvePolarEnable = null;
        MaterialProperty _DissolvePolarParameters = null;
        MaterialProperty _DissolvePolarUOffset = null;
        MaterialProperty _DissolvePolarVOffset = null;
        MaterialProperty _DissolveCustomPolarOffset = null;
        
        MaterialProperty _DistortionPolarEnable = null;
        MaterialProperty _DistortionPolarParameters = null;
        MaterialProperty _DistortionPolarUOffset = null;
        MaterialProperty _DistortionPolarVOffset = null;
        MaterialProperty _DistortionCustomPolarOffset = null;
        
        MaterialProperty _MaskPolarEnable = null;
        MaterialProperty _MaskPolarParameters = null;
        MaterialProperty _MaskPolarUOffset = null;
        MaterialProperty _MaskPolarVOffset = null;
        MaterialProperty _MaskCustomPolarOffset = null;

        MaterialProperty _WorldMask = null;
        MaterialProperty _WorldVerticalMask = null;
        MaterialProperty _WorldVerticalMaskIntense = null;

        #endregion

        #region FindProperties

        private void FindProperties(MaterialProperty[] props)
        {
            //Editor
            BlendMode = FindProperty("_Blend", props);
            _BlendOp = FindProperty("_BlendOp", props);
            _SrcBlend = FindProperty("_SrcBlend", props);
            _DstBlend = FindProperty("_DstBlend", props);
            CullMode = FindProperty("_CullMode", props);
            ZTest = FindProperty("_ZTest", props);
            ZWrite = FindProperty("_ZWrite", props);
            _AlphaTest = FindProperty("_AlphaTest", props);
            _AlphaCutoff = FindProperty("_AlphaCutoff", props);
            _Stencil = FindProperty("_Stencil", props);
            _StencilComp = FindProperty("_StencilComp", props);
            _StencilFail = FindProperty("_StencilFail", props);
            _StencilPass = FindProperty("_StencilPass", props);
            _StencilToggle = FindProperty("_StencilToggle", props);
            //
            _ColorProcessEnable = FindProperty("_ColorProcessEnable", props);
            _DissolveEnable = FindProperty("_DissolveEnable", props);
            _DistortionEnable = FindProperty("_DistortionEnable", props);
            _VertexOffsetEnable = FindProperty("_VertexOffsetEnable", props);
            _VertexMaskEnable = FindProperty("_VertexMaskEnable", props);
            _MainPolarEnable = FindProperty("_MainPolarEnable", props);
            _SoftParticles = FindProperty("_SoftParticles", props);
            _SoftParticleValue = FindProperty("_SoftParticle", props);
            _MaskEnable = FindProperty("_MaskEnable", props);
            _FresnelEnable = FindProperty("_FresnelEnable", props);
            //Toggle
            _BaseDetailToggle = FindProperty("_BaseDetailToggle", props);
            _MaskDetailToggle = FindProperty("_MaskDetailToggle", props);
            _BaseRtoAToggle = FindProperty("_BaseRtoA", props);
            _MaskRtoAToggle = FindProperty("_MaskRtoA", props);
            _ColorMask = FindProperty("_MaskColorToggle", props);
            _DissolveDetailToggle = FindProperty("_DissolveDetailToggle", props);
            _DistortionDetailToggle = FindProperty("_DistortionDetailToggle", props);
            _DissolveRtoAToggle = FindProperty("_DissolveRtoA", props);
            _DistortionRtoAToggle = FindProperty("_DistortionRtoA", props);

            _VertexOffsetRtoAToggle = FindProperty("_VORtoA", props);
            _VertexMaskRtoAToggle = FindProperty("_VOMaskRtoA", props);
            _VertexOffsetUClampToggle = FindProperty("_VoUClamp", props);
            _VertexOffsetVClampToggle = FindProperty("_VoVClamp", props);
            _VertexMaskUClampToggle = FindProperty("_VoMaskUClamp", props);
            _VertexMaskVClampToggle = FindProperty("_VoMaskVClamp", props);
            _VertexOffsetUMirrorToggle = FindProperty("_VoUMirror", props);
            _VertexOffsetVMirrorToggle = FindProperty("_VoVMirror", props);
            _VertexMaskUMirrorToggle = FindProperty("_VoMaskUMirror", props);
            _VertexMaskVMirrorToggle = FindProperty("_VoMaskVMirror", props);
            _VertexOffsetToggle = FindProperty("_VODetailToggle", props);
            _VertexMaskToggle = FindProperty("_VOMaskDetailToggle", props);
            _RemapToggle = FindProperty("_Remap", props);
            _FresneltoAlphaToggle = FindProperty("_FresneltoA", props);
            _InvFresnelToggle = FindProperty("_InvFresnel", props);
            _DissolveColorToggle = FindProperty("_DissolveColorToggle", props);
            //CustomData
            _CustomMainOffset = FindProperty("_CustomMainOffset", props);
            _CustomDissolveValue = FindProperty("_CustomDissolveValue", props);
            _CustomDistortionStrength = FindProperty("_CustomDistortionStrength", props);
            _CustomMaskOffset = FindProperty("_CustomMaskOffset", props);
            _CustomPolarOffset = FindProperty("_CustomPolarOffset", props);
            //BaseColor
            _BaseMap = FindProperty("_BaseMap", props);
            _BaseColor = FindProperty("_BaseColor", props);
            _Rotation = FindProperty("_Rotation", props);
            _Brightness = FindProperty("_Brightness", props);
            _Saturation = FindProperty("_Saturation", props);
            _GrayScale = FindProperty("_GrayScale", props);
            _Offset = FindProperty("_Offset", props);
            _Dispersion = FindProperty("_Dispersion", props);
            _Contrast = FindProperty("_Contrast", props);
            _HueOffset = FindProperty("_HueOffset", props);
            _BaseUVX = FindProperty("_BaseUVX", props);
            _BaseUVY = FindProperty("_BaseUVY", props);
            _UClamp = FindProperty("_UClamp", props);
            _VClamp = FindProperty("_VClamp", props);
            _UMirror = FindProperty("_UMirror", props);
            _VMirror = FindProperty("_VMirror", props);
            _ClampRepeat = FindProperty("_ClampRepeat", props);
            _Alpha = FindProperty("_AlphaStrength", props);


            //Mask
            _MaskMap = FindProperty("_MaskMap", props);
            _MaskColor = FindProperty("_MaskColor", props);
            _MaskUVX = FindProperty("_MaskUVX", props);
            _MaskUVY = FindProperty("_MaskUVY", props);
            _MaskUMirror = FindProperty("_MaskUMirror", props);
            _MaskVMirror = FindProperty("_MaskVMirror", props);
            _MaskRotation = FindProperty("_MaskRotation", props);
            _MaskStrength = FindProperty("_MaskStrength", props);

            //Dissolve
            _DissolveMap = FindProperty("_DissolveMap", props);
            _DissolveValue = FindProperty("_DissolveValue", props);
            _DissolveColor = FindProperty("_DissolveColor", props);
            _DissolveUVX = FindProperty("_DissolveUVX", props);
            _DissolveUVY = FindProperty("_DissolveUVY", props);
            _DissolveHardness = FindProperty("_DissolveHardness", props);
            _DissolveRotation = FindProperty("_DissolveRotation", props);
            _DissolveWidth = FindProperty("_DissolveWidth", props);
            _DissolveUMirror = FindProperty("_DissolveUMirror", props);
            _DissolveVMirror = FindProperty("_DissolveVMirror", props);

            //Distortion
            _DistortionMap = FindProperty("_DistortionMap", props);
            _DistortionUStrength = FindProperty("_DistortionUStrength", props);
            _DistortionVStrength = FindProperty("_DistortionVStrength", props);
            _DistortionUVX = FindProperty("_DistortionUVX", props);
            _DistortionUVY = FindProperty("_DistortionUVY", props);
            _DistortionRotation = FindProperty("_DistortionRotation", props);
            _DistortionUMirror = FindProperty("_DistortionUMirror", props);
            _DistortionVMirror = FindProperty("_DistortionVMirror", props);

            //VertexOffset
            _VertexOffsetMap = FindProperty("_VOTex", props);
            _VertexOffsetStrength = FindProperty("_VOStrength", props);
            _VertexOffsetUVX = FindProperty("_VOUVX", props);
            _VertexOffsetUVY = FindProperty("_VOUVY", props);
            _VertexOffsetRotation = FindProperty("_VORotation", props);
            _VetexOffsetPower = FindProperty("_VOPower", props);
            _VertexMaskMap = FindProperty("_VOMaskMap", props);
            _VertexMaskRotation = FindProperty("_VOMaskRotation", props);

            //Fresnel
            _FresnelColor = FindProperty("_FresnelColor", props);
            _FresnelPower = FindProperty("_FresnelPower", props);
            _FresnelScale = FindProperty("_FresnelScale", props);
            _InvFresnelScale = FindProperty("_InvFresnelScale", props);
            //Polar
            _PolarParameters = FindProperty("_PolarParameters", props);
            _PolarUOffset = FindProperty("_PolarUOffset", props);
            _PolarVOffset = FindProperty("_PolarVOffset", props);

            _DissolvePolarEnable = FindProperty("_DissolvePolarEnable", props);
            _DissolvePolarParameters = FindProperty("_DissolvePolarParameters", props);
            _DissolvePolarUOffset = FindProperty("_DissolvePolarUOffset", props);
            _DissolvePolarVOffset = FindProperty("_DissolvePolarVOffset", props);
            _DissolveCustomPolarOffset = FindProperty("_DissolveCustomPolarOffset", props);
            
            _DistortionPolarEnable = FindProperty("_DistortionPolarEnable", props);
            _DistortionPolarParameters = FindProperty("_DistortionPolarParameters", props);
            _DistortionPolarUOffset = FindProperty("_DistortionPolarUOffset", props);
            _DistortionPolarVOffset = FindProperty("_DistortionPolarVOffset", props);
            _DistortionCustomPolarOffset = FindProperty("_DistortionCustomPolarOffset", props);
            
            _MaskPolarEnable = FindProperty("_MaskPolarEnable", props);
            _MaskPolarParameters = FindProperty("_MaskPolarParameters", props);
            _MaskPolarUOffset = FindProperty("_MaskPolarUOffset", props);
            _MaskPolarVOffset = FindProperty("_MaskPolarVOffset", props);
            _MaskCustomPolarOffset = FindProperty("_MaskCustomPolarOffset", props);
            _WorldMask = FindProperty("_WorldMask", props);
            _WorldVerticalMask = FindProperty("_WorldVerticalMask", props);
            _WorldVerticalMaskIntense = FindProperty("_WorldVerticalMaskIntense", props);
            
        }

        #endregion


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);
            var material = materialEditor.target as Material;
            m_MaterialEditor = materialEditor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _Feature_Foldout = Foldout(_Feature_Foldout, "功能设置(Feature Setting)");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_Feature_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("不使用的功能记得关闭！！", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_MaskEnable, new GUIContent("遮罩(Mask)", "遮罩功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_DissolveEnable, new GUIContent("溶解(Dissolve)", "溶解功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_DistortionEnable, new GUIContent("扭曲(Distortion)", "扭曲功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_VertexOffsetEnable, new GUIContent("顶点偏移(Vertex Offset)", "顶点偏移功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_FresnelEnable, new GUIContent("菲涅尔(Fresnel)", "菲涅尔功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_SoftParticles, new GUIContent("软粒子(Soft Particles)", "软粒子功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_StencilToggle, new GUIContent("模板(Stencil)", "模板功能开关"));
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_WorldMask, new GUIContent("世界遮罩(World Mask)", "按世界Y高度做透明度渐隐"));
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            #region 基础设置

            _Base_Foldout = Foldout(_Base_Foldout, "基础设置(Baisc Setting)");

            if (_Base_Foldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(BlendMode,
                    new GUIContent("混合模式",
                        "Alpha传统透明度,PreMultiply预乘，Additive叠加模式,SoftAdditive柔性叠加模式,Multiply正片叠加,2xMultiply2倍相乘,Darken变暗,Lighten变亮,Screen滤色,Linear Dodge线性减淡"));
                if (material != null)
                    switch (material.GetInt("_Blend"))
                    {
                        case 0:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 5;
                            _DstBlend.floatValue = 10;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 1:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 1;
                            _DstBlend.floatValue = 10;
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 2:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 5;
                            _DstBlend.floatValue = 1;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 3:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 4;
                            _DstBlend.floatValue = 1;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 4:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 2;
                            _DstBlend.floatValue = 0;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 5:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 2;
                            _DstBlend.floatValue = 3;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 6:
                            _BlendOp.floatValue = 3;
                            _SrcBlend.floatValue = 1;
                            _DstBlend.floatValue = 1;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 7:
                            _BlendOp.floatValue = 4;
                            _SrcBlend.floatValue = 1;
                            _DstBlend.floatValue = 1;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 8:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 4;
                            _DstBlend.floatValue = 1;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case 9:
                            _BlendOp.floatValue = 0;
                            _SrcBlend.floatValue = 1;
                            _DstBlend.floatValue = 1;
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                    }

                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("剔除模式");

                switch (CullMode.floatValue)
                {
                    case 0:
                    {
                        if (GUILayout.Button("双面显示", shortButtonStyle))
                        {
                            CullMode.floatValue = 1;
                        }

                        break;
                    }
                    case 1:
                    {
                        if (GUILayout.Button("显示背面", shortButtonStyle))
                        {
                            CullMode.floatValue = 2;
                        }

                        break;
                    }
                    default:
                    {
                        if (GUILayout.Button("显示正面", shortButtonStyle))
                        {
                            CullMode.floatValue = 0;
                        }

                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("显示在最前层");
                if (Math.Abs(ZTest.floatValue - 4) < 1)
                {
                    if (GUILayout.Button("是", shortButtonStyle))
                    {
                        ZTest.floatValue = 8;
                    }
                }
                else
                {
                    if (GUILayout.Button("否", shortButtonStyle))
                    {
                        ZTest.floatValue = 4;
                    }
                }


                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*开启后深度测试ZTest设置为always，可以让特效显示在角色之前，避免穿模", style);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("写入深度");

                if (ZWrite.floatValue == 0)
                {
                    if (GUILayout.Button("否", shortButtonStyle))
                    {
                        ZWrite.floatValue = 1;
                    }
                }
                else
                {
                    if (GUILayout.Button("是", shortButtonStyle))
                    {
                        ZWrite.floatValue = 0;
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*开启后深度写入ZWrite改为on，开启后可以一定程度防止模型破面,但是透明区域后的其他特效层级显示会出现问题", style);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(_AlphaTest, "Alpha Clip");
                if (_AlphaTest.floatValue != 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(_AlphaCutoff, "Cut off");
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(10);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region 主贴图(Main)

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _Main_Foldout = Foldout(_Main_Foldout, "主贴图(Main)");
            if (_Main_Foldout)
            {
                EditorGUI.indentLevel++;

                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("主贴图"), _BaseMap, _BaseColor);
                GUILayout.Space(5);

                if (_BaseMap.textureValue != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.TextureScaleOffsetProperty(_BaseMap);
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    m_MaterialEditor.ShaderProperty(_BaseDetailToggle, "细节设置显示");
                    GUILayout.Space(5);

                    if (_BaseDetailToggle.floatValue != 0)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_MainPolarEnable, "开启极坐标");
                        if (_MainPolarEnable.floatValue != 0)
                        {
                            if (material != null) material.EnableKeyword("_MAIN_POLAR_ENABLE");
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_PolarParameters, "中心点X,中心点Y,半径,角度");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_PolarUOffset, "极坐标U偏移");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_PolarVOffset, "极坐标V偏移");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_CustomPolarOffset, "是否使用自定义曲线");
                            EditorGUILayout.LabelField("CustomData2.zw控制贴图uv偏移,打开后原有的偏移将会失效,不使用记得关闭",
                                EditorStyles.miniBoldLabel);
                            GUILayout.Space(5);
                            EditorGUILayout.EndVertical();
                        }
                        else
                        {
                            if (material != null) material.DisableKeyword("_MAIN_POLAR_ENABLE");
                            _PolarParameters.vectorValue = new Vector4(0.5f, 0.5f, 1f, 0);
                            _PolarUOffset.floatValue = 0;
                            _PolarVOffset.floatValue = 0;
                            _CustomPolarOffset.floatValue = 0;
                        }

                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_BaseRtoAToggle, "透明通道选择R通道");
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_ClampRepeat, "UVClampRepeat");
                        if (_ClampRepeat.floatValue != 0)
                        {
                            if (material != null) material.EnableKeyword("_CLAMPREPEAT_MAIN");
                        }
                        else
                        {
                            if (material != null) material.DisableKeyword("_CLAMPREPEAT_MAIN");
                        }

                        GUILayout.Space(5);
                        EditorGUILayout.BeginHorizontal();
                        m_MaterialEditor.ShaderProperty(_UClamp, "UVClamp");
                        m_MaterialEditor.ShaderProperty(_VClamp, "");
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        EditorGUILayout.BeginHorizontal();
                        m_MaterialEditor.ShaderProperty(_UMirror, "镜像");
                        m_MaterialEditor.ShaderProperty(_VMirror, "");
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_Rotation, "旋转");
                        GUILayout.Space(5);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_ColorProcessEnable, "开启颜色处理");
                        if (_ColorProcessEnable.floatValue != 0)
                        {
                            if (material != null) material.EnableKeyword("_COLORPROCESS_ENABLE");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_Brightness, "亮度");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_Saturation, "去饱和度");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_GrayScale, "灰度");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_Offset, "色散");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_HueOffset, "色相偏移");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_Dispersion, "离散");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_Contrast, "对比度");
                            GUILayout.Space(5);
                        }
                        else
                        {
                            if (material != null) material.DisableKeyword("_COLORPROCESS_ENABLE");
                            _Brightness.floatValue = 1;
                            _Saturation.floatValue = 0;
                            _GrayScale.floatValue = 0;
                            _Offset.floatValue = 0;
                            _HueOffset.floatValue = 0;
                            _Dispersion.floatValue = 256;
                            _Contrast.floatValue = 1;
                        }

                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_BaseUVX, "U流动");
                        m_MaterialEditor.ShaderProperty(_BaseUVY, "V流动");
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_CustomMainOffset, "是否使用自定义曲线");
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("CustomData1.xy控制贴图uv偏移,打开后原有的偏移将会失效,不使用记得关闭",
                            EditorStyles.miniBoldLabel);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    _ColorProcessEnable.floatValue = 0;
                    _BaseDetailToggle.floatValue = 0;
                    _BaseRtoAToggle.floatValue = 0;
                    _ClampRepeat.floatValue = 0;
                    _UClamp.floatValue = 0;
                    _VClamp.floatValue = 0;
                    _UMirror.floatValue = 0;
                    _VMirror.floatValue = 0;
                    _Rotation.floatValue = 0;
                    _Brightness.floatValue = 1;
                    _Saturation.floatValue = 0;
                    _GrayScale.floatValue = 0;
                    _Offset.floatValue = 0;
                    _HueOffset.floatValue = 0;
                    _Dispersion.floatValue = 256;
                    _Contrast.floatValue = 1;
                    _BaseUVX.floatValue = 0;
                    _BaseUVY.floatValue = 0;
                    _CustomMainOffset.floatValue = 0;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            #endregion

            Mask(material);
            Dissolve(material);
            Distortion(material);
            VertexOffset(material);
            Fresnel(material);
            Stencil();
            SoftParticles(material);
            WorldMask(material);
            Common();
        }

        #region ##遮罩

        private void Mask(Material material)
        {
            if (_MaskEnable.floatValue != 0)
            {
                if (material != null) material.EnableKeyword("_MASK_ENABLE");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Mask_Foldout = Foldout(_Mask_Foldout, "遮罩贴图(Mask)");
                if (_Mask_Foldout)
                {
                    EditorGUI.indentLevel++;
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("遮罩贴图"), _MaskMap, _MaskColor);
                    GUILayout.Space(5);
                    if (_MaskMap.textureValue != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.TextureScaleOffsetProperty(_MaskMap);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_MaskDetailToggle, "细节设置显示");
                        GUILayout.Space(5);
                        if (_MaskDetailToggle.floatValue != 0)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_MaskPolarEnable, "开启极坐标");
                            if (_MaskPolarEnable.floatValue != 0)
                            {
                                if (material != null) material.EnableKeyword("_MASK_POLAR_ENABLE");
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                m_MaterialEditor.ShaderProperty(_MaskPolarParameters, "中心点X,中心点Y,半径,角度");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_MaskPolarUOffset, "极坐标U偏移");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_MaskPolarVOffset, "极坐标V偏移");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_MaskCustomPolarOffset, "是否使用自定义曲线");
                                EditorGUILayout.LabelField("CustomData2.zw控制贴图uv偏移,打开后原有的偏移将会失效,不使用记得关闭",
                                    EditorStyles.miniBoldLabel);
                                GUILayout.Space(5);
                                EditorGUILayout.EndVertical();
                            }
                            else
                            {
                                if (material != null) material.DisableKeyword("_MASK_POLAR_ENABLE");
                                _MaskPolarParameters.vectorValue = new Vector4(0.5f, 0.5f, 1f, 0);
                                _MaskPolarUOffset.floatValue = 0;
                                _MaskPolarVOffset.floatValue = 0;
                                _MaskCustomPolarOffset.floatValue = 0;
                            }

                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_MaskRtoAToggle, "透明通道选择R通道");
                            GUILayout.Space(5);

                            m_MaterialEditor.ShaderProperty(_ColorMask, "颜色遮罩");
                            GUILayout.Space(5);

                            EditorGUILayout.BeginHorizontal();
                            m_MaterialEditor.ShaderProperty(_MaskUMirror, "镜像");
                            m_MaterialEditor.ShaderProperty(_MaskVMirror, "");
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5);

                            m_MaterialEditor.ShaderProperty(_MaskRotation, "旋转");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_MaskStrength, "遮罩强度");

                            GUILayout.Space(5);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.ShaderProperty(_MaskUVX, "U流动");
                            m_MaterialEditor.ShaderProperty(_MaskUVY, "V流动");
                            EditorGUILayout.EndVertical();

                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_CustomMaskOffset, "是否使用自定义曲线");

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("CustomData2.xy控制贴图uv偏移,打开后原有的偏移将会失效,不使用记得关闭",
                                EditorStyles.miniBoldLabel);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                if (material != null) material.DisableKeyword("_MASK_ENABLE");
                _MaskMap.textureValue = null;
                _MaskDetailToggle.floatValue = 0;
                _MaskRtoAToggle.floatValue = 0;
                _MaskUMirror.floatValue = 0;
                _MaskVMirror.floatValue = 0;
                _MaskRotation.floatValue = 0;
                _MaskStrength.floatValue = 1;
                _MaskUVX.floatValue = 0;
                _MaskUVY.floatValue = 0;
                _CustomMaskOffset.floatValue = 0;
            }
        }

        #endregion

        #region ##溶解贴图

        private void Dissolve(Material material)
        {
            if (_DissolveEnable.floatValue != 0)
            {
                material.EnableKeyword("_DISSOLVE_ENABLE");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Dissolve_Foldout = Foldout(_Dissolve_Foldout, "溶解贴图(Dissolve)");
                if (_Dissolve_Foldout)
                {
                    EditorGUI.indentLevel++;
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("溶解贴图"), _DissolveMap, _DissolveColor);
                    GUILayout.Space(5);

                    if (_DissolveMap.textureValue != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.TextureScaleOffsetProperty(_DissolveMap);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_DissolveDetailToggle, "细节设置显示");
                        GUILayout.Space(5);

                        if (_DissolveDetailToggle.floatValue != 0)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DissolvePolarEnable, "开启极坐标");
                            if (_DissolvePolarEnable.floatValue != 0)
                            {
                                if (material != null) material.EnableKeyword("_DISSOLVE_POLAR_ENABLE");
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                m_MaterialEditor.ShaderProperty(_DissolvePolarParameters, "中心点X,中心点Y,半径,角度");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_DissolvePolarUOffset, "极坐标U偏移");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_DissolvePolarVOffset, "极坐标V偏移");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_DissolveCustomPolarOffset, "是否使用自定义曲线");
                                EditorGUILayout.LabelField("CustomData2.zw控制贴图uv偏移,打开后原有的偏移将会失效,不使用记得关闭",
                                    EditorStyles.miniBoldLabel);
                                GUILayout.Space(5);
                                EditorGUILayout.EndVertical();
                            }
                            else
                            {
                                if (material != null) material.DisableKeyword("_DISSOLVE_POLAR_ENABLE");
                                _DissolvePolarParameters.vectorValue = new Vector4(0.5f, 0.5f, 1f, 0);
                                _DissolvePolarUOffset.floatValue = 0;
                                _DissolvePolarVOffset.floatValue = 0;
                                _DissolveCustomPolarOffset.floatValue = 0;
                            }

                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DissolveRtoAToggle, "透明通道选择R通道");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DissolveColorToggle, "颜色遮罩");
                            GUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal();
                            m_MaterialEditor.ShaderProperty(_DissolveUMirror, "镜像");
                            m_MaterialEditor.ShaderProperty(_DissolveVMirror, "");
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5);

                            m_MaterialEditor.ShaderProperty(_DissolveRotation, "旋转");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DissolveValue, "溶解值");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DissolveHardness, "溶解硬度");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DissolveWidth, "溶解宽度");
                            GUILayout.Space(5);

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.ShaderProperty(_DissolveUVX, "U流动");
                            m_MaterialEditor.ShaderProperty(_DissolveUVY, "V流动");
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);

                            m_MaterialEditor.ShaderProperty(_CustomDissolveValue, "是否使用自定义曲线");

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("CustomData1.z控制溶解值,打开后原有的偏移将会失效,不使用记得关闭",
                                EditorStyles.miniBoldLabel);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);
                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndVertical();
                    }
                    

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                material.DisableKeyword("_DISSOLVE_ENABLE");
                _DissolveMap.textureValue = null;
                _DissolveDetailToggle.floatValue = 0;
                _DissolveRtoAToggle.floatValue = 0;
                _DissolveColorToggle.floatValue = 0;
                _DissolveUMirror.floatValue = 0;
                _DissolveVMirror.floatValue = 0;
                _DissolveRotation.floatValue = 0;
                _DissolveValue.floatValue = 0;
                _DissolveHardness.floatValue = 0;
                _DissolveWidth.floatValue = 0;
                _DissolveUVX.floatValue = 0;
                _DissolveUVY.floatValue = 0;
                _CustomDissolveValue.floatValue = 0;
            }
        }

        #endregion

        #region ##扭曲

        private void Distortion(Material material)
        {
            if (_DistortionEnable.floatValue != 0)
            {
                material.EnableKeyword("_DISTORTION_ENABLE");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Distortion_Foldout = Foldout(_Distortion_Foldout, "扭曲贴图(Distortion)");
                if (_Distortion_Foldout)
                {
                    EditorGUI.indentLevel++;
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("扭曲贴图"), _DistortionMap);
                    GUILayout.Space(5);
                    if (_DistortionMap.textureValue != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.TextureScaleOffsetProperty(_DistortionMap);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_DistortionDetailToggle, "细节设置显示");
                        GUILayout.Space(5);
                        if (_DistortionDetailToggle.floatValue != 0)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DistortionPolarEnable, "开启极坐标");
                            if (_DistortionPolarEnable.floatValue != 0)
                            {
                                if (material != null) material.EnableKeyword("_DISTORTION_POLAR_ENABLE");
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                m_MaterialEditor.ShaderProperty(_DistortionPolarParameters, "中心点X,中心点Y,半径,角度");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_DistortionPolarUOffset, "极坐标U偏移");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_DistortionPolarVOffset, "极坐标V偏移");
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_DistortionCustomPolarOffset, "是否使用自定义曲线");
                                EditorGUILayout.LabelField("CustomData2.zw控制贴图uv偏移,打开后原有的偏移将会失效,不使用记得关闭",
                                    EditorStyles.miniBoldLabel);
                                GUILayout.Space(5);
                                EditorGUILayout.EndVertical();
                            }
                            else
                            {
                                if (material != null) material.DisableKeyword("_DISTORTION_POLAR_ENABLE");
                                _DistortionPolarParameters.vectorValue = new Vector4(0.5f, 0.5f, 1f, 0);
                                _DistortionPolarUOffset.floatValue = 0;
                                _DistortionPolarVOffset.floatValue = 0;
                                _DistortionCustomPolarOffset.floatValue = 0;
                            }

                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DistortionRtoAToggle, "透明通道选择R通道");
                            GUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal();
                            m_MaterialEditor.ShaderProperty(_DistortionUMirror, "镜像");
                            m_MaterialEditor.ShaderProperty(_DistortionVMirror, "");
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_DistortionRotation, "旋转");
                            GUILayout.Space(5);
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.ShaderProperty(_DistortionUStrength, "U扭曲强度");
                            m_MaterialEditor.ShaderProperty(_DistortionVStrength, "V扭曲强度");
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.ShaderProperty(_DistortionUVX, "U流动");
                            m_MaterialEditor.ShaderProperty(_DistortionUVY, "V流动");
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);

                            m_MaterialEditor.ShaderProperty(_CustomDistortionStrength, "是否使用自定义曲线");

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("CustomData1.w控制扭曲强度,打开后原有的偏移将会失效,不使用记得关闭",
                                EditorStyles.miniBoldLabel);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);

                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                material.DisableKeyword("_DISTORTION_ENABLE");
                _DistortionMap.textureValue = null;
                _DistortionDetailToggle.floatValue = 0;
                _DistortionRtoAToggle.floatValue = 0;
                _DistortionUMirror.floatValue = 0;
                _DistortionVMirror.floatValue = 0;
                _DistortionRotation.floatValue = 0;
                _DistortionUStrength.floatValue = 0;
                _DistortionVStrength.floatValue = 0;
                _DistortionUVX.floatValue = 0;
                _DistortionUVY.floatValue = 0;
                _CustomDistortionStrength.floatValue = 0;
            }
        }

        #endregion

        #region ##顶点偏移

        private void VertexOffset(Material material)
        {
            if (_VertexOffsetEnable.floatValue != 0)
            {
                material.EnableKeyword("_VERTEXOFFSET_ENABLE");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _VertexOffset_Foldout = Foldout(_VertexOffset_Foldout, "顶点偏移(VertexOffset)");

                if (_VertexOffset_Foldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("顶点偏移贴图"), _VertexOffsetMap);
                    if (_VertexOffsetMap.textureValue != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.TextureScaleOffsetProperty(_VertexOffsetMap);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                        m_MaterialEditor.ShaderProperty(_VertexMaskEnable, "是否使用顶点遮罩");
                        GUILayout.Space(5);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_VertexOffsetToggle, "细节设置显示");
                        GUILayout.Space(5);
                        if (_VertexOffsetToggle.floatValue != 0)
                        {
                            m_MaterialEditor.ShaderProperty(_VertexOffsetRtoAToggle, "选择R通道");
                            GUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal();
                            m_MaterialEditor.ShaderProperty(_VertexOffsetUClampToggle, "UVClamp");
                            m_MaterialEditor.ShaderProperty(_VertexOffsetVClampToggle, "");
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal();
                            m_MaterialEditor.ShaderProperty(_VertexOffsetUMirrorToggle, "镜像");
                            m_MaterialEditor.ShaderProperty(_VertexOffsetVMirrorToggle, "");
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_VertexOffsetRotation, "旋转");
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_VetexOffsetPower,
                                new GUIContent("锐度", "锐度越高,可以让变化更陡峭"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_RemapToggle,
                                new GUIContent("重映射", "把贴图数据从0-1映射到-0.5-0.5之间的任意值"));
                            GUILayout.Space(5);
                            m_MaterialEditor.ShaderProperty(_VertexOffsetStrength, "顶点偏移强度");
                            GUILayout.Space(5);
                        }
                        else
                        {
                            _VertexOffsetRtoAToggle.floatValue = 0;
                            _VertexOffsetUClampToggle.floatValue = 0;
                            _VertexOffsetVClampToggle.floatValue = 0;
                            _VertexOffsetUMirrorToggle.floatValue = 0;
                            _VertexOffsetVMirrorToggle.floatValue = 0;
                            _VertexOffsetRotation.floatValue = 0;
                            _VetexOffsetPower.floatValue = 0;
                            _RemapToggle.floatValue = 0;
                            _VertexOffsetStrength.floatValue = 0;
                        }

                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);
                    if (_VertexMaskEnable.floatValue != 0)
                    {
                        material.EnableKeyword("_VERTEXMASK_ENABLE");
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("顶点遮罩贴图"), _VertexMaskMap);
                        GUILayout.Space(5);
                        if (_VertexMaskMap.textureValue != null)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.TextureScaleOffsetProperty(_VertexMaskMap);
                            EditorGUILayout.EndVertical();
                            GUILayout.Space(5);
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            m_MaterialEditor.ShaderProperty(_VertexMaskToggle, "细节设置显示");
                            GUILayout.Space(5);
                            if (_VertexMaskToggle.floatValue != 0)
                            {
                                m_MaterialEditor.ShaderProperty(_VertexMaskRtoAToggle, "选择R通道");
                                GUILayout.Space(5);
                                EditorGUILayout.BeginHorizontal();
                                m_MaterialEditor.ShaderProperty(_VertexMaskUClampToggle, "UVClamp");
                                m_MaterialEditor.ShaderProperty(_VertexMaskVClampToggle, "");
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(5);

                                EditorGUILayout.BeginHorizontal();
                                m_MaterialEditor.ShaderProperty(_VertexMaskUMirrorToggle, "镜像");
                                m_MaterialEditor.ShaderProperty(_VertexMaskVMirrorToggle, "");
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(5);
                                m_MaterialEditor.ShaderProperty(_VertexMaskRotation, "旋转");
                                GUILayout.Space(5);
                            }

                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        material.DisableKeyword("_VERTEXMASK_ENABLE");
                        _VertexMaskMap.textureValue = null;
                        _VertexMaskToggle.floatValue = 0;
                        _VertexMaskRtoAToggle.floatValue = 0;
                        _VertexMaskUClampToggle.floatValue = 0;
                        _VertexMaskVClampToggle.floatValue = 0;
                        _VertexMaskUMirrorToggle.floatValue = 0;
                        _VertexMaskVMirrorToggle.floatValue = 0;
                        _VertexMaskRotation.floatValue = 0;
                    }

                    if (_VertexMaskMap.textureValue != null || _VertexOffsetMap.textureValue != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        m_MaterialEditor.ShaderProperty(_VertexOffsetUVX, "U流动");
                        m_MaterialEditor.ShaderProperty(_VertexOffsetUVY, "V流动");
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(5);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                material.DisableKeyword("_VERTEXOFFSET_ENABLE");
                _VertexOffsetMap.textureValue = null;
                _VertexMaskMap.textureValue = null;
                _VertexOffsetToggle.floatValue = 0;
                _VertexOffsetRtoAToggle.floatValue = 0;
                _VertexOffsetUClampToggle.floatValue = 0;
                _VertexOffsetVClampToggle.floatValue = 0;
                _VertexOffsetUMirrorToggle.floatValue = 0;
                _VertexOffsetVMirrorToggle.floatValue = 0;
                _VertexOffsetRotation.floatValue = 0;
                _VetexOffsetPower.floatValue = 0;
                _RemapToggle.floatValue = 0;
                _VertexOffsetStrength.floatValue = 0;
                _VertexMaskToggle.floatValue = 0;
                _VertexMaskRtoAToggle.floatValue = 0;
                _VertexMaskUClampToggle.floatValue = 0;
                _VertexMaskVClampToggle.floatValue = 0;
                _VertexMaskUMirrorToggle.floatValue = 0;
                _VertexMaskVMirrorToggle.floatValue = 0;
                _VertexMaskRotation.floatValue = 0;
            }
        }

        #endregion

        #region ##边缘光

        private void Fresnel(Material material)
        {
            if (_FresnelEnable.floatValue != 0)
            {
                material.EnableKeyword("_FRESNEL_ENABLE");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Fresnel_Foldout = Foldout(_Fresnel_Foldout, "菲涅耳(Fresnel)");
                if (_Fresnel_Foldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(_InvFresnelToggle, "反向菲涅耳");
                    if (_InvFresnelToggle.floatValue != 0)
                    {
                        m_MaterialEditor.ShaderProperty(_InvFresnelScale, "模型边缘软化");
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_FresneltoAlphaToggle, "影响透明度");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_FresnelColor, "菲涅耳颜色");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_FresnelScale, "菲涅耳锐化");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_FresnelPower, "菲涅耳强度");
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                material.DisableKeyword("_FRESNEL_ENABLE");
                _InvFresnelToggle.floatValue = 0;
                _InvFresnelScale.floatValue = 0;
                _FresneltoAlphaToggle.floatValue = 0;
                _FresnelColor.colorValue = Color.white;
                _FresnelScale.floatValue = 0;
                _FresnelPower.floatValue = 0;
            }
        }

        #endregion

        #region ##模板测试

        private void Stencil()
        {
            if (_StencilToggle.floatValue != 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Stencil_Foldout = Foldout(_Stencil_Foldout, "模板测试(Stencil)");
                if (_Stencil_Foldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(_Stencil, "模板值");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_StencilComp, "模板比较方式");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_StencilPass, "通过模板值");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_StencilFail, "失败模板值");
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                else
                {
                    _Stencil.floatValue = 0;
                    _StencilComp.floatValue = 8;
                    _StencilPass.floatValue = 0;
                    _StencilFail.floatValue = 0;
                }

                EditorGUILayout.EndVertical();
            }
        }

        #endregion

        #region ##软粒子

        private void SoftParticles(Material material)
        {
            if (_SoftParticles.floatValue != 0)
            {
                if (material != null) material.EnableKeyword("_SOFTPARTICLES");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _SoftParticle_Foldout = Foldout(_SoftParticle_Foldout, "软粒子(Soft Particles)");
                if (_SoftParticle_Foldout)
                {
                    EditorGUI.indentLevel++;
                    m_MaterialEditor.ShaderProperty(_SoftParticleValue, "淡出距离(Fade Distance)");
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("*粒子离场景几何越近越透明,柔化硬相交边缘。需要URP Asset开启Depth Texture", EditorStyles.miniBoldLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (material != null) material.DisableKeyword("_SOFTPARTICLES");
                _SoftParticleValue.floatValue = 1;
            }
        }

        #endregion

        #region ##世界遮罩

        private void WorldMask(Material material)
        {
            if (_WorldMask.floatValue != 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _WorldMask_Foldout = Foldout(_WorldMask_Foldout, "世界遮罩(World Mask)");
                if (_WorldMask_Foldout)
                {
                    EditorGUI.indentLevel++;
                    m_MaterialEditor.ShaderProperty(_WorldVerticalMask, "渐隐高度(Height)");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(_WorldVerticalMaskIntense, "渐隐强度(Intense)");
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("*按粒子世界空间Y高度做透明度渐隐：低于设定高度渐显，超过则快速淡出。地面特效离地过高可自动消失", EditorStyles.miniBoldLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
        }

        #endregion

        #region ##综合设置

        void Common()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _Common_Foldout = Foldout(_Common_Foldout, "综合设置(Common Settings)");

            if (_Common_Foldout)
            {
                EditorGUI.indentLevel++;
                m_MaterialEditor.ShaderProperty(_Alpha, "总透明度");
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.BeginChangeCheck();
                {
                    GUILayout.Space(5);
                    MaterialProperty[] props = { };
                    base.OnGUI(m_MaterialEditor, props);
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
#endif