// Lighting/GI.hlsl — custom global illumination (supports ClearCoat when enabled)

#ifndef TA_SHADERLIB_LIGHTING_GI
#define TA_SHADERLIB_LIGHTING_GI

half3 CustomGlobalIllumination(BRDFData brdfData, half3 bakedGI, half occlusion,
                                float3 positionWS, half3 normalWS, half3 viewDirectionWS,
                                float2 normalizedScreenSpaceUV)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half  NoV = saturate(dot(normalWS, viewDirectionWS));
    half  fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse  = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(
        reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h,
        normalizedScreenSpaceUV);

#if defined(_ENABLE_CLEARCOAT_ON)
    float topIor = lerp(1.0, 1.5, _ClearCoat);
    float viewAngle = lerp(NoV,
        sqrt(1.0 + Sq(1.0h / topIor) * (Sq(dot(normalWS, viewDirectionWS)) - 1.0)),
        _ClearCoat);
    float3 F_Iridescence = EvalIridescence(topIor, viewAngle, _IridescenceThickness, indirectSpecular);
    indirectSpecular = lerp(indirectSpecular, F_Iridescence, _Iridescence);
#endif

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    if (IsOnlyAOLightingFeatureEnabled())
        color = half3(1, 1, 1);
    return color * occlusion;
}

#endif
