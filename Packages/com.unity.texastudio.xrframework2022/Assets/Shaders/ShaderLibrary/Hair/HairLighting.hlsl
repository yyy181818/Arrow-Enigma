#ifndef UNIVERSAL_LIGHTING_INCLUDED
#define UNIVERSAL_LIGHTING_INCLUDED

#include "../ShaderLibrary/Hair/HairBRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#if defined(LIGHTMAP_ON)
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) float2 lmName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT) OUT.xy = lightmapUV.xy * lightmapScaleOffset.xy + lightmapScaleOffset.zw;
    #define OUTPUT_SH(normalWS, OUT)
#else
    #define DECLARE_LIGHTMAP_OR_SH(lmName, shName, index) half3 shName : TEXCOORD##index
    #define OUTPUT_LIGHTMAP_UV(lightmapUV, lightmapScaleOffset, OUT)
    #define OUTPUT_SH(normalWS, OUT) OUT.xyz = SampleSHVertex(normalWS)
#endif

struct LightingData
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 vertexLightingColor;
    half3 emissionColor;
};

half3 CalculateLightingColor(LightingData lightingData, half3 albedo)
{
    half3 lightingColor = 0;

    if (IsOnlyAOLightingFeatureEnabled())
    {
        return lightingData.giColor; // Contains white + AO
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_GLOBAL_ILLUMINATION))
    {
        lightingColor += lightingData.giColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_MAIN_LIGHT))
    {
        lightingColor += lightingData.mainLightColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_ADDITIONAL_LIGHTS))
    {
        lightingColor += lightingData.additionalLightsColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_VERTEX_LIGHTING))
    {
        lightingColor += lightingData.vertexLightingColor;
    }

    lightingColor *= albedo;

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_EMISSION))
    {
        lightingColor += lightingData.emissionColor;
    }

    return lightingColor;
}

half4 CalculateFinalColor(LightingData lightingData, half alpha)
{
    half3 finalColor = CalculateLightingColor(lightingData, 1);

    return half4(finalColor, alpha);
}

half4 CalculateFinalColor(LightingData lightingData, half3 albedo, half alpha, float fogCoord)
{
    #if defined(_FOG_FRAGMENT)
        #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        float viewZ = -fogCoord;
        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
        half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
    #else
        half fogFactor = 0;
        #endif
    #else
    half fogFactor = fogCoord;
    #endif
    half3 lightingColor = CalculateLightingColor(lightingData, albedo);
    half3 finalColor = MixFog(lightingColor, fogFactor);

    return half4(finalColor, alpha);
}

LightingData CreateLightingData(InputData inputData, SurfaceData surfaceData)
{
    LightingData lightingData;

    lightingData.giColor = inputData.bakedGI;
    lightingData.emissionColor = surfaceData.emission;
    lightingData.vertexLightingColor = 0;
    lightingData.mainLightColor = 0;
    lightingData.additionalLightsColor = 0;

    return lightingData;
}

struct HairData
{  
    half3 specularTint;
    half3 secondarySpecularTint;
    half specularShift;
    half secondarySpecularShift;
    half perceptualSmoothness; 
    half secondaryPerceptualSmoothness;
    half3 transmissionColor;
    half transmissionIntensity;
};

void InitializeHairData(SurfaceData surfaceData, out HairData hairData)
{
    hairData.specularTint = surfaceData.specularTint;
    hairData.secondarySpecularTint = surfaceData.secondarySpecularTint;
    hairData.specularShift = surfaceData.specularShift;
    hairData.secondarySpecularShift = surfaceData.secondarySpecularShift;
    hairData.perceptualSmoothness = surfaceData.perceptualSmoothness;
    hairData.secondaryPerceptualSmoothness = surfaceData.secondaryPerceptualSmoothness;
    hairData.transmissionColor = surfaceData.transmissionColor;
    hairData.transmissionIntensity = surfaceData.transmissionIntensity;
}

Light GetStaticLight()
{
    Light light;
    light.direction = _StaticLightVector.xyz;
    light.distanceAttenuation = 1.0;
    light.shadowAttenuation = 1.0;
    light.color = _StaticLightColor.rgb;

    return light;
}

half RoughnessToBlinnPhongSpecularExponent(half roughness)
{
    return clamp(2 * rcp(roughness * roughness) - 2, FLT_EPS, rcp(FLT_EPS));
}

void HairSpecularWithSingleRoughness(BRDFData brdfData, HairData hairData, 
                                    half3 normalWS, half3 geomNormalWS, half3 viewDirectionWS, 
                                    half3 tangentWS, half3 lightDirectionWS, half NdotL, 
                                    out half3 specR, out half3 specT)
{
    half NdotV = saturate(dot(geomNormalWS, viewDirectionWS));

    half LdotV, NdotH, LdotH, invLenLV;
    GetBSDFAngle(viewDirectionWS, lightDirectionWS, NdotL, NdotV, LdotV, NdotH, LdotH, invLenLV);

    half perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(hairData.perceptualSmoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    half specularExponent = RoughnessToBlinnPhongSpecularExponent(roughness);
    half3 t1 = ShiftTangent(tangentWS, normalWS, hairData.specularShift);
    half3 t2 = ShiftTangent(tangentWS, normalWS, hairData.secondarySpecularShift);

    half3 H = (lightDirectionWS + viewDirectionWS) * invLenLV;

    half3 hairSpec1 = hairData.specularTint * D_KajiyaKay(t1, H, specularExponent);
    half3 hairSpec2 = hairData.secondarySpecularTint * D_KajiyaKay(t2, H, specularExponent);
    half f0 = 0.5 + (perceptualRoughness + perceptualRoughness * LdotV);
    half3 F = F_Schlick(f0, LdotH) * INV_PI;
    
    half scatterFresnel1 = pow(saturate(-LdotV), 9.0) * pow(saturate(1.0 - NdotV * NdotV), 12.0) * hairData.transmissionIntensity;
    half scatterFresnel2 = saturate(PositivePow((1.0 - NdotV), 20.0));
    
    specR = 0.25h * F * (hairSpec1 + hairSpec2) * NdotL * saturate(NdotV * FLT_MAX);
    specT = hairData.transmissionColor * (scatterFresnel1 + hairData.transmissionIntensity * scatterFresnel2);
}

half3 LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    half NdotL = saturate(dot(normal, lightDir));
    return lightColor * NdotL;
}

half3 HairLighting(BRDFData brdfData, HairData hairData, Light light, half3 normalWS, half3 geomNormalWS, half3 tangentWS, half3 viewDirectionWS, bool specularHighlightsOff)
{
    half3 lightAttenuation = light.color * light.distanceAttenuation * light.shadowAttenuation;
    half NdotL = saturate(dot(normalWS, light.direction));
    half3 radiance = NdotL * lightAttenuation;

    half3 brdf = brdfData.diffuse * radiance;
    half3 specularR, specularT;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        HairSpecularWithSingleRoughness(brdfData, hairData, normalWS, geomNormalWS, viewDirectionWS, tangentWS, light.direction, NdotL, specularR, specularT);
        brdf += saturate(specularR + specularT) * lightAttenuation;
    }
#endif // _SPECULARHIGHLIGHTS_OFF
    return brdf;
}

half3 VertexLighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < lightsCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, positionWS);
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
    }
#endif

    return vertexLightColor;
}

///////////////////////////////////////////////////////////////////////////////
//                      Fragment Functions                                   //
//       Used by ShaderGraph and others builtin renderers                    //
///////////////////////////////////////////////////////////////////////////////
half4 HairFragment(InputData inputData, SurfaceData surfaceData, half3 bitangent)
{
    bool specularHighlightsOff = false;
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    specularHighlightsOff = true;
    #endif

    bool staticSpecularHighlightOff = false;
    #if defined(_STATICSPECULARHIGHLIGHT_OFF)
    staticSpecularHighlightOff = true;
    #endif

    BRDFData brdfData;
    HairData hairData;

    // NOTE: can modify alpha
    InitializeBRDFData(surfaceData.albedo, 0.0, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
    InitializeHairData(surfaceData, hairData);
    hairData.specularTint *= surfaceData.perceptualSmoothness;
    hairData.secondarySpecularTint *= surfaceData.perceptualSmoothness;

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
    Light staticLight = GetStaticLight();

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);
    lightingData.giColor = GlobalIllumination(brdfData, inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        lightingData.mainLightColor = HairLighting(brdfData, hairData, mainLight, inputData.normalWS, surfaceData.geomNormalWS, bitangent, inputData.viewDirectionWS, specularHighlightsOff);
    }
    
    
    #if !defined(_STATICSPECULARHIGHLIGHT_OFF)
    half3 staticSpecularR, staticSpecularT;
    HairSpecularWithSingleRoughness(brdfData, hairData, inputData.normalWS, surfaceData.geomNormalWS, 
                                    inputData.viewDirectionWS, bitangent, staticLight.direction, 1, staticSpecularR, staticSpecularT);
    lightingData.mainLightColor += saturate(staticSpecularR + staticSpecularT) * staticLight.color * _StaticLightVector.w;
    #endif

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
        
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += HairLighting(brdfData, hairData, light, inputData.normalWS, surfaceData.geomNormalWS, bitangent, inputData.viewDirectionWS, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            lightingData.additionalLightsColor += HairLighting(brdfData, hairData, light, inputData.normalWS, surfaceData.geomNormalWS, bitangent, inputData.viewDirectionWS, specularHighlightsOff);
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
#endif
