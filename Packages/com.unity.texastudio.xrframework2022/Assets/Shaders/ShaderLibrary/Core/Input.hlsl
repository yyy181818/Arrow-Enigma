// Core/Input.hlsl — shared vertex attributes and varying structures

#ifndef TA_SHADERLIB_CORE_INPUT
#define TA_SHADERLIB_CORE_INPUT

#ifndef REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#if defined(_NORMALMAP) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif
#endif

#if !defined(_RECEIVE_SHADOWS_OFF)
#define _SHADOWS_SOFT 1
#endif

struct Attributes_forward_base
{
    float4 vertex     : POSITION;
    float2 uv         : TEXCOORD0;
    float3 normalOS   : NORMAL;
    float4 tangentOS  : TANGENT;
    float2 staticLightmapUV : TEXCOORD1;
    //float2 uv2          : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings_forward_base
{
    float2 uv                       : TEXCOORD0;
    float3 positionWS               : TEXCOORD1;
    float3 normalWS                 : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4   tangentWS               : TEXCOORD3;
#endif
    //float2 uv2                     : TEXCOORD4;
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4   fogFactorAndVertexLight : TEXCOORD5;
#else
    half    fogFactor               : TEXCOORD5;
#endif
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4  shadowCoord             : TEXCOORD6;
#endif
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
    float4  positionCS              : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#endif
