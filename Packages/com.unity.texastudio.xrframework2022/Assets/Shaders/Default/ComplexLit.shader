Shader "TA Shaders/Default/ComplexLit"
{
    Properties
    {
        /*************************************/
        /***********Surface Options***********/
        /*************************************/
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
        // Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _ZTest("ZTest", Int) = 4
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0
        // Normal Mode
        [Enum(Flip, 0, Mirror, 1, None, 2)] _DoubleSidedNormalMode("Double sided normal mode", Float) = 1
        [HideInInspector] _DoubleSidedConstants("Double-Sided Constants", Vector) = (1.0, 1.0, 1.0, 0.0)
        // Material Type
        [HideInInspector] _MaterialType("Material Type", Float) = 1.0
        [ToggleUI] _TransmissionEnable("TransmissionEnable", Float) = 0.0
        [ToggleUI] _SSSShadowsEnable("SSSShadowsEnable", Float) = 0.0
        // Alpha Clipping
        [ToggleUI] _AlphaCutoffEnable("Alpha Cutoff Enable", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [ToggleUI] _UseShadowThreshold("_UseShadowThreshold", Float) = 0.0
        _AlphaCutoffShadow("_AlphaCutoffShadow", Range(0.0, 1.0)) = 0.5
        // Geometric AA
        [ToggleUI] _EnableGeometricSpecularAA("EnableGeometricSpecularAA", Float) = 0.0
        _SpecularAAScreenSpaceVariance("SpecularAAScreenSpaceVariance", Range(0.0, 1.0)) = 0.1
        _SpecularAAThreshold("SpecularAAThreshold", Range(0.0, 1.0)) = 0.2
        // Displacement Mode
        [Enum(None, 0, Vertex Displacement, 1, Pixel Displacement, 2)]_DisplacementMode("DisplacementMode", Float) = 0.0
        [ToggleUI] _DisplacementLockObjectScale("displacement lock object scale", Float) = 1.0
        [ToggleUI] _DepthOffsetEnable("Depth Offset View space", Float) = 0.0
        _PPDMinSamples("Min sample for POM", Range(1.0, 64.0)) = 5
        _PPDMaxSamples("Max sample for POM", Range(1.0, 64.0)) = 15
        _PPDLodThreshold("Start lod to fade out the POM effect", Range(0.0, 16.0)) = 5
        _PPDPrimitiveLength("Primitive length for POM", Float) = 1
        _PPDPrimitiveWidth("Primitive width for POM", Float) = 1
        [HideInInspector] _InvPrimScale("Inverse primitive scale for non-planar POM", Vector) = (1, 1, 0, 0)
        
        //Planar Shadow
        [Toggle]_EnableClipShadow ("Enable Clip Shadow", Float) = 0
        [Toggle(_PLANARSHADOW_ENABLE)] _PlanarShadowEnable ("PlanarShadow Enable", Float) = 0
        [KeywordEnum(Global,Local,Planar)] _PlanarShadowMode ("PlanarShadow Mode", Float) = 0
        _PlanarShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowFalloff ("Shadow Falloff", Range(0,1)) = 0.1
        _GlobalHeight ("Global Ground Height", float) = -1
        _LocalHeight ("Local Ground Height", float) = 1
        _ShadowOffset ("Shadow Offset", float) = 1
        _ShadowIntensity ("Shadow Intensity", float) = 5
        
        /************************************/
        /***********Surface Inputs***********/
        /************************************/
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        // Mask Map
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _MaskMap("MaskMap", 2D) = "white" {}
        _MetallicRemapMin("MetallicRemapMin", Range(0.0, 1.0)) = 0.0
        _MetallicRemapMax("MetallicRemapMax", Range(0.0, 1.0)) = 1.0
        _SmoothnessRemapMin("SmoothnessRemapMin", Range(0.0, 1.0)) = 0.0
        _SmoothnessRemapMax("SmoothnessRemapMax", Range(0.0, 1.0)) = 1.0
        _AORemapMin("AORemapMin", Range(0.0, 1.0)) = 0.0
        _AORemapMax("AORemapMax", Range(0.0, 1.0)) = 1.0
        // Specular Map
        _SpecularColor("SpecularColor", Color) = (0.2, 0.2, 0.2, 1.0)
        _SpecularPower ("Specular Power", Range(0.0,16)) = 1
        _SpecularIntensity ("Specular Intensity", Range(0.0,128.0)) = 1
        _SpecularColorMap("SpecularColorMap", 2D) = "white" {}
        // Normal Map
        _BumpScale("Scale", Range(0.0, 8.0)) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BentNormalMap("Bent Normal Map", 2D) = "bump" {}
        // Anisotropy
        _TangentMap("Tangent Map", 2D) = "bump" {}
        _Anisotropy("Anisotropy", Range(-1.0, 1.0)) = 0.0
        _AnisotropyMap("AnisotropyMap", 2D) = "white" {}
        // ThicknessCurvature Map for SSS/Translucency
        _Thickness("Thickness", Range(0.0, 1.0)) = 0.5
        _Curvature("Curvature", Range(0.0, 1.0)) = 0.5
        _ThicknessCurvatureMap("Thickness Map", 2D) = "white" {}
        _ThicknessCurvatureRemap("Thickness Remap", Vector) = (0, 1, 0, 1)
        // SSS
        _SSSLUT("SSS LUT", 2D) = "grey" {}
        _ScatteringColor("Scattering Color", Color) = (0.0, 0.0, 0.0)
        _ScatteringShadowsColor("ScatteredShadows Color", Color) = (0.5, 0.5, 0.5)
        _TransmissionScale("Transmission Scale", Float) = 1.0
        // Translucency
        _TranslucencyScale("Translucency Scale", Float) = 1.0
        _TranslucencyPower("Translucency Power", Range(0.0, 1.0)) = 0.05
        _TranslucencyAmbient("Translucency Ambient", Range(0.0, 10.0)) = 0.0
        _TranslucencyDistortion("Translucency Distortion", Range(0.0, 1.0)) = 0.0
        _TranslucencyShadows("Translucency Shadows", Range(0.0, 1.0)) = 0.5
        // Iridescence
        _IridescenceLUT("Iridescence LUT", 2D) = "grey" {}
        _IridescenceShift("Iridescence Shift", Range(-1.0, 1.0)) = 0.0
        _IridescenceThicknessMap("Iridescence Thickness Map", 2D) = "white" {}
        _IridescenceThickness("Iridescence Thickness", Range(0.0, 1.0)) = 1.0
        _IridescenceThicknessRemap("Iridescence Thickness Remap", Vector) = (0, 1, 0, 0)
        _IridescenceMaskMap("Iridescence Mask Map", 2D) = "white" {}
        _IridescenceMaskScale("Iridescence Mask Scale", Range(0.0, 1.0)) = 1.0

        // HeightMap
        _HeightMap("Height Map", 2D) = "black" {}
        [HideInInspector] _HeightAmplitude("Height Amplitude", Float) = 0.02 // In world units. This will be computed in the UI.
        [HideInInspector] _HeightCenter("Height Center", Range(0.0, 1.0)) = 0.5 // In texture space

        [Enum(MinMax, 0, Amplitude, 1)] _HeightMapParametrization("Heightmap Parametrization", Int) = 1
        // MinMax mode
        _HeightMin("Heightmap Min", Float) = -1
        _HeightMax("Heightmap Max", Float) = 1
        // Amplitude mode
        _HeightTessAmplitude("Amplitude", Float) = 2.0 // in Centimeters
        _HeightTessCenter("Height Center", Range(0.0, 1.0)) = 0.5 // In texture space
        // These parameters are for vertex displacement/Tessellation
        _HeightOffset("Height Offset", Float) = 0
        // These parameters are for pixel displacement
        _HeightPoMAmplitude("Height Amplitude", Float) = 2.0 // In centimeters

        // ClearCoat
        _ClearCoatMap("ClearCoatMap", 2D) = "white" {}
        _ClearCoatMask("ClearCoatMask", Range(0.0, 1.0)) = 0.0
        _ClearCoatSmoothness("ClearCoatSmoothness", Range(0.0, 1.0)) = 1.0
        _CoatNormalMap("Coat Normal Map", 2D) = "bump" {}
        _CoatNormalScale("CoatNormal Scale", Range(0.0, 8.0)) = 1.0

        /*************************************/
        /************Detail Inputs************/
        /*************************************/
        _DetailMap("Detail Map", 2D) = "white" {}
        _DetailAlbedoScale("Detail Albedo Scale", Range(0.0, 2.0)) = 1.0
        _DetailNormalScale("Detail Normal Scale", Range(0.0, 2.0)) = 1.0
        _DetailSmoothnessScale("Detail Smoothness Scale", Range(0.0, 2.0)) = 1.0

        /************************************/
        /*********Transparency Inputs********/
        /************************************/
        [ToggleUI] _RefractionEnable("Refraction Enable", Float) = 0.0
        _Ior("Index Of Refraction", Range(-1.0, 1.0)) = 0
        _TransmittanceColor("Transmittance Color", Color) = (1.0, 1.0, 1.0)
        _TransmittanceColorMap("TransmittanceColorMap", 2D) = "white" {}
        [ToggleUI] _ChromaticAberrationEnable("Chromatic Aberration Enable", Float) = 0.0
        _ChromaticAberration("Сhromatic Aberration", Range(0.0, 1.0)) = 0.1

        /*************************************/
        /***********Emission Inputs***********/
        /*************************************/
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        _EmissionScale("Emission Intensity", Float) = 1.0
        [ToggleUI] _AlbedoAffectEmissive("Albedo Affect Emissive", Float) = 0.0

        /************************************/
        /**********Advanced Options**********/
        /************************************/
        [ToggleUI] _CastShadows("Cast Shadows", Float) = 0.0
        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
        [ToggleUI] _CoatNormal("ClearCoat Second Normal", Float) = 0.0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // Specular Occlusion
        [ToggleOff] _HorizonOcclusion("Horizon Occlusion", Float) = 0.0
        _HorizonFade("HorizonFade", Range(0.0, 1.0)) = 0.5
        [Enum(Off, 0, From Ambient Occlusion, 1, From AO and Bent Normals, 2, From GI, 3)] _SpecularOcclusionMode("Specular Occlusion Mode", Int) = 0
        _GIOcclusionBias("GIOcclusion Bias", Range(0.0, 1.0)) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections("EnvironmentReflections", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        
        //Color Processing
        [Toggle(_ENABLE_TONEMAP)] _EnableAcesToneMapping ("Enable ACES ToneMapping", Float) = 0
        [Toggle(_ENABLE_COLOR_PROCESSING)] _EnableColorProcessing ("Enable Color Processing", Float) = 0
        _Brightness ("Brightness", float) = 1
        _Contrast ("Contrast", float) = 1
        _Saturation ("Saturation", float) = 1
        [Toggle] _EnableHue ("Enable Hue", Float) = 0
        _Hue ("Hue", Range(0,360)) = 0
    }
    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="3.0"
        }
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _DOUBLESIDED_ON
            #pragma shader_feature_local_fragment _ENABLE_GEOMETRIC_SPECULAR_AA
            #pragma shader_feature_local_fragment _REFRACTION
            #pragma shader_feature_local_fragment _CHROMATIC_ABERRATION

            #pragma shader_feature_local_fragment _MASKMAP
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _BENTNORMALMAP

            #pragma shader_feature_local_fragment _SSS_LUT
            #pragma shader_feature_local_fragment _THICKNESS_CURVATUREMAP
            #pragma shader_feature_local_fragment _TANGENTMAP
            #pragma shader_feature_local_fragment _ANISOTROPYMAP
            #pragma shader_feature_local_fragment _IRIDESCENCE_THICKNESSMAP

            #pragma shader_feature_local _HEIGHTMAP
            #pragma shader_feature_local _ _VERTEX_DISPLACEMENT _PIXEL_DISPLACEMENT
            #pragma shader_feature_local_vertex _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE
            #pragma shader_feature_local_fragment _DEPTHOFFSET_ON

            #pragma shader_feature_local_fragment _CLEARCOAT
            #pragma shader_feature_local_fragment _CLEARCOATMAP
            #pragma shader_feature_local _CLEARCOAT_NORMALMAP

            #pragma shader_feature_local_fragment _DETAIL

            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _EMISSION_WITH_BASE

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _BENTNORMAL_SPECULAR_OCCLUSION
            #pragma shader_feature_local_fragment _AO_SPECULAR_OCCLUSION
            #pragma shader_feature_local_fragment _GI_SPECULAR_OCCLUSION
            #pragma shader_feature_local_fragment _HORIZON_SPECULAR_OCCLUSION

            // MaterialFeature are used as shader feature to allow compiler to optimize properly
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSMISSION
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_FAKE_SSS_SHADOWS
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSLUCENCY
            #pragma shader_feature_local _MATERIAL_FEATURE_ANISOTROPY
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_IRIDESCENCE

            // -------------------------------------
            // Universal Pipeline keywords
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            //#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            //#pragma multi_compile_fragment _ _LIGHT_COOKIES
            //#pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS

            // -------------------------------------
            // ShaderQuality keywords
            #pragma multi_compile_fragment _ _SHADER_QUALITY_PREINTEGRATED_SSS
            #pragma multi_compile_fragment _ _SHADER_QUALITY_IRIDESCENCE_APPROXIMATION
            #pragma multi_compile_fragment _ _SHADER_QUALITY_MICRO_SHADOWS
            #pragma multi_compile_fragment _ _SHADER_QUALITY_COAT_GEOMETRIC_AA

            // -------------------------------------
            // Unity defined keywords
           #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
            //#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog

            //Custom Keywords
            #pragma shader_feature _ENABLE_COLOR_PROCESSING
            #pragma shader_feature _ENABLE_TONEMAP
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "../ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
            #include "../ShaderLibrary/ComplexLit/ComplexLitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest[_ZTest]
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SHADOW_CUTOFF

            #pragma shader_feature_local _HEIGHTMAP
            #pragma shader_feature_local _VERTEX_DISPLACEMENT
            #pragma shader_feature_local_vertex _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "../ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
            #include "../ShaderLibrary/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ZTest[_ZTest]
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SHADOW_CUTOFF

            #pragma shader_feature_local _HEIGHTMAP
            #pragma shader_feature_local _VERTEX_DISPLACEMENT
            #pragma shader_feature_local_vertex _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE

            #include "../ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
            #include "../ShaderLibrary/UniversalDepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        //        Pass
        //        {
        //            Name "DepthNormals"
        //            Tags{"LightMode" = "DepthNormals"}
        //
        //            ZWrite On
        //            ZTest[_ZTest]
        //            Cull[_Cull]
        //
        //            HLSLPROGRAM
        //            #pragma only_renderers gles gles3 glcore d3d11 vulkan
        //            #pragma target 3.0
        //
        //            #pragma vertex DepthNormalsVertex
        //            #pragma fragment DepthNormalsFragment
        //
        //            // -------------------------------------
        //            // Material Keywords
        //            #pragma shader_feature_local_fragment _ALPHATEST_ON
        //
        //            #pragma shader_feature_local_fragment _MASKMAP
        //            #pragma shader_feature_local _NORMALMAP
        //            #pragma shader_feature_local_fragment _DETAIL
        //
        //            #pragma shader_feature_local _HEIGHTMAP
        //            #pragma shader_feature_local _ _VERTEX_DISPLACEMENT _PIXEL_DISPLACEMENT
        //            #pragma shader_feature_local_vertex _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE
        //            #pragma shader_feature_local_fragment _DEPTHOFFSET_ON
        //
        //            // -------------------------------------
        //            // ShaderQuality keywords
        //            #pragma multi_compile_fragment _ _SHADER_QUALITY_HIGH_QUALITY_DEPTH_NORMALS
        //
        //            //--------------------------------------
        //            // GPU Instancing
        //            #pragma multi_compile_instancing
        //
        //            #include "ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
        //            #include "ShaderLibrary/UniversalDepthNormalsPass.hlsl"
        //            ENDHLSL
        //        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            Cull Off

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _DOUBLESIDED_ON
            #pragma shader_feature_local_fragment _ENABLE_GEOMETRIC_SPECULAR_AA

            #pragma shader_feature_local_fragment _MASKMAP
            #pragma shader_feature_local_fragment _BENTNORMALMAP

            #pragma shader_feature_local_fragment _SSS_LUT
            #pragma shader_feature_local_fragment _THICKNESS_CURVATUREMAP
            #pragma shader_feature_local_fragment _TANGENTMAP
            #pragma shader_feature_local_fragment _ANISOTROPYMAP
            #pragma shader_feature_local_fragment _IRIDESCENCE_THICKNESSMAP

            #pragma shader_feature_local _HEIGHTMAP
            #pragma shader_feature_local _ _VERTEX_DISPLACEMENT _PIXEL_DISPLACEMENT
            #pragma shader_feature_local_vertex _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE
            #pragma shader_feature_local_fragment _DEPTHOFFSET_ON

            #pragma shader_feature_local_fragment _CLEARCOAT
            #pragma shader_feature_local_fragment _CLEARCOATMAP
            #pragma shader_feature_local _CLEARCOAT_NORMALMAP

            #pragma shader_feature_local_fragment _DETAIL

            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _EMISSION_WITH_BASE

            #pragma shader_feature_local_fragment _BENTNORMAL_SPECULAR_OCCLUSION
            #pragma shader_feature_local_fragment _AO_SPECULAR_OCCLUSION
            #pragma shader_feature_local_fragment _GI_SPECULAR_OCCLUSION
            #pragma shader_feature_local_fragment _HORIZON_SPECULAR_OCCLUSION

            // MaterialFeature are used as shader feature to allow compiler to optimize properly
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSMISSION
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_FAKE_SSS_SHADOWS
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSLUCENCY
            #pragma shader_feature_local _MATERIAL_FEATURE_ANISOTROPY
            #pragma shader_feature_local_fragment _MATERIAL_FEATURE_IRIDESCENCE

            #include "../ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
            #include "../ShaderLibrary/UniversalMetaPass.hlsl"
            ENDHLSL
        }
        //        Pass
        //        {
        //            Name "Universal2D"
        //            Tags{ "LightMode" = "Universal2D" }
        //
        //            Blend[_SrcBlend][_DstBlend]
        //            ZWrite[_ZWrite]
        //            ZTest[_ZTest]
        //            Cull[_Cull]
        //
        //            HLSLPROGRAM
        //            #pragma only_renderers gles gles3 glcore d3d11 vulkan
        //            #pragma target 2.0
        //
        //            #pragma vertex vert
        //            #pragma fragment frag
        //            #pragma shader_feature_local_fragment _ALPHATEST_ON
        //            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
        //
        //            #include "ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
        //            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
        //            ENDHLSL
        //        }

        // Planar Shadows平面阴影
        Pass
        {
            Name "PlanarShadow"
            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }
            //用使用模板测试以保证alpha显示正确
            Stencil
            {
                Ref 0
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }
            Cull Off

            Blend [_SrcBlend] [_DstBlend]

            ZWrite Off

            Offset -1 ,0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma only_renderers gles gles3 glcore d3d11 vulkan

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma shader_feature _PLANARSHADOW_ENABLE
            #pragma shader_feature _PLANARSHADOWMODE_GLOBAL _PLANARSHADOWMODE_LOCAL _PLANARSHADOWMODE_PLANAR
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #include "../ShaderLibrary/ComplexLit/ComplexLitInput.hlsl"
            #include "../ShaderLibrary/Universal/PlanarShadowPass.hlsl"
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.ComplexLitShaderGUI"
}