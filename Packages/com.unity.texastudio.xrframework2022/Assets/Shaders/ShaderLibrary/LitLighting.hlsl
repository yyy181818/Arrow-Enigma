#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "../ShaderLibrary/VectorsData.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/UniversalGlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "../ShaderLibrary/BSDF.hlsl"
#include "../ShaderLibrary/Radiance.hlsl"
#include "../ShaderLibrary/LightingFunctions.hlsl"
#include "../ShaderLibrary/Refraction.hlsl"

#if defined(LIGHTMAP_ON)
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float2 lmName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
    #define OUTPUT_SH(normalWS, OUT)
#else
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half3 shName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
    #define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
#endif

half4 SimpleLitFragment(InputData inputData, SurfaceData surfaceData, VectorsData vectorsData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;
    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);
    #ifdef _REFRACTION
    half3 refractionColor = SimpleRefraction(inputData, surfaceData, brdfData.perceptualRoughness);
    brdfData.diffuse = lerp(refractionColor, brdfData.diffuse, surfaceData.alpha);
    #endif

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;
    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination(brdfData, inputData, inputData.positionWS, vectorsData.geomNormalWS, aoFactor.indirectAmbientOcclusion, surfaceData.horizonFade);

    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        lightingData.mainLightColor = SimpleLitLighting(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, surfaceData.alpha, specularHighlightsOff);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += SimpleLitLighting(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, surfaceData.alpha, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += SimpleLitLighting(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, surfaceData.alpha, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    #if !defined(_REFRACTION)
    return CalculateFinalColor(lightingData, surfaceData.alpha);
    #else 
    return CalculateRefractionFinalColor(lightingData, surfaceData.alpha);
    #endif
}

half4 ComplexLitFragment(InputData inputData, SurfaceData surfaceData, VectorsData vectorsData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;
    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);
    #ifdef _REFRACTION
    half3 refractionColor = SimpleRefraction(inputData, surfaceData, brdfData.perceptualRoughness);
    brdfData.diffuse = lerp(refractionColor, brdfData.diffuse, surfaceData.alpha);
    #endif

    BSDFData bsdfData;
    half3 iridescence = half3(0.2, 0.2, 0.2);
    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
    iridescence = IridescenceSpecular(inputData.normalWS, inputData.viewDirectionWS, brdfData.specular, surfaceData.iridescenceTMS, surfaceData.clearCoatMask);
    #endif
    InitializeBSDFData(bsdfData, surfaceData, iridescence);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;
    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
    LightingData lightingData = CreateLightingData(inputData, surfaceData);
    
    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, bsdfData, surfaceData, vectorsData, inputData.positionWS, inputData.bakedGI, aoFactor.indirectAmbientOcclusion);
    
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        lightingData.mainLightColor = ComplexLitLighting(brdfData, brdfDataClearCoat, bsdfData, vectorsData, mainLight, surfaceData.occlusion, surfaceData.alpha, specularHighlightsOff);
        #ifdef _MATERIAL_FEATURE_TRANSLUCENCY
        lightingData.mainLightColor += Translucency(bsdfData, mainLight, brdfData.diffuse, inputData.normalWS, inputData.viewDirectionWS);
        #endif
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += ComplexLitLighting(brdfData, brdfDataClearCoat, bsdfData, vectorsData, light, surfaceData.occlusion, surfaceData.alpha, specularHighlightsOff);
            #ifdef _MATERIAL_FEATURE_TRANSLUCENCY
            lightingData.additionalLightsColor += Translucency(bsdfData, light, brdfData.diffuse, inputData.normalWS, inputData.viewDirectionWS);
            #endif
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += ComplexLitLighting(brdfData, brdfDataClearCoat, bsdfData, vectorsData, light, surfaceData.occlusion, surfaceData.alpha, specularHighlightsOff);
            #ifdef _MATERIAL_FEATURE_TRANSLUCENCY
            lightingData.additionalLightsColor += Translucency(bsdfData, light, brdfData.diffuse, inputData.normalWS, inputData.viewDirectionWS);
            #endif
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    #if !defined(_REFRACTION)
    return CalculateFinalColor(lightingData, surfaceData.alpha);
    #else 
    return CalculateRefractionFinalColor(lightingData, surfaceData.alpha);
    #endif
}
#endif