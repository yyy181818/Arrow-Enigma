#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "SimpleSurfaceData.hlsl"
#include "../ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
    half _Surface;
    half4 _DoubleSidedConstants;
    half _Cutoff;
    half _AlphaCutoffShadow;
    half _SpecularAAScreenSpaceVariance;
    half _SpecularAAThreshold;

    half4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecularColor;
    half _SpecularPower;
    half _SpecularIntensity;

    half _Metallic;
    half _Smoothness;
    half _MetallicRemapMin;
    half _MetallicRemapMax;
    half _SmoothnessRemapMin;
    half _SmoothnessRemapMax;
    half _AORemapMin;
    half _AORemapMax;
    half _BumpScale;

    half _Ior;
    half4 _TransmittanceColor;
    half _ChromaticAberration;

    half4 _EmissionColor;
    half _EmissionScale;

    half _HorizonFade;

    half _EnableClipShadow;
    half4 _PlanarShadowColor;
    half _ShadowFalloff;
    float _GlobalHeight;
    float _LocalHeight;
    float _ShadowOffset;
    half _ShadowIntensity;
CBUFFER_END

float4x4 _World2Ground; //阴影接收平面（世界空间到模型空间的转换矩阵）
float4x4 _Ground2World; //阴影接收平面（模型空间到世界空间的转换矩阵）

// NOTE: Do not ifdef the properties for dots instancing, but ifdef the actual usage.
// Otherwise you might break CPU-side as property constant-buffer offsets change per variant.
// NOTE: Dots instancing is orthogonal to the constant buffer above.
#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
    UNITY_DOTS_INSTANCED_PROP(float4, _DoubleSidedConstants)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _AlphaCutoffShadow)
    UNITY_DOTS_INSTANCED_PROP(float , _SpecularAAScreenSpaceVariance)
    UNITY_DOTS_INSTANCED_PROP(float , _SpecularAAThreshold)

    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecularColor)
    UNITY_DOTS_INSTANCED_PROP(float , _SpecularPower)
    UNITY_DOTS_INSTANCED_PROP(float , _SpecularIntensity)

    UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
    UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
    UNITY_DOTS_INSTANCED_PROP(float , _MetallicRemapMin)
    UNITY_DOTS_INSTANCED_PROP(float , _MetallicRemapMax)
    UNITY_DOTS_INSTANCED_PROP(float , _SmoothnessRemapMin)
    UNITY_DOTS_INSTANCED_PROP(float , _SmoothnessRemapMax)
    UNITY_DOTS_INSTANCED_PROP(float , _AORemapMin)
    UNITY_DOTS_INSTANCED_PROP(float , _AORemapMax)
    UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)

    UNITY_DOTS_INSTANCED_PROP(float , _Ior)
    UNITY_DOTS_INSTANCED_PROP(float4, _TransmittanceColor)
    UNITY_DOTS_INSTANCED_PROP(float , _ChromaticAberration)
    
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DOTS_INSTANCED_PROP(float , _EmissionScale)
    
    UNITY_DOTS_INSTANCED_PROP(float , _HorizonFade)

    UNITY_DOTS_INSTANCED_PROP(float4, _ShadowColor)
    UNITY_DOTS_INSTANCED_PROP(float , _ShadowFalloff)
    UNITY_DOTS_INSTANCED_PROP(float , _GlobalHeight)
    UNITY_DOTS_INSTANCED_PROP(float , _LocalHeight)
    UNITY_DOTS_INSTANCED_PROP(float , _ShadowOffset)
    UNITY_DOTS_INSTANCED_PROP(float , _ShadowIntensity)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)


#define _Surface                                      UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Surface)
#define _DoubleSidedConstants                         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__DoubleSidedConstants)
#define _Cutoff                                       UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Cutoff)
#define _AlphaCutoffShadow                            UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__AlphaCutoffShadow)
#define _SpecularAAScreenSpaceVariance                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SpecularAAScreenSpaceVariance)
#define _SpecularAAThreshold                          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SpecularAAThreshold)

#define _BaseColor                                    UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseColor)
#define _SpecularColor                                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__SpecularColor)
#define _SpecularPower                                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SpecularPower)
#define _SpecularIntensity                            UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SpecularIntensity)

#define _Metallic                                     UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Metallic)
#define _Smoothness                                   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Smoothness)
#define _MetallicRemapMin                             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__MetallicRemapMin)
#define _MetallicRemapMax                             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__MetallicRemapMax)
#define _SmoothnessRemapMin                           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SmoothnessRemapMin)
#define _SmoothnessRemapMax                           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__SmoothnessRemapMax)
#define _AORemapMin                                   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__AORemapMin)
#define _AORemapMax                                   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__AORemapMax)
#define _BumpScale                                    UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__BumpScale)

#define _Ior                                          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Ior)
#define _TransmittanceColor                           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__TransmittanceColor)
#define _ChromaticAberration                          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ChromaticAberration)

#define _EmissionColor                                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__EmissionColor)
#define _EmissionScale                                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__EmissionScale)

#define _HorizonFade                                  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__HorizonFade)

#define _ShadowColor                                  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__ShadowColor)
#define _ShadowFalloff                                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ShadowFalloff)
#define _GlobalHeight                                 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__GlobalHeight)
#define _LocalHeight                                  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__LocalHeight)
#define _ShadowOffset                                 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ShadowOffset)
#define _ShadowIntensity                              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ShadowIntensity)
#endif

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    half4 mads = half4(_Metallic, 1.0, 1.0, _Smoothness);
    half2 metallicMinMax = half2(_MetallicRemapMin, _MetallicRemapMax);
    half2 aoMinMax = half2(_AORemapMin, _AORemapMax);
    half2 smoothnessMinMax = half2(_SmoothnessRemapMin, _SmoothnessRemapMax);
    half4 maskMap = SampleMaskMap(uv, TEXTURE2D_ARGS(_MaskMap, sampler_MaskMap), metallicMinMax, aoMinMax,
                                  smoothnessMinMax, mads);

    #ifdef _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = SAMPLE_TEXTURE2D(_SpecularColorMap, sampler_SpecularColorMap, uv).rgb * _SpecularColor.rgb;
    #else
    outSurfaceData.metallic = 1 - maskMap.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
    #endif

    outSurfaceData.smoothness = maskMap.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = maskMap.g;

    outSurfaceData.ior = _Ior;
    outSurfaceData.transmittanceColor = _TransmittanceColor.rgb * SAMPLE_TEXTURE2D(
        _TransmittanceColorMap, sampler_TransmittanceColorMap, uv).rgb;
    outSurfaceData.chromaticAberration = _ChromaticAberration;

    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, _EmissionScale,
                                             TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    outSurfaceData.clearCoatMask = 0.0;
    outSurfaceData.clearCoatSmoothness = 1.0;

    outSurfaceData.horizonFade = _HorizonFade;
}
#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
