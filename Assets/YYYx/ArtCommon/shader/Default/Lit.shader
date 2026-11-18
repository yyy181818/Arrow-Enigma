Shader "TA Shaders/Default/Lit"
{
    Properties
    {
        [KeywordEnum(Lit,BlinnPhong,Lambert,UnLit)] _ShadingModel ("Shading Model", Float) = 0
        [Toggle(_ENABLE_SPECULAR)]_EnableSpecular ("Enable Specular", Float) = 0
        [Toggle]_Surface ("Surface Type", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [HideInInspector][Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4

        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [IntRange]_StencilRef ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Stencil Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("Stencil ZFail", Float) = 0
        [HideInInspector]_AlphaToMask ("Alpha To Mask", float) = 0
        [Toggle] _AlphaClip ("Alpha Clip", Float) = 0
        [Enum(A,0,R,1)]_AlphaChannel ("Alpha Channel", float) = 0
        _Cutoff ("Cut off", Range(0,1)) = 0.5
        _Alpha ("Alpha", Range(0,1)) = 1
        _QueueOffset ("RenderQueue Offset", Range(-50,50)) = 0
        _TillingOffset ("Tilling", Vector) = (1,1,0,0)
        [MainTexture]_BaseMap ("Base Map", 2D) = "white" {}
        _BaseColorStrength ("Base Color Strength", Range(0,32)) = 1
        _DirectStrength ("Direct Strength", Range(0,32)) = 1
        _InDirectStrength ("InDirect Strength", Range(0,32)) = 1
        _LightMapIntensity ("_LightMapIntensity", Range(0,2)) = 1
        [MainColor]_BaseColor ("Base Color", Color) = (1,1,1,1)

        [ToggleOff]_SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff]_EnvironmentReflections("Environment Reflections", Float) = 1.0
        [Toggle]_ReceiveShadows ("Receive Shadows", Float) = 0
        [ToggleOff]_TransparentShadows("Transparent Shadows", Float) = 0.0

        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _SpecularPower ("Specular Power", float) = 1
        _SpecularIntensity ("Specular Intensity", float) = 1
        [KeywordEnum(NMR,MRO,Single,None)] _TextureMode ("Texture Mode", Float) = 0
        _CombineMap ("normal(xy),metal&rough(zw)", 2D) = "white"{}
        _MetallicTex ("Metallic Texture", 2D) = "white" {}
        [Gamma]_Metallic ("Metallic", Range(0,1)) = 0
        _RoughnessTex ("Roughness Texture", 2D) = "white" {}
        _Roughness ("Roughness", Range(0,1)) = 0.5
        [Normal]_BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,8)) = 1
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (0,0,0,0)
        [Channel]_EmissionChannel ("Emission Channel", Vector) = (0,0,0,0)
        [Toggle]_EnableSampleRGB ("Enable Sample RGB", Float) = 0
        _OcclusionMap ("Occlusion Map", 2D) = "white"{}
        _OcclusionStrength ("AO Strength", Range(0,1)) = 1.0
        //Features
        [Toggle(_PLANARSHADOW_ENABLE)]_PlanarShadowEnable ("PlanarShadow Enable", Float) = 0
        [KeywordEnum(Global,Local,Planar)] _PlanarShadowMode ("PlanarShadow Mode", Float) = 0
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowFalloff ("Shadow Falloff", Range(0,1)) = 0.1
        _GlobalHeight ("Global Ground Height", float) = -1
        _LocalHeight ("Local Ground Height", float) = 1
        _ShadowOffset ("Shadow Offset", float) = 1
        _ShadowIntensity ("Shadow Intensity", float) = 5

        [Toggle(_DISSOLVE_ENABLE)]_DissolveEnable ("Dissolve Enable", Float) = 0
        [KeywordEnum(Albedo,Emission)] _EdgesAffect ("Dissolve ColorMode", Float) = 2
        _DissolveMap ("Dissolve Map", 2D) = "white"{}
        [HDR]_DissolveColor ("Dissolve Color", Color) = (1,1,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        [Toggle]_InvertDissolveMinMax ("Invert Dissolve MinMax", Float) = 0
        _DissolveMinAmount ("Dissolve Min Amount", float) = -1
        _DissolveMaxAmount ("Dissolve Max Amount", float) = 1
        [KeywordEnum(X,Y,Z)] _DissolveAxis ("Dissolve Axis", Float) = 4
        [Enum(uv,0,pos,1)] _SampleMode ("Dissolve UV Mode", Float) = 0
        _DissolveEdgeWidth ("Dissolve EdgeWidth", Range(0,0.5)) = 0.01
        _GuideStrength("Guide Strength", Range(0,10)) = 1
        [Toggle(_GUIDEAFFECTSEDGESBLENDING_ON)] _GuideAffectsEdgesBlending("Guide Affects Edges Blending", Float) = 0
        [Toggle(_USETRIPLANAR_ON)] _UseTriplanar("UseTriplanar", Float) = 0
        _UVTilingTriplanar("UV Tiling Triplanar", float) = 0.5

        [Toggle(_ENALBE_MAINEDGEPATTERN)]_EnableMainEdgePattern ("Enable Main Edge Pattern", Float) = 0
        _MainEdgePattern ("Main Edge Pattern", 2D) = "black"{}
        [Enum(Multiply,0,Add,1)]_MainColorMode ("MainColor Mode", float) = 1
        _MainEdgePatternTiling ("Main Edge Pattern Tiling", float) = 1
        _MainEdgePatternSpeed ("Main Edge Pattern Speed", float) = 0.1
        [HDR]_MainEdgeColor2 ("Main Edge Color2", Color) = (1,1,1,1)
        _MainEdgePatternStrength ("MainEdgePattern Strength",float) = 1

        [Toggle(_ENALBE_SECONDEDGEPATTERN)]_EnableSecondEdgePattern ("Enable Second Edge Pattern", Float) = 0
        _SecondEdgePattern ("Second Edge Pattern", 2D) = "black"{}
        [Enum(Multiply,0,Add,1)]_SecondColorMode ("Second ColorMode", float) = 1
        _SecondEdgePatternTiling ("Second Edge Pattern Tiling", float) = 1
        _SecondEdgeWidth ("Second Edge Width", Range(0,0.5)) = 0.01
        _SecondEdgePatternSpeed ("Second EdgePattern Speed",float) = 0.1
        [HDR]_SecondEdgeColor1 ("Second Edge Color1", Color) = (1,1,1,1)
        [HDR]_SecondEdgeColor2 ("Second Edge Color2", Color) = (1,1,1,1)
        _SecondEdgePatternStrength ("Second EdgePattern Strength",Float) = 1
        [Toggle(_2SIDESSECONDEDGE_ON)]_2SidesSecondEdge("2 Sides Second Edge", Float) = 0

        [Toggle] _RimEnable ("Rim Enable", Float) = 0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", float) = 1
        _RimScale ("Rim Scale", float) = 1

        [Toggle] _SphereMaskEnable ("SphereMask Enable", Float) = 0
        [Toggle] _MaskType ("Mask Type", Float) = 0
        [Toggle] _UseTextureMask ("Use Texture Mask", Float) = 0
        _CutoutBaseMap ("Cutout Base Map", 2D) = "white"{}
        _CutoutMap ("Cutout Map", 2D) = "white"{}
        _NoiseStrength ("Noise Strength", float) = 1
        [Toggle]_Invert ("Invert", float) = 0
        _Radius ("Radius", float) = 1
        _SmoothEdge ("Smooth Edge", float) = 2
        [HDR]_EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeSize ("Edge Size", float) = 0.5
        _EdgeIntensity ("Edge Intensity", float) = 1
        _NoiseWidth ("Noise Width", Range(0,1)) = 1
        _NoiseSpeed ("Noise Speed", Range(0,1)) = 0.05

        [Toggle(_TOON_ENABLE)]_ToonEnable ("Toon Enable", Float) = 0
        _SmoothstepMin ("Smoothstep Min", float) = 0
        _SmoothstepMax ("Smoothstep Max", float) = 1
        _SmoothstepEdge ("Smoothstep Edge", float) = 1.5
        _SmoothstepIntensity ("Smoothstep Intensity", float) = 20
        [Toggle(_ENABLE_SMOOTHNORMALS)]_EnableSmoothNormals ("Enable Smooth Normals", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,1)) = 0.01

        [Toggle(_ENABLE_TONEMAP)]_EnableAcesToneMapping ("Enable ACES ToneMapping", Float) = 0
        [Toggle(_ENABLE_COLOR_PROCESSING)]_EnableColorProcessing ("Enable Color Processing", Float) = 0
        _Brightness ("Brightness", float) = 1
        _Contrast ("Contrast", float) = 1
        _Saturation ("Saturation", float) = 1
        [Toggle] _EnableHue ("Enable Hue", Float) = 0
        _Hue ("Hue", Range(0,360)) = 0

        [Toggle]_EnableClipShadow ("Enable Clip Shadow", Float) = 0

        //Clear Coat
        [Toggle] _EnableClearCoat ("Enable ClearCoat", Float) = 0
        _ClearCoat ("Clear Coat", Range(0,1)) = 0
        _Iridescence ("Iridescence", Range(0,1)) = 0
        _IridescenceThickness ("Iridescence Thickness", Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _TillingOffset;
            half4 _BaseColor;
            half _BaseColorStrength;
            half _AlphaChannel;
            half _Alpha;
            half4 _SpecularColor;
            half _SpecularIntensity;
            half _SpecularPower;
            float4 _BaseMap_TexelSize;
            float4 _BaseMap_ST;
            half _Metallic;
            half _Roughness;
            half _OcclusionStrength;
            half _BumpScale;
            //half _BumpMapToggle;
            half _DirectStrength;
            half _InDirectStrength;
            float _LightMapIntensity;
            float _Cutoff;
            half4 _EmissionColor;
            half4 _EmissionChannel;
            half _EnableSampleRGB;
            float _Surface;

            //Features
            //half _PlanarShadowEnable;
            half4 _ShadowColor;
            float _GlobalHeight;
            float _LocalHeight;
            float _ShadowFalloff;
            float _ShadowOffset;
            float _ShadowIntensity;
            //
            //half _DissolveEnable;
            float4 _DissolveMap_ST;
            half4 _DissolveColor;
            half _DissolveAmount;
            half _DissolveEdgeWidth;
            half _DissolveMinAmount;
            half _DissolveMaxAmount;
            half _GuideStrength;
            float _UVTilingTriplanar;
            half _SampleMode;
            half _InvertDissolveMinMax;

            float _MainEdgePatternTiling;
            half _MainColorMode;
            float _MainEdgePatternSpeed;
            half4 _MainEdgeColor2;
            half _MainEdgePatternStrength;

            float _SecondEdgePatternTiling;
            half _SecondColorMode;
            float _SecondEdgePatternSpeed;
            half _SecondEdgePatternStrength;
            half _SecondEdgeWidth;
            half4 _SecondEdgeColor1;
            half4 _SecondEdgeColor2;

            //
            //half _RimEnable;
            half4 _RimColor;
            half _RimPower;
            half _RimScale;
            //
            //half _SphereMaskEnable;
            half _MaskType;
            half _UseTextureMask;
            half _NoiseStrength;
            float4 _CutoutMap_ST;
            half4 _EdgeColor;
            half _Invert;
            half _Radius;
            half _SmoothEdge;
            half _EdgeSize;
            half _EdgeIntensity;
            half _NoiseSpeed;
            half _NoiseWidth;

            //Toon
            half _SmoothstepMin;
            half _SmoothstepMax;
            half _SmoothstepEdge;
            half _SmoothstepIntensity;

            half4 _OutlineColor;
            float _OutlineWidth;

            half _Brightness;
            half _Contrast;
            half _Saturation;
            half _EnableHue;
            half _Hue;

            half _EnableClipShadow;

            //Clear Coat
            half _ClearCoat;
            half _Iridescence;
            half _IridescenceThickness;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        #if defined(_DISSOLVE_ENABLE)
        #define _DISSOLVEAXIS_X _DISSOLVEAXIS_Y _DISSOLVEAXIS_Z
        #define _EDGESAFFECT_ALBEDO _EDGESAFFECT_EMISSION
        #else
        #define _DISSOLVEAXIS_X 0
        #endif
        
        #if defined(_EMISSION)
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        #endif

        #if defined(_NORMALMAP) || defined(_DISSOLVE_ENABLE) || defined(_TEXTUREMODE_SINGLE) || defined(_TEXTUREMODE_MRO)
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        #endif

        #if defined(_TEXTUREMODE_NMR) || defined(_TEXTUREMODE_MRO)
        TEXTURE2D(_CombineMap);
        SAMPLER(sampler_CombineMap);
        #endif

        #if defined(_DISSOLVE_ENABLE)
        sampler2D _DissolveMap;
        #endif
        #if defined(_SPHEREMASK_ENABLE)
        TEXTURE2D(_CutoutMap);
        SAMPLER(sampler_CutoutMap);
        TEXTURE2D(_CutoutBaseMap);
        SAMPLER(sampler_CutoutBaseMap);
        #endif

        #if defined(_TEXTUREMODE_SINGLE)
        TEXTURE2D(_MetallicTex);
        SAMPLER(sampler_MetallicTex);
        TEXTURE2D(_RoughnessTex);
        SAMPLER(sampler_RoughnessTex);
        #endif

        #if defined(_OCCLUSIONMAP)
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        #endif

        #if defined(_DISSOLVE_ENABLE)
        sampler2D _MainEdgePattern;
        sampler2D _SecondEdgePattern;
        #endif

        #if defined(_SPHEREMASK_ENABLE)
        half _UseSecondPosition;
        float3 _MaskPosition;
        half3 _MaskNormal;
        float3 _MaskSecondPosition;
        half3 _MaskSecondNormal;
        #endif
        #if defined(_PLANARSHADOW_ENABLE)
        float4x4 _World2Ground; //阴影接收平面（世界空间到模型空间的转换矩阵）
        float4x4 _Ground2World; //阴影接收平面（模型空间到世界空间的转换矩阵）
        #endif

        uniform half _Light;

        #define SphereMaskSmooth(x) x = saturate(x * x * (3 - 2 * x));
        // GLES2 has limited amount of interpolators
        #if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
        #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
        #endif

        #if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
        #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
        #endif

        #if !defined(_RECEIVE_SHADOWS_OFF)
        #define _SHADOWS_SOFT 1
        #endif

        struct Attributes_forward_base
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 staticLightmapUV : TEXCOORD1;
            float2 dynamicLightmapUV : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings_forward_base
        {
            float2 uv : TEXCOORD0;

            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            float3 positionWS : TEXCOORD1;
            #endif

            float3 normalWS : TEXCOORD2;
            #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
            half4 tangentWS : TEXCOORD3; // xyz: tangent, w: sign
            #endif

            #ifdef _ADDITIONAL_LIGHTS_VERTEX
            half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
            #else
            half fogFactor : TEXCOORD5;
            #endif

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            float4 shadowCoord : TEXCOORD6;
            #endif

            // #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            // half3 viewDirTS : TEXCOORD7;
            // #endif

            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
            #ifdef DYNAMICLIGHTMAP_ON
            float2 dynamicLightmapUV : TEXCOORD8; // Dynamic lightmap UVs
            #endif

            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings_forward_base vert(Attributes_forward_base input)
        {
            Varyings_forward_base output = (Varyings_forward_base)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);

            VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

            half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

            half fogFactor = 0;
            #if !defined(_FOG_FRAGMENT)
            fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            #endif

            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

            // already normalized from normal transform to WS.
            output.normalWS = normalInput.normalWS;
            #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            real sign = input.tangentOS.w * GetOddNegativeScale();
            half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
            #endif
            #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
            output.tangentWS = tangentWS;
            #endif

            // #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            // half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
            // half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
            // output.viewDirTS = viewDirTS;
            // #endif

            OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
            #ifdef DYNAMICLIGHTMAP_ON
            output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST
                .zw;
            #endif
            OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
            #ifdef _ADDITIONAL_LIGHTS_VERTEX
            output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            #else
            output.fogFactor = fogFactor;
            #endif
            output.positionWS = vertexInput.positionWS;

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = GetShadowCoord(vertexInput);
            #endif

            output.positionCS = vertexInput.positionCS;
            return output;
        }

        void InitializeInputData(Varyings_forward_base input, half3 normalTS, out InputData inputData)
        {
            inputData = (InputData)0;
            
            inputData.positionWS = input.positionWS;

            half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
            #if defined(_NORMALMAP)
            float sgn = input.tangentWS.w; // should be either +1 or -1
            float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
            half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

            #if defined(_NORMALMAP)
            inputData.tangentToWorld = tangentToWorld;
            #endif
            inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
            #else
            inputData.normalWS = input.normalWS;
            #endif

            inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
            inputData.viewDirectionWS = viewDirWS;

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            inputData.shadowCoord = input.shadowCoord;
            #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
            inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
            #else
            inputData.shadowCoord = float4(0, 0, 0, 0);
            #endif
            #ifdef _ADDITIONAL_LIGHTS_VERTEX
            inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
            inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
            #else
            inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
            #endif

            #if defined(DYNAMICLIGHTMAP_ON)
            inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH,
                                          inputData.normalWS) * _LightMapIntensity;
            #else
            inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS) * _LightMapIntensity;
            #endif
            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
            inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

            #if defined(DEBUG_DISPLAY)
            #if defined(DYNAMICLIGHTMAP_ON)
            inputData.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if defined(LIGHTMAP_ON)
            inputData.staticLightmapUV = input.staticLightmapUV;
            #else
            inputData.vertexSH = input.vertexSH;
            #endif
            #endif
        }

        half Alpha(half albedoAlpha, half4 color, half cutoff)
        {
            half alpha = albedoAlpha * color.a;
            alpha = AlphaDiscard(alpha, cutoff);
            return alpha;
        }

        half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
        {
            return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
        }

        float2 SphereMask_HeightField(float3 vertexWorldPosition, float noise, float3 maskPosition, float radius,
                                      float intensity, float noiseStrength,
                                      float edgeSize,
                                      float smooth)
        {
            //Distance
            float d = distance(maskPosition, vertexWorldPosition);
            float d1 = distance(maskPosition, vertexWorldPosition);
            //Noise
            float r = radius;
            radius += noise * noiseStrength;

            float shape = 1 - saturate(max(0, d - radius + edgeSize) / edgeSize);
            float shape1 = 1 - saturate(max(0, d1 - r + edgeSize) / edgeSize);
            //Smooth
            shape = pow(shape, smooth + 0.01);
            shape1 = pow(shape1, smooth + 0.01);
            //Fade
            shape *= intensity;
            shape1 *= intensity;
            return float2(shape, shape1);
        }

        float2 SphereMask_PlaneMask(float3 planePosition, float3 planeNormal, float3 vertexPositionWS, float noise,
                                    float noiseStrength)
        {
            float edge = dot(planeNormal, planePosition - vertexPositionWS);
            float edge2 = dot(planeNormal, planePosition - vertexPositionWS);
            edge += noise * noiseStrength;
            edge = saturate(edge / max(0.01, _NoiseWidth));
            edge2 = saturate(edge2 / max(0.01, _NoiseWidth));
            SphereMaskSmooth(edge);
            SphereMaskSmooth(edge2);
            return float2(edge, edge2);
        }

        float4 GetMask(float3 positionWS, float2 uv)
        {
            half3 MaskColor = half3(1, 1, 1);
            half mask = 0;
            #if defined(_SPHEREMASK_ENABLE)
            float2 noiseUV = TRANSFORM_TEX(uv, _CutoutMap);
            noiseUV += _Time.y * _NoiseSpeed;
            half noise = SAMPLE_TEXTURE2D(_CutoutMap, sampler_CutoutMap, noiseUV).r;
            noise = _Invert == 1 ? 1 - noise : noise;
            half2 SphereMask_Sphere = _UseSecondPosition == 0
                                          ? SphereMask_HeightField(
                                              positionWS, noise, _MaskPosition, _Radius,
                                              _EdgeIntensity, _NoiseStrength, _EdgeSize,
                                              _SmoothEdge)
                                          : SphereMask_HeightField(
                                              positionWS, noise, _MaskSecondPosition, _Radius,
                                              _EdgeIntensity, _NoiseStrength, _EdgeSize,
                                              _SmoothEdge);
            half2 SphereMask_Plane = _UseSecondPosition == 0
                            ? SphereMask_PlaneMask(
                                _MaskPosition, _MaskNormal,
                                positionWS, noise,
                                _NoiseStrength)
                            : SphereMask_PlaneMask(
                                _MaskSecondPosition,
                                _MaskSecondNormal,
                                positionWS, noise, _NoiseStrength);
            MaskColor = _MaskType == 1
                                  ? (SphereMask_Plane.x -
                                      SphereMask_Plane.y) * _EdgeColor.
                                  rgb
                                  : (SphereMask_Sphere.x -
                                      SphereMask_Sphere.y) * _EdgeColor.
                                  rgb;
            half maskValue = _MaskType == 1 ? SphereMask_Plane.x : SphereMask_Sphere.x;
            mask = _Invert == 1 ? 1 - maskValue : maskValue;
            #endif
            return float4(MaskColor, mask);
        }

        half3 NormalStrength(half3 In, half Strength)
        {
            return half3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }

        half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
        {
            #if defined(_NORMALMAP)
            half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
            #if BUMP_SCALE_NOT_SUPPORTED
            return UnpackNormal(n);
            #else
            return UnpackNormalScale(n, scale);
            #endif
            #else
            return half3(0.0h, 0.0h, 1.0h);
            #endif
        }

        half3 DecodeNormalFormRG(half2 rg,half scale)
        {
            half3 ret;
            ret.xy = rg.xy * 2.0 - 1.0;
            ret.z = sqrt(1.0 - saturate(dot(ret.xy, ret.xy)));
            ret.xy *= _BumpScale;
            return ret;
        }

        half3 SampleEmission(float2 uv, half3 emissionColor, TEXTURE2D_PARAM(emissionMap, sampler_emissionMap))
        {
            #if defined (_EMISSION)
            half3 emission = _EnableSampleRGB
            ? SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb * emissionColor.rgb
            : dot(_EmissionChannel.rgb,SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb)
            * emissionColor.rgb;
            #else
            half3 emission = half3(0.0h, 0.0h, 0.0h);
            #endif
            return emission;
        }

        void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
        {
            uv = uv * _TillingOffset.xy + _TillingOffset.zw;
            half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
            half alpha = _AlphaChannel == 0 ? albedoAlpha.a : albedoAlpha.r;
            outSurfaceData.alpha = Alpha(alpha, _BaseColor, _Cutoff) * _Alpha;
            outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb * _BaseColorStrength;
            outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

            //half ao = _Surface == 0 ? 1 : albedoAlpha.a;
            half occlusion = half(1.0);
            #if defined(_TEXTUREMODE_NMR) || defined(_TEXTUREMODE_MRO)
            half4 combine = SAMPLE_TEXTURE2D_X(_CombineMap, sampler_CombineMap, uv);
            #endif

            #if defined(_TEXTUREMODE_NMR)
            half3 normalTS = DecodeNormalFormRG(combine.xy, _BumpScale);
            half metallic = combine.z * _Metallic;
            half roughness = combine.w * _Roughness;
            #if defined(_OCCLUSIONMAP)
            half occ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a;
            occlusion = LerpWhiteTo(occ, _OcclusionStrength);
            #else
            occlusion = half(1.0);
            #endif

            #elif defined(_TEXTUREMODE_MRO)
            half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
            half metallic = combine.r * _Metallic;
            half roughness = combine.g * _Roughness;
            #if defined(_OCCLUSIONMAP)
            occlusion = LerpWhiteTo(combine.b, _OcclusionStrength);
            #else
            occlusion = half(1.0);
            #endif
            #elif defined(_TEXTUREMODE_SINGLE)
            half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
            normalTS = NormalStrength(normalTS, _BumpScale);
            half metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, uv).r * _Metallic;
            half roughness = SAMPLE_TEXTURE2D(_RoughnessTex, sampler_RoughnessTex, uv).r * _Roughness;
            #if defined(_OCCLUSIONMAP)
            half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
            occlusion = LerpWhiteTo(occ, _OcclusionStrength);
            #else
            occlusion = half(1.0);
            #endif
            #else
            //half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
            //normalTS = NormalStrength(normalTS, _BumpScale);
            half3 normalTS = half3(0.0h, 0.0h, 1.0h);
            half metallic = _Metallic;
            half roughness = _Roughness;
            #endif

            //#if _SPECULAR_SETUP
            //outSurfaceData.metallic = half(1.0);
            //outSurfaceData.specular = _SpecularColor.rgb;
            //#else
            outSurfaceData.metallic = metallic;
            //outSurfaceData.specular = lerp(kDielectricSpec.rgb, outSurfaceData.albedo.rgb, metallic);
            outSurfaceData.specular = half3(0.0, 0.0, 0.0);
            //#endif
            // half smoothness = PerceptualRoughnessToPerceptualSmoothness(roughness) *
            //     PerceptualRoughnessToPerceptualSmoothness(roughness);
            half perceptualSmoothness = PerceptualRoughnessToPerceptualSmoothness(roughness);
            outSurfaceData.smoothness = max(0.02f, perceptualSmoothness);

            outSurfaceData.normalTS = normalTS.xyz;
            outSurfaceData.occlusion = occlusion;
            #if defined(_EMISSION)
            outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb,
                                        TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
            #else
            outSurfaceData.emission = 0;
            #endif

            outSurfaceData.clearCoatMask = half(0.0);
            outSurfaceData.clearCoatSmoothness = half(0.0);
        }

//         half CustomDirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
//         {
//             float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
//             float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));
//
//             float NoH = saturate(dot(float3(normalWS), halfDir));
//             half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
//
//             // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
//             // BRDFspec = (D * V * F) / 4.0
//             // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
//             // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
//             // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
//             // https://community.arm.com/events/1155
//
//             // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
//             // We further optimize a few light invariant terms
//             // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
//             #if defined(_ENABLE_CLEARCOAT_ON)
//             float NoL = saturate(dot(float3(normalWS), lightDirectionWSFloat3));
//             float NoV = saturate(dot(float3(normalWS), float3(viewDirectionWS)));
//             float DV = DV_SmithJointGGX(NoH, NoL, NoV, brdfData.roughness);
//             float3 F = F_Schlick(brdfData.specular, NoV);
//             float topIor = lerp(1.0h, 1.5h, _ClearCoat);
//             float viewAngle = lerp(
//                 NoV, sqrt(1.0 + Sq(1.0h / topIor) * (Sq(dot(float3(normalWS), float3(viewDirectionWS))) - 1.0)),
//                 _ClearCoat);
//             float3 F_Iridescence = EvalIridescence(topIor, viewAngle, _IridescenceThickness, brdfData.specular);
//             F = lerp(F, F_Iridescence, _Iridescence);
//             #else
//             float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
//             #endif
//
//             half LoH2 = LoH * LoH;
//             #if defined(_TOON_ENABLE)
//             half specularTerm = smoothstep(min(_SmoothstepMin * _SmoothstepEdge, 1),
// min(_SmoothstepMax * _SmoothstepEdge, 1),
// brdfData.roughness2 / ((d * d) * max(
//         0.1h, LoH2) * brdfData.
//     normalizationTerm));
//             specularTerm *= max(0.1h, _SmoothstepIntensity);
//             #else
//             #if defined(_ENABLE_CLEARCOAT_ON)
//             //half specularTerm = DV * F / (4.0h * NoL * NoV);
//             half specularTerm = DV * F / (4.0h);
//             #else
//             half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);
//             #endif
//             #endif
//
//
//             // On platforms where half actually means something, the denominator has a risk of overflow
//             // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
//             // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
//             #if REAL_IS_HALF
//             specularTerm = specularTerm - HALF_MIN;
//             // Update: Conservative bump from 100.0 to 1000.0 to better match the full float specular look.
//             // Roughly 65504.0 / 32*2 == 1023.5,
//             // or HALF_MAX / ((mobile) MAX_VISIBLE_LIGHTS * 2),
//             // to reserve half of the per light range for specular and half for diffuse + indirect + emissive.
//             specularTerm = clamp(specularTerm, 0.0, 1000.0); // Prevent FP16 overflow on mobiles
//             #endif
//
//             return pow(specularTerm, max(0.1h, _SpecularPower)) * _SpecularIntensity;;
//         }

        half CustomDirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
        {
            float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
            float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));

            float NoH = saturate(dot(float3(normalWS), halfDir));
            half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));

            // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
            // BRDFspec = (D * V * F) / 4.0
            // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
            // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
            // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
            // https://community.arm.com/events/1155

            // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
            // We further optimize a few light invariant terms
            // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
            float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;

            half LoH2 = LoH * LoH;
            half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);

            // On platforms where half actually means something, the denominator has a risk of overflow
            // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
            // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
            #if REAL_IS_HALF
            specularTerm = specularTerm - HALF_MIN;
            // Update: Conservative bump from 100.0 to 1000.0 to better match the full float specular look.
            // Roughly 65504.0 / 32*2 == 1023.5,
            // or HALF_MAX / ((mobile) MAX_VISIBLE_LIGHTS * 2),
            // to reserve half of the per light range for specular and half for diffuse + indirect + emissive.
            specularTerm = clamp(specularTerm, 0.0, 1000.0); // Prevent FP16 overflow on mobiles
            #endif

            return specularTerm;
        }

        half3 CustomLightingPhysicallyBased(BRDFData brdfData, Light mainLight,
                                           half lightAttenuation, float3 normalWS,
                                           half3 viewDirectionWS,
                                           bool specularHighlightsOff)
        {
            half3 lightDirectionWS = mainLight.direction;
            half3 lightColor = mainLight.color;
            #if defined(_TOON_ENABLE)
            half NdotL = smoothstep(_SmoothstepMin, _SmoothstepMax, dot(normalWS, lightDirectionWS) * 0.5 + 0.5);
            #else

            half NdotL = saturate(dot(lightDirectionWS, normalWS));
            //float NdotL =  dot((float3)lightDirectionWS, (float3)normalWS) * 0.5f + 0.5f;
            #endif

            float3 radiance = lightColor * (lightAttenuation * NdotL);

            half3 brdf = brdfData.diffuse;
            #ifndef _SPECULARHIGHLIGHTS_OFF
            [branch] if (!specularHighlightsOff)
            {
                brdf += brdfData.specular * _SpecularColor.rgb * CustomDirectBRDFSpecular(
                    brdfData, normalWS, lightDirectionWS, viewDirectionWS);
            }
            #endif // _SPECULARHIGHLIGHTS_OFF
            return brdf * radiance;
        }

        half3 CustomLightingPhysicallyBased(BRDFData brdfData, Light light, float3 normalWS,
                                    half3 viewDirectionWS,
                                    bool specularHighlightsOff)
        {
            return CustomLightingPhysicallyBased(brdfData, light, light.distanceAttenuation * light.
                        shadowAttenuation, normalWS,
                        viewDirectionWS, specularHighlightsOff);
        }

        half3 CustomGlobalIllumination(BRDFData brdfData,half3 bakedGI, half occlusion, float3 positionWS,
                                                          half3 normalWS, half3 viewDirectionWS,
                                                          float2 normalizedScreenSpaceUV)
        {
            half3 reflectVector = reflect(-viewDirectionWS, normalWS);
            half NoV = saturate(dot(normalWS, viewDirectionWS));
            half fresnelTerm = Pow4(1.0 - NoV);

            half3 indirectDiffuse = bakedGI;
            half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS,
                brdfData.perceptualRoughness, 1.0h,
                normalizedScreenSpaceUV);

            #if defined(_ENABLE_CLEARCOAT_ON)
            //unity内置的镭射效果的实现 在环境光漫反射里面，直接去修改SpecularColor的值
            float topIor = lerp(1.0, 1.5, _ClearCoat); //镭射反射率，有涂层的话就提高反射率
            float viewAngle = lerp(NoV, sqrt(1.0 + Sq(1.0h / topIor) * (Sq(dot(normalWS, viewDirectionWS)) - 1.0)),
                                              _ClearCoat); //视角方向
            float3 F_Iridescence = EvalIridescence(topIor, viewAngle, _IridescenceThickness, indirectSpecular); //求出镭射的F
            indirectSpecular = lerp(indirectSpecular, F_Iridescence, _Iridescence); //和原来默认的进行插值
            #endif

            half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

            if (IsOnlyAOLightingFeatureEnabled())
            {
                color = half3(1, 1, 1); // "Base white" for AO debug lighting mode
            }
            return color * occlusion;
        }

        half4 CustomUniversalFragmentPBR(InputData inputData, SurfaceData surfaceData)
        {
            #if defined(_SPECULARHIGHLIGHTS_OFF)
            bool specularHighlightsOff = true;
            #else
            bool specularHighlightsOff = false;
            #endif
            BRDFData brdfData;

            // NOTE: can modify "surfaceData"...
            InitializeBRDFData(surfaceData, brdfData);

            #if defined(DEBUG_DISPLAY)
            half4 debugColor;

            if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
            {
                return debugColor;
            }
            #endif

            // Clear-coat calculation...
            //BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
            half4 shadowMask = CalculateShadowMask(inputData);
            AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
            uint meshRenderingLayers = GetMeshRenderingLayer();
            Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

            // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
            MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

            LightingData lightingData = CreateLightingData(inputData, surfaceData);

            lightingData.giColor = CustomGlobalIllumination(brdfData,
                        inputData.bakedGI,
                        aoFactor.indirectAmbientOcclusion,
                        inputData.positionWS,
                        inputData.normalWS, inputData.viewDirectionWS,
                        inputData.normalizedScreenSpaceUV) * _InDirectStrength;
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
            #endif
            {
                lightingData.mainLightColor = CustomLightingPhysicallyBased(brdfData, mainLight,
          inputData.normalWS,
          inputData.viewDirectionWS,
          specularHighlightsOff);
            }

            #if defined(_ADDITIONAL_LIGHTS)
            uint pixelLightCount = GetAdditionalLightsCount();

            #if USE_FORWARD_PLUS
            for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex
                                                                   ++)
            {
                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

                Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                lightingData.additionalLightsColor += CustomLightingPhysicallyBased(brdfData, light,
    inputData.normalWS, inputData.viewDirectionWS,
    specularHighlightsOff);
            }
    }
            #endif

            LIGHT_LOOP_BEGIN(pixelLightCount)
                Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                lightingData.additionalLightsColor += CustomLightingPhysicallyBased(
                    brdfData, light, inputData.normalWS, inputData.viewDirectionWS,
                    specularHighlightsOff);
            }
            LIGHT_LOOP_END
            #endif

            #if defined(_ADDITIONAL_LIGHTS_VERTEX)
            lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
            #endif

            #if REAL_IS_HALF
            // Clamp any half.inf+ to HALF_MAX
            return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
            #else
            return CalculateFinalColor(lightingData, surfaceData.alpha);
            #endif
        }

        half3 CustomCalculateBlinnPhong(Light light, InputData inputData, SurfaceData surfaceData)
        {
            half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
            half3 lightDiffuseColor = LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);

            half3 lightSpecularColor = half3(0, 0, 0);
            //#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
            #if defined(_ENABLE_SPECULAR)
            half smoothness = exp2(10 * surfaceData.smoothness + 1);
            lightSpecularColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS,
                                                             inputData.viewDirectionWS,
                                                             half4(surfaceData.specular, 1), smoothness);
            #endif

            #if _ALPHAPREMULTIPLY_ON
            return lightDiffuseColor * surfaceData.albedo * surfaceData.alpha + lightSpecularColor;
            #else
            return lightDiffuseColor * surfaceData.albedo + lightSpecularColor;
            #endif
        }

        half4 CustomUniversalFragmentBlinnPhong(InputData inputData, SurfaceData surfaceData)
        {
            #if defined(DEBUG_DISPLAY)
            half4 debugColor;

            if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
            {
                return debugColor;
            }
            #endif

            uint meshRenderingLayers = GetMeshRenderingLayer();
            half4 shadowMask = CalculateShadowMask(inputData);
            AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
            Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

            MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);

            inputData.bakedGI *= surfaceData.albedo;

            LightingData lightingData = CreateLightingData(inputData, surfaceData);
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
            #endif
            {
                lightingData.mainLightColor += CustomCalculateBlinnPhong(mainLight, inputData, surfaceData);
            }

            #if defined(_ADDITIONAL_LIGHTS)
            uint pixelLightCount = GetAdditionalLightsCount();

            #if USE_FORWARD_PLUS
            for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex
          ++)
            {
                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

                Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                lightingData.additionalLightsColor += CustomCalculateBlinnPhong(light, inputData, surfaceData);
            }
    }
            #endif

            LIGHT_LOOP_BEGIN(pixelLightCount)
                Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                lightingData.additionalLightsColor += CustomCalculateBlinnPhong(light, inputData, surfaceData);
            }
            LIGHT_LOOP_END
            #endif

            #if defined(_ADDITIONAL_LIGHTS_VERTEX)
            lightingData.vertexLightingColor += inputData.vertexLighting * surfaceData.albedo;
            #endif

            return CalculateFinalColor(lightingData, surfaceData.alpha);
        }

        half4 CustomUniversalFragmentLambert(InputData inputData, SurfaceData surfaceData)
        {
            half4 shadowMask = CalculateShadowMask(inputData);
            AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
            Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
            half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.
                shadowAttenuation);
            //half NoL = max(0.0f, dot(inputData.normalWS, mainLight.direction) * 0.5f + 0.5f);
            half NoL = saturate(dot(mainLight.direction, inputData.normalWS));
            half3 lightColor = NoL * attenuatedLightColor;
            lightColor *= surfaceData.albedo;
            uint meshRenderingLayers = GetMeshRenderingLayer();
            LightingData lightingData = CreateLightingData(inputData, surfaceData);
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
            #endif

            {
                lightingData.mainLightColor += lightColor;
            }
            return CalculateFinalColor(lightingData, surfaceData.alpha);
        }

        half4 TriplanarSampling(sampler2D topTexMap, float3 worldPos, half3 worldNormal, half falloff,
                                                       float2 tiling)
        {
            half3 projNormal = pow(abs(worldNormal), falloff);
            projNormal /= (projNormal.x + projNormal.y + projNormal.z) + 0.00001;
            half3 nsign = sign(worldNormal);
            half4 xNorm = tex2D(topTexMap, tiling * worldPos.zy * float2(nsign.x, 1.0));
            half4 yNorm = tex2D(topTexMap, tiling * worldPos.xz * float2(nsign.y, 1.0));
            half4 zNorm = tex2D(topTexMap, tiling * worldPos.xy * float2(-nsign.z, 1.0));
            return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
        }
        #if defined(_DISSOLVE_ENABLE)
        void Disslove(float2 uv, half4 albedo, float3 pos, half3 normalWS, out half4 DissolveColor,
    out half4 DissolveEmission)
        {
            half DissolveAmount13_g20 = _DissolveAmount;
            half _MainEdgeWidth = _DissolveEdgeWidth;
            float4 temp_output_101_0_g20 = albedo;
            float3 center = TransformObjectToWorld(float3(0, 0, 0));
        #if defined(_DISSOLVEAXIS_X)
        float staticSwitch23_g20 = pos.x - center.x;
        uv= _SampleMode? pos.xx : uv;
        #elif defined(_DISSOLVEAXIS_Y)
        float staticSwitch23_g20 = pos.y - center.y;
        uv= _SampleMode? pos.yy : uv;
        #elif defined(_DISSOLVEAXIS_Z)
        float staticSwitch23_g20 = pos.z - center.z;
        uv= _SampleMode? pos.zz : uv;
        #else
        float staticSwitch23_g20 = pos.x - center.x;

        #endif
        float2 dissolveUV = TRANSFORM_TEX(uv, _DissolveMap);
        half3 normal = TransformWorldToObjectNormal(normalWS);
        #if defined(_USETRIPLANAR_ON)
        half staticSwitch17_g20 = TriplanarSampling(_DissolveMap, pos, normal, 1.0, _UVTilingTriplanar).r;
        #else
        half staticSwitch17_g20 = tex2D(_DissolveMap, dissolveUV).r;
        #endif
        half temp_output_33_0_g20 = staticSwitch17_g20 * _GuideStrength + staticSwitch23_g20;
        half2 appendResult12_g20 = half2(_DissolveMinAmount, _DissolveMaxAmount);
        half2 appendResult14_g20 = half2(_DissolveMaxAmount, _DissolveMinAmount);
        half2 staticSwitch19_g20 = _InvertDissolveMinMax ? appendResult14_g20 : appendResult12_g20;

        half2 break24_g20 = staticSwitch19_g20;
        half DissolvelerpA29_g20 = break24_g20.x;
        half temp_output_1_0_g21 = DissolvelerpA29_g20;
        half DissolvelerpB31_g20 = break24_g20.y;
        half temp_output_47_0_g20 = (temp_output_33_0_g20 - temp_output_1_0_g21) / (DissolvelerpB31_g20 -
            temp_output_1_0_g21);
        half temp_output_54_0_g20 = step(DissolveAmount13_g20, temp_output_47_0_g20);
        #if defined(_ENALBE_SECONDEDGEPATTERN)
        half smoothstepResult73_g20 = smoothstep(
            0.0, 0.06, temp_output_54_0_g20 - step(DissolveAmount13_g20 + (_MainEdgeWidth + _SecondEdgeWidth),
       temp_output_47_0_g20));
        #else
        half smoothstepResult73_g20 = smoothstep(
            0.0, 0.06, temp_output_54_0_g20 - step(DissolveAmount13_g20 + _MainEdgeWidth,
                     temp_output_47_0_g20));
        #endif


        half EdgeTexBlendAlpha83_g20 = smoothstepResult73_g20;


        // #if defined(_ENALBE_MAINEDGEPATTERN) || defined(_ENALBE_SECONDEDGEPATTERN)
        //
        // #elif defined(_ENALBE_MAINEDGEPATTERN)
        //
        // #else
        // half4 lerpResult79_g20 = _DissolveColor;
        // #endif


        #if defined(_ENALBE_MAINEDGEPATTERN)
        float2 temp_cast_5 = (_MainEdgePatternTiling).xx;
        float2 time2 = frac(_Time.y * _MainEdgePatternSpeed);
        float2 texCoord50_g20 = uv * temp_cast_5 + time2;
        half4 triplanar61_g20 = TriplanarSampling(_MainEdgePattern, pos, normal, 1.0, temp_cast_5);
        #if defined(_USETRIPLANAR_ON)
        half staticSwitch67_g20 = triplanar61_g20.x;
        staticSwitch67_g20*= _MainEdgePatternStrength;
        half4 mainPattern = _MainEdgeColor2 * staticSwitch67_g20;
        #else
        half staticSwitch67_g20 = saturate(pow(saturate(tex2D(_MainEdgePattern, texCoord50_g20).r),
               _MainEdgePatternStrength));
        //staticSwitch67_g20 *= _MainEdgePatternStrength;
        half4 mainPattern = _MainEdgeColor2 * staticSwitch67_g20;
        #endif
        //half4 lerpResult82_g20 = lerp(_DissolveColor, _MainEdgeColor2, staticSwitch67_g20);
        half4 lerpResult82_g20 = _MainColorMode
                                                  ? lerp(_DissolveColor, _MainEdgeColor2,
                staticSwitch67_g20)
                                                  : _DissolveColor * mainPattern;
        #else
        half4 lerpResult82_g20 = _DissolveColor;
        #endif


        #if defined(_ENALBE_SECONDEDGEPATTERN) && defined(_ENALBE_MAINEDGEPATTERN)
        float2 temp_cast_3 = (_SecondEdgePatternTiling).xx;
        float2 time1 = frac(_Time.y * _SecondEdgePatternSpeed);
        float2 texCoord53_g20 = uv * temp_cast_3 + time1;
        half4 triplanar62_g20 = TriplanarSampling(_SecondEdgePattern, pos, normal, 1.0, temp_cast_3);
        #if defined(_USETRIPLANAR_ON)
        half staticSwitch71_g20 = triplanar62_g20.x;
        half4 secondPattern = _SecondEdgeColor2 * staticSwitch71_g20;
        #else
        half staticSwitch71_g20 = saturate(pow(saturate(tex2D(_SecondEdgePattern, texCoord53_g20).r),
                                      _SecondEdgePatternStrength));
        half4 secondPattern = _SecondEdgeColor2 * staticSwitch71_g20;
        #endif
        half4 lerpResult79_g20 = _SecondColorMode
                                     ? lerp(_SecondEdgeColor1, _SecondEdgeColor2,
                                         staticSwitch71_g20)
                                     : _SecondEdgeColor1 * secondPattern;
        #if defined(_2SIDESSECONDEDGE_ON)
        half staticSwitch34_g20 = (_SecondEdgeWidth / 2.0);
        #else
        half staticSwitch34_g20 = 0.0;
        #endif
        #if defined(_GUIDEAFFECTSEDGESBLENDING_ON)
        half staticSwitch37_g20 = temp_output_33_0_g20;
        #else
        half staticSwitch37_g20 = staticSwitch23_g20;
        #endif
        half temp_output_1_0_g22 = DissolvelerpA29_g20;
        half temp_output_43_0_g20 = ((staticSwitch37_g20 - temp_output_1_0_g22) / (DissolvelerpB31_g20 -
            temp_output_1_0_g22));
        half DissolveWithEdges32_g20 = (DissolveAmount13_g20 + _MainEdgeWidth);
        half EdgesAlpha75_g20 = (step((DissolveAmount13_g20 + staticSwitch34_g20), temp_output_43_0_g20) - step(
            (DissolveWithEdges32_g20 + staticSwitch34_g20), temp_output_43_0_g20));
        half4 lerpResult85_g20 = lerp(lerpResult79_g20, lerpResult82_g20, EdgesAlpha75_g20);
        #elif defined(_ENALBE_MAINEDGEPATTERN)
        half4 lerpResult85_g20 = lerpResult82_g20;
        #elif defined(_ENALBE_SECONDEDGEPATTERN)
        half4 lerpResult85_g20 = lerpResult79_g20;

        #else
        half4 lerpResult85_g20 = _DissolveColor;
        #endif


        half4 lerpResult89_g20 = lerp(float4(0, 0, 0, 0), lerpResult85_g20, EdgeTexBlendAlpha83_g20);
        half4 EmissionColor109_g20 = lerpResult89_g20;
        half4 lerpResult106_g20 = lerp(temp_output_101_0_g20, EmissionColor109_g20, EdgeTexBlendAlpha83_g20);

        #if defined(_EDGESAFFECT_ALBEDO)
        half4 staticSwitch99_g20 = lerpResult106_g20;
        #elif defined(_EDGESAFFECT_EMISSION)
        half4 lerpResult103_g20 = lerp(temp_output_101_0_g20, float4(0, 0, 0, 1), EdgeTexBlendAlpha83_g20);
        half4 staticSwitch99_g20 = lerpResult103_g20;
        #else
        half4 staticSwitch99_g20 = lerpResult106_g20;
        #endif
        half4 Albedo213 = staticSwitch99_g20;
        half FinalAlpha96_g20 = temp_output_54_0_g20;
        half DissolveAlpha214 = FinalAlpha96_g20;
        DissolveColor= half4(Albedo213.rgb, DissolveAlpha214);
        DissolveEmission= EmissionColor109_g20;
        }
        #endif

        half3 Rim(InputData input)
        {
            half NoV = saturate(dot(normalize(input.normalWS), normalize(input.viewDirectionWS)));
            half fresnel = pow(abs((1.0 - NoV) * _RimScale), _RimPower);
            half4 rimColor = half4(_RimColor.rgb, 1);
            rimColor.rgb *= fresnel;
            return rimColor.rgb;
        }

        half3 ACESToneMapping(half3 color, float adapted_lum)
        {
            const float A = 2.51f;
            const float B = 0.03f;
            const float C = 2.43f;
            const float D = 0.59f;
            const float E = 0.14f;

            color *= adapted_lum;
            return saturate((color * (A * color + B)) / (color * (C * color + D) + E));
        }

        half3 ApplyHue(float3 aColor, float aHue)
        {
            float angle = radians(aHue);
            float3 k = float3(0.57735, 0.57735, 0.57735);
            float sinAngle, cosAngle;
            sincos(angle, sinAngle, cosAngle);

            return aColor * cosAngle + cross(k, aColor) * sinAngle + k * dot(k, aColor) * (1 - cosAngle);
        }

        // float PointMatrix(float4 screenPos)
        // {
        //     float4x4 thresholdMatrix =
        //     {
        //         1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
        //         13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
        //         4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
        //         16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
        //     };
        //     float4x4 _RowAccess = {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1};
        //     float2 pos = screenPos.xy / screenPos.w;
        //     pos *= _ScreenParams.xy; // pixel position
        //     return thresholdMatrix[fmod(pos.x, 4)] * _RowAccess[fmod(pos.y, 4)];
        // }

        void frag(
            Varyings_forward_base input
            , out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
            , out float4 outRenderingLayers : SV_Target1
            #endif
        )
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            SurfaceData surfaceData;
            InitializeStandardLitSurfaceData(input.uv, surfaceData);

            InputData inputData;
            InitializeInputData(input, surfaceData.normalTS, inputData);


            half4 color;
            half4 mask = 0;
            #if defined(_SPHEREMASK_ENABLE)
            mask = GetMask(input.positionWS, input.uv);
            //Cutout
            float dissolveValue1 = surfaceData.alpha - _Cutoff;
            float dissolveEdgeWidth1 = _EdgeSize * (_Cutoff < 0.1 ? _Cutoff * 10 : 1);
            //Geometric
            float dissolveValue2 = mask.a;
            float dissolveEdgeWidth2 = _NoiseWidth;
            float xyzAxisCoef = 0.998;
            //Lerp
            float lerpValue = (mask.a - _NoiseWidth * xyzAxisCoef);
            lerpValue = lerpValue > 0 ? 1 : 0;
            float dissolveValue = lerp(dissolveValue2, dissolveValue1, lerpValue);
            float dissolveEdgeWidth = lerp(dissolveEdgeWidth2, dissolveEdgeWidth1, lerpValue);
            float edgeGradient = saturate(dissolveValue) * (1.0 / saturate(dissolveEdgeWidth));
            //float invertGradient = 1 - edgeGradient;
            mask.a += edgeGradient;
            mask.a = saturate(mask.a);
            half4 CutoutColor = SAMPLE_TEXTURE2D(_CutoutBaseMap, sampler_CutoutBaseMap, input.uv);
            //CutoutColor.rgb *= _BaseColorStrength;
            //surfaceData.albedo = lerp(surfaceData.albedo, CutoutColor.rgb, mask.a);
            #if defined(_ALPHATEST_ON)
            clip(mask.a - _Cutoff);
            #endif
            #endif

            #if defined(_DISSOLVE_ENABLE)
            half4 dissolve, dissolveEmission;
            Disslove(input.uv, half4(surfaceData.albedo, surfaceData.alpha), input.positionWS, inputData.normalWS,
                   dissolve, dissolveEmission);
            surfaceData.albedo = dissolve.rgb;
            clip(dissolve.a - _Cutoff);
            //surfaceData.alpha *= dissolve.a;
            #if defined(_EDGESAFFECT_EMISSION)
            surfaceData.emission += dissolveEmission.rgb;
            #endif

            #endif

            #if defined(_SHADINGMODEL_LIT)
            color = CustomUniversalFragmentPBR(inputData, surfaceData);
            #elif defined(_SHADINGMODEL_BLINNPHONG)
            color = CustomUniversalFragmentBlinnPhong(inputData, surfaceData);
            #elif defined(_SHADINGMODEL_LAMBERT)
            color = CustomUniversalFragmentLambert(inputData, surfaceData);
            #else
            color = half4(surfaceData.albedo + surfaceData.emission, surfaceData.alpha);
            #endif

            #if defined(_RIM_ENABLE)
            half3 rim = Rim(inputData);
            color.rgb += rim;
            #endif
            color.rgb = MixFog(color.rgb, inputData.fogCoord) + mask.rgb;

            #if defined(_ENABLE_COLOR_PROCESSING)
            color.rgb *= _Brightness;
            color.rgb = _EnableHue ? ApplyHue(color.rgb, _Hue) : color.rgb;
            half gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
            half3 grayColor = half3(gray, gray, gray);
            color.rgb = lerp(grayColor, color.rgb, _Saturation);
            half3 avgColor = half3(0.5, 0.5, 0.5);
            color.rgb = lerp(avgColor, color.rgb, _Contrast);
            #endif

            #if defined(_ENABLE_TONEMAP)
            color.rgb = ACESToneMapping(color.rgb, 1.0);
            #endif
            color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

            #ifdef _WRITE_RENDERING_LAYERS
            uint renderingLayers = GetMeshRenderingLayer();
            outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
            #endif

            color = color * (1 - _Light);
            outColor = color;
        }

        struct Attributes_PlanarShadow
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
        };

        struct Varings_PlanarShadow
        {
            float4 vertex : SV_POSITION;
            float4 color : TEXCOORD1;
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD2;
            float3 normalWS : TEXCOORD3;
        };

        float3 ShadowProjectPos(float4 vertPos)
        {
            float3 shadowPos = 0;
            //得到顶点的世界空间坐标
            float3 worldPos = TransformObjectToWorld(vertPos.xyz);
            float3 lightDir = normalize(_MainLightPosition.xyz);
            #if defined(_PLANARSHADOWMODE_GLOBAL)
            //阴影的世界空间坐标（低于地面的部分不做改变）
            shadowPos.y += min(worldPos.y, _GlobalHeight);
            shadowPos.xz = worldPos.xz - lightDir.xz * max(0, worldPos.y - _GlobalHeight) * _ShadowOffset / lightDir.y;
            #endif
            #if defined(_PLANARSHADOWMODE_LOCAL)
            shadowPos.y += unity_ObjectToWorld[1].w - _LocalHeight;
            shadowPos.xz = worldPos.xz - lightDir.xz * max(0, worldPos.y - _GlobalHeight) * _ShadowOffset / lightDir.y;
            #endif
            shadowPos = _EnableClipShadow ? _WorldSpaceCameraPos.y - shadowPos.y > 0 ? shadowPos : 0 : shadowPos;
            return shadowPos;
        }

        #if defined(_PLANARSHADOW_ENABLE)
        float3 ShadowGround(float4 vertex)
        {
            float3 worldPos = TransformObjectToWorld(vertex.xyz);
            Light mainLight = GetMainLight();
            float3 lightDir = normalize(mainLight.direction);
            lightDir = mul(_World2Ground, float4(lightDir, 0)).xyz;
            float4 groundPos = mul(_World2Ground, float4(worldPos, 1));
            groundPos.xz = groundPos.xz - groundPos.y / lightDir.y * _ShadowOffset * lightDir.xz;
            //groundPos.xz = groundPos.xz - lightDir.xz * _ShadowOffset / lightDir.y;
            groundPos.y = 0;
            groundPos = mul(_Ground2World, groundPos);
            groundPos = _EnableClipShadow ? _WorldSpaceCameraPos.y - groundPos.y > 0 ? groundPos : 0 : groundPos;
            groundPos = mul(unity_WorldToObject, groundPos);
            return groundPos.xyz;
        }
        #endif

        float GetAlpha(Varings_PlanarShadow i)
        {
            half4 albedoAlpha = SampleAlbedoAlpha(i.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
            half alpha = _AlphaChannel == 0 ? albedoAlpha.a : albedoAlpha.r;
            alpha = Alpha(alpha, _BaseColor, _Cutoff);
            return alpha;
        }

        Varings_PlanarShadow vert_shadow(Attributes_PlanarShadow v)
        {
            Varings_PlanarShadow o;
            ZERO_INITIALIZE(Varings_PlanarShadow, o);
            float3 shadowPos = 0;
            float falloff = 0;
            o.color = 1;
            #if defined(_DISSOLVE_ENABLE)
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            #endif
            #if defined(_PLANARSHADOW_ENABLE)
            #if defined(_PLANARSHADOWMODE_GLOBAL)
            //得到阴影的世界空间坐标
            shadowPos = ShadowProjectPos(v.vertex);
            //转换到裁切空间
            o.vertex = TransformWorldToHClip(shadowPos);
            o.positionWS = shadowPos;
            //得到中心点世界坐标
            float3 center = float3(unity_ObjectToWorld[0].w, _GlobalHeight, unity_ObjectToWorld[2].w);
            //计算阴影衰减
            falloff = 1 - saturate(distance(shadowPos, center) * _ShadowFalloff);
            //阴影颜色
            o.color *= _ShadowColor;
            o.color.a *= falloff;
            #elif defined(_PLANARSHADOWMODE_LOCAL)
            //得到阴影的世界空间坐标
            shadowPos = ShadowProjectPos(v.vertex);
            //转换到裁切空间
            o.vertex = TransformWorldToHClip(shadowPos);
            o.positionWS = shadowPos;
            //得到中心点世界坐标
            float3 center = float3(unity_ObjectToWorld[0].w, _LocalHeight, unity_ObjectToWorld[2].w);
            //计算阴影衰减
            falloff = 1 - saturate(distance(shadowPos, center) * _ShadowFalloff * 0.5 + 0.5);
            //阴影颜色
            o.color *= _ShadowColor * falloff;
            #else
            shadowPos = ShadowGround(v.vertex);
            o.vertex = TransformObjectToHClip(shadowPos);
            falloff = 1 - saturate(distance(shadowPos, v.vertex) / _ShadowIntensity);
            o.positionWS = shadowPos;
            o.color *= _ShadowColor * falloff;
            #endif

            #endif

            o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

            return o;
        }

        half4 frag_shadow(Varings_PlanarShadow i) : SV_Target
        {
            half4 mask = 1;
            half4 dissolve = 1;
            half4 dissolveEmission = 0;
            #if defined(_SPHEREMASK_ENABLE)
            mask *= GetMask(i.positionWS, i.uv);
            half4 maskColor = _EdgeColor * mask;
            i.color.rgb += maskColor.rgb;
            #endif

            #if defined(_DISSOLVE_ENABLE)
            Disslove(i.uv, i.color, i.positionWS, i.normalWS, dissolve, dissolveEmission);;
            #endif

            #if defined(_ALPHATEST_ON)
            half alpha = GetAlpha(i) * mask.a * dissolve.a * i.color.a;
            i.color.a *= step(_Cutoff, alpha);
            //clip(alpha - _Cutoff);
            #endif

            i.color = i.color * (1 - _Light);
            return i.color;
        }
        ENDHLSL

        Pass
        {
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            AlphaToMask [_AlphaToMask]
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers gles3 d3d11 vulkan
            #pragma vertex vert
            #pragma fragment frag
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            //#pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            //#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            //#pragma shader_feature_local_fragment _SPECULAR_SETUP


            // -------------------------------------
            // Features
            #pragma shader_feature_local _DISSOLVE_ENABLE
            #pragma shader_feature_local _RIM_ENABLE
            #pragma shader_feature_local _SPHEREMASK_ENABLE
            #pragma shader_feature_local _TOON_ENABLE
            #pragma shader_feature_local_fragment _USETRIPLANAR_ON
            #pragma shader_feature_local_fragment _DISSOLVEAXIS_X _DISSOLVEAXIS_Y _DISSOLVEAXIS_Z
            #pragma shader_feature_local_fragment _ENALBE_MAINEDGEPATTERN
            #pragma shader_feature_local_fragment _ENALBE_SECONDEDGEPATTERN
            #pragma shader_feature_local_fragment _EDGESAFFECT_ALBEDO _EDGESAFFECT_EMISSION
            #pragma shader_feature_local_fragment _GUIDEAFFECTSEDGESBLENDING_ON
            #pragma shader_feature_local_fragment _2SIDESSECONDEDGE_ON
            #pragma shader_feature_local_fragment _SHADINGMODEL_LIT _SHADINGMODEL_UNLIT _SHADINGMODEL_LAMBERT _SHADINGMODEL_BLINNPHONG
            #pragma shader_feature_local_fragment _ENABLE_SPECULAR
            #pragma shader_feature_local_fragment _ENABLE_DITHERING
            #pragma shader_feature_local_fragment _ENABLE_TONEMAP
            #pragma shader_feature_local_fragment _ENABLE_COLOR_PROCESSING
            #pragma shader_feature_local_fragment _ENABLE_CLEARCOAT_ON
            #pragma shader_feature_local_fragment _TEXTUREMODE_NMR _TEXTUREMODE_SINGLE _TEXTUREMODE_MRO _TEXTUREMODE_NONE
            #pragma shader_feature_local_fragment _SINGLE_NORMAL
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
            //#pragma multi_compile _ _FORWARD_PLUS
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
            //#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            //#pragma multi_compile_fragment _ DEBUG_DISPLAY
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            //#pragma multi_compile _ DOTS_INSTANCING_ON
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_CullMode]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_CullMode]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            // -------------------------------------
            // Material Keywords
            //#pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            //#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            //#pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"
            ENDHLSL
        }
        // Planar Shadows平面阴影
//        Pass
//        {
//            Name "PlanarShadow"
////            Tags
////            {
////                "LightMode" = "UniversalForwardOnly"
////            }
//            Tags
//            {
//                "LightMode" = "PlanarShadowPass"
//            }
//            //用使用模板测试以保证alpha显示正确
//            Stencil
//            {
//                Ref 0
//                Comp equal
//                Pass incrWrap
//                Fail keep
//                ZFail keep
//            }
//            Cull[_CullMode]
//
//            Blend [_SrcBlend] [_DstBlend]
//
//            ZWrite [_ZWrite]
//
//            Offset -1 ,-1
//
//            HLSLPROGRAM
//            #pragma target 4.5
//            #pragma only_renderers gles3 d3d11 vulkan
//
//            #pragma shader_feature_local_fragment _ALPHATEST_ON
//
//            #pragma shader_feature_local _PLANARSHADOW_ENABLE
//            #pragma shader_feature_local _SPHEREMASK_ENABLE
//            #pragma shader_feature_local _DISSOLVE_ENABLE
//            #pragma shader_feature_local _PLANARSHADOWMODE_GLOBAL _PLANARSHADOWMODE_LOCAL _PLANARSHADOWMODE_PLANAR
//            // GPU Instancing
//            #pragma multi_compile_instancing
//            //#pragma multi_compile _ DOTS_INSTANCING_ON
//
//            #pragma vertex vert_shadow
//            #pragma fragment frag_shadow
//            ENDHLSL
//        }
//        //Outline
//
//        Pass
//        {
//            Name "Outline"
//            Cull Front
////            Tags
////            {
////                "LightMode" = "SRPDefaultUnlit"
////            }
//            Tags
//            {
//                "LightMode" = "OutlinePass"
//            }
//            HLSLPROGRAM
//            #pragma target 4.5
//            #pragma only_renderers gles3 d3d11 vulkan
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma shader_feature_local_fragment _ALPHATEST_ON
//            #pragma shader_feature_local _TOON_ENABLE
//            #pragma shader_feature_local _ENABLE_SMOOTHNORMALS
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//                float3 normal : NORMAL;
//                #if defined(_ENABLE_SMOOTHNORMALS)
//                float4 tangent : TANGENT;
//                #endif
//            };
//
//            struct v2f
//            {
//                float4 pos : SV_POSITION;
//                float2 uv : TEXCOORD0;
//                float3 positionWS : TEXCOORD1;
//            };
//
//
//            v2f vert(appdata v)
//            {
//                v2f o;
//                #if defined(_TOON_ENABLE)
//                float3 positionOS = v.vertex.xyz;
//                //o.positionOS = v.vertex.xyz;
//                float4 pos = TransformObjectToHClip(o.positionOS);
//                #if defined(_ENABLE_SMOOTHNORMALS)
//                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.tangent.xyz);
//                #else
//                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal.xyz);
//                #endif
//                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal)) * pos.w;
//                float4 nearUpperRight = mul(unity_CameraInvProjection,
//                                            float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
//                float aspect = abs(nearUpperRight.y / nearUpperRight.x);
//                ndcNormal.x *= aspect;
//                pos.xy += 0.01 * _OutlineWidth * ndcNormal.xy;
//                o.pos = pos;
//                o.uv = v.uv;
//                #else
//                o.positionWS = TransformObjectToWorld(v.vertex.xyz);
//                o.pos = TransformWorldToHClip(o.positionWS);
//                o.uv = v.uv;
//                #endif
//                return o;
//            }
//
//            half4 frag(v2f i) : SV_Target
//            {
//                #if defined(_TOON_ENABLE)
//                #if defined(_DISSOLVE_ENABLE)
//                half4 dissolveColor = _OutlineColor;
//                half4 dissolveEmission;
//                Disslove(i.uv, dissolveColor, i.positionWS, half3(0, 0, 0), dissolveColor, dissolveEmission);
//                #if defined(_ALPHATEST_ON)
//                clip(dissolveColor.a - _Cutoff);
//                #endif
//                return dissolveColor;
//                #endif
//                #endif
//                _OutlineColor = _OutlineColor * (1 - _Light);
//                return _OutlineColor;
//            }
//            ENDHLSL
//        }
    }
    CustomEditor "IKASAMADUS.LitShaderGUI"
}