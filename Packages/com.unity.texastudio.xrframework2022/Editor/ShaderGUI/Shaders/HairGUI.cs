using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public class HairGUI : BaseShaderGUI
    {
        private MaterialProperty baseColor;
		private MaterialProperty baseMap;
        private MaterialProperty aoMap;
        private MaterialProperty aoMin;
        private MaterialProperty aoMax;
        private MaterialProperty smoothnessMaskMap;
        private MaterialProperty smoothness;
		private MaterialProperty smoothnessMin;
        private MaterialProperty smoothnessMax;
        private MaterialProperty bumpMap;
		private MaterialProperty bumpMapScale;
        private MaterialProperty specularColor;
        private MaterialProperty specularMultiplier;
        private MaterialProperty specularShift;
        private MaterialProperty secondarySpecularMultiplier;
        private MaterialProperty secondarySpecularShift;
        private MaterialProperty transmissionColor;
        private MaterialProperty transmissionRim;

        private MaterialProperty staticLightColor;
        private MaterialProperty staticLightVector;
        private MaterialProperty staticSpecularHighlight;

        private PropertyStructures.SurfaceOptions surfaceOptions;
        private PropertyStructures.AdvancedOptions advancedOptions;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            // Surface Options Props
            surfaceOptions = new PropertyStructures.SurfaceOptions(properties);

            // Surface Input Props
            // BaseMap properties
            baseMap = BaseShaderGUI.FindProperty("_BaseMap", properties, false); 
            baseColor = BaseShaderGUI.FindProperty("_BaseColor", properties, false);
            // Ambient Occlusion properties
            aoMap = BaseShaderGUI.FindProperty("_AmbientOcclusionMap", properties, false);
            aoMin = BaseShaderGUI.FindProperty("_AORemapMin", properties, false);
            aoMax = BaseShaderGUI.FindProperty("_AORemapMax", properties, false);
            smoothnessMaskMap = BaseShaderGUI.FindProperty("_SmoothnessMaskMap", properties, false);
            smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
            smoothnessMin = BaseShaderGUI.FindProperty("_SmoothnessRemapMin", properties, false);
            smoothnessMax = BaseShaderGUI.FindProperty("_SmoothnessRemapMax", properties, false);
            // NormalMap properties
            bumpMap = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
            bumpMapScale = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
            // Specular properties
            specularColor = BaseShaderGUI.FindProperty("_SpecularColor", properties, false);
            specularMultiplier = BaseShaderGUI.FindProperty("_SpecularMultiplier", properties, false);
            specularShift = BaseShaderGUI.FindProperty("_SpecularShift", properties, false);
            secondarySpecularMultiplier = BaseShaderGUI.FindProperty("_SecondarySpecularMultiplier", properties, false);
            secondarySpecularShift = BaseShaderGUI.FindProperty("_SecondarySpecularShift", properties, false);
            transmissionColor = BaseShaderGUI.FindProperty("_TransmissionColor", properties, false);
            transmissionRim = BaseShaderGUI.FindProperty("_TransmissionIntensity", properties, false);

            staticLightColor = BaseShaderGUI.FindProperty("_StaticLightColor", properties, false);
            staticLightVector = BaseShaderGUI.FindProperty("_StaticLightVector", properties, false);

            // Advanced Props
            advancedOptions = new PropertyStructures.AdvancedOptions(properties);
            staticSpecularHighlight = BaseShaderGUI.FindProperty("_StaticSpecularHighlight", properties, false);
        }

        public override void ValidateMaterial(Material material) => SetMaterialKeywords(material);

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;
            surfaceOptions.DrawBaseSurfaceOptions(materialEditor, material);
            surfaceOptions.DrawAlphaCutoffGUI(materialEditor);
            surfaceOptions.DrawSpecularAAArea(materialEditor);
            EditorGUILayout.EndVertical();
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            /****Base Block****/
            materialEditor.BeginBlock("Base Block");
            DrawBaseProperties(material);
            BaseShaderGUI.DrawNormalArea(materialEditor, bumpMap, bumpMapScale);
			materialEditor.TexturePropertySingleLine(URPPlusStyles.aoMapText, aoMap);
            if(aoMap.textureValue != null)
                materialEditor.MinMaxShaderProperty(aoMin, aoMax, 0.0f, 1.0f, URPPlusStyles.aoRemappingText);
                
			materialEditor.TextureScaleOffsetProperty(baseMap);
            materialEditor.EndBlock();

            /****Specular Block****/
            materialEditor.BeginBlock("Specular Block");
            materialEditor.TexturePropertySingleLine(URPPlusStyles.smoothnessMaskText, smoothnessMaskMap);
            if(smoothnessMaskMap.textureValue != null)
            { 
                materialEditor.MinMaxShaderProperty(smoothnessMin, smoothnessMax, 0.0f, 1.0f, URPPlusStyles.smoothnessRemappingText);
                materialEditor.TextureScaleOffsetProperty(smoothnessMaskMap);
            }
            else
            {
                materialEditor.ShaderProperty(smoothness, URPPlusStyles.smoothnessText);
            }
            materialEditor.ShaderProperty(specularColor, URPPlusStyles.specularColorHairText);
            materialEditor.ShaderProperty(specularMultiplier, URPPlusStyles.specularMultiplierText);
            materialEditor.ShaderProperty(specularShift, URPPlusStyles.specularShiftText);
            materialEditor.ShaderProperty(secondarySpecularMultiplier, URPPlusStyles.secondarySpecularMultiplierText);
            materialEditor.ShaderProperty(secondarySpecularShift, URPPlusStyles.secondarySpecularShiftText);
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(transmissionColor, URPPlusStyles.transmissionColorText);
            materialEditor.ShaderProperty(transmissionRim, URPPlusStyles.transmissionRimText);
            materialEditor.EndBlock();

            /****Static Light Block****/
            if(material.GetFloat("_StaticSpecularHighlight") == 1.0f)
            {
                materialEditor.BeginBlock("Static Light Block");
                materialEditor.ShaderProperty(staticLightColor, URPPlusStyles.staticLightColorText);
                materialEditor.ShaderProperty(staticLightVector, URPPlusStyles.staticLightVectorText);
                materialEditor.EndBlock();
            }
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if ((SurfaceType)surfaceOptions.surfaceType.floatValue == SurfaceType.Transparent)
                SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.castShadowText, castShadowsProp);

            SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.receiveShadowText, receiveShadowsProp);
            if (advancedOptions.reflections != null && advancedOptions.highlights != null)
            {
                materialEditor.ShaderProperty(advancedOptions.highlights, URPPlusStyles.highlightsText);
                materialEditor.ShaderProperty(staticSpecularHighlight, URPPlusStyles.staticHighlightText);
                materialEditor.ShaderProperty(advancedOptions.reflections, URPPlusStyles.reflectionsText);
            }

            EditorGUILayout.Space();

            base.DrawAdvancedOptions(material);

            EditorGUILayout.EndVertical();
        }

        public static void SetMaterialKeywords(Material material)
        {
            SetMaterialProperties.UpdateMaterialSurfaceOptions(material, automaticRenderQueue: true);

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)

            // Setup double sided GI based on Cull state
            if (material.HasProperty("_Cull"))
                material.doubleSidedGI = (RenderFace)material.GetFloat("_Cull") != RenderFace.Front;

            // Temporary fix for lightmapping. TODO: to be replaced with attribute tag.
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", material.GetTexture("_BaseMap"));
                material.SetTextureScale("_MainTex", material.GetTextureScale("_BaseMap"));
                material.SetTextureOffset("_MainTex", material.GetTextureOffset("_BaseMap"));
            }

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", material.GetColor("_BaseColor"));

            // Cull Mode
            if ((RenderFace)material.GetFloat("_Cull") == RenderFace.Both)
                if ((DoubleSidedNormalMode)material.GetFloat("_DoubleSidedNormalMode") != DoubleSidedNormalMode.None)
                    material.EnableKeyword("_DOUBLESIDED_ON");

            // Alpha Clipping
            if (material.HasProperty("_AlphaCutoffEnable"))
                CoreUtils.SetKeyword(material, "_ALPHATEST_ON", material.GetFloat("_AlphaCutoffEnable") >= 0.5f);

            if (material.HasProperty("_UseShadowThreshold"))
                CoreUtils.SetKeyword(material, "_SHADOW_CUTOFF", material.GetFloat("_UseShadowThreshold") >= 0.5f);

            // Specular AA
            if (material.HasProperty("_EnableGeometricSpecularAA"))
                CoreUtils.SetKeyword(material, "_ENABLE_GEOMETRIC_SPECULAR_AA", material.GetFloat("_EnableGeometricSpecularAA") == 1.0f);

            // Normal Map
            if (material.HasProperty("_BumpMap"))
                CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

            // AO Map
            if (material.HasProperty("_AmbientOcclusionMap"))
                CoreUtils.SetKeyword(material, "_AO_MAP", material.GetTexture("_AmbientOcclusionMap"));

            // Smoothness Map
            if (material.HasProperty("_SmoothnessMaskMap"))
                CoreUtils.SetKeyword(material, "_SMOOTHNESS_MASK", material.GetTexture("_SmoothnessMaskMap"));

            // Additional Options 
            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetFloat("_SpecularHighlights") == 0.0f);
            
            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_STATICSPECULARHIGHLIGHT_OFF", material.GetFloat("_StaticSpecularHighlight") == 0.0f);
            
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF", material.GetFloat("_EnvironmentReflections") == 0.0f);
        }
    }
}