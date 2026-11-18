#ifndef UNIVERSAL_SURFACE_DATA_INCLUDED
#define UNIVERSAL_SURFACE_DATA_INCLUDED

TEXTURE2D(_BaseMap);                      SAMPLER(sampler_BaseMap);
float4 _BaseMap_TexelSize;                float4 _BaseMap_MipInfo;
TEXTURE2D(_MaskMap);                      SAMPLER(sampler_MaskMap);
TEXTURE2D(_SpecularColorMap);             SAMPLER(sampler_SpecularColorMap);
TEXTURE2D(_BumpMap);                      SAMPLER(sampler_BumpMap);
TEXTURE2D(_TransmittanceColorMap);        SAMPLER(sampler_TransmittanceColorMap);
TEXTURE2D(_EmissionMap);                  SAMPLER(sampler_EmissionMap);

struct SurfaceData
{
    half3 albedo;
    half3 specular;
    half  metallic;
    half  smoothness;
    half3 normalTS;
    half3 emission;
    half  occlusion;
    half  horizonFade;
    half  alpha;
    half  clearCoatMask;
    half  clearCoatSmoothness;

    half  ior;
    half3 transmittanceColor;
    half  chromaticAberration;
};

#endif