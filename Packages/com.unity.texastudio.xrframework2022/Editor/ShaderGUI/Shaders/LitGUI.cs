using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public class LitShaderGUI : BaseShaderGUI
    {
        private MaterialProperty baseColor;
		private MaterialProperty baseMap;
        private MaterialProperty maskMap;
        private MaterialProperty metallic;
        private MaterialProperty metallicMin;
        private MaterialProperty metallicMax;
        private MaterialProperty smoothness;
		private MaterialProperty smoothnessMin;
        private MaterialProperty smoothnessMax;
        private MaterialProperty aoMin;
        private MaterialProperty aoMax;
        private MaterialProperty bumpMap;
		private MaterialProperty bumpMapScale;
        private MaterialProperty bentNormalMap;
        private MaterialProperty specularMap;
        private MaterialProperty specularColor;
        private MaterialProperty clearCoatMap;
        private MaterialProperty clearCoatMask;
        private MaterialProperty clearCoatSmoothness;
        private MaterialProperty coatNormalMap;
        private MaterialProperty coatNormalScale;
        
		private MaterialProperty detailMap;
		private MaterialProperty detailAlbedoScale;
		private MaterialProperty detailNormalScale;
		private MaterialProperty detailSmoothnessScale;

        private PropertyStructures.SurfaceOptions surfaceOptions;
        private PropertyStructures.DisplacementBlock displacementBlock;
        private PropertyStructures.TessellationOptions tessellationOptions;
        private PropertyStructures.TransparencyInputs transparencyInputs;
        private PropertyStructures.EmissionInputs emissionInputs;
        private PropertyStructures.AdvancedOptions advancedOptions;
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            
            // Surface Options Props
            surfaceOptions = new PropertyStructures.SurfaceOptions(properties);
            displacementBlock = new PropertyStructures.DisplacementBlock(properties);

            // Tessellation Options Props
            tessellationOptions = new PropertyStructures.TessellationOptions(properties);

            // Surface Input Props
            // BaseMap properties
            baseMap = BaseShaderGUI.FindProperty("_BaseMap", properties, false); 
            baseColor = BaseShaderGUI.FindProperty("_BaseColor", properties, false);

            // MaskMap properties
            maskMap = BaseShaderGUI.FindProperty("_MaskMap", properties, false);
            metallic = BaseShaderGUI.FindProperty("_Metallic", properties, false);
            metallicMin = BaseShaderGUI.FindProperty("_MetallicRemapMin", properties, false);
            metallicMax = BaseShaderGUI.FindProperty("_MetallicRemapMax", properties, false);
            smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
            smoothnessMin = BaseShaderGUI.FindProperty("_SmoothnessRemapMin", properties, false);
            smoothnessMax = BaseShaderGUI.FindProperty("_SmoothnessRemapMax", properties, false);
            aoMin = BaseShaderGUI.FindProperty("_AORemapMin", properties, false);
            aoMax = BaseShaderGUI.FindProperty("_AORemapMax", properties, false);

            // NormalMap properties
            bumpMap = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
            bumpMapScale = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
            bentNormalMap = BaseShaderGUI.FindProperty("_BentNormalMap", properties, false);

            // SpecularMap properties
            specularColor = BaseShaderGUI.FindProperty("_SpecularColor", properties, false);
            specularMap = BaseShaderGUI.FindProperty("_SpecularColorMap", properties, false);

            // ClearCoat
            clearCoatMap = BaseShaderGUI.FindProperty("_ClearCoatMap", properties, false);
            clearCoatMask = BaseShaderGUI.FindProperty("_ClearCoatMask", properties, false);
            clearCoatSmoothness = BaseShaderGUI.FindProperty("_ClearCoatSmoothness", properties, false);
            coatNormalMap = BaseShaderGUI.FindProperty("_CoatNormalMap", properties, false);
            coatNormalScale = BaseShaderGUI.FindProperty("_CoatNormalScale", properties, false);

			// Detail properties
			detailMap = BaseShaderGUI.FindProperty("_DetailMap", properties, false);
            detailAlbedoScale = BaseShaderGUI.FindProperty("_DetailAlbedoScale", properties, false);
			detailNormalScale = BaseShaderGUI.FindProperty("_DetailNormalScale", properties, false);
			detailSmoothnessScale = BaseShaderGUI.FindProperty("_DetailSmoothnessScale", properties, false);

            //Transparency props
            transparencyInputs = new PropertyStructures.TransparencyInputs(properties);
			
            // Emission Props
            emissionInputs = new PropertyStructures.EmissionInputs(properties);

            // Advanced Props
            advancedOptions = new PropertyStructures.AdvancedOptions(properties);

        }

        public override void ValidateMaterial(Material material) => SetMaterialProperties.SetLitMaterialKeywords(material);

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException("materialEditorIn");

            materialEditor = materialEditorIn;
            Material material = materialEditor.target as Material;

            FindProperties(properties);   // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

            // SurfaceOptions
            using (var header = new MaterialHeaderScope(Styles.SurfaceOptions, (uint)LitExpandable.SurfaceOptions, materialEditor))
                if (header.expanded)
                    DrawSurfaceOptions(material);
            
            // Tessellation
            if (material != null && material.HasProperty("_TessellationMode"))
                using (var header = new MaterialHeaderScope(URPPlusStyles.TessellationOptions, (uint)LitExpandable.Tessellation, materialEditor))
                    if (header.expanded)
                        tessellationOptions.DrawTessellationOptions(materialEditor, material);

            // SurfaceInputs
            using (var header = new MaterialHeaderScope(Styles.SurfaceInputs, (uint)LitExpandable.SurfaceInputs, materialEditor))
                if (header.expanded)
                    DrawSurfaceInputs(material);

            // TransparencyInputs
            if ((surfaceOptions.surfaceType != null) && ((SurfaceType)surfaceOptions.surfaceType.floatValue == SurfaceType.Transparent))
                using (var header = new MaterialHeaderScope(URPPlusStyles.TransparencyInputs, (uint)LitExpandable.Transparency, materialEditor))
                    if (header.expanded)
                        transparencyInputs.DrawTransparencyInputs(materialEditor, material);

            // EmissionInputs
            using (var header = new MaterialHeaderScope(URPPlusStyles.EmissionInputs, (uint)LitExpandable.Emission, materialEditor))
                if (header.expanded)
                    emissionInputs.DrawEmissionInputs(materialEditor, material);

            // AdvancedOptions
            using (var header = new MaterialHeaderScope(Styles.AdvancedLabel, (uint)LitExpandable.Advanced, materialEditor))
                if (header.expanded)
                    DrawAdvancedOptions(material);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;
            surfaceOptions.DrawBaseSurfaceOptions(materialEditor, material);
            surfaceOptions.DrawAlphaCutoffGUI(materialEditor);
            surfaceOptions.DrawSpecularAAArea(materialEditor);
            displacementBlock.DrawDisplacementGUI(materialEditor, material);
            bool pixelDisplacementMode = (DisplacementMode)displacementBlock.displacementMode.floatValue == DisplacementMode.PixelDisplacement;
            if(material.HasProperty("_PPDLodThreshold") && pixelDisplacementMode)
            {
                EditorGUI.indentLevel++;
                materialEditor.MinFloatShaderProperty(displacementBlock.ppdPrimitiveLength, URPPlusStyles.ppdPrimitiveLength, 0.01f);
                materialEditor.MinFloatShaderProperty(displacementBlock.ppdPrimitiveWidth, URPPlusStyles.ppdPrimitiveWidth, 0.01f);
                displacementBlock.invPrimScale.vectorValue = new Vector4(1.0f / displacementBlock.ppdPrimitiveLength.floatValue, 1.0f / displacementBlock.ppdPrimitiveWidth.floatValue);
                materialEditor.DrawFloatToggleProperty(new GUIContent("DepthOffset"), displacementBlock.depthOffset);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            /*****Base Block*****/
            materialEditor.BeginBlock("Base Block");
            DrawBaseProperties(material);
            materialEditor.DrawMaskMapArea(surfaceOptions.workflowMode, maskMap, metallic, smoothness, 
                                            metallicMin, metallicMax, smoothnessMin, smoothnessMax, aoMin, aoMax);

            if((WorkflowMode)surfaceOptions.workflowMode.floatValue == WorkflowMode.Specular)
                materialEditor.TexturePropertySingleLine(URPPlusStyles.specularColorText, specularMap, specularColor);

            BaseShaderGUI.DrawNormalArea(materialEditor, bumpMap, bumpMapScale);
            materialEditor.TexturePropertySingleLine(URPPlusStyles.bentNormalMapText, bentNormalMap);

            //ClearCoat
            materialEditor.TexturePropertySingleLine(URPPlusStyles.clearCoatMaskText, clearCoatMap, clearCoatMask);
            if (clearCoatMask.floatValue > 0) 
            {
                materialEditor.ShaderProperty(clearCoatSmoothness, URPPlusStyles.clearCoatSmoothnessText);
                if(advancedOptions.coatNormalEnabled.floatValue == 1)
                {
                    materialEditor.TexturePropertySingleLine(URPPlusStyles.coatNormalMapText, coatNormalMap, coatNormalMap.textureValue != null ? coatNormalScale : null);
                }
            }

			materialEditor.TextureScaleOffsetProperty(baseMap);
            materialEditor.EndBlock();

            
            /*****Height Block*****/
            DisplacementMode displacementModeEnum = (DisplacementMode)displacementBlock.displacementMode.floatValue;
            if(displacementModeEnum != DisplacementMode.None)
                displacementBlock.DrawHeightBlock(materialEditor, material);

            /*****Detail Block*****/
            materialEditor.BeginBlock("Detail Block");
            materialEditor.TexturePropertySingleLine(URPPlusStyles.detailMapNormalText, detailMap);
            if(detailMap.textureValue != null)
            {
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(detailAlbedoScale, URPPlusStyles.detailAlbedoScaleText);
                materialEditor.ShaderProperty(detailNormalScale, URPPlusStyles.detailNormalScaleText);
                materialEditor.ShaderProperty(detailSmoothnessScale, URPPlusStyles.detailSmoothnessScaleText);
                EditorGUI.indentLevel--;

			    materialEditor.TextureScaleOffsetProperty(detailMap);
            }
            materialEditor.EndBlock();
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
                materialEditor.ShaderProperty(advancedOptions.reflections, URPPlusStyles.reflectionsText);
            }

            if(clearCoatMask.floatValue > 0)
                SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.secondaryClearCoatNormalText, advancedOptions.coatNormalEnabled);

            EditorGUILayout.Space();

            base.DrawAdvancedOptions(material);
            EditorGUILayout.EndVertical();

            advancedOptions.DrawSpecularOcclusion(materialEditor);
        }
    }
}