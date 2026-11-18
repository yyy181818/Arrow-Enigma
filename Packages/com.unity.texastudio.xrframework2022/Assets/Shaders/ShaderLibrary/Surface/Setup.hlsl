// Surface/Setup.hlsl — SurfaceData init, supports _TEXTUREMODE_NMR/MRO/NONE

#ifndef TA_SHADERLIB_SURFACE_SETUP
#define TA_SHADERLIB_SURFACE_SETUP

void InitLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    uv = uv * _TillingOffset.xy + _TillingOffset.zw;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half alpha = _AlphaChannel == 0 ? albedoAlpha.a : albedoAlpha.r;
    outSurfaceData.alpha = Alpha(alpha, _BaseColor, _Cutoff) * _Alpha;
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb * _BaseColorStrength;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

    half3 normalTS;
    half  metallic;
    half  roughness;
    half  occlusion;

#if defined(_TEXTUREMODE_MRO)
    half4 combine = SAMPLE_TEXTURE2D_X(_CombineMap, sampler_CombineMap, uv);
    normalTS  = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    metallic  = combine.r * _Metallic;
    roughness = combine.g * _Roughness;
    #if defined(_OCCLUSIONMAP)
    occlusion = LerpWhiteTo(combine.b, _OcclusionStrength);
    #else
    occlusion = half(1.0);
    #endif
#elif defined(_TEXTUREMODE_NMR)
    half4 combine = SAMPLE_TEXTURE2D_X(_CombineMap, sampler_CombineMap, uv);
    normalTS  = DecodeNormalFormRG(combine.xy, _BumpScale);
    metallic  = combine.z * _Metallic;
    roughness = combine.w * _Roughness;
    #if defined(_OCCLUSIONMAP)
    half occ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a;
    occlusion = LerpWhiteTo(occ, _OcclusionStrength);
    #else
    occlusion = half(1.0);
    #endif
#else
    normalTS  = half3(0.0h, 0.0h, 1.0h);
    metallic  = _Metallic;
    roughness = _Roughness;
    occlusion = half(1.0);
#endif

    outSurfaceData.metallic  = metallic;
    outSurfaceData.specular  = half3(0.0, 0.0, 0.0);
    outSurfaceData.smoothness = max(0.02f, PerceptualRoughnessToPerceptualSmoothness(roughness));
    outSurfaceData.normalTS  = normalTS.xyz;
    outSurfaceData.occlusion = occlusion;

#if defined(_EMISSION)
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb,
                                TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
#else
    outSurfaceData.emission = 0;
#endif

    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
}

#endif
