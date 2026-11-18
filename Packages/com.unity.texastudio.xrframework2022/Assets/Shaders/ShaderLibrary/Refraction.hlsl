#ifndef UNIVERSAL_REFRACTION_INCLUDED
#define UNIVERSAL_REFRACTION_INCLUDED

#ifndef UNITY_SPECCUBE_LOD_STEPS
    // This is actuall the last mip index, we generate 7 mips of convolution
    #define UNITY_SPECCUBE_LOD_STEPS 6
#endif

half _SSRefractionInvScreenWeightDistance;

#ifdef _REFRACTION
TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);

half4 CalculateRefractionFinalColor(LightingData lightingData, half alpha)
{
    half3 finalColor = CalculateLightingColor(lightingData, 1);

    return half4(finalColor, 1.0);
}

half EdgeOfScreenFade(half2 coordNDC, half fadeRcpLength)
{
    half2 coordCS = coordNDC * 2 - 1;
    half2 t = Remap10(abs(coordCS), fadeRcpLength, fadeRcpLength);
    return Smoothstep01(t.x) * Smoothstep01(t.y);
}

// Big thanks for reference for AE Tuts
// Ref: https://www.youtube.com/watch?v=VMsOPUUj0JA&t=106s
half3 SimpleRefraction(InputData inputData, SurfaceData surfaceData, half perceptualRoughness)
{
    half3 R = normalize(refract(inputData.viewDirectionWS, inputData.normalWS, surfaceData.ior));
    half4 RVP = ComputeScreenPos(mul(UNITY_MATRIX_VP, half4(R + inputData.positionWS, 1.0)));
    half2 screenUV = RVP.xy / RVP.w;
    half3 rgb = half3(0.5, 0.5, 0.5);
    //half mip = PositivePow(perceptualRoughness, 1.3) * uint(max(UNITY_SPECCUBE_LOD_STEPS - 1, 0));
#ifdef _CHROMATIC_ABERRATION
    half r = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV).r;
	half g = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, (screenUV - (0.025 * surfaceData.chromaticAberration))).g;
	half b = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, (screenUV + (0.025 * surfaceData.chromaticAberration))).b;
	rgb = half3(r, g, b);
#else
    rgb = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV).rgb;
#endif
    half weight = EdgeOfScreenFade(screenUV, _SSRefractionInvScreenWeightDistance);
    half3 indirectSpecular = GlossyEnvironmentReflection(R, inputData.positionWS, perceptualRoughness, 1.0h);

    return lerp(indirectSpecular, rgb, weight) * surfaceData.transmittanceColor;
}
#endif
#endif