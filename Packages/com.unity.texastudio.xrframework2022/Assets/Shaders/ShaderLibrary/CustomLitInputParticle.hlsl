#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesInput.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"

#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
// //float4 _BaseMap_ST;
// float4 _DetailAlbedoMap_ST;
// //half4 _BaseColor;
// half4 _SpecColor;
// half4 _EmissionColor;
// half _Cutoff;
// half _Smoothness;
// half _Metallic;
// half _BumpScale;
// half _Parallax;
// half _OcclusionStrength;
// half _ClearCoatMask;
// half _ClearCoatSmoothness;
// half _DetailAlbedoMapScale;
// half _DetailNormalMapScale;
// half _Surface;

float4 _ReflectionCubeMap_HDR;
half4 _MainTex;
half4 _NormalMap_ST;
half4 _AddColor;
half _EmissionStrength;
half _LightIntensitiy  ;
half _SpecShininess;
half4 _Tiliing;
half _HeightLight;

half4 _BaseColor;
float4 _BaseMap_ST;
half _MixAlpha;
half _PartcleAdvanceAlpha;
        
half _MaskChannel;
float4 _MaskTex_ST;
half4 _MaskMinMaxSacle;
half _MaskU2;

float4 _NoiseTex_ST;
half _NoiseScale;
half _NoiseU2;
        
half4 _DissolveTex_ST;
half _DissolvelU2;
half _DissolveInt;
half _DissolveHardness;
half _DissolveEdg;
half4 _DissolveColor;

half4 _MainMaskFlow;
half4 _NoiseDissFlow;

float4 _uvRotate;
//half _Alpha;

half4 _FresnelColor;
half _FresnelScale;
half _FresnelPower;
half _FresnelBias1;

half4 _InnerFresnelColor;
half _InnerFresnelScale;
half _InnerFresnelPower;
half _InnerFresnelBias1;

half _Speed;

half _MixRGB;
CBUFFER_END
half _Light;




struct AttributesDepthOnlyParticle
{
    float4 vertex                       : POSITION;

    #if defined(_ALPHATEST_ON)
    half4 color                     : COLOR;

    #if defined(_FLIPBOOKBLENDING_ON) && !defined(UNITY_PARTICLE_INSTANCING_ENABLED)
    float4 texcoords            : TEXCOORD0;
    float texcoordBlend         : TEXCOORD1;
    #else
    float2 texcoords            : TEXCOORD0;
    #endif
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsDepthOnlyParticle
{
    float4 clipPos                      : SV_POSITION;

    #if defined(_ALPHATEST_ON)
    float2 texcoord                 : TEXCOORD0;
    half4 color                     : COLOR;

    #if defined(_FLIPBOOKBLENDING_ON)
    float3 texcoord2AndBlend    : TEXCOORD5;
    #endif
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
