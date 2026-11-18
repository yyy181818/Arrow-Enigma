using System;
using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public class SimpleLitShaderGUI : BaseShaderGUI
    {
        // Surface Input Props
        private MaterialProperty _baseColor;
		private MaterialProperty _baseMap;
        private MaterialProperty _maskMap;
        private MaterialProperty _metallic;
        private MaterialProperty _metallicMin;
        private MaterialProperty _metallicMax;
        private MaterialProperty _smoothness;
		private MaterialProperty _smoothnessMin;
        private MaterialProperty _smoothnessMax;
        private MaterialProperty _aoMin;
        private MaterialProperty _aoMax;
        private MaterialProperty _bumpMap;
		private MaterialProperty _bumpMapScale;
        private MaterialProperty _specularMap;
        private MaterialProperty _specularColor;

        private PropertyStructures.SurfaceOptions _surfaceOptions;
        private PropertyStructures.TransparencyInputs _transparencyInputs;
        private PropertyStructures.PlanarShadowInputs _planarShadowInputs;
        private PropertyStructures.EmissionInputs _emissionInputs;
        private PropertyStructures.AdvancedOptions _advancedOptions;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            // Surface Options Props
            _surfaceOptions = new PropertyStructures.SurfaceOptions(properties);
            
            // Surface Input Props
            // BaseMap properties
            _baseMap = FindProperty("_BaseMap", properties, false); 
            _baseColor = FindProperty("_BaseColor", properties, false);
            // MaskMap properties
            _maskMap = FindProperty("_MaskMap", properties, false);
            _metallic = FindProperty("_Metallic", properties, false);
            _metallicMin = FindProperty("_MetallicRemapMin", properties, false);
            _metallicMax = FindProperty("_MetallicRemapMax", properties, false);
            _smoothness = FindProperty("_Smoothness", properties, false);
            _smoothnessMin = FindProperty("_SmoothnessRemapMin", properties, false);
            _smoothnessMax = FindProperty("_SmoothnessRemapMax", properties, false);
            _aoMin = FindProperty("_AORemapMin", properties, false);
            _aoMax = FindProperty("_AORemapMax", properties, false);
            // NormalMap properties
            _bumpMap = FindProperty("_BumpMap", properties, false);
            _bumpMapScale = FindProperty("_BumpScale", properties, false);
            // SpecularMap properties
            _specularColor = FindProperty("_SpecularColor", properties, false);
            _specularMap = FindProperty("_SpecularColorMap", properties, false);

            //Transparency props
            _transparencyInputs = new PropertyStructures.TransparencyInputs(properties);
            
            // PlanarShadow props
            _planarShadowInputs = new PropertyStructures.PlanarShadowInputs(properties);
            
            // Emission Props
            _emissionInputs = new PropertyStructures.EmissionInputs(properties);

            // Advanced Props
            _advancedOptions = new PropertyStructures.AdvancedOptions(properties);
        }

        public override void ValidateMaterial(Material material) => SetMaterialProperties.SetLitMaterialKeywords(material);

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException(nameof(materialEditorIn));

            materialEditor = materialEditorIn;
            var material = materialEditor.target as Material;

            FindProperties(properties);   // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

            // SurfaceOptions
            using (var header = new MaterialHeaderScope(Styles.SurfaceOptions, (uint)LitExpandable.SurfaceOptions, materialEditor))
                if (header.expanded)
                    DrawSurfaceOptions(material);

            // SurfaceInputs
            using (var header = new MaterialHeaderScope(Styles.SurfaceInputs, (uint)LitExpandable.SurfaceInputs, materialEditor))
                if (header.expanded)
                    DrawSurfaceInputs(material);

            // TransparencyInputs
            if (_surfaceOptions.surfaceType != null && (SurfaceType)_surfaceOptions.surfaceType.floatValue == SurfaceType.Transparent)
                using (var header = new MaterialHeaderScope(URPPlusStyles.TransparencyInputs, (uint)LitExpandable.Transparency, materialEditor))
                    if (header.expanded)
                        _transparencyInputs.DrawTransparencyInputs(materialEditor, material);
            
            // PlanarShadowInputs
            using (var header = new MaterialHeaderScope(URPPlusStyles.PlanarShadowInputs, (uint)LitExpandable.PlanarShadow, materialEditor))
                if (header.expanded)
                    _planarShadowInputs.DrawPlanarShadowInputs(materialEditor, material);
            
            // EmissionInputs
            using (var header = new MaterialHeaderScope(URPPlusStyles.EmissionInputs, (uint)LitExpandable.Emission, materialEditor))
                if (header.expanded)
                    _emissionInputs.DrawEmissionInputs(materialEditor, material);

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
            _surfaceOptions.DrawBaseSurfaceOptions(materialEditor, material);
            _surfaceOptions.DrawAlphaCutoffGUI(materialEditor);
            _surfaceOptions.DrawSpecularAAArea(materialEditor);
            EditorGUILayout.EndVertical();
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawBaseProperties(material);
            materialEditor.DrawMaskMapArea(_surfaceOptions.workflowMode, _maskMap, _metallic, _smoothness, 
                                            _metallicMin, _metallicMax, _smoothnessMin, _smoothnessMax, _aoMin, _aoMax);
            if((WorkflowMode)_surfaceOptions.workflowMode.floatValue == WorkflowMode.Specular)
                materialEditor.TexturePropertySingleLine(URPPlusStyles.specularColorText, _specularMap, _specularColor);
            
            DrawNormalArea(materialEditor, _bumpMap, _bumpMapScale);
			//materialEditor.TexturePropertySingleLine(URPPlusStyles.baseColorText,_baseMap, _baseColor);
            EditorGUILayout.EndVertical();
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if ((SurfaceType)_surfaceOptions.surfaceType.floatValue == SurfaceType.Transparent)
                SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.castShadowText, castShadowsProp);

            SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.receiveShadowText, receiveShadowsProp);
            if (_advancedOptions is { reflections: not null, highlights: not null })
            {
                materialEditor.ShaderProperty(_advancedOptions.highlights, URPPlusStyles.highlightsText);
                materialEditor.ShaderProperty(_advancedOptions.reflections, URPPlusStyles.reflectionsText);
            }

            //HorizonOcclusion
            SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.horizonOcclusionText,_advancedOptions.horizonOcclusion);
            if (Math.Abs(_advancedOptions.horizonOcclusion.floatValue - 1) < 1) 
            {
                EditorGUI.indentLevel++;
	        	materialEditor.ShaderProperty(_advancedOptions.horizonFade, "Horizon Fade");
                EditorGUI.indentLevel--;
	        }
            EditorGUILayout.Space();

            base.DrawAdvancedOptions(material);

            EditorGUILayout.EndVertical();
        }
    }
}