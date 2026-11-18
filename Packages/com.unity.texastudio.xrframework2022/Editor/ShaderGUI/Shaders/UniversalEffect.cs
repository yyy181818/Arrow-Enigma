#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;

public class UniversalEffect : ShaderGUI
{
    public override void OnGUI(MaterialEditor m, MaterialProperty[] properties)
    {
        var targetMat = m.target as Material;

        m.SetDefaultGUIWidths();
   
            //        var _Polar = ShaderGUI.FindProperty("_Polar", properties);
            //        var _TwistStrength = ShaderGUI.FindProperty("_TwistStrength", properties);
            //        var _BendStrength = ShaderGUI.FindProperty("_BendStrength", properties);        
                
        //        // var _FlipPartition = ShaderGUI.FindProperty("_FlipPartition", properties);
        //        // var _FlipTime = ShaderGUI.FindProperty("_FlipTime", properties);
        //        // var _FlipStartPercent = ShaderGUI.FindProperty("_FlipStartPercent", properties);  

        //        var _RGBSplit = ShaderGUI.FindProperty("_RGBSplit", properties);
        //        var _RGBSplitIntense = ShaderGUI.FindProperty("_RGBSplitIntense", properties);

        var _Cull = ShaderGUI.FindProperty("_Cull", properties);
        var _ZWrite = ShaderGUI.FindProperty("_ZWrite", properties);
        var _ZTest = ShaderGUI.FindProperty("_ZTest", properties);
        var _Blend = ShaderGUI.FindProperty("_Blend", properties);
        var _SrcBlend = ShaderGUI.FindProperty("_SrcBlend", properties);
        var _DstBlend = ShaderGUI.FindProperty("_DstBlend", properties);
        var _BlendOp = ShaderGUI.FindProperty("_BlendOp", properties);

        var _Color = ShaderGUI.FindProperty("_Color", properties);
        var _BaseMap = ShaderGUI.FindProperty("_BaseMap", properties);
        var _NormalMap = ShaderGUI.FindProperty("_NormalMap", properties);
        var _NORMALMAP = ShaderGUI.FindProperty("_NORMALMAP", properties);
        var _NormalScale = ShaderGUI.FindProperty("_NormalScale", properties);
        var _N_Offset_Xspeed = ShaderGUI.FindProperty("_N_Offset_Xspeed", properties);
        var _N_Offset_Yspeed = ShaderGUI.FindProperty("_N_Offset_Yspeed", properties);
        var _M_Rotation = ShaderGUI.FindProperty("_M_Rotation", properties);
        var _Main_Power = ShaderGUI.FindProperty("_Main_Power", properties);
        var _Main_Saturation = ShaderGUI.FindProperty("_Main_Saturation", properties);
        var _ClampU_Main = ShaderGUI.FindProperty("_ClampU_Main", properties);
        var _ClampV_Main = ShaderGUI.FindProperty("_ClampV_Main", properties);
        var _ClampRepeat_Main = ShaderGUI.FindProperty("_ClampRepeat_Main", properties);
        
        var _M_Offset_Xspeed = ShaderGUI.FindProperty("_M_Offset_Xspeed", properties);
        var _M_Offset_Yspeed = ShaderGUI.FindProperty("_M_Offset_Yspeed", properties);
        var _RasAlpha_Main = ShaderGUI.FindProperty("_RasAlpha_Main", properties);
        var _CustomData_Main = ShaderGUI.FindProperty("_CustomData_Main", properties);

        var _AddTex = ShaderGUI.FindProperty("_AddTex", properties);
        var _AddBlend = ShaderGUI.FindProperty("_AddBlend", properties);
        var _LerpValue = ShaderGUI.FindProperty("_LerpValue", properties);
        var _Add_Color = ShaderGUI.FindProperty("_Add_Color", properties);
        var _Add_Rotation = ShaderGUI.FindProperty("_Add_Rotation", properties);
        var _Add_Offset_Xspeed = ShaderGUI.FindProperty("_Add_Offset_Xspeed", properties);
        var _Add_Offset_Yspeed = ShaderGUI.FindProperty("_Add_Offset_Yspeed", properties);

        var _MaskTex = ShaderGUI.FindProperty("_MaskTex", properties);
        var _MaskStrength = ShaderGUI.FindProperty("_MaskStrength", properties);
        //var _MaskSmoothstepMin = ShaderGUI.FindProperty("_MaskSmoothstepMin", properties);
        //var _MaskSmoothstepMax = ShaderGUI.FindProperty("_MaskSmoothstepMax", properties);
        var _Mask_Offset_Xspeed = ShaderGUI.FindProperty("_Mask_Offset_Xspeed", properties);
        var _Mask_Offset_Yspeed = ShaderGUI.FindProperty("_Mask_Offset_Yspeed", properties);
        var _MaskRot = ShaderGUI.FindProperty("_MaskRot", properties);
        var _ClampU_Mask = ShaderGUI.FindProperty("_ClampU_Mask", properties);
        var _ClampV_Mask = ShaderGUI.FindProperty("_ClampV_Mask", properties);
        var _RasAlpha_Mask = ShaderGUI.FindProperty("_RasAlpha_Mask", properties);
        var _CustomData_Mask = ShaderGUI.FindProperty("_CustomData_Mask", properties);
        var _Color_Mask = ShaderGUI.FindProperty("_Color_Mask", properties);
        
        

        var _WarpTex = ShaderGUI.FindProperty("_WarpTex", properties);
        var _Warp_Intensity = ShaderGUI.FindProperty("_Warp_Intensity", properties);
        var _Warp_Offset_Xspeed = ShaderGUI.FindProperty("_Warp_Offset_Xspeed", properties);
        var _Warp_Offset_Yspeed = ShaderGUI.FindProperty("_Warp_Offset_Yspeed", properties);
        var _WarpRot = ShaderGUI.FindProperty("_WarpRot", properties);
        var _Warp_Main = ShaderGUI.FindProperty("_Warp_Main", properties);
        var _Warp_Mask = ShaderGUI.FindProperty("_Warp_Mask", properties);
        var _Warp_Dissolve = ShaderGUI.FindProperty("_Warp_Dissolve", properties);
        var _CustomData_Warp = ShaderGUI.FindProperty("_CustomData_Warp", properties);

        var _DissolveTex = ShaderGUI.FindProperty("_DissolveTex", properties);
        var _Dissolve_Offset_Xspeed = ShaderGUI.FindProperty("_Dissolve_Offset_Xspeed", properties);
        var _Dissolve_Offset_Yspeed = ShaderGUI.FindProperty("_Dissolve_Offset_Yspeed", properties);
        var _DissolveRot = ShaderGUI.FindProperty("_DissolveRot", properties);
        var _Dissolve_Value = ShaderGUI.FindProperty("_Dissolve_Value", properties);
        var _Dissolve_Width = ShaderGUI.FindProperty("_Dissolve_Width", properties);
        var _Dissolve_Soft = ShaderGUI.FindProperty("_Dissolve_Soft", properties);
        var _Dissolve_SoftValue = ShaderGUI.FindProperty("_Dissolve_SoftValue", properties);
        var _Dissolve_Color = ShaderGUI.FindProperty("_Dissolve_Color", properties);
        var _CustomData_Dissolve = ShaderGUI.FindProperty("_CustomData_Dissolve", properties);

        var _FresnelColor = ShaderGUI.FindProperty("_FresnelColor", properties);
        var _FresnalPower = ShaderGUI.FindProperty("_FresnalPower", properties);
        var _FresnelBias = ShaderGUI.FindProperty("_FresnelBias", properties);
        var _FresnelScale = ShaderGUI.FindProperty("_FresnelScale", properties);
        var _Fresnel_Rev = ShaderGUI.FindProperty("_Fresnel_Rev", properties);

        var _EnableWorldMask = ShaderGUI.FindProperty("_EnableWorldMask", properties);
        var _WorldVerticalMask = ShaderGUI.FindProperty("_WorldVerticalMask", properties);
        var _WorldVerticalMaskIntense = ShaderGUI.FindProperty("_WorldVerticalMaskIntense", properties);
        var _Alpha = ShaderGUI.FindProperty("_Alpha", properties);
        var _AlphaPower = ShaderGUI.FindProperty("_AlphaPower", properties);
        //var _Saturation = ShaderGUI.FindProperty("_Saturation", properties);
        //Basic Settings
        EditorGUILayout.LabelField("基本设置", EditorStyles.centeredGreyMiniLabel);
        m.ShaderProperty(_Blend, new GUIContent("混合模式", "Normal普通模式，Add线性减淡，Lighten变亮, Screen滤色,Darken变暗,LinearBurn线性加深,SoftAdditive,Multiply正片叠底,2XMultiply,SoftMultiply"));
        if (targetMat != null)
        {
            switch (targetMat.GetInt("_Blend"))
            {
                case 0:
                    targetMat.SetFloat("_SrcBlend", 5);
                    targetMat.SetFloat("_DstBlend", 10);
                    targetMat.SetFloat("_BlendOp", 0);
                    targetMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case 1:

                    targetMat.SetFloat("_SrcBlend", 5);
                    targetMat.SetFloat("_DstBlend", 1);
                    targetMat.SetFloat("_BlendOp", 0);
                    targetMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case 2:
                    targetMat.SetFloat("_SrcBlend", 1);
                    targetMat.SetFloat("_DstBlend", 10);
                    targetMat.SetFloat("_BlendOp", 0);
                    targetMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                // case 2:

                // 	targetMat.SetFloat("_SrcBlend", 5);
                //     targetMat.SetFloat("_DstBlend", 1);
                // 	targetMat.SetFloat("_BlendOp", 4);
                //     break;
                // case 3:            
                //     targetMat.SetFloat("_SrcBlend", 4);
                //     targetMat.SetFloat("_DstBlend", 1);
                // 	targetMat.SetFloat("_BlendOp", 4);
                //     break;
                // case 4:            
                //     targetMat.SetFloat("_SrcBlend", 5);
                //     targetMat.SetFloat("_DstBlend", 1);
                // 	targetMat.SetFloat("_BlendOp", 3);
                //     break;
                //case 2:
                //    targetMat.SetFloat("_SrcBlend", 5);
                //    targetMat.SetFloat("_DstBlend", 1);
                //    targetMat.SetFloat("_BlendOp", 2);
                //    break;
                //case 3:
                //    targetMat.SetFloat("_SrcBlend", 4);
                //    targetMat.SetFloat("_DstBlend", 1);
                //    targetMat.SetFloat("_BlendOp", 0);
                //    break;
                //case 4:
                //    targetMat.SetFloat("_SrcBlend", 2);
                //    targetMat.SetFloat("_DstBlend", 0);
                //    targetMat.SetFloat("_BlendOp", 0);
                //    break;
                //case 5:
                //    targetMat.SetFloat("_SrcBlend", 2);
                //    targetMat.SetFloat("_DstBlend", 3);
                //    targetMat.SetFloat("_BlendOp", 0);
                //    break;
                //case 6:
                //    targetMat.SetFloat("_SrcBlend", 2);
                //    targetMat.SetFloat("_DstBlend", 10);
                //    targetMat.SetFloat("_BlendOp", 0);
                //    break;
            }

            //  m.ShaderProperty(_SrcBlend, _SrcBlend.displayName);
            //  m.ShaderProperty(_DstBlend, _DstBlend.displayName);
            m.ShaderProperty(_ZWrite, "深度写入");
            m.ShaderProperty(_ZTest, "深度检测");
            m.ShaderProperty(_Cull, "剔除模式");


            //        //
            //        m.ShaderProperty(_Color,  new GUIContent( "Color", "染色颜色"));
            //        m.ShaderProperty(_Polar, new GUIContent( "Polar", "开启极坐标变换"));
            //        m.ShaderProperty(_TwistStrength, new GUIContent( "TwistStrength", "扭曲强度"));
            //        m.ShaderProperty(_BendStrength, new GUIContent( "BendStrength", "弯曲强度"));
            //        m.ShaderProperty(_MainTexRotCenterX, new GUIContent( "MainTexRotCenterX", "旋转中心坐标X"));
            //        m.ShaderProperty(_MainTexRotCenterY, new GUIContent( "MainTexRotCenterY", "旋转中心坐标Y"));


            //MainTex
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("MainTexture", EditorStyles.centeredGreyMiniLabel);
            m.ShaderProperty(_Color, _Color.displayName);
            m.TextureProperty(_BaseMap, "MainTexture");
            if (targetMat.GetTexture("_BaseMap"))
            {
                m.ShaderProperty(_Main_Power, "主贴图锐化");
                m.ShaderProperty(_Main_Saturation, "主贴图去饱和度");
                m.ShaderProperty(_M_Rotation, "旋转角度");
                m.ShaderProperty(_ClampU_Main, "ClampU");
                m.ShaderProperty(_ClampV_Main, "ClampV");
                m.ShaderProperty(_ClampRepeat_Main, "Clamp同时重复UV流动");

                m.ShaderProperty(_M_Offset_Xspeed, new GUIContent("主贴图U流动", "主贴图偏移速度X"));
                m.ShaderProperty(_M_Offset_Yspeed, new GUIContent("主贴图V流动", "主贴图偏移速度Y"));
                m.ShaderProperty(_RasAlpha_Main, "R通道颜色作为透明度");
                m.ShaderProperty(_CustomData_Main, "custom1.xy控制主贴图uv偏移");
            }

            m.ShaderProperty(_NORMALMAP, _NORMALMAP.displayName);
            if (_NORMALMAP.floatValue == 1)
            {
                m.TextureProperty(_NormalMap, "NormalMap");
                m.ShaderProperty(_NormalScale, "NormalScale");
                m.ShaderProperty(_N_Offset_Xspeed, "N_Offset_Xspeed");
                m.ShaderProperty(_N_Offset_Yspeed, "N_Offset_Yspeed");
                targetMat.EnableKeyword("_NORMALMAP");
            }
            else
            {
                targetMat.DisableKeyword("_NORMALMAP");
            }

            //AddTex
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("附加图", EditorStyles.centeredGreyMiniLabel);
            m.TextureProperty(_AddTex, "AddTexture");

            if (targetMat.GetTexture("_AddTex"))
            {
                targetMat.EnableKeyword("_ADDTEX");
                m.ShaderProperty(_Add_Rotation, "旋转角度");
                m.ShaderProperty(_Add_Color, _Add_Color.displayName);
                m.ShaderProperty(_Add_Offset_Xspeed, "U流动");
                m.ShaderProperty(_Add_Offset_Yspeed, "V流动");
                m.ShaderProperty(_AddBlend, "叠加模式");
                m.ShaderProperty(_LerpValue, "Lerp系数");
            }
            else
            {
                targetMat.DisableKeyword("_ADDTEX");
            }


            //MaskTex
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("遮罩", EditorStyles.centeredGreyMiniLabel);

            m.TextureProperty(_MaskTex, "MaskTex");
            if (targetMat.GetTexture("_MaskTex"))
            {
                targetMat.EnableKeyword("_MASK");
                m.ShaderProperty(_MaskStrength, "遮罩强度");
                //m.ShaderProperty(_MaskSmoothstepMin, new GUIContent("MaskSmoothstepMin", "轮廓遮罩贴图调整Min"));
                //m.ShaderProperty(_MaskSmoothstepMax, new GUIContent("MaskSmoothstepMax", "轮廓遮罩贴图调整Max"));
                m.ShaderProperty(_ClampU_Mask, "ClampU");
                m.ShaderProperty(_ClampV_Mask, "ClampV");
                m.ShaderProperty(_Mask_Offset_Xspeed, new GUIContent("U流动", "轮廓遮罩贴图偏移速度X"));
                m.ShaderProperty(_Mask_Offset_Yspeed, new GUIContent("V流动", "轮廓遮罩贴图偏移速度Y"));
                m.ShaderProperty(_MaskRot, new GUIContent("旋转角度", "轮廓遮罩贴图旋转角度"));
                m.ShaderProperty(_RasAlpha_Mask, new GUIContent("R通道颜色作为透明度", "R通道颜色作为透明度（去黑底）"));
                m.ShaderProperty(_Color_Mask, "Mask贴图颜色开关");
                m.ShaderProperty(_CustomData_Mask, "custom2.xy控制遮罩贴图uv偏移");
            }
            else
            {
                targetMat.DisableKeyword("_MASK");
            }

            //Dissolve
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("溶解", EditorStyles.centeredGreyMiniLabel);
            m.TextureProperty(_DissolveTex, "DissolveTex");
            if (targetMat.GetTexture("_DissolveTex"))
            {
                targetMat.EnableKeyword("_DISSOLVE");
                m.ShaderProperty(_Dissolve_Offset_Xspeed, "U流动");
                m.ShaderProperty(_Dissolve_Offset_Yspeed, "V流动");
                m.ShaderProperty(_DissolveRot, "旋转角度");
                m.ShaderProperty(_Dissolve_Value, "溶解系数");
                m.ShaderProperty(_Dissolve_Width, "溶解宽度");
                m.ShaderProperty(_Dissolve_Color, "溶解颜色");
                m.ShaderProperty(_Dissolve_Soft, "软边模式");
                if (_Dissolve_Soft.floatValue == 1)
                {
                    targetMat.EnableKeyword("_DISSOLVE_SOFT");
                }
                else
                {
                    targetMat.DisableKeyword("_DISSOLVE_SOFT");
                }
                m.ShaderProperty(_Dissolve_SoftValue, "软边强度");
                m.ShaderProperty(_CustomData_Dissolve, "custom1.z控制溶解值");
                if (_CustomData_Dissolve.floatValue == 1)
                {
                    targetMat.EnableKeyword("_CUSTOMDATA_DISSOLVE");
                }
                else
                {
                    targetMat.DisableKeyword("_CUSTOMDATA_DISSOLVE");
                }
            }
            else
            {
                targetMat.DisableKeyword("_DISSOLVE");
            }

            //WarpTex
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("扰动", EditorStyles.centeredGreyMiniLabel);
            m.TextureProperty(_WarpTex, "WarpTex");
            if (targetMat.GetTexture("_WarpTex"))
            {
                targetMat.EnableKeyword("_WARP");
                m.ShaderProperty(_WarpRot, new GUIContent("旋转角度", "扭曲贴图旋转角度"));
                m.ShaderProperty(_Warp_Offset_Xspeed, new GUIContent("U流动", "扭曲贴图偏移速度X"));
                m.ShaderProperty(_Warp_Offset_Yspeed, new GUIContent("V流动", "扭曲贴图偏移速度Y"));
                m.ShaderProperty(_Warp_Intensity, new GUIContent("扭曲强度", "扭曲强度"));
                m.ShaderProperty(_CustomData_Warp, "custom1.w控制uv扭曲强度");
                m.ShaderProperty(_Warp_Main, "扭动主贴图开关");
                m.ShaderProperty(_Warp_Mask, "扭动mask开关");
                m.ShaderProperty(_Warp_Dissolve, "扭动溶解图开关");
            }
            else
            {
                targetMat.DisableKeyword("_WARP");
            }
        }


        //Fresnel
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Fresnal", EditorStyles.centeredGreyMiniLabel);
        m.ShaderProperty(_FresnelColor, "Fresnel颜色");
        m.ShaderProperty(_FresnalPower, "Fresnal锐化");
        m.ShaderProperty(_FresnelScale, "Fresnel强度");
        //m.ShaderProperty(_FresnelBias, "FresnelBias");
        m.ShaderProperty(_Fresnel_Rev, "反向");

        //FinalControl
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("总体控制", EditorStyles.centeredGreyMiniLabel);
        m.ShaderProperty(_EnableWorldMask, "开启位置渐变");
        m.ShaderProperty(_WorldVerticalMask, "位置垂直渐变位置");
        m.ShaderProperty(_WorldVerticalMaskIntense, "位置垂直渐变强度");
        m.ShaderProperty(_Alpha, "总透明度");
        m.ShaderProperty(_AlphaPower, "总锐化");
        //m.ShaderProperty(_Saturation, "去饱和度");
        
        // m.ShaderProperty(_Premultiply,new GUIContent( "Premultiply", "预乘透明度"));

        EditorGUILayout.Space(10);
        m.RenderQueueField();
        m.EnableInstancingField();
        //var material = (Material)m.target;
        // material.enableInstancing = true;

    }
}
#endif