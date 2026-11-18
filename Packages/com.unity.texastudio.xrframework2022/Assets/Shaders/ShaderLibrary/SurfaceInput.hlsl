#ifndef UNIVERSAL_INPUT_SURFACE_INCLUDED
#define UNIVERSAL_INPUT_SURFACE_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    #ifdef _CLEARCOAT_NORMALMAP
    #define _CLEARCOAT_NORMAL
    #endif
#endif

#if defined(_BENTNORMAL_SPECULAROCCLUSION)
    #ifdef _BENTNORMALMAP
    #define _BENT_NORMAL
    #endif
#endif

///////////////////////////////////////////////////////////////////////////////
//                      Material Property Helpers                            //
///////////////////////////////////////////////////////////////////////////////
half Alpha(half albedoAlpha, half4 color, half cutoff)
{
    half alpha = albedoAlpha * color.a;
    #if defined(_ALPHATEST_ON)
    clip(alpha - cutoff);
    #endif

    return alpha;
}

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
}

half4 SampleMaskMap(float2 uv, TEXTURE2D_PARAM(maskMap, sampler_maskMap), 
                    half2 metallicMinMax, half2 aoMinMax, half2 smoothnessMinMax, half4 mads)
{
    half4 Out = mads;
#ifdef _MASKMAP
    half4 m = SAMPLE_TEXTURE2D(maskMap, sampler_maskMap, uv);
    Out.r = metallicMinMax.x + m.r * (metallicMinMax.y - metallicMinMax.x);
    Out.g = aoMinMax.x + m.g * (aoMinMax.y - aoMinMax.x);
    Out.b = m.b;
    Out.a = smoothnessMinMax.x + m.a * (smoothnessMinMax.y - smoothnessMinMax.x);
#endif

    return Out;
}

half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = 1.0h)
{
#ifdef _NORMALMAP
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

half3 SampleCoatNormal(float2 uv, TEXTURE2D_PARAM(coatNormalMap, sampler_coatNormalMap), half scale = 1.0h)
{
#ifdef _CLEARCOAT_NORMALMAP
    half4 n = SAMPLE_TEXTURE2D(coatNormalMap, sampler_coatNormalMap, uv);
    #if BUMP_SCALE_NOT_SUPPORTED
        return UnpackNormal(n);
    #else
        return UnpackNormalScale(n, scale);
    #endif
#else
    return half3(0.0h, 0.0h, 1.0h);
#endif
}

half3 SampleBentNormal(float2 uv, TEXTURE2D_PARAM(bentNormalMap, sampler_bentNormalMap), half scale = 1.0h)
{
#ifdef _BENTNORMALMAP
    half4 n = SAMPLE_TEXTURE2D(bentNormalMap, sampler_bentNormalMap, uv);
    #if BUMP_SCALE_NOT_SUPPORTED
        return UnpackNormal(n);
    #else
        return UnpackNormalScale(n, scale);
    #endif
#else
    return half3(0.0h, 0.0h, 1.0h);
#endif
}

half3 BumpStrength(half3 In, half Strength)
{
    return half3(In.rg * Strength, lerp(1.0h, In.b, saturate(Strength)));
}

//Calculate DetailAlbedo
half3 DetailAlbedo(half3 baseColor, half detailBaseColor, half detailMask, half detailScale)
{
    half albedoDetailSpeed = saturate(abs(detailBaseColor) * detailScale);
    half3 baseColorOverlay = lerp(sqrt(baseColor), (detailBaseColor < 0.0h) ? half3(0.0h, 0.0h, 0.0h) : half3(1.0h, 1.0h, 1.0h), albedoDetailSpeed * albedoDetailSpeed);
    baseColorOverlay *= baseColorOverlay;

    return lerp(baseColor, saturate(baseColorOverlay), detailMask);
}

//Calculate DetailNormal
half3 DetailNormal(half3 normalTS, half3 normalDetail, half detailMask)
{
    return lerp(normalTS, BlendNormalRNM(normalTS, normalize(normalDetail)), detailMask);
}

//Calculate DetailSmoothness
half DetailSmoothness(half smoothness, half detailSmoothness, half detailSmoothnessScale, half detailMask)
{
    detailSmoothness = detailSmoothness * 2.0h - 1.0h;
    half smoothnessDetailSpeed = saturate(abs(detailSmoothness) * detailSmoothnessScale);
    half smoothnessOverlay = lerp(smoothness, (detailSmoothness < 0.0h) ? 0.0h : 1.0h, smoothnessDetailSpeed);

    return lerp(smoothness, saturate(smoothnessOverlay), detailMask);
}

half3 SampleEmission(float2 uv, half3 emissionColor, half emissionScale, TEXTURE2D_PARAM(emissionMap, sampler_emissionMap))
{
    half3 emission = 0;
#ifdef _EMISSION
    emission = emissionScale * emissionColor * SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb; 
    #ifdef _EMISSION_WITH_BASE
    emission *= outSurfaceData.albedo;
    #endif
#endif
    return emission;
}
#endif
