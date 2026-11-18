using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public class FabricGUI : BaseShaderGUI
    {

        // Surface Input Props
        private MaterialProperty materialType;

        private MaterialProperty baseColor;
		private MaterialProperty baseMap;
        private MaterialProperty maskMap;
        private MaterialProperty sheenMap;
        private MaterialProperty specularColor;
        private MaterialProperty smoothness;
		private MaterialProperty smoothnessMin;
        private MaterialProperty smoothnessMax;
        private MaterialProperty aoMin;
        private MaterialProperty aoMax;
        private MaterialProperty anisotropy;
        private MaterialProperty bumpMap;
		private MaterialProperty bumpMapScale;
        //Translucency
        private MaterialProperty translucency;
        private MaterialProperty thicknessMap;
        private MaterialProperty thickness;
        private MaterialProperty thicknessRemap;
        private MaterialProperty translucencyColor;
        private MaterialProperty translucencyScale;
        private MaterialProperty translucencyPower;
        private MaterialProperty translucencyAmbient;
        private MaterialProperty translucencyDistortion;
        private MaterialProperty translucencyShadowsStrength;
        //ThreadMap
		private MaterialProperty threadMap;
    	private MaterialProperty threadAOScale;
    	private MaterialProperty threadNormalScale;
    	private MaterialProperty threadSmoothnessScale;
        //FuzzMap
        private MaterialProperty fuzzMap;
        private MaterialProperty fuzzSize;
        private MaterialProperty fuzzScale;

        private PropertyStructures.SurfaceOptions surfaceOptions;
        private PropertyStructures.AdvancedOptions advancedOptions;
        private PropertyStructures.PlanarShadowInputs planarShadowInputs; 
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            
            // Surface Options Props
            surfaceOptions = new PropertyStructures.SurfaceOptions(properties);
            materialType = FindProperty("_MaterialType", properties, false);

            // Surface Input Props
            // BaseMap properties
            baseMap = FindProperty("_BaseMap", properties, false); 
            baseColor = FindProperty("_BaseColor", properties, false);

            // MaskMap properties
            maskMap = FindProperty("_MaskMap", properties, false);
            sheenMap = FindProperty("_SheenMap", properties, false);
            anisotropy = FindProperty("_Anisotropy", properties, false);
            smoothness = FindProperty("_Smoothness", properties, false);
            smoothnessMin = FindProperty("_SmoothnessRemapMin", properties, false);
            smoothnessMax = FindProperty("_SmoothnessRemapMax", properties, false);
            aoMin = FindProperty("_AORemapMin", properties, false);
            aoMax = FindProperty("_AORemapMax", properties, false);

            // NormalMap properties
            bumpMap = FindProperty("_BumpMap", properties, false);
            bumpMapScale = FindProperty("_BumpScale", properties, false);

            // SpecularMap properties
            specularColor = FindProperty("_SpecColor", properties, false);

            //Translucency
            translucency = FindProperty("_Translucency", properties, false);
            thicknessMap = FindProperty("_ThicknessMap", properties, false);
            thickness = FindProperty("_Thickness", properties, false);
            thicknessRemap = FindProperty("_ThicknessRemap", properties, false);
            translucencyColor = FindProperty("_TranslucencyColor", properties, false);
            translucencyScale = FindProperty("_TranslucencyScale", properties, false);
            translucencyPower = FindProperty("_TranslucencyPower", properties, false);
            translucencyAmbient = FindProperty("_TranslucencyAmbient", properties, false);
            translucencyDistortion = FindProperty("_TranslucencyDistortion", properties, false);
            translucencyShadowsStrength = FindProperty("_TranslucencyShadows", properties, false);

			//Thread properties
			threadMap = FindProperty("_ThreadMap", properties, false);
            threadAOScale = FindProperty("_ThreadAOScale", properties, false);
			threadNormalScale = FindProperty("_ThreadNormalScale", properties, false);
			threadSmoothnessScale = FindProperty("_ThreadSmoothnessScale", properties, false);
            fuzzMap = FindProperty("_FuzzMap", properties, false);
            fuzzSize = FindProperty("_FuzzMapScale", properties, false);
            fuzzScale = FindProperty("_FuzzStrength", properties, false);
    
            // Advanced Props
            advancedOptions = new PropertyStructures.AdvancedOptions(properties);
            
            // Planar Shadow Props
            planarShadowInputs = new PropertyStructures.PlanarShadowInputs(properties);
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
            materialEditor.PopupShaderProperty(materialType, URPPlusStyles.materialIDText, Enum.GetNames(typeof(FabricMaterialType)));
            surfaceOptions.DrawSpecularAAArea(materialEditor);
            EditorGUILayout.EndVertical();
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            /*****Base Block*****/
            materialEditor.BeginBlock("Base Block");
            DrawBaseProperties(material);
            //MaskMap
            if(maskMap.textureValue != null)
            {
                materialEditor.MinMaxShaderProperty(smoothnessMin, smoothnessMax, 0.0f, 1.0f, URPPlusStyles.smoothnessRemappingText);
                materialEditor.MinMaxShaderProperty(aoMin, aoMax, 0.0f, 1.0f, URPPlusStyles.aoRemappingText);
            }
            else
            {
                materialEditor.ShaderProperty(smoothness, URPPlusStyles.smoothnessText);
            }

            if((FabricMaterialType)materialType.floatValue == FabricMaterialType.Silk)
                materialEditor.RangeProperty(anisotropy, "Anisotropy");

			materialEditor.TexturePropertySingleLine(URPPlusStyles.maskMapText, maskMap);
            materialEditor.TexturePropertySingleLine(new GUIContent("Sheen Map"), sheenMap, specularColor);
            DrawNormalArea(materialEditor, bumpMap, bumpMapScale);
			materialEditor.TextureScaleOffsetProperty(baseMap);
            materialEditor.EndBlock();
            
            /****Thread Block****/
            materialEditor.BeginBlock("Thread Block");
			materialEditor.TexturePropertySingleLine(new GUIContent("Thread Map"), threadMap);
            if(threadMap.textureValue != null)
            {
                EditorGUI.indentLevel++;
			    materialEditor.ShaderProperty(threadAOScale, "Thread AO Scale");
			    materialEditor.ShaderProperty(threadNormalScale, "Thread Normal Scale");
			    materialEditor.ShaderProperty(threadSmoothnessScale, "Thread Smoothness Scale");
                EditorGUI.indentLevel--;
            }

            //Fuzz
            materialEditor.TexturePropertySingleLine(new GUIContent("Fuzz Map"), fuzzMap);
            if(fuzzMap.textureValue != null)
            {
                EditorGUI.indentLevel++;
			    materialEditor.ShaderProperty(fuzzSize, "Fuzz Size");
			    materialEditor.ShaderProperty(fuzzScale, "Fuzz Scale");
                EditorGUI.indentLevel--;
            }

			materialEditor.TextureScaleOffsetProperty(threadMap);
            materialEditor.EndBlock();

            /****Translucency Block****/
            materialEditor.BeginBlock("Translucency Block");
            materialEditor.DrawFloatToggleProperty(new GUIContent("Translucency"), translucency);
            if(Math.Abs(translucency.floatValue - 1) < 1)
            {
                materialEditor.TexturePropertySingleLine(URPPlusStyles.thicknessMapText, thicknessMap, translucencyColor);
                if(thicknessMap.textureValue != null)
                    materialEditor.MinMaxShaderProperty(thicknessRemap, 0.0f, 1.0f, URPPlusStyles.thicknessRemapText);
                else
                {
                    materialEditor.ShaderProperty(thickness, URPPlusStyles.thicknessText);
                }
                materialEditor.ShaderProperty(translucencyScale, URPPlusStyles.translucencyScaleText);
                materialEditor.ShaderProperty(translucencyPower, URPPlusStyles.translucencyPowerText);
                materialEditor.ShaderProperty(translucencyAmbient, URPPlusStyles.translucencyAmbientText);
                materialEditor.ShaderProperty(translucencyDistortion, URPPlusStyles.translucencyDistortionText);
                materialEditor.ShaderProperty(translucencyShadowsStrength, URPPlusStyles.translucencyShadowsText);
            }
            materialEditor.EndBlock();
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException(nameof(materialEditorIn));
            
            materialEditor = materialEditorIn;
            var material = materialEditor.target as Material;
            
            FindProperties(properties);
            SetMaterialKeywords(material);
            // SurfaceOptions
            using (var header = new MaterialHeaderScope(Styles.SurfaceOptions, (uint)LitExpandable.SurfaceOptions, materialEditor))
                if (header.expanded)
                    DrawSurfaceOptions(material);
            
            // SurfaceInputs
            using (var header = new MaterialHeaderScope(Styles.SurfaceInputs, (uint)LitExpandable.SurfaceInputs, materialEditor))
                if (header.expanded)
                    DrawSurfaceInputs(material);
            
            // PlanarShadowInputs
            using (var header = new MaterialHeaderScope(URPPlusStyles.PlanarShadowInputs, (uint)LitExpandable.PlanarShadow, materialEditor))
                if (header.expanded)
                    planarShadowInputs.DrawPlanarShadowInputs(materialEditor, material);
            
            // Advanced
            using (var header = new MaterialHeaderScope(Styles.AdvancedLabel, (uint)LitExpandable.Advanced, materialEditor))
                if (header.expanded)
                    DrawAdvancedOptions(material);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if ((SurfaceType)surfaceOptions.surfaceType.floatValue == SurfaceType.Transparent)
                SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.castShadowText, castShadowsProp);

            SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.receiveShadowText, receiveShadowsProp);
            if (advancedOptions is { reflections: not null, highlights: not null })
            {
                materialEditor.ShaderProperty(advancedOptions.highlights, URPPlusStyles.highlightsText);
                materialEditor.ShaderProperty(advancedOptions.reflections, URPPlusStyles.reflectionsText);
            }

            EditorGUILayout.Space();
            base.DrawAdvancedOptions(material);

            EditorGUILayout.EndVertical();
        }
        
        private static void SetMaterialKeywords(Material material)
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

            // MaterialType
            if (material.HasProperty("_MaterialType"))
            {
                if ((FabricMaterialType)material.GetFloat("_MaterialType") == FabricMaterialType.CottonWool)
                    material.EnableKeyword("_MATERIAL_FEATURE_SHEEN");
                else
                    material.DisableKeyword("_MATERIAL_FEATURE_SHEEN");
            }
            
            // Specular AA
            if (material.HasProperty("_EnableGeometricSpecularAA"))
                CoreUtils.SetKeyword(material, "_ENABLE_GEOMETRIC_SPECULAR_AA", material.GetFloat("_EnableGeometricSpecularAA") == 1.0f);

            // Normal Map
            if (material.HasProperty("_BumpMap"))
                CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

            // Mask Map
            if (material.HasProperty("_MaskMap"))
                CoreUtils.SetKeyword(material, "_MASKMAP", material.GetTexture("_MaskMap"));

            // Sheen Map
            if (material.HasProperty("_SheenMap"))
                CoreUtils.SetKeyword(material, "_SHEENMAP", material.GetTexture("_SheenMap"));
            
            // Thread Map
            if (material.HasProperty("_ThreadMap"))
                CoreUtils.SetKeyword(material, "_THREADMAP", material.GetTexture("_ThreadMap"));

            // Fuzz Map
            if (material.HasProperty("_FuzzMap"))
                CoreUtils.SetKeyword(material, "_FUZZMAP", material.GetTexture("_FuzzMap"));

            // Translucency
            if (material.HasProperty("_Translucency"))
                CoreUtils.SetKeyword(material, "_MATERIAL_FEATURE_TRANSLUCENCY", Math.Abs(material.GetFloat("_Translucency") - 1.0f) < 1);

            // Thickness Map
            if (material.HasProperty("_ThicknessMap"))
                CoreUtils.SetKeyword(material, "_THICKNESSMAP", material.GetTexture("_ThicknessMap"));

            // Additional Options 
            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetFloat("_SpecularHighlights") == 0.0f);
            
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF", material.GetFloat("_EnvironmentReflections") == 0.0f);
        }
    }
}