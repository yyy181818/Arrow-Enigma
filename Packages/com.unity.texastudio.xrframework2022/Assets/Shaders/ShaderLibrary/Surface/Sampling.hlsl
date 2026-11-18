// Surface/Sampling.hlsl — texture sampling utilities

#ifndef TA_SHADERLIB_SURFACE_SAMPLING
#define TA_SHADERLIB_SURFACE_SAMPLING

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
}

half Alpha(half albedoAlpha, half4 color, half cutoff)
{
    half alpha = albedoAlpha * color.a;
    alpha = AlphaDiscard(alpha, cutoff);
    return alpha;
}

half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale)
{
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return (half3)UnpackNormalScale(n, scale);
}

half3 DecodeNormalFormRG(half2 rg, half scale)
{
    half4 packed = half4(rg, 0.0h, 0.0h);
    half3 normalTS = (half3)UnpackNormal(packed);
    normalTS.xy *= scale;
    normalTS.z = sqrt(1.0h - saturate(dot(normalTS.xy, normalTS.xy)));
    return normalTS;
}

half3 SampleEmission(float2 uv, half3 emissionColor,
                     TEXTURE2D_PARAM(emissionMap, sampler_emissionMap))
{
#if defined(_EMISSION)
    half3 sampled = SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb;
    return _EnableSampleRGB
        ? sampled * emissionColor.rgb
        : dot(_EmissionChannel.rgb, sampled) * emissionColor.rgb;
#else
    return half3(0.0h, 0.0h, 0.0h);
#endif
}

#endif
