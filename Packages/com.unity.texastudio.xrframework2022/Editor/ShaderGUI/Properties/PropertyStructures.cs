using System;
using UnityEngine;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

namespace UnityEditor
{
    public class PropertyStructures : ShaderGUI
    {
        public struct SurfaceOptions
        {
            public MaterialProperty workflowMode;
            public MaterialProperty surfaceType;
            public MaterialProperty blendMode;
            public MaterialProperty alphaCutoffEnable;
            public MaterialProperty alphaCutoff;
            public MaterialProperty useShadowThreshold;
            public MaterialProperty alphaCutoffShadow;
            public MaterialProperty alphaToMask;
            public MaterialProperty depthWrite;
            public MaterialProperty depthTest;
            public MaterialProperty renderFace;
            public MaterialProperty doubleSidedNormalMode;
            public MaterialProperty specularAA;
            public MaterialProperty specularAAScreenSpaceVariance;
            public MaterialProperty specularAAThreshold;

            public SurfaceOptions(MaterialProperty[] properties)
            {
                workflowMode = BaseShaderGUI.FindProperty("_WorkflowMode", properties, false);
                surfaceType = BaseShaderGUI.FindProperty("_Surface", properties, false);
                blendMode = BaseShaderGUI.FindProperty("_Blend", properties, false);
                depthWrite = BaseShaderGUI.FindProperty("_ZWrite", properties, false);
                depthTest = BaseShaderGUI.FindProperty("_ZTest", properties, false);
                alphaCutoffEnable = BaseShaderGUI.FindProperty("_AlphaCutoffEnable", properties, false);
                alphaCutoff = BaseShaderGUI.FindProperty("_Cutoff", properties, false);
                useShadowThreshold = BaseShaderGUI.FindProperty("_UseShadowThreshold", properties, false);
                alphaCutoffShadow = BaseShaderGUI.FindProperty("_AlphaCutoffShadow", properties, false);
                alphaToMask = BaseShaderGUI.FindProperty("_AlphaToMask", properties, false);
                renderFace = BaseShaderGUI.FindProperty("_Cull", properties, false);
                doubleSidedNormalMode = BaseShaderGUI.FindProperty("_DoubleSidedNormalMode", properties, false);
                specularAA = BaseShaderGUI.FindProperty("_EnableGeometricSpecularAA", properties, false);
                specularAAScreenSpaceVariance =
                    BaseShaderGUI.FindProperty("_SpecularAAScreenSpaceVariance", properties, false);
                specularAAThreshold = BaseShaderGUI.FindProperty("_SpecularAAThreshold", properties, false);
            }

            private void DoPopup(GUIContent label, MaterialEditor materialEditor, MaterialProperty property,
                string[] options)
            {
                if (property == null)
                    return;

                materialEditor.PopupShaderProperty(property, label, options);
            }

            public void DrawBaseSurfaceOptions(MaterialEditor materialEditor, Material material)
            {
                DoPopup(URPPlusStyles.workflowModeText, materialEditor, workflowMode,
                    Enum.GetNames(typeof(WorkflowMode)));
                DoPopup(URPPlusStyles.surfaceTypeText, materialEditor, surfaceType, Enum.GetNames(typeof(SurfaceType)));
                EditorGUI.indentLevel++;
                if ((surfaceType != null) && ((SurfaceType)surfaceType.floatValue == SurfaceType.Transparent))
                {
                    DoPopup(URPPlusStyles.blendingModeText, materialEditor, blendMode,
                        Enum.GetNames(typeof(BlendMode)));
                    //DepthWrite and DepthTest
                    materialEditor.DrawFloatToggleProperty(URPPlusStyles.zWriteEnableText, depthWrite);
                    if (depthTest != null)
                        materialEditor.PopupShaderProperty(depthTest, URPPlusStyles.transparentZTestText,
                            Enum.GetNames(typeof(UnityEngine.Rendering.CompareFunction)));
                }

                DoPopup(URPPlusStyles.cullingText, materialEditor, renderFace, Enum.GetNames(typeof(RenderFace)));

                if (renderFace != null)
                {
                    if ((RenderFace)material.GetFloat("_Cull") == RenderFace.Both)
                    {
                        DoubleSidedNormalMode doubleSidedNormalModeEnum =
                            (DoubleSidedNormalMode)doubleSidedNormalMode.floatValue;
                        switch (doubleSidedNormalModeEnum)
                        {
                            case DoubleSidedNormalMode.Mirror: // Mirror mode (in tangent space)
                                material.SetVector("_DoubleSidedConstants", new Vector4(1.0f, 1.0f, -1.0f, 0.0f));
                                break;
                            case DoubleSidedNormalMode.Flip: // Flip mode (in tangent space)
                                material.SetVector("_DoubleSidedConstants", new Vector4(-1.0f, -1.0f, -1.0f, 0.0f));
                                break;
                            case DoubleSidedNormalMode.None: // None mode (in tangent space)
                                material.SetVector("_DoubleSidedConstants", new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (doubleSidedNormalMode != null)
                            materialEditor.ShaderProperty(doubleSidedNormalMode,
                                URPPlusStyles.doubleSidedNormalModeText);
                    }
                }

                EditorGUI.indentLevel--;
            }

            public void DrawAlphaCutoffGUI(MaterialEditor materialEditor)
            {
                if (alphaCutoffEnable != null)
                    materialEditor.ShaderProperty(alphaCutoffEnable, URPPlusStyles.alphaCutoffEnableText);

                if (alphaCutoffEnable == null || !(Math.Abs(alphaCutoffEnable.floatValue - 1.0f) < 1)) return;
                EditorGUI.indentLevel++;
                if (alphaCutoff != null)
                    materialEditor.ShaderProperty(alphaCutoff, URPPlusStyles.alphaCutoffText);

                var showUseShadowThreshold = useShadowThreshold != null;

                if (showUseShadowThreshold)
                    materialEditor.ShaderProperty(useShadowThreshold, URPPlusStyles.useShadowThresholdText);

                if (alphaCutoffShadow != null && useShadowThreshold != null &&
                    Math.Abs(useShadowThreshold.floatValue - 1.0f) < 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(alphaCutoffShadow, URPPlusStyles.alphaCutoffShadowText);
                    EditorGUI.indentLevel--;
                }

                materialEditor.DrawFloatToggleProperty(URPPlusStyles.alphaToMaskText, alphaToMask);
                EditorGUI.indentLevel--;
            }

            public void DrawSpecularAAArea(MaterialEditor materialEditor)
            {
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.enableGeometricSpecularAAText, specularAA);
                var specularAAToggle = Math.Abs(specularAA.floatValue - 1) < 1;
                if (!specularAAToggle) return;
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(specularAAScreenSpaceVariance,
                    URPPlusStyles.specularAAScreenSpaceVarianceText);
                materialEditor.ShaderProperty(specularAAThreshold, URPPlusStyles.specularAAThresholdText);
                EditorGUI.indentLevel--;
            }
        }

        public struct LayeredSurfaceInputsProperties
        {
            // Surface Input Prop
            public MaterialProperty layerMask;
            public MaterialProperty layerCount;
            public MaterialProperty vertexColorMode;
            public MaterialProperty mainInfluenceProp;
            public MaterialProperty heightBasedBlendingProp;
            public MaterialProperty heightTransitionProp;
            public MaterialProperty layerInfluenceMaskMap;

            public LayeredSurfaceInputsProperties(MaterialProperty[] properties)
            {
                // Surface Inputs Props
                layerMask = BaseShaderGUI.FindProperty("_LayerMaskMap", properties, false);
                layerInfluenceMaskMap = BaseShaderGUI.FindProperty("_LayerInfluenceMaskMap", properties, false);
                layerCount = BaseShaderGUI.FindProperty("_LayerCount", properties, false);
                vertexColorMode = BaseShaderGUI.FindProperty("_VertexColorMode", properties, false);
                mainInfluenceProp = BaseShaderGUI.FindProperty("_UseMainLayerInfluence", properties, false);
                heightBasedBlendingProp = BaseShaderGUI.FindProperty("_UseHeightBasedBlend", properties, false);
                heightTransitionProp = BaseShaderGUI.FindProperty("_HeightTransition", properties, false);
            }
        }

        public struct DisplacementBlock
        {
            public MaterialProperty displacementMode;
            public MaterialProperty PPDMinSamples;
            public MaterialProperty PPDMaxSamples;
            public MaterialProperty PPDLodThreshold;
            public MaterialProperty ppdPrimitiveLength;
            public MaterialProperty ppdPrimitiveWidth;
            public MaterialProperty invPrimScale;
            public MaterialProperty lockWithObjectScale;
            public MaterialProperty depthOffset;

            // HeightMap properties
            public MaterialProperty heightMap;
            public MaterialProperty heightParametrization;
            public MaterialProperty heightCenter;
            public MaterialProperty heightAmplitude;
            public MaterialProperty heightTessCenter;
            public MaterialProperty heightTessAmplitude;
            public MaterialProperty heightMin;
            public MaterialProperty heightMax;
            public MaterialProperty heightOffset;
            public MaterialProperty heightPoMAmplitude;

            public DisplacementBlock(MaterialProperty[] properties)
            {
                displacementMode = BaseShaderGUI.FindProperty("_DisplacementMode", properties, false);
                PPDMinSamples = BaseShaderGUI.FindProperty("_PPDMinSamples", properties, false);
                PPDMaxSamples = BaseShaderGUI.FindProperty("_PPDMaxSamples", properties, false);
                PPDLodThreshold = BaseShaderGUI.FindProperty("_PPDLodThreshold", properties, false);
                ppdPrimitiveLength = BaseShaderGUI.FindProperty("_PPDPrimitiveLength", properties, false);
                ppdPrimitiveWidth = BaseShaderGUI.FindProperty("_PPDPrimitiveWidth", properties, false);
                invPrimScale = BaseShaderGUI.FindProperty("_InvPrimScale", properties, false);
                lockWithObjectScale = BaseShaderGUI.FindProperty("_DisplacementLockObjectScale", properties, false);
                depthOffset = BaseShaderGUI.FindProperty("_DepthOffsetEnable", properties, false);

                // HeightMap properties
                heightMap = BaseShaderGUI.FindProperty("_HeightMap", properties, false);
                heightParametrization = BaseShaderGUI.FindProperty("_HeightMapParametrization", properties, false);
                heightCenter = BaseShaderGUI.FindProperty("_HeightCenter", properties, false);
                heightAmplitude = BaseShaderGUI.FindProperty("_HeightAmplitude", properties, false);
                heightTessCenter = BaseShaderGUI.FindProperty("_HeightTessCenter", properties, false);
                heightTessAmplitude = BaseShaderGUI.FindProperty("_HeightTessAmplitude", properties, false);
                heightMin = BaseShaderGUI.FindProperty("_HeightMin", properties, false);
                heightMax = BaseShaderGUI.FindProperty("_HeightMax", properties, false);
                heightOffset = BaseShaderGUI.FindProperty("_HeightOffset", properties, false);
                heightPoMAmplitude = BaseShaderGUI.FindProperty("_HeightPoMAmplitude", properties, false);
            }

            public void DrawDisplacementGUI(MaterialEditor materialEditor, Material material)
            {
                var displacementModeEnum = (DisplacementMode)displacementMode.floatValue;
                if (!material.HasProperty("_TessellationMode"))
                {
                    materialEditor.ShaderProperty(displacementMode, URPPlusStyles.displacementModeText);
                    EditorGUI.indentLevel++;
                    if (displacementModeEnum != DisplacementMode.None)
                        materialEditor.DrawFloatToggleProperty(URPPlusStyles.lockWithObjectScaleText,
                            lockWithObjectScale);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    if (displacementModeEnum != DisplacementMode.PixelDisplacement) return;
                    EditorGUI.indentLevel++;
                    materialEditor.IntSliderShaderProperty(PPDMinSamples, URPPlusStyles.ppdMinSamplesText);
                    materialEditor.IntSliderShaderProperty(PPDMaxSamples, URPPlusStyles.ppdMaxSamplesText);
                    materialEditor.ShaderProperty(PPDLodThreshold, URPPlusStyles.ppdLodThresholdText);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    materialEditor.ShaderProperty(displacementMode, URPPlusStyles.displacementModeText);
                    EditorGUI.indentLevel++;
                    if (displacementModeEnum == DisplacementMode.VertexDisplacement)
                        materialEditor.DrawFloatToggleProperty(URPPlusStyles.lockWithObjectScaleText,
                            lockWithObjectScale);
                    EditorGUI.indentLevel--;
                }
            }

            public void DrawHeightBlock(MaterialEditor materialEditor, Material material)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Height Block", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                var displacementModeEnum = (DisplacementMode)displacementMode.floatValue;
                materialEditor.TexturePropertySingleLine(URPPlusStyles.heightMapText, heightMap);
                switch (displacementModeEnum)
                {
                    case DisplacementMode.PixelDisplacement:
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(heightPoMAmplitude, URPPlusStyles.heightMapAmplitudeText);
                        material.SetFloat("_HeightAmplitude",
                            material.GetFloat("_HeightPoMAmplitude") * 0.01f); // Convert centimeters to meters.
                        material.SetFloat("_HeightCenter", 1.0f);
                        EditorGUI.indentLevel--;
                        break;
                    case DisplacementMode.VertexDisplacement:
                    case DisplacementMode.Tessellation:
                    {
                        EditorGUI.indentLevel++;
                        //Height Parametrization
                        EditorGUI.BeginChangeCheck();
                        HeightParametrization heightParametrizationEnum =
                            (HeightParametrization)heightParametrization.floatValue;
                        heightParametrizationEnum =
                            (HeightParametrization)EditorGUILayout.EnumPopup("Parametrization",
                                heightParametrizationEnum);
                        if (EditorGUI.EndChangeCheck())
                        {
                            materialEditor.RegisterPropertyChangeUndo("Displacement Mode");
                            heightParametrization.floatValue = (float)heightParametrizationEnum;
                        }

                        if (heightParametrizationEnum == HeightParametrization.Amplitude)
                        {
                            materialEditor.ShaderProperty(heightTessAmplitude, URPPlusStyles.heightMapAmplitudeText);
                            materialEditor.ShaderProperty(heightTessCenter, URPPlusStyles.heightMapCenterText);

                            float offset = material.GetFloat("_HeightOffset");
                            float center = material.GetFloat("_HeightTessCenter");
                            float amplitude = material.GetFloat("_HeightTessAmplitude");

                            material.SetFloat("_HeightAmplitude", amplitude * 0.01f); // Convert centimeters to meters.
                            material.SetFloat("_HeightCenter", -offset / Mathf.Max(1e-6f, amplitude) + center);
                        }
                        else
                        {
                            materialEditor.ShaderProperty(heightMin, URPPlusStyles.heightMapMinText);
                            materialEditor.ShaderProperty(heightMax, URPPlusStyles.heightMapMaxText);

                            var offset = material.GetFloat("_HeightOffset");
                            var minHeight = material.GetFloat("_HeightMin");
                            var amplitude = material.GetFloat("_HeightMax") - minHeight;

                            material.SetFloat("_HeightAmplitude", amplitude * 0.01f); // Convert centimeters to meters.
                            material.SetFloat("_HeightCenter", -(minHeight + offset) / Mathf.Max(1e-6f, amplitude));
                        }

                        materialEditor.ShaderProperty(heightOffset, URPPlusStyles.heightMapOffsetText);
                        EditorGUI.indentLevel--;
                        break;
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        public struct TessellationOptions
        {
            public MaterialProperty tessellationMode;
            public MaterialProperty phongTessellationEnable;
            public MaterialProperty tessellationShapeFactor;
            public MaterialProperty tessellationFactor;
            public MaterialProperty tessellationEdgeLength;
            public MaterialProperty tessellationFactorMinDistance;
            public MaterialProperty tessellationFactorMaxDistance;
            public MaterialProperty tessellationBackFaceCullEpsilon;

            public TessellationOptions(MaterialProperty[] properties)
            {
                tessellationMode = BaseShaderGUI.FindProperty("_TessellationMode", properties, false);
                phongTessellationEnable = BaseShaderGUI.FindProperty("_PhongTessellationMode", properties, false);
                tessellationShapeFactor = BaseShaderGUI.FindProperty("_TessellationShapeFactor", properties, false);
                tessellationFactor = BaseShaderGUI.FindProperty("_TessellationFactor", properties, false);
                tessellationEdgeLength = BaseShaderGUI.FindProperty("_TessellationEdgeLength", properties, false);
                tessellationFactorMinDistance =
                    BaseShaderGUI.FindProperty("_TessellationFactorMinDistance", properties, false);
                tessellationFactorMaxDistance =
                    BaseShaderGUI.FindProperty("_TessellationFactorMaxDistance", properties, false);
                tessellationBackFaceCullEpsilon =
                    BaseShaderGUI.FindProperty("_TessellationBackFaceCullEpsilon", properties, false);
            }

            public void DrawTessellationOptions(MaterialEditor materialEditor, Material material)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                // Use default labelWidth
                EditorGUIUtility.labelWidth = 0f;

                materialEditor.ShaderProperty(tessellationMode, URPPlusStyles.tessellationModeText);
                TessellationMode tessellationModeEnum = (TessellationMode)tessellationMode.floatValue;
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.phongTessellationText, phongTessellationEnable);

                if (Math.Abs(phongTessellationEnable.floatValue - 1) < 1)
                    materialEditor.ShaderProperty(tessellationShapeFactor, URPPlusStyles.tessellationShapeFactorText);

                switch (tessellationModeEnum)
                {
                    case TessellationMode.EdgeLength:
                        materialEditor.ShaderProperty(tessellationFactor, URPPlusStyles.tessellationFactorText);
                        materialEditor.ShaderProperty(tessellationEdgeLength,
                            URPPlusStyles.tessellationFactorTriangleSizeText);
                        break;
                    case TessellationMode.Distance:
                        materialEditor.ShaderProperty(tessellationFactor, URPPlusStyles.tessellationFactorText);
                        materialEditor.ShaderProperty(tessellationFactorMinDistance,
                            URPPlusStyles.tessellationFactorMinDistanceText);
                        materialEditor.ShaderProperty(tessellationFactorMaxDistance,
                            URPPlusStyles.tessellationFactorMaxDistanceText);
                        break;
                    case TessellationMode.None:
                    default:
                        materialEditor.ShaderProperty(tessellationFactor, URPPlusStyles.tessellationFactorText);
                        break;
                }

                materialEditor.ShaderProperty(tessellationBackFaceCullEpsilon,
                    URPPlusStyles.tessellationBackFaceCullEpsilonText);
                EditorGUILayout.EndVertical();
            }
        }

        public struct TransparencyInputs
        {
            public MaterialProperty refractionEnable;
            public MaterialProperty IOR;
            public MaterialProperty transmittanceColor;
            public MaterialProperty transmittanceColorMap;
            public MaterialProperty chromaticAberrationEnable;
            public MaterialProperty chromaticAberration;

            public TransparencyInputs(MaterialProperty[] properties)
            {
                // Surface Inputs Props
                refractionEnable = BaseShaderGUI.FindProperty("_RefractionEnable", properties, false);
                IOR = BaseShaderGUI.FindProperty("_Ior", properties, false);
                transmittanceColor = BaseShaderGUI.FindProperty("_TransmittanceColor", properties, false);
                transmittanceColorMap = BaseShaderGUI.FindProperty("_TransmittanceColorMap", properties, false);
                chromaticAberrationEnable = BaseShaderGUI.FindProperty("_ChromaticAberrationEnable", properties, false);
                chromaticAberration = BaseShaderGUI.FindProperty("_ChromaticAberration", properties, false);
            }

            public void DrawTransparencyInputs(MaterialEditor materialEditor, Material material)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.enableRefractionText, refractionEnable);
                var refractionToggle = Math.Abs(refractionEnable.floatValue - 1) < 1;
                if (refractionToggle)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(IOR, URPPlusStyles.refractionIorText);
                    materialEditor.TexturePropertySingleLine(URPPlusStyles.transmittanceColorText,
                        transmittanceColorMap, transmittanceColor);
                    materialEditor.DrawFloatToggleProperty(URPPlusStyles.enableChromaticAberrationText,
                        chromaticAberrationEnable);
                    var chromaticAberrationToggle = (Math.Abs(chromaticAberrationEnable.floatValue - 1) < 1);
                    if (chromaticAberrationToggle)
                    {
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(chromaticAberration, URPPlusStyles.chromaticAberrationText);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        public struct PlanarShadowInputs
        {
            private MaterialProperty _EnableClipShadow;
            private MaterialProperty _PlanarShadowEnable;
            private MaterialProperty _ShadowColor;
            private MaterialProperty _ShadowFalloff;
            private MaterialProperty _GlobalHeight;
            private MaterialProperty _LocalHeight;
            private MaterialProperty _ShadowOffset;
            private MaterialProperty _PlanarShadowMode;
            private MaterialProperty _ShadowIntensity;
            private bool _isAddedComponent;
            private bool _setPlanaerShadowCaster;

            public PlanarShadowInputs(MaterialProperty[] properties)
            {
                _EnableClipShadow = BaseShaderGUI.FindProperty("_EnableClipShadow", properties, false);
                _PlanarShadowEnable = BaseShaderGUI.FindProperty("_PlanarShadowEnable", properties, false);
                _ShadowColor = BaseShaderGUI.FindProperty("_PlanarShadowColor", properties, false);
                _ShadowFalloff = BaseShaderGUI.FindProperty("_ShadowFalloff", properties, false);
                _GlobalHeight = BaseShaderGUI.FindProperty("_GlobalHeight", properties, false);
                _LocalHeight = BaseShaderGUI.FindProperty("_LocalHeight", properties, false);
                _ShadowOffset = BaseShaderGUI.FindProperty("_ShadowOffset", properties, false);
                _PlanarShadowMode = BaseShaderGUI.FindProperty("_PlanarShadowMode", properties, false);
                _ShadowIntensity = BaseShaderGUI.FindProperty("_ShadowIntensity", properties, false);
                _isAddedComponent = false;
                _setPlanaerShadowCaster = false;
            }

            public void DrawPlanarShadowInputs(MaterialEditor materialEditor, Material material)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.enablePlanarShadowText, _PlanarShadowEnable);
                if (_PlanarShadowEnable.floatValue == 0)
                {
                    if (material != null) material.SetShaderPassEnabled("UniversalForwardOnly", false);
                    material.DisableKeyword("_PLANARSHADOW_ENABLE");
                }
                else
                {
                    if (material != null) material.SetShaderPassEnabled("UniversalForwardOnly", true);
                    material.EnableKeyword("_PLANARSHADOW_ENABLE");
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(_EnableClipShadow, URPPlusStyles.enableClipShadowText);
                    materialEditor.ShaderProperty(_PlanarShadowMode, URPPlusStyles.planerShadowModeText);
                    switch (_PlanarShadowMode.floatValue)
                    {
                        case 0:
                            _setPlanaerShadowCaster = false;
                            materialEditor.ShaderProperty(_ShadowFalloff, new GUIContent("Shadow Falloff", "阴影衰减"));
                            materialEditor.ShaderProperty(_GlobalHeight, new GUIContent("Ground Height", "地面高度"));
                            break;
                        case 1:
                            _setPlanaerShadowCaster = false;
                            materialEditor.ShaderProperty(_LocalHeight, new GUIContent("Local Height", "阴影高度"));
                            materialEditor.ShaderProperty(_GlobalHeight, new GUIContent("Ground Height", "地面高度"));
                            break;
                        case 2:
                            _setPlanaerShadowCaster = true;
                            materialEditor.ShaderProperty(_ShadowIntensity, new GUIContent("Shadow Intensity", "阴影强度"));
                            break;
                    }

                    materialEditor.ShaderProperty(_ShadowColor, URPPlusStyles.shadowColorText);
                    materialEditor.ShaderProperty(_ShadowOffset, URPPlusStyles.shadowOffsetText);
                    EditorGUI.indentLevel--;
                    if (true)
                    {
                        var obj = Selection.activeGameObject;
                        if (obj is not null)
                        {
                            if (_PlanarShadowMode.floatValue is 2 && !_isAddedComponent)
                            {
                                var objs = obj.GetComponents<PlanaerShadowCaster>();
                                if (objs.Length > 0)
                                {
                                    for (var i = 0; i < objs.Length - 1; i++)
                                    {
                                        UnityEngine.Object.DestroyImmediate(objs[i]);
                                    }

                                    _isAddedComponent = true;
                                }
                                else
                                {
                                    obj.AddComponent<PlanaerShadowCaster>();
                                    _isAddedComponent = true;
                                }
                            }

                            if (_PlanarShadowMode.floatValue is not 2 && _isAddedComponent)
                            {
                                var objs = obj.GetComponents<PlanaerShadowCaster>();
                                foreach (var Object in objs)
                                {
                                    UnityEngine.Object.DestroyImmediate(Object);
                                }

                                _isAddedComponent = false;
                            }
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        public struct EmissionInputs
        {
            public MaterialProperty emissionColor;
            public MaterialProperty emissionMap;
            public MaterialProperty emissionScale;
            public MaterialProperty emissionWithBase;

            public EmissionInputs(MaterialProperty[] properties)
            {
                emissionColor = BaseShaderGUI.FindProperty("_EmissionColor", properties, false);
                emissionMap = BaseShaderGUI.FindProperty("_EmissionMap", properties, false);
                emissionScale = BaseShaderGUI.FindProperty("_EmissionScale", properties, false);
                emissionWithBase = BaseShaderGUI.FindProperty("_AlbedoAffectEmissive", properties, false);
            }

            public void DrawEmissionInputs(MaterialEditor materialEditor, Material material, bool tilling = false)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertyWithHDRColor(URPPlusStyles.emissiveMapText, emissionMap, emissionColor,
                    false);
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(emissionScale, URPPlusStyles.emissiveIntensityText);
                if (EditorGUI.EndChangeCheck())
                {
                    var brightness = emissionColor.colorValue.maxColorComponent;
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                    if (brightness <= 0f)
                    {
                        material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    }
                }

                EditorGUI.indentLevel--;

                //Emission multiply with Base
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.albedoAffectEmissiveText, emissionWithBase);
                materialEditor.LightmapEmissionProperty();
                if (tilling)
                    materialEditor.TextureScaleOffsetProperty(emissionMap);

                EditorGUILayout.EndVertical();
            }
        }

        public struct AdvancedOptions
        {
            public MaterialProperty castShadows;
            public MaterialProperty receiveShadows;
            public MaterialProperty highlights;
            public MaterialProperty reflections;
            public MaterialProperty coatNormalEnabled;

            public MaterialProperty horizonOcclusion;
            public MaterialProperty horizonFade;
            public MaterialProperty specularOcclusionMode;
            public MaterialProperty giOcclusionBias;

            //ColorAdjustment
            private MaterialProperty _EnableAcesToneMapping;
            private MaterialProperty _EnableColorProcessing;
            private MaterialProperty _Brightness;
            private MaterialProperty _Contrast;
            private MaterialProperty _Saturation;
            private MaterialProperty _EnableHue;
            private MaterialProperty _Hue;

            public AdvancedOptions(MaterialProperty[] properties)
            {
                castShadows = BaseShaderGUI.FindProperty("_CastShadows", properties, false);
                receiveShadows = BaseShaderGUI.FindProperty("_ReceiveShadows", properties, false);
                highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, false);
                reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", properties, false);
                coatNormalEnabled = BaseShaderGUI.FindProperty("_CoatNormal", properties, false);

                horizonOcclusion = BaseShaderGUI.FindProperty("_HorizonOcclusion", properties, false);
                horizonFade = BaseShaderGUI.FindProperty("_HorizonFade", properties, false);
                specularOcclusionMode = BaseShaderGUI.FindProperty("_SpecularOcclusionMode", properties, false);
                giOcclusionBias = BaseShaderGUI.FindProperty("_GIOcclusionBias", properties, false);

                //ColorAdjustment
                _EnableAcesToneMapping = BaseShaderGUI.FindProperty("_EnableAcesToneMapping", properties, false);
                _EnableColorProcessing = BaseShaderGUI.FindProperty("_EnableColorProcessing", properties, false);
                _Brightness = BaseShaderGUI.FindProperty("_Brightness", properties, false);
                _Contrast = BaseShaderGUI.FindProperty("_Contrast", properties, false);
                _Saturation = BaseShaderGUI.FindProperty("_Saturation", properties, false);
                _EnableHue = BaseShaderGUI.FindProperty("_EnableHue", properties, false);
                _Hue = BaseShaderGUI.FindProperty("_Hue", properties, false);
            }

            public void DrawSpecularOcclusion(MaterialEditor materialEditor)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //HorizonOcclusion
                Rendering.Universal.ShaderGUI.SetMaterialProperties.DrawFloatToggleProperty(
                    URPPlusStyles.horizonOcclusionText, horizonOcclusion);
                if (Math.Abs(horizonOcclusion.floatValue - 1) < 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(horizonFade, "Horizon Fade");
                    EditorGUI.indentLevel--;
                }

                //SpecularOcclusion Mode
                materialEditor.ShaderProperty(specularOcclusionMode, URPPlusStyles.specularOcclusionModeText);

                EditorGUI.indentLevel++;
                if ((SpecularOcclusionMode)specularOcclusionMode.floatValue == SpecularOcclusionMode.FromGI)
                    materialEditor.ShaderProperty(giOcclusionBias, URPPlusStyles.giOcclusionBiasText);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            public void DrawColorAdjustment(Material material, MaterialEditor materialEditor)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.enableAcesToneMappingText, _EnableAcesToneMapping);
                CoreUtils.SetKeyword(material, "_ENABLE_TONEMAP", Math.Abs(_EnableAcesToneMapping.floatValue - 1) < 1);
                materialEditor.DrawFloatToggleProperty(URPPlusStyles.enableColorProcessingText, _EnableColorProcessing);
                CoreUtils.SetKeyword(material, "_ENABLE_COLOR_PROCESSING", Math.Abs(_EnableColorProcessing.floatValue - 1) < 1);
                if (Math.Abs(_EnableColorProcessing.floatValue - 1) < 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(_Brightness, URPPlusStyles.brightnessText);
                    materialEditor.ShaderProperty(_Contrast, URPPlusStyles.contrastText);
                    materialEditor.ShaderProperty(_Saturation, URPPlusStyles.saturationText);
                    materialEditor.DrawFloatToggleProperty(URPPlusStyles.enableHueText, _EnableHue);
                    if (Math.Abs(_EnableHue.floatValue - 1) < 1)
                        materialEditor.ShaderProperty(_Hue, URPPlusStyles.hueText);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}