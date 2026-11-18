// Lighting/BRDF.hlsl — mobile-optimized GGX specular

#ifndef TA_SHADERLIB_LIGHTING_BRDF
#define TA_SHADERLIB_LIGHTING_BRDF

half CustomDirectBRDFSpecular(BRDFData brdfData, half3 normalWS,
                               half3 lightDirectionWS, half3 viewDirectionWS)
{
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));
    float NoH = saturate(dot(float3(normalWS), halfDir));
    half  LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));

    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2
                      / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);
#if REAL_IS_HALF
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 1000.0);
#endif
    return specularTerm;
}

#endif
