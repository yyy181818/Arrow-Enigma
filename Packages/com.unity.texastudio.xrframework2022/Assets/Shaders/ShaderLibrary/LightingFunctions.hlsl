#ifndef UNIVERSAL_LIGHT_FUNCTIONS_INCLUDED
#define UNIVERSAL_LIGHT_FUNCTIONS_INCLUDED

struct LightingData
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 vertexLightingColor;
    half3 emissionColor;
};

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

half DirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    
    #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_MOBILE)
    half3 lightDirectionWSFloat3 = lightDirectionWS;
    half3 halfDir = SafeNormalize(lightDirectionWSFloat3 + viewDirectionWS);
    half NoH = saturate(dot(normalWS, halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    half d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    #else
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));
    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    #endif
    
    half d2 = half(d * d);
    half LoH2 = LoH * LoH;

    half specularTerm = brdfData.roughness2 / (d2 * max(0.1h, LoH2) * brdfData.normalizationTerm);

    #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
    #endif

    return specularTerm;
}

half ComplexDirectBRDFSpecular(BRDFData brdfData, BSDFData bsdfData, half3 normalWS, half4 tangentWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_MOBILE)
    half3 lightDirectionWSFloat3 = lightDirectionWS;
    half3 halfDir = SafeNormalize(lightDirectionWSFloat3 + viewDirectionWS);
    float NoH = saturate(dot(normalWS, halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    half d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    #else
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));
    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    #endif
    
    half d2 = half(d * d);
    half LoH2 = LoH * LoH;
    half F = NoH * NoH * NoH;

    #ifdef _MATERIAL_FEATURE_ANISOTROPY
    half3 bitangentWS = tangentWS.w * cross(normalWS, tangentWS.xyz);
    half3 H = SafeNormalize(lightDirectionWSFloat3 + viewDirectionWS);
    half specularTerm = DV_Anisotropy(brdfData.perceptualRoughness, bsdfData.anisotropy, lightDirectionWSFloat3, viewDirectionWS, H, tangentWS, bitangentWS, normalWS) * F;
    #else
    half specularTerm = brdfData.roughness2 / (d2 * max(half(0.1), LoH2) * brdfData.normalizationTerm);
    #endif
    specularTerm = pow(specularTerm, _SpecularPower) * _SpecularIntensity;
    #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0);
    #endif

    return specularTerm;
}

///////////////////////////////////////////////////////////////////////////////
//                      Lighting Functions                                   //
///////////////////////////////////////////////////////////////////////////////
half3 LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    half NdotL = saturate(dot(normal, lightDir));
    return lightColor * NdotL;
}

half3 SimpleLitLighting(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, half alpha, bool specularHighlightsOff)
{
    half3 radiance = ComputeRadiance(light, normalWS, alpha);
    half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, light.direction, viewDirectionWS);
    }
#endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 LitLighting(BRDFData brdfData, BRDFData brdfDataClearCoat, VectorsData vectorsData, Light light, half clearCoatMask, half occlusion, half alpha, bool specularHighlightsOff)
{
    half3 radiance = ComputeRadiance(vectorsData, light, occlusion, alpha);
    half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, vectorsData.normalWS, light.direction, vectorsData.viewDirectionWS);

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        #if defined(_CLEARCOAT_NORMALMAP)
            half3 brdfCoat = kDielectricSpec.rrr * DirectBRDFSpecular(brdfDataClearCoat, vectorsData.coatNormalWS, light.direction, vectorsData.viewDirectionWS);
            half NoV = saturate(dot(vectorsData.coatNormalWS, vectorsData.viewDirectionWS));
        #else
            half3 brdfCoat = kDielectricSpec.rrr * DirectBRDFSpecular(brdfDataClearCoat, vectorsData.normalWS, light.direction, vectorsData.viewDirectionWS);
            half NoV = saturate(dot(vectorsData.normalWS, vectorsData.viewDirectionWS));
        #endif

        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
#endif // _CLEARCOAT
    }
#endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 ComplexLitLighting(BRDFData brdfData, BRDFData brdfDataClearCoat, BSDFData bsdfData, VectorsData vectorsData, Light light, half occlusion, half alpha, bool specularHighlightsOff)
{
    half3 radiance = ComputeComplexRadiance(brdfData, bsdfData, vectorsData, light, occlusion, alpha);
    half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        half anisotropy = 0.0;
    #if defined (_MATERIAL_FEATURE_ANISOTROPY)
        anisotropy = bsdfData.anisotropy;
    #endif
    #if defined (_MATERIAL_FEATURE_IRIDESCENCE)
        brdfData.specular = bsdfData.iridescence;
    #endif
        brdf += brdfData.specular * ComplexDirectBRDFSpecular(brdfData, bsdfData, vectorsData.normalWS, vectorsData.tangentWS, light.direction, vectorsData.viewDirectionWS);

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        #if defined(_CLEARCOAT_NORMALMAP)
            half3 brdfCoat = kDielectricSpec.rrr * ComplexDirectBRDFSpecular(brdfDataClearCoat, bsdfData, vectorsData.coatNormalWS, vectorsData.tangentWS, light.direction, vectorsData.viewDirectionWS);
            half NoV = saturate(dot(vectorsData.coatNormalWS, vectorsData.viewDirectionWS));
        #else
            half3 brdfCoat = kDielectricSpec.rrr * ComplexDirectBRDFSpecular(brdfDataClearCoat, bsdfData, vectorsData.normalWS, vectorsData.tangentWS, light.direction, vectorsData.viewDirectionWS);
            half NoV = saturate(dot(vectorsData.normalWS, vectorsData.viewDirectionWS));
        #endif
        
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0h - NoV);
    
        brdf = brdf * (1.0 - bsdfData.clearCoatMask * coatFresnel) + brdfCoat * bsdfData.clearCoatMask;
#endif // _CLEARCOAT
    }
#endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 VertexLighting(float3 positionWS, half3 normalWS)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(lightsCount)
        Light light = GetAdditionalLight(lightIndex, positionWS);
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingLambert(lightColor, light.direction, normalWS);
    LIGHT_LOOP_END
#endif

    return vertexLightColor;
}
#endif