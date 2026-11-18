#ifndef UNIVERSAL_SURFACE_DATA_INCLUDED
#define UNIVERSAL_SURFACE_DATA_INCLUDED

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
float4 _BaseMap_TexelSize;      float4 _BaseMap_MipInfo;
TEXTURE2D(_ScleraNormalMap);    SAMPLER(sampler_ScleraNormalMap);
TEXTURE2D(_IrisMap);            SAMPLER(sampler_IrisMap);
TEXTURE2D(_IrisNormalMap);      SAMPLER(sampler_IrisNormalMap);
TEXTURE2D(_EmissionMap);        SAMPLER(sampler_EmissionMap);

struct SurfaceData
{
    half3 albedo;
    half3 specular;
    half  metallic;
    half  smoothness;
    half3 normalTS;
    half3 emission;
    half  occlusion;
    half  alpha;
    half  surfaceMask;
    half clearCoatMask;
    half clearCoatSmoothness;
};

#endif
