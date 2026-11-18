#ifndef UNIVERSAL_SURFACE_DATA_INCLUDED
#define UNIVERSAL_SURFACE_DATA_INCLUDED

TEXTURE2D(_BaseMap);                      SAMPLER(sampler_BaseMap);
float4 _BaseMap_TexelSize;                float4 _BaseMap_MipInfo;
TEXTURE2D(_MaskMap);                      SAMPLER(sampler_MaskMap);
TEXTURE2D(_SpecularColorMap);             SAMPLER(sampler_SpecularColorMap);
TEXTURE2D(_BumpMap);                      SAMPLER(sampler_BumpMap);
TEXTURE2D(_BentNormalMap);                SAMPLER(sampler_BentNormalMap);
TEXTURE2D(_HeightMap);                    SAMPLER(sampler_HeightMap);
TEXTURE2D(_TangentMap);                   SAMPLER(sampler_TangentMap);
TEXTURE2D(_AnisotropyMap);                SAMPLER(sampler_AnisotropyMap);
TEXTURE2D(_ThicknessCurvatureMap);        SAMPLER(sampler_ThicknessCurvatureMap);
TEXTURE2D(_IridescenceMaskMap);           SAMPLER(sampler_IridescenceMaskMap);
TEXTURE2D(_IridescenceThicknessMap);      SAMPLER(sampler_IridescenceThicknessMap);
TEXTURE2D(_ClearCoatMap);                 SAMPLER(sampler_ClearCoatMap);
TEXTURE2D(_CoatNormalMap);                SAMPLER(sampler_CoatNormalMap);
TEXTURE2D(_DetailMap);                    SAMPLER(sampler_DetailMap);
TEXTURE2D(_TransmittanceColorMap);        SAMPLER(sampler_TransmittanceColorMap);
TEXTURE2D(_EmissionMap);                  SAMPLER(sampler_EmissionMap);

struct SurfaceData
{
    half3 albedo;
    half3 specular;
    half  metallic;
    half  smoothness;
    half  anisotropy;
    half3 normalTS;
    half3 bentNormalTS;
    half3 tangentTS;
    half  clearCoatMask;
    half  clearCoatSmoothness;
    half3 coatNormalTS;

    half3 iridescenceTMS; // Iridescence Thickness/Mask/Shift

    half  thickness;
    half  curvature;

    half3 scatteringColor;
    half3 scatteringShadowsColor;
    half  transmissionScale;

    half  translucencyPower;
    half  translucencyScale;
    half  translucencyAmbient;
    half  translucencyDistortion;
    half  translucencyShadows;

    half ior;
    half3 transmittanceColor;
    half chromaticAberration;
    
    half3 emission;
    half  occlusion;
    half  alpha;
    
    half  horizonFade;
    half giOcclusionBias;
};

#endif