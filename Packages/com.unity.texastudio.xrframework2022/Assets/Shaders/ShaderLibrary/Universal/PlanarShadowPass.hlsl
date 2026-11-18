#ifndef UNIVERSAL_PLANAR_SHADOW_PASS_INCLUDED
#define UNIVERSAL_PLANAR_SHADOW_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes_PlanarShadow
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
};

struct Varings_PlanarShadow
{
    float4 vertex : SV_POSITION;
    float4 color : TEXCOORD1;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
};

float3 ShadowProjectPos(float4 vertPos)
{
    float3 shadowPos = 0;
    //得到顶点的世界空间坐标
    float3 worldPos = TransformObjectToWorld(vertPos.xyz);
    float3 lightDir = normalize(_MainLightPosition.xyz);
    #if defined(_PLANARSHADOWMODE_GLOBAL)
    //阴影的世界空间坐标（低于地面的部分不做改变）
    shadowPos.y = min(worldPos.y, _GlobalHeight);
    shadowPos.xz = worldPos.xz - lightDir.xz * max(0, worldPos.y - _GlobalHeight) * _ShadowOffset / lightDir.y;
    #endif
    #if defined(_PLANARSHADOWMODE_LOCAL)
    shadowPos.y += unity_ObjectToWorld[1].w - _LocalHeight;
    shadowPos.xz = worldPos.xz - lightDir.xz * max(0,worldPos.y - _GlobalHeight) * _ShadowOffset / lightDir.y;
    #endif
    shadowPos = _EnableClipShadow ? _WorldSpaceCameraPos.y - shadowPos.y > 0 ? shadowPos : 0 : shadowPos;
    return shadowPos;
}

float3 ShadowGround(float4 vertex)
{
    float3 worldPos = TransformObjectToWorld(vertex.xyz);
    Light mainLight = GetMainLight();
    float3 lightDir = normalize(mainLight.direction);
    lightDir = mul(_World2Ground, float4(lightDir, 0)).xyz;
    float4 groundPos = mul(_World2Ground, float4(worldPos, 1));
    groundPos.xz = groundPos.xz - groundPos.y / lightDir.y * _ShadowOffset * lightDir.xz;
    //groundPos.xz = groundPos.xz - lightDir.xz * _ShadowOffset / lightDir.y;
    groundPos.y = 0;
    groundPos = mul(_Ground2World, groundPos);
    groundPos = _EnableClipShadow ? _WorldSpaceCameraPos.y - groundPos.y > 0 ? groundPos : 0 : groundPos;
    groundPos = mul(unity_WorldToObject, groundPos);
    return groundPos.xyz;
}

float GetAlpha(Varings_PlanarShadow i)
{
    half albedoAlpha = SampleAlbedoAlpha(i.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a;
    albedoAlpha = Alpha(albedoAlpha, _BaseColor, _Cutoff);
    return albedoAlpha;
}

Varings_PlanarShadow vert_shadow(Attributes_PlanarShadow v)
{
    Varings_PlanarShadow o;
    ZERO_INITIALIZE(Varings_PlanarShadow, o);
    float3 shadowPos = 0;
    float falloff = 0;
    o.color = 1;
    #if defined(_PLANARSHADOW_ENABLE)
        #if defined(_PLANARSHADOWMODE_GLOBAL)
                //得到阴影的世界空间坐标
                shadowPos = ShadowProjectPos(v.vertex);
                //转换到裁切空间
                o.vertex = TransformWorldToHClip(shadowPos);
                o.positionWS = shadowPos;
                //得到中心点世界坐标
                float3 center = float3(unity_ObjectToWorld[0].w, _GlobalHeight, unity_ObjectToWorld[2].w);
                //计算阴影衰减
                falloff = 1 - saturate(distance(shadowPos, center) * _ShadowFalloff);
                //阴影颜色
                o.color *= _PlanarShadowColor;
                o.color.a *= falloff;
        #elif defined(_PLANARSHADOWMODE_LOCAL)
                    //得到阴影的世界空间坐标
                    shadowPos = ShadowProjectPos(v.vertex);
                     //转换到裁切空间
                    o.vertex = TransformWorldToHClip(shadowPos);
                    o.positionWS = shadowPos;
                    //得到中心点世界坐标
                    float3 center = float3(unity_ObjectToWorld[0].w, _LocalHeight, unity_ObjectToWorld[2].w);
                    //计算阴影衰减
                    falloff = 1 - saturate(distance(shadowPos, center) * _ShadowFalloff * 0.5 + 0.5);
                    //阴影颜色
                    o.color *= _PlanarShadowColor * falloff;
        #else
                    shadowPos = ShadowGround(v.vertex);
                    o.vertex = TransformObjectToHClip(shadowPos);
                    falloff = 1 - saturate(distance(shadowPos, v.vertex) / _ShadowIntensity);
                    o.positionWS = shadowPos;
                    o.color *= _PlanarShadowColor * falloff;
        #endif
    #endif

    o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

    return o;
}

half4 frag_shadow(Varings_PlanarShadow i) : SV_Target
{
    #if defined(_ALPHATEST_ON)
                float alpha = GetAlpha(i) * i.color.a;
                i.color.a *= step(_Cutoff, alpha);
                //clip(alpha - _Cutoff);
    #endif

    return i.color;
}
#endif
//
