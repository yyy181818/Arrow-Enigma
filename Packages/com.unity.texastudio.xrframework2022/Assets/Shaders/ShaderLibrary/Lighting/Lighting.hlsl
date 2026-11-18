// Lighting/Lighting.hlsl — custom PBR direct lighting

#ifndef TA_SHADERLIB_LIGHTING_LIGHTING
#define TA_SHADERLIB_LIGHTING_LIGHTING

half3 CustomLightingPhysicallyBased(BRDFData brdfData, Light mainLight,
                                     half lightAttenuation, float3 normalWS,
                                     half3 viewDirectionWS, bool specularHighlightsOff)
{
    half3 lightDirectionWS = mainLight.direction;
    half NdotL = saturate(dot(lightDirectionWS, normalWS));
    float3 radiance = mainLight.color * (lightAttenuation * NdotL);
    half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * _SpecularColor.rgb
              * CustomDirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
    }
#endif
    return brdf * radiance * _DirectStrength;
}

half3 CustomLightingPhysicallyBased(BRDFData brdfData, Light light, float3 normalWS,
                                     half3 viewDirectionWS, bool specularHighlightsOff)
{
    return CustomLightingPhysicallyBased(brdfData, light,
        light.distanceAttenuation * light.shadowAttenuation,
        normalWS, viewDirectionWS, specularHighlightsOff);
}

#endif
