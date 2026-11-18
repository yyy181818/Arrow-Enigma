using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public class ComplexLitShaderGUI : BaseShaderGUI
    {
        private MaterialProperty materialType;
        private MaterialProperty transmissionEnable;
        private MaterialProperty sssShadowsEnable;

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
        private MaterialProperty specularPower;
        private MaterialProperty specularIntensity;

        private MaterialProperty clearCoatMap;
        private MaterialProperty clearCoatMask;
        private MaterialProperty clearCoatSmoothness;
        private MaterialProperty coatNormalEnabled;
        private MaterialProperty coatNormalMap;
        private MaterialProperty coatNormalScale;

        private MaterialProperty sssLUT;
        private MaterialProperty scatteringColor;
        private MaterialProperty scatteredShadowsColor;
        private MaterialProperty transmissionScale;

        private MaterialProperty thickness;
        private MaterialProperty curvature;
        private MaterialProperty thicknessCurvatureMap;
        private MaterialProperty thicknessCurvatureRemap;

        private MaterialProperty translucencyScale;
        private MaterialProperty translucencyPower;
        private MaterialProperty translucencyAmbient;
        private MaterialProperty translucencyDistortion;
        private MaterialProperty translucencyShadowsStrength;

        private MaterialProperty tangentMap;
        private MaterialProperty anisotropyMap;
        private MaterialProperty anisotropy;

        private MaterialProperty iridescenceLUT;
        private MaterialProperty iridescenceShift;
        private MaterialProperty iridescenceThicknessMap;
        private MaterialProperty iridescenceThickness;
        private MaterialProperty iridescenceThicknessRemap;
        private MaterialProperty iridescenceMaskMap;
        private MaterialProperty iridescenceMaskScale;

        private MaterialProperty detailMap;
        private MaterialProperty detailAlbedoScale;
        private MaterialProperty detailNormalScale;
        private MaterialProperty detailSmoothnessScale;

        private PropertyStructures.SurfaceOptions surfaceOptions;
        private PropertyStructures.DisplacementBlock displacementBlock;
        private PropertyStructures.TessellationOptions tessellationOptions;
        private PropertyStructures.TransparencyInputs transparencyInputs;
        private PropertyStructures.PlanarShadowInputs planarShadowInputs;
        private PropertyStructures.EmissionInputs emissionInputs;
        private PropertyStructures.AdvancedOptions advancedOptions;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            // Surface Options Props
            surfaceOptions = new PropertyStructures.SurfaceOptions(properties);

            materialType = BaseShaderGUI.FindProperty("_MaterialType", properties, false);
            transmissionEnable = BaseShaderGUI.FindProperty("_TransmissionEnable", properties, false);
            sssShadowsEnable = BaseShaderGUI.FindProperty("_SSSShadowsEnable", properties, false);

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
            specularPower = BaseShaderGUI.FindProperty("_SpecularPower", properties, false);
            specularIntensity = BaseShaderGUI.FindProperty("_SpecularIntensity", properties, false);
            specularMap = BaseShaderGUI.FindProperty("_SpecularColorMap", properties, false);
            //ClearCoat
            clearCoatMap = BaseShaderGUI.FindProperty("_ClearCoatMap", properties, false);
            clearCoatMask = BaseShaderGUI.FindProperty("_ClearCoatMask", properties, false);
            clearCoatSmoothness = BaseShaderGUI.FindProperty("_ClearCoatSmoothness", properties, false);
            coatNormalEnabled = BaseShaderGUI.FindProperty("_CoatNormal", properties, false);
            coatNormalMap = BaseShaderGUI.FindProperty("_CoatNormalMap", properties, false);
            coatNormalScale = BaseShaderGUI.FindProperty("_CoatNormalScale", properties, false);

            //SSS
            sssLUT = BaseShaderGUI.FindProperty("_SSSLUT", properties, false);
            scatteringColor = BaseShaderGUI.FindProperty("_ScatteringColor", properties, false);
            scatteredShadowsColor = BaseShaderGUI.FindProperty("_ScatteringShadowsColor", properties, false);
            transmissionScale = BaseShaderGUI.FindProperty("_TransmissionScale", properties, false);
            //Translucency
            thickness = BaseShaderGUI.FindProperty("_Thickness", properties, false);
            curvature = BaseShaderGUI.FindProperty("_Curvature", properties, false);
            thicknessCurvatureMap = BaseShaderGUI.FindProperty("_ThicknessCurvatureMap", properties, false);
            thicknessCurvatureRemap = BaseShaderGUI.FindProperty("_ThicknessCurvatureRemap", properties, false);
            translucencyScale = BaseShaderGUI.FindProperty("_TranslucencyScale", properties, false);
            translucencyPower = BaseShaderGUI.FindProperty("_TranslucencyPower", properties, false);
            translucencyAmbient = BaseShaderGUI.FindProperty("_TranslucencyAmbient", properties, false);
            translucencyDistortion = BaseShaderGUI.FindProperty("_TranslucencyDistortion", properties, false);
            translucencyShadowsStrength = BaseShaderGUI.FindProperty("_TranslucencyShadows", properties, false);
            //Anisotropy properties
            tangentMap = BaseShaderGUI.FindProperty("_TangentMap", properties, false);
            anisotropy = BaseShaderGUI.FindProperty("_Anisotropy", properties, false);
            anisotropyMap = BaseShaderGUI.FindProperty("_AnisotropyMap", properties, false);
            //Iridescence properties
            iridescenceLUT = BaseShaderGUI.FindProperty("_IridescenceLUT", properties, false);
            iridescenceShift = BaseShaderGUI.FindProperty("_IridescenceShift", properties, false);
            iridescenceThicknessMap = BaseShaderGUI.FindProperty("_IridescenceThicknessMap", properties, false);
            iridescenceThickness = BaseShaderGUI.FindProperty("_IridescenceThickness", properties, false);
            iridescenceThicknessRemap = BaseShaderGUI.FindProperty("_IridescenceThicknessRemap", properties, false);
            iridescenceMaskMap = BaseShaderGUI.FindProperty("_IridescenceMaskMap", properties, false);
            iridescenceMaskScale = BaseShaderGUI.FindProperty("_IridescenceMaskScale", properties, false);

            // Detail properties
            detailMap = BaseShaderGUI.FindProperty("_DetailMap", properties, false);
            detailAlbedoScale = BaseShaderGUI.FindProperty("_DetailAlbedoScale", properties, false);
            detailNormalScale = BaseShaderGUI.FindProperty("_DetailNormalScale", properties, false);
            detailSmoothnessScale = BaseShaderGUI.FindProperty("_DetailSmoothnessScale", properties, false);
            
                
            //Transparency props
            transparencyInputs = new PropertyStructures.TransparencyInputs(properties);

            //PlanarShadow props
            planarShadowInputs = new PropertyStructures.PlanarShadowInputs(properties);

            // Emission Props
            emissionInputs = new PropertyStructures.EmissionInputs(properties);

            // Advanced Props
            advancedOptions = new PropertyStructures.AdvancedOptions(properties);
        }

        public override void ValidateMaterial(Material material) =>
            SetMaterialProperties.SetLitMaterialKeywords(material);

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException(nameof(materialEditorIn));

            materialEditor = materialEditorIn;
            var material = materialEditor.target as Material;

            FindProperties(
                properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

            // SurfaceOptions
            using (var header = new MaterialHeaderScope(Styles.SurfaceOptions, (uint)LitExpandable.SurfaceOptions,
                       materialEditor))
                if (header.expanded)
                    DrawSurfaceOptions(material);

            // Tessellation
            if (material != null && material.HasProperty("_TessellationMode"))
                using (var header = new MaterialHeaderScope(URPPlusStyles.TessellationOptions,
                           (uint)LitExpandable.Tessellation, materialEditor))
                    if (header.expanded)
                        tessellationOptions.DrawTessellationOptions(materialEditor, material);

            // SurfaceInputs
            using (var header =
                   new MaterialHeaderScope(Styles.SurfaceInputs, (uint)LitExpandable.SurfaceInputs, materialEditor))
                if (header.expanded)
                    DrawSurfaceInputs(material);

            // TransparencyInputs
            if (surfaceOptions.surfaceType != null &&
                (SurfaceType)surfaceOptions.surfaceType.floatValue == SurfaceType.Transparent)
                using (var header = new MaterialHeaderScope(URPPlusStyles.TransparencyInputs,
                           (uint)LitExpandable.Transparency, materialEditor))
                    if (header.expanded)
                        transparencyInputs.DrawTransparencyInputs(materialEditor, material);

            // PlanarShadowInputs
            using (var header = new MaterialHeaderScope(URPPlusStyles.PlanarShadowInputs,
                       (uint)LitExpandable.PlanarShadow, materialEditor))
                if (header.expanded)
                    planarShadowInputs.DrawPlanarShadowInputs(materialEditor, material);


            // EmissionInputs
            using (var header = new MaterialHeaderScope(URPPlusStyles.EmissionInputs, (uint)LitExpandable.Emission,
                       materialEditor))
                if (header.expanded)
                    emissionInputs.DrawEmissionInputs(materialEditor, material);

            // AdvancedOptions
            using (var header =
                   new MaterialHeaderScope(Styles.AdvancedLabel, (uint)LitExpandable.Advanced, materialEditor))
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

            materialEditor.PopupShaderProperty(materialType, URPPlusStyles.materialIDText,
                Enum.GetNames(typeof(MaterialType)));
            if ((MaterialType)materialType.floatValue == MaterialType.SubsurfaceScattering)
            {
                EditorGUI.indentLevel++;
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.transmissionEnableText, transmissionEnable);
                materialEditor.DrawFloatToggleProperty(new GUIContent("Fake SSS Shadows"), sssShadowsEnable);
                EditorGUI.indentLevel--;
            }

            surfaceOptions.DrawSpecularAAArea(materialEditor);
            displacementBlock.DrawDisplacementGUI(materialEditor, material);
            var pixelDisplacementMode = (DisplacementMode)displacementBlock.displacementMode.floatValue ==
                                        DisplacementMode.PixelDisplacement;
            if (material.HasProperty("_PPDLodThreshold") && pixelDisplacementMode)
            {
                EditorGUI.indentLevel++;
                materialEditor.MinFloatShaderProperty(displacementBlock.ppdPrimitiveLength,
                    URPPlusStyles.ppdPrimitiveLength, 0.01f);
                materialEditor.MinFloatShaderProperty(displacementBlock.ppdPrimitiveWidth,
                    URPPlusStyles.ppdPrimitiveWidth, 0.01f);
                displacementBlock.invPrimScale.vectorValue = new Vector4(
                    1.0f / displacementBlock.ppdPrimitiveLength.floatValue,
                    1.0f / displacementBlock.ppdPrimitiveWidth.floatValue);
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
            //SpecularColor
            if ((WorkflowMode)surfaceOptions.workflowMode.floatValue == WorkflowMode.Specular)
                materialEditor.TexturePropertySingleLine(URPPlusStyles.specularColorText, specularMap, specularColor);
            materialEditor.ShaderProperty(specularPower, URPPlusStyles.specularPowerText);
            materialEditor.ShaderProperty(specularIntensity, URPPlusStyles.specularIntensityText);
            DrawNormalArea(materialEditor, bumpMap, bumpMapScale);
            materialEditor.TexturePropertySingleLine(URPPlusStyles.bentNormalMapText, bentNormalMap);

            //ClearCoat
            materialEditor.TexturePropertySingleLine(URPPlusStyles.clearCoatMaskText, clearCoatMap, clearCoatMask);
            if (clearCoatMask.floatValue > 0)
            {
                materialEditor.ShaderProperty(clearCoatSmoothness, URPPlusStyles.clearCoatSmoothnessText);
                if (Math.Abs(coatNormalEnabled.floatValue - 1) < 1)
                    materialEditor.TexturePropertySingleLine(URPPlusStyles.coatNormalMapText, coatNormalMap,
                        coatNormalMap.textureValue != null ? coatNormalScale : null);
            }

            //Tilling
            materialEditor.TextureScaleOffsetProperty(baseMap);
            materialEditor.EndBlock();

            /*****Height Block*****/
            var displacementModeEnum = (DisplacementMode)displacementBlock.displacementMode.floatValue;
            if (displacementModeEnum != DisplacementMode.None)
                displacementBlock.DrawHeightBlock(materialEditor, material);

            /*****MaterialID Block*****/
            if ((MaterialType)materialType.floatValue != MaterialType.Standard)
                DrawMaterialIDBlock(material);

            /*****Detail Block*****/
            materialEditor.BeginBlock("Detail Block");
            materialEditor.TexturePropertySingleLine(URPPlusStyles.detailMapNormalText, detailMap);
            if (detailMap.textureValue != null)
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

        private void DrawMaterialIDBlock(Material material)
        {
            var blockName = "";
            var materialTypeEnum = (MaterialType)materialType.floatValue;
            blockName = materialTypeEnum switch
            {
                MaterialType.SubsurfaceScattering => "SSS Block",
                MaterialType.Anisotropy => "Anisotropy Block",
                MaterialType.Iridescence => "Iridescence Block",
                MaterialType.Translucency => "Translucency Block",
                _ => blockName
            };
            materialEditor.BeginBlock(blockName);
            switch (materialTypeEnum)
            {
                //SubsurfaceScattering
                case MaterialType.SubsurfaceScattering:
                {
                    if (URPPlusSettings.sssLUT)
                        materialEditor.TexturePropertySingleLine(new GUIContent("SSS LUT"), sssLUT);

                    //ThicknessCurvatureMap
                    if (thicknessCurvatureMap.textureValue != null)
                    {
                        materialEditor.TexturePropertySingleLine(URPPlusStyles.thicknessCurvatureMapText,
                            thicknessCurvatureMap, scatteringColor);
                        EditorGUI.indentLevel++;
                        materialEditor.MinMaxShaderPropertyXY(thicknessCurvatureRemap, 0.0f, 1.0f,
                            URPPlusStyles.thicknessRemapText);
                        materialEditor.MinMaxShaderPropertyZW(thicknessCurvatureRemap, 0.0f, 1.0f,
                            URPPlusStyles.curvatureRemapText);
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        materialEditor.TexturePropertySingleLine(URPPlusStyles.thicknessCurvatureMapText,
                            thicknessCurvatureMap, scatteringColor);
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(thickness, URPPlusStyles.thicknessText);
                        materialEditor.ShaderProperty(curvature, URPPlusStyles.curvatureText);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel++;
                    if (transmissionEnable.floatValue >= 0.5f)
                        materialEditor.ShaderProperty(transmissionScale, URPPlusStyles.transmissionScaleText);

                    if (sssShadowsEnable.floatValue >= 0.5f)
                        materialEditor.ShaderProperty(scatteredShadowsColor, new GUIContent("ScatteredShadows Color"));

                    EditorGUI.indentLevel--;
                    break;
                }
                //Anisotropy
                case MaterialType.Anisotropy:
                    materialEditor.TexturePropertySingleLine(URPPlusStyles.tangentMapText, tangentMap);
                    materialEditor.ShaderProperty(anisotropy, URPPlusStyles.anisotropyText);
                    materialEditor.TexturePropertySingleLine(URPPlusStyles.anisotropyMapText, anisotropyMap);
                    break;
                //Iridescence
                case MaterialType.Iridescence:
                {
                    if (URPPlusSettings.iridescenceLUT)
                        materialEditor.TexturePropertySingleLine(URPPlusStyles.iridescenceLUTText, iridescenceLUT,
                            iridescenceShift);

                    materialEditor.TexturePropertySingleLine(URPPlusStyles.iridescenceMaskText, iridescenceMaskMap,
                        iridescenceMaskScale);
                    if (iridescenceThicknessMap.textureValue != null)
                    {
                        materialEditor.TexturePropertySingleLine(URPPlusStyles.iridescenceThicknessMapText,
                            iridescenceThicknessMap);
                        materialEditor.MinMaxShaderProperty(iridescenceThicknessRemap, 0.0f, 1.0f,
                            URPPlusStyles.iridescenceThicknessRemapText);
                    }
                    else
                    {
                        materialEditor.TexturePropertySingleLine(URPPlusStyles.iridescenceThicknessMapText,
                            iridescenceThicknessMap, iridescenceThickness);
                    }

                    break;
                }
                //Translucency
                case MaterialType.Translucency:
                {
                    materialEditor.TexturePropertySingleLine(URPPlusStyles.thicknessMapText, thicknessCurvatureMap,
                        scatteringColor);

                    if (thicknessCurvatureMap.textureValue != null)
                        materialEditor.MinMaxShaderProperty(thicknessCurvatureRemap, 0.0f, 1.0f,
                            URPPlusStyles.thicknessRemapText);
                    else
                        materialEditor.ShaderProperty(thickness, URPPlusStyles.thicknessText);

                    materialEditor.ShaderProperty(translucencyScale, URPPlusStyles.translucencyScaleText);
                    materialEditor.ShaderProperty(translucencyPower, URPPlusStyles.translucencyPowerText);
                    materialEditor.ShaderProperty(translucencyAmbient, URPPlusStyles.translucencyAmbientText);
                    materialEditor.ShaderProperty(translucencyDistortion, URPPlusStyles.translucencyDistortionText);
                    materialEditor.ShaderProperty(translucencyShadowsStrength, URPPlusStyles.translucencyShadowsText);
                    break;
                }
                case MaterialType.Standard:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
            if (advancedOptions is { reflections: not null, highlights: not null })
            {
                materialEditor.ShaderProperty(advancedOptions.highlights, URPPlusStyles.highlightsText);
                materialEditor.ShaderProperty(advancedOptions.reflections, URPPlusStyles.reflectionsText);
            }

            if (clearCoatMask.floatValue > 0)
                SetMaterialProperties.DrawFloatToggleProperty(URPPlusStyles.secondaryClearCoatNormalText,
                    advancedOptions.coatNormalEnabled);

            EditorGUILayout.Space();

            base.DrawAdvancedOptions(material);
            
            EditorGUILayout.EndVertical();
            //ColorAdjustment
            advancedOptions.DrawColorAdjustment(material,materialEditor);
            //SpecularOcclusion
            advancedOptions.DrawSpecularOcclusion(materialEditor);
            
            
        }
    }
}