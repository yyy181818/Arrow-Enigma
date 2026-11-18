#ifndef UNIVERSAL_BSDF_INCLUDED
#define UNIVERSAL_BSDF_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "./Common.hlsl"
half _MicroShadowOpacity;

struct BSDFData
{
    half thickness;
    half curvature;

    half3 scatteringColor;
    half3 scatteringShadowsColor;
    half transmissionScale;

    half translucencyPower;
    half translucencyScale;
    half translucencyAmbient;
    half translucencyDistortion;
    half translucencyShadows;

    half anisotropy;
    half3 iridescence;
    half clearCoatMask;
};

inline void InitializeBSDFData(inout BSDFData bsdfData, SurfaceData surfaceData, half3 iridescence)
{
    bsdfData.thickness = 1.0h;
    bsdfData.scatteringColor = 0.0h;
    bsdfData.scatteringShadowsColor = 0.0h;
    bsdfData.transmissionScale = 0.0h;
    bsdfData.curvature = 0.0h;
    bsdfData.translucencyPower = 0.0h;
    bsdfData.translucencyScale = 0.0h;
    bsdfData.translucencyAmbient = 0.0h;
    bsdfData.translucencyDistortion = 0.0h;
    bsdfData.translucencyShadows = 0.0h;
    bsdfData.anisotropy = 0.0h;
    bsdfData.iridescence = 0.0h;
    bsdfData.clearCoatMask = 0.0h;

    #if (defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && defined(_MATERIAL_FEATURE_TRANSMISSION)) || defined(_MATERIAL_FEATURE_TRANSLUCENCY)
    bsdfData.thickness = surfaceData.thickness;
    #endif

    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING)
    bsdfData.scatteringColor        = surfaceData.scatteringColor;
    bsdfData.scatteringShadowsColor = surfaceData.scatteringShadowsColor;
    bsdfData.transmissionScale      = surfaceData.transmissionScale;
    bsdfData.curvature              = surfaceData.curvature;
    #endif

    #if defined(_MATERIAL_FEATURE_TRANSLUCENCY)
    bsdfData.scatteringColor        = surfaceData.scatteringColor;
    bsdfData.translucencyPower      = surfaceData.translucencyPower;
    bsdfData.translucencyScale      = surfaceData.translucencyScale;
    bsdfData.translucencyAmbient    = surfaceData.translucencyAmbient;
    bsdfData.translucencyDistortion = surfaceData.translucencyDistortion;
    bsdfData.translucencyShadows    = surfaceData.translucencyShadows;
    #endif

    #if defined(_MATERIAL_FEATURE_ANISOTROPY)
    bsdfData.anisotropy = surfaceData.anisotropy;
    #endif

    #if defined(_MATERIAL_FEATURE_IRIDESCENCE)
    bsdfData.iridescence = iridescence;
    #endif

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    bsdfData.clearCoatMask = surfaceData.clearCoatMask;
    #endif
}

TEXTURE2D(_IridescenceLUT);
SAMPLER(sampler_IridescenceLUT);
half3 CalculateIridescence(half eta_1, half cosTheta1, half2 iridescenceTS, half3 specular)
{
    half3 I = half3(1.0, 1.0, 1.0);
    #if defined(_SHADER_QUALITY_IRIDESCENCE_APPROXIMATION)
    half eta_2 = 2.0 - iridescenceTS.x;
    half sinTheta2Sq = Sq(eta_1 / eta_2) * (1.0 - Sq(cosTheta1));
    half cosTheta2Sq = 1.0 - sinTheta2Sq;
    half cosTheta2 = sqrt(cosTheta2Sq);
    half k = iridescenceTS.y + (cosTheta2 * (PI * iridescenceTS.x));
    half R0 = IorToFresnel0(eta_2, eta_1);
    half R12 = saturate(sqrt(F_Schlick(R0, cosTheta1)));

    half3 iridescenceColor = lerp(2.0 * SAMPLE_TEXTURE2D(_IridescenceLUT, sampler_IridescenceLUT, float2(k * 1.5, 1.0)).rgb - 0.2, R12, R12); //just because :)

    I = lerp(specular, iridescenceColor, R12);
    #else
    I = EvalIridescence(eta_1, cosTheta1, iridescenceTS.x, specular);
    #endif

    return I;
}

#define CLEAR_COAT_IOR 1.5
half3 IridescenceSpecular(half3 normalWS, half3 viewDirectionWS, half3 specular, half3 iridescenceTMS,
                          half clearCoatMask)
{
    half NdotV = dot(normalWS, viewDirectionWS);
    half clampedNdotV = ClampNdotV(NdotV);
    half viewAngle = clampedNdotV;
    half topIor = 1.0;
    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    topIor = lerp(1.0, CLEAR_COAT_IOR, clearCoatMask);
    viewAngle = sqrt(1.0 + Sq(1.0 / topIor) * (Sq(dot(normalWS, viewDirectionWS)) - 1.0));
    #endif

    if (iridescenceTMS.y > 0.0)
    {
        specular = lerp(specular, CalculateIridescence(topIor, viewAngle, iridescenceTMS.xz, specular),
                        iridescenceTMS.y);
    }

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    specular = lerp(specular, ConvertF0ForClearCoat15(specular), clearCoatMask);
    #endif

    return specular;
}
half DV_Anisotropy(half perceptualRoughness, half anisotropy, half3 lightDirWS, half3 viewDirWS, half3 H,
                   half4 tangentWS, half3 bitangentWS, half3 normalWS)
{
    half NdotL = dot(normalWS, lightDirWS);
    half clampedNdotV = ClampNdotV(dot(normalWS, viewDirWS));
    half NdotH = saturate(dot(normalWS, H));
    //Anisotropy to Roughness
    half roughnessT;
    half roughnessB;
    ConvertAnisotropyToRoughness(perceptualRoughness, anisotropy, roughnessT, roughnessB);
    roughnessT = max(0.01, roughnessT);
    roughnessB = max(0.01, roughnessB);
    half TdotH = dot(tangentWS.xyz, H);
    half TdotL = dot(tangentWS.xyz, lightDirWS);
    half TdotV = dot(tangentWS.xyz, viewDirWS);
    half BdotH = dot(bitangentWS, H);
    half BdotL = dot(bitangentWS, lightDirWS);
    half BdotV = dot(bitangentWS, viewDirWS);

    half partLambdaV = GetSmithJointGGXAnisoPartLambdaV(TdotV, BdotV, clampedNdotV, roughnessT, roughnessB);
    half DV = DV_SmithJointGGXAniso(TdotH, BdotH, NdotH, clampedNdotV, TdotL, BdotL, abs(NdotL), roughnessT, roughnessB,
                                    partLambdaV);

    return max(0, DV * PI * saturate(NdotL));
}

void GeometricAAFiltering(inout float perceptualSmoothness, inout float coatPerceptualSmoothness,
                          float3 geometricNormalWS, float screenSpaceVariance, float threshold)
{
    float variance = GeometricNormalVariance(geometricNormalWS, screenSpaceVariance);
    perceptualSmoothness = NormalFiltering(perceptualSmoothness, variance, threshold);
    #ifdef _SHADER_QUALITY_COAT_GEOMETRIC_AA
    coatPerceptualSmoothness = NormalFiltering(coatPerceptualSmoothness, variance, threshold);
    #endif
}

// void ApplyDoubleSidedFlipOrMirror(half faceSign, inout half3 normalTS)
// {
//     normalTS = faceSign > 0 ? normalTS : normalTS * _DoubleSidedConstants.xyz;
// }

half3 SpecularBTDF(half D,half G, half3 F, half NdotL, half NdotV, half VdotH, half LdotH, half etaIn, half etaOut)
{
    half term1 = VdotH * LdotH / (NdotV * NdotL);
    half3 term2 = etaOut * etaOut * (1 - F) * G * D;
    half term3 = (etaIn * VdotH + etaOut * LdotH) * (etaIn * VdotH + etaOut * LdotH) + 0.001f;

    half3 refractionBrdf = term1 * term2 / term3;

    return refractionBrdf;
}


half3 SpecularBSDF(half3 baseColor,half3 N, half3 L, half3 V, half3 H,half metallic,half roughness,half ior)
{
    bool formOutside = dot(V, N) < 0;
    N = formOutside ? N : -N;
    half etai = 1;
    half etat = ior;
    half3 F0 = lerp(kDielectricSpec.rgb, baseColor, metallic);

    //ior = 1.55
    //specularRatio
    half kr = Calculatefresnel(L, N, ior);
    //refractionRatio
    half kt = 1 - kr;
    half VoH = saturate(dot(V, H));
    half3 F = FresnelSchlick(VoH, F0);
    half eta = formOutside ? etai / etat : etat / etai;
    L = normalize(refract(V, N, eta));

    [branch]
    if (!formOutside)
    {
        //since the BTDF is not reciprocal, we need to invert the direction of our vectors.
        const half3 temp = L;
        L = V;
        V = temp;

        N = -N;
        H = -H;
    }

    half NdotL = saturate(dot(N, L));
    half NdotV = saturate(dot(N, V));

    half NdotH = saturate(dot(N, H));
    half VdotH = saturate(dot(V, H));
    half LdotH = saturate(dot(L, H));

    //refraction
    half D = DistributionGGX(NdotH, roughness);
    half G = GeometrySmith(NdotV, NdotL, roughness);
    half3 refractionBrdf = SpecularBTDF(D, G, F, NdotL, NdotV, VdotH, LdotH, etai, etat);
    half refractionPdf = ImportanceSampleGGX_PDF(D, NdotH, VdotH);

    //BTDF
    half3 totalBrdf =refractionBrdf;
    half totalPDF = refractionPdf * kt;
    
    return totalPDF > 0 ? totalBrdf / totalPDF : 0;
}
half Pow5(half x)
{
    return x * x * x * x * x;
}
half3 DiffuseBSDF(half NdotL,half NdotV,half LdotH,half perceptualRoughness,half r,half scatterDist)
{
    half lightScatter = Pow5(1.0 - NdotL);
    half viewScatter = Pow5(1.0 - NdotV);

    half Rr = 2 * LdotH * LdotH * perceptualRoughness;

    //diffusion model 散射模型
    // r 为 随机扩散剖面
    //scatterDist 为 散射距离;
    //将散射模型替换原本的漫反射lambert项
    half d = scatterDist;
    half Rd = exp(-r / d) + exp(-r / 3 * d) / 8 * PI * r * d;

    //lambert
    //half kd = baseColor / PI;

    //retro-reflection 逆向反射
    half retro = Rd * Rr * (lightScatter + viewScatter + lightScatter * lightScatter * (Rr - 1.0));
    half Lambertian = Rd * (1.0 - 0.5 * lightScatter) * (1.0 - 0.5 * viewScatter);

    return Lambertian + retro;
}
#endif
