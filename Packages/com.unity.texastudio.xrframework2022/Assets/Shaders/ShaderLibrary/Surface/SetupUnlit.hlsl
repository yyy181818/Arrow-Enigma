// Surface/SetupUnlit.hlsl — Unlit 版 SurfaceData 初始化
//   与 Setup.hlsl 里的 InitLitSurfaceData 解耦：本文件不引用任何 lit uniform
//   （_Metallic / _Roughness / _CombineMap / _BumpMap / _OcclusionStrength 等），
//   因此 Unlit shader 的 CBUFFER 无需声明这些字段即可编译。
//   调用方需已定义 SurfaceData 结构体（include Lighting.hlsl / MetaInput.hlsl 之后），
//   并已声明 _TillingOffset / _BaseMap / _BaseColor / _AlphaChannel / _Cutoff /
//   _Alpha / _BaseColorStrength / _EmissionColor / _EmissionMap。

#ifndef TA_SHADERLIB_SURFACE_SETUP_UNLIT
#define TA_SHADERLIB_SURFACE_SETUP_UNLIT

void InitUnlitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    uv = uv * _TillingOffset.xy + _TillingOffset.zw;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half a = _AlphaChannel == 0 ? albedoAlpha.a : albedoAlpha.r;
    outSurfaceData.alpha = Alpha(a, _BaseColor, _Cutoff) * _Alpha;
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb * _BaseColorStrength;

    // Unlit：normal / metallic / specular / smoothness / occlusion 均为中性占位
    outSurfaceData.normalTS    = half3(0.0h, 0.0h, 1.0h);
    outSurfaceData.metallic    = half(0.0);
    outSurfaceData.specular    = half3(0.0, 0.0, 0.0);
    outSurfaceData.smoothness  = half(1.0);
    outSurfaceData.occlusion   = half(1.0);

#if defined(_EMISSION)
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb,
                                TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
#else
    outSurfaceData.emission = half3(0.0h, 0.0h, 0.0h);
#endif

    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
}

#endif
