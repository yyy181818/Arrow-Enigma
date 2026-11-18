using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public class EyeGUI : BaseShaderGUI
    { 
        private MaterialProperty enableMydriasisMiosis;
        private MaterialProperty sunSensitivity;
        private MaterialProperty enableLightSensitivity;
        private MaterialProperty lightSensitivity;
        private MaterialProperty pupilFactorMin;
        private MaterialProperty pupilFactorMax;

		private MaterialProperty scleraMap;
        private MaterialProperty scleraSmoothness;
        private MaterialProperty scleraNormalMap;
        private MaterialProperty scleraNormalStrength;

        private MaterialProperty irisMap;
		private MaterialProperty irisClampColor;
        private MaterialProperty irisNormalMap;
		private MaterialProperty irisNormalStrength;
        private MaterialProperty irisOffset;

		private MaterialProperty pupilRadius;
		private MaterialProperty pupilAperture;
        private MaterialProperty minimalPupilAperture;
		private MaterialProperty maximalPupilAperture;

		private MaterialProperty limbalRingSizeIris;
		private MaterialProperty limbalRingSizeSclera;
        private MaterialProperty limbalRingFade;
        private MaterialProperty limbalRingIntensity;

        private MaterialProperty corneaSmoothness;

        private MaterialProperty meshScale;
        private MaterialProperty positionOffset;

        private PropertyStructures.SurfaceOptions surfaceOptions;
        private PropertyStructures.EmissionInputs emissionInputs;
        private PropertyStructures.AdvancedOptions advancedOptions;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            // Surface Options Props
            surfaceOptions = new PropertyStructures.SurfaceOptions(properties);
            enableMydriasisMiosis = BaseShaderGUI.FindProperty("_EnableMydriasis", properties, false);
            sunSensitivity = BaseShaderGUI.FindProperty("_SunSensitivity", properties, false);
            enableLightSensitivity = BaseShaderGUI.FindProperty("_EnableLightSensitivity", properties, false);
            lightSensitivity = BaseShaderGUI.FindProperty("_LightSensitivity", properties, false);
            pupilFactorMin = BaseShaderGUI.FindProperty("_PupilFactorMin", properties, false);
            pupilFactorMax = BaseShaderGUI.FindProperty("_PupilFactorMax", properties, false);

            // Sclera properties
            scleraMap = BaseShaderGUI.FindProperty("_BaseMap", properties, false);
            scleraSmoothness = BaseShaderGUI.FindProperty("_ScleraSmoothness", properties, false);
            scleraNormalMap = BaseShaderGUI.FindProperty("_ScleraNormalMap", properties, false);
            scleraNormalStrength = BaseShaderGUI.FindProperty("_ScleraNormalScale", properties, false);

            // Iris properties
            irisMap = BaseShaderGUI.FindProperty("_IrisMap", properties, false);
            irisClampColor = BaseShaderGUI.FindProperty("_IrisClampColor", properties, false);
            irisNormalMap = BaseShaderGUI.FindProperty("_IrisNormalMap", properties, false);
            irisNormalStrength = BaseShaderGUI.FindProperty("_IrisNormalScale", properties, false);
		    irisOffset = BaseShaderGUI.FindProperty("_IrisOffset", properties, false);

            // Pupil properties
            pupilRadius = BaseShaderGUI.FindProperty("_PupilRadius", properties, false);
            pupilAperture = BaseShaderGUI.FindProperty("_PupilAperture", properties, false);
            minimalPupilAperture = BaseShaderGUI.FindProperty("_MinimalPupilAperture", properties, false);
            maximalPupilAperture = BaseShaderGUI.FindProperty("_MaximalPupilAperture", properties, false);

            // Limbal Ring properties
            limbalRingSizeIris = BaseShaderGUI.FindProperty("_LimbalRingSizeIris", properties, false);
		    limbalRingSizeSclera = BaseShaderGUI.FindProperty("_LimbalRingSizeSclera", properties, false);
		    limbalRingFade = BaseShaderGUI.FindProperty("_LimbalRingFade", properties, false);
            limbalRingIntensity = BaseShaderGUI.FindProperty("_LimbalRingIntensity", properties, false);

            // Cornea properties
            corneaSmoothness = BaseShaderGUI.FindProperty("_CorneaSmoothness", properties, false);

            // Geometry properties
            meshScale = BaseShaderGUI.FindProperty("_MeshScale", properties, false);
            positionOffset = BaseShaderGUI.FindProperty("_PositionOffset", properties, false);

            // Emission Props
            emissionInputs = new PropertyStructures.EmissionInputs(properties);

            // Advanced Props
            advancedOptions = new PropertyStructures.AdvancedOptions(properties);
        }

		public override void ValidateMaterial(Material material) => SetMaterialKeywords(material);
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

            // SurfaceInputs
            using (var header = new MaterialHeaderScope(Styles.SurfaceInputs, (uint)LitExpandable.SurfaceInputs, materialEditor))
                if (header.expanded)
                    DrawSurfaceInputs(material);

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

            // Mydriasis/Miosis
            materialEditor.DrawFloatToggleProperty(new GUIContent("Enable Mydriasis/Miosis"), enableMydriasisMiosis);
            if (enableMydriasisMiosis.floatValue == 1.0f)
            {
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(sunSensitivity, "Sun Sensitivity");
                materialEditor.DrawFloatToggleProperty(new GUIContent("Enable Light Sensitivity"), enableLightSensitivity);
                if (enableLightSensitivity.floatValue == 1.0f)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(lightSensitivity, "Light Sensitivity");
                    EditorGUI.indentLevel--;
                }
                materialEditor.ShaderProperty(pupilFactorMin, "Pupil Factor Min");
                materialEditor.ShaderProperty(pupilFactorMax, "Pupil Factor Max");
                EditorGUI.indentLevel--;
	        }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            surfaceOptions.DrawSpecularAAArea(materialEditor);
            EditorGUILayout.EndVertical();
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            //Sclera Block
            materialEditor.BeginBlock("Sclera Block");
            materialEditor.TexturePropertySingleLine(new GUIContent("Sclera Map"), scleraMap);
            materialEditor.RangeProperty(scleraSmoothness, "Sclera Smoothness");
            materialEditor.DrawCustomNormalArea(new GUIContent("Sclera Normal Map"), scleraNormalMap, scleraNormalStrength);
            materialEditor.EndBlock();

            //Iris Block
            materialEditor.BeginBlock("Iris Block");
            materialEditor.TexturePropertySingleLine(new GUIContent("Iris Map"), irisMap);
            materialEditor.ColorProperty(irisClampColor, "Iris ClampColor");
            materialEditor.RangeProperty(irisOffset, "Iris Offset");
            materialEditor.DrawCustomNormalArea(new GUIContent("Iris Normal Map"), irisNormalMap, irisNormalStrength);
            materialEditor.EndBlock();

            //Pupil Block
            materialEditor.BeginBlock("Pupil Block");
            materialEditor.RangeProperty(pupilRadius, "Pupil Radius");
            materialEditor.RangeProperty(pupilAperture, "Pupil Aperture");
            materialEditor.RangeProperty(minimalPupilAperture, "MinimalPupil Aperture");
            materialEditor.RangeProperty(maximalPupilAperture, "MaximalPupil Aperture");
            materialEditor.EndBlock();

            //Limbal Ring
            materialEditor.BeginBlock("Limbal Ring Block");
            materialEditor.RangeProperty(limbalRingSizeIris, "LimbalRingSizeIris");
            materialEditor.RangeProperty(limbalRingSizeSclera, "LimbalRingSizeSclera");
            materialEditor.RangeProperty(limbalRingFade, "LimbalRingFade");
            materialEditor.RangeProperty(limbalRingIntensity, "LimbalRingIntensity");
            materialEditor.EndBlock();

            //Cornea
            materialEditor.BeginBlock("Cornea Block");
            materialEditor.RangeProperty(corneaSmoothness, "Cornea Smoothness");
            materialEditor.EndBlock();

            //Geometry
            materialEditor.BeginBlock("Geometry Block");
            materialEditor.FloatProperty(meshScale, "Mesh Scale");
            materialEditor.VectorProperty(positionOffset, "Position Offset");
            materialEditor.EndBlock();
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.receiveShadowText, receiveShadowsProp);
            if (advancedOptions.reflections != null && advancedOptions.highlights != null)
            {
                materialEditor.ShaderProperty(advancedOptions.highlights, URPPlusStyles.highlightsText);
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

            // Temporary fix for lightmapping. TODO: to be replaced with attribute tag.
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", material.GetTexture("_BaseMap"));
                material.SetTextureScale("_MainTex", material.GetTextureScale("_BaseMap"));
                material.SetTextureOffset("_MainTex", material.GetTextureOffset("_BaseMap"));
            }

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", material.GetColor("_BaseColor"));

            // Enable Mydriasis
            if (material.HasProperty("_EnableMydriasis"))
                CoreUtils.SetKeyword(material, "_ENABLE_MYDRIASIS_MIOSIS", material.GetFloat("_EnableMydriasis") == 1.0f);
            
            // Enable LightSensitivity
            if (material.HasProperty("_EnableLightSensitivity"))
                CoreUtils.SetKeyword(material, "_ENABLE_LIGHT_SENSITIVITY", material.GetFloat("_EnableLightSensitivity") == 1.0f);

            // Specular AA
            if (material.HasProperty("_EnableGeometricSpecularAA"))
                CoreUtils.SetKeyword(material, "_ENABLE_GEOMETRIC_SPECULAR_AA", material.GetFloat("_EnableGeometricSpecularAA") == 1.0f);

            // Normal Map
            if (material.HasProperty("_ScleraNormalMap") && material.HasProperty("_IrisNormalMap"))
                CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_ScleraNormalMap") || material.GetTexture("_IrisNormalMap"));

            // Emission
            if (material.HasProperty("_EmissionColor"))
                CoreUtils.SetKeyword(material, "_EMISSION", material.GetColor("_EmissionColor") != new Color(0, 0, 0));
            
            if (material.HasProperty("_AlbedoAffectEmissive"))
                CoreUtils.SetKeyword(material, "_EMISSION_WITH_BASE", material.GetFloat("_AlbedoAffectEmissive") == 1.0f);

            // Additional Options 
            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetFloat("_SpecularHighlights") == 0.0f);
            
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF", material.GetFloat("_EnvironmentReflections") == 0.0f);
        }
	}
}