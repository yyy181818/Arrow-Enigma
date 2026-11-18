#include "../ShaderLibrary/SubsurfaceScattering.hlsl"

half3 ComputeRadiance(Light light, half3 normalWS, half alpha)
{
    half NdotL = saturate(dot(normalWS, light.direction));
    half lightAttenuation = light.distanceAttenuation;
    #ifndef _REFRACTION
    lightAttenuation *= light.shadowAttenuation * NdotL;
    #else
    lightAttenuation *= lerp(1.0, light.shadowAttenuation * NdotL, alpha);
    #endif

    half3 radiance = light.color * lightAttenuation;
    return radiance;
}

half3 ComputeRadiance(VectorsData vectorsData, Light light, half occlusion, half alpha)
{
    half NdotL = saturate(dot(vectorsData.normalWS, light.direction));
    half lightAttenuation = light.distanceAttenuation;
    #ifndef _REFRACTION
    lightAttenuation *= light.shadowAttenuation * NdotL;
    #else
    lightAttenuation *= lerp(1.0, light.shadowAttenuation * NdotL, alpha);
    #endif

    #if defined(_SHADER_QUALITY_MICRO_SHADOWS) && !defined(_REFRACTION)
    lightAttenuation *= NdotL >= 0.0h ? ComputeMicroShadowing(occlusion, dot(vectorsData.normalWS, light.direction), _MicroShadowOpacity) : 1.0h;
    #endif

    half3 radiance = light.color * lightAttenuation;
    return radiance;
}

half3 ComputeComplexRadiance(BRDFData brdfData, BSDFData bsdfData, VectorsData vectorsData, Light light, half occlusion, half alpha)
{
    half3 lightAttenuation = light.distanceAttenuation;
#ifndef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
    half NdotL = saturate(dot(vectorsData.normalWS, light.direction));
    #ifndef _REFRACTION
    lightAttenuation *= light.shadowAttenuation * NdotL;
    #else
    lightAttenuation *= lerp(1.0, light.shadowAttenuation * NdotL, alpha);
    #endif
#else
    half3 NdotL = SubSurfaceScattering(brdfData, bsdfData, light, vectorsData.normalWS);
    #if defined(_MATERIAL_FEATURE_TRANSMISSION)
        NdotL += Transmission(light, vectorsData.normalWS, bsdfData.scatteringColor, bsdfData.thickness, bsdfData.transmissionScale);
    #endif
    //SSS shadows
    #if defined(_MATERIAL_FEATURE_FAKE_SSS_SHADOWS)
        bsdfData.scatteringShadowsColor = min(bsdfData.scatteringShadowsColor, 0.45h);
        half3 scatteredShadows = saturate(pow(abs(light.shadowAttenuation), (1.0h - bsdfData.scatteringShadowsColor)));
        #ifndef _REFRACTION
        lightAttenuation *= scatteredShadows * NdotL;
        #else
        lightAttenuation *= lerp(1.0, scatteredShadows * NdotL, alpha);
        #endif
    #else
        #ifndef _REFRACTION
        lightAttenuation *= light.shadowAttenuation * NdotL;
        #else
        lightAttenuation *= lerp(1.0, light.shadowAttenuation * NdotL, alpha);
        #endif
    #endif
#endif

#if defined(_SHADER_QUALITY_MICRO_SHADOWS) && !defined(_REFRACTION)
    half microShadows = ComputeMicroShadowing(occlusion, dot(vectorsData.normalWS, light.direction), _MicroShadowOpacity);
    #if !defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING)
        lightAttenuation *= NdotL >= 0.0h ? microShadows : 1.0h;
    #else
        lightAttenuation *= NdotL.g >= 0.0h ? microShadows : 1.0h;
    #endif
#endif

    half3 radiance = light.color * lightAttenuation;

    return radiance;
}