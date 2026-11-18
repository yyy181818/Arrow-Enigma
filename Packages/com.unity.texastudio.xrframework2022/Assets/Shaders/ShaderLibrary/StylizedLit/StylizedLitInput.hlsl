#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;

//StylizedLit
float4 _MedColor, _ShadowColor, _ReflectColor;
float4 _SpecularLightOffset;
float _MedThreshold, _MedSmooth, _ShadowThreshold, _ShadowSmooth, _ReflectThreshold, _ReflectSmooth;
float _SpecularThreshold, _SpecularSmooth, _SpecularIntensity ,_FresnelIntensity, _FresnelThreshold, _FresnelSmooth;
float _ReflProbeIntensity, _MetalReflProbeIntensity;
float _MedBrushStrength, _ShadowBrushStrength, _ReflBrushStrength;
float _ReceiveShadows;
float _GIIntensity, _GGXSpecular, _DirectionalFresnel;
float4 _BrushTex_ST;

//Planar Shadow
half _EnableClipShadow;
half4 _PlanarShadowColor;
float _GlobalHeight;
float _LocalHeight;
float _ShadowFalloff;
float _ShadowOffset;
float _ShadowIntensity;

//Color Processing
half _EnableHue;
half _Contrast;
half _Saturation;
half _Brightness;
        half _Hue;

        // Vertex Sway
        float _SwayAmount, _SwaySpeed, _SwayFrequency, _SwayHeight;

        half _Surface;
CBUFFER_END
float4x4 _World2Ground; //阴影接收平面（世界空间到模型空间的转换矩阵）
float4x4 _Ground2World; //阴影接收平面（模型空间到世界空间的转换矩阵）
float _ReflProbeRotation;

TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_BrushTex);           SAMPLER(sampler_BrushTex);          

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
    specGloss = SAMPLE_METALLICSPECULAR(uv);
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif
#else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
        specGloss.rgb = _SpecColor.rgb;
    #else
        specGloss.rgb = _Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a = _Smoothness;
    #endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
#if defined(SHADER_API_GLES)
    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
#else
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
#endif
#else
    return 1.0;
#endif
}

// Alpha-To-Coverage（A2C）支持：_ALPHATOMASK 开启时绕过 _Cutoff 硬剪，
// 仅剔除完全空的像素，让 MSAA 覆盖采样根据 alpha 全范围 (0..1) 平滑 cutout 边缘。
// 真正的硬件开关由 ForwardLit pass 的 AlphaToMask On 提供；本关键字只控制是否输出真实 alpha。
half TexaAlpha(half albedoAlpha, half4 color, half cutoff)
{
#if defined(_ALPHATOMASK)
    half a = albedoAlpha * color.a;
    clip(a - 0.01);
    return a;
#elif defined(_ALPHATEST_ON)
    clip(albedoAlpha * color.a - cutoff);
    return albedoAlpha * color.a;
#else
    return albedoAlpha * color.a;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = TexaAlpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

#if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
#endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
