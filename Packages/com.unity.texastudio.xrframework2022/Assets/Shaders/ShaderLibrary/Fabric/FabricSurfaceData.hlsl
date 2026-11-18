#ifndef UNIVERSAL_SURFACE_DATA_INCLUDED
#define UNIVERSAL_SURFACE_DATA_INCLUDED

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
float4 _BaseMap_TexelSize;      float4 _BaseMap_MipInfo;
TEXTURE2D(_MaskMap);            SAMPLER(sampler_MaskMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
TEXTURE2D(_SheenMap);           SAMPLER(sampler_SheenMap);
TEXTURE2D(_ThreadMap);          SAMPLER(sampler_ThreadMap);
TEXTURE2D(_FuzzMap);            SAMPLER(sampler_FuzzMap);
TEXTURE2D(_ThicknessMap);       SAMPLER(sampler_ThicknessMap);

struct SurfaceData
{
    half3 albedo;
    half3 specular;
    half  metallic;
    half  anisotropy;
    half  smoothness;
    half3 normalTS;
    half3 emission;
    half  occlusion;
    half  alpha;
    half  clearCoatMask;
    half  clearCoatSmoothness;

    half3 sheenColor;
    half  sheenSmoothness;
    half  thickness;
    half3 translucencyColor;
    half  translucencyPower;
    half  translucencyScale;
    half  translucencyAmbient;
    half  translucencyDistortion;
    half  translucencyShadows;
};

#endif
