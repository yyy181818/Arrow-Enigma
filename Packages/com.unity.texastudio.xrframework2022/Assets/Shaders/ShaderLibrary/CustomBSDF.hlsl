#ifndef UNIVERSAL_BSDF_INCLUDED
#define UNIVERSAL_BSDF_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
float SchlickFresnel(float u)
{
    float m = clamp(1 - u, 0, 1);
    float m2 = m * m;
    return m2 * m2 * m; // pow(m,5)
}

float GTR1(float NdotH, float a)
{
    if (a >= 1)
        return 1 / PI;
    float a2 = a * a;
    float t = 1 + (a2 - 1) * NdotH * NdotH;
    return (a2 - 1) / (PI * log(a2) * t);
}

float GTR2_aniso(float NdotH, float HdotX, float HdotY, float ax, float ay)
{
    return 1 / (PI * ax * ay * Sq(Sq(HdotX / ax) + Sq(HdotY / ay) + NdotH * NdotH));
}

float smithG_GGX(float NdotV, float alphaG)
{
    float a = alphaG * alphaG;
    float b = NdotV * NdotV;
    return 1 / (NdotV + sqrt(a + b - a * b));
}

float smithG_GGX_aniso(float NdotV, float VdotX, float VdotY, float ax, float ay)
{
    return 1 / (NdotV + sqrt(Sq(VdotX * ax) + Sq(VdotY * ay) + Sq(NdotV)));
}

half3 SpecularBrdf(half3 baseColor,half subsurface,half3 specular,half3 specularTint,half3 sheenTint,half metallic,
                   half roughness,
                   half anisotropic,half sheen,half clearcoat,half clearCoatGloss,half3 L,half3 H,half NoL, half NoV,
                   half NoH,half LoH)
{
    half3 Cdlin = Gamma22ToLinear(baseColor);
    half Cdlum = Luminance(Cdlin);
    half3 Ctint = Cdlum > 0 ? Cdlin / Cdlum : half3(1.0, 1.0, 1.0);

    //half3 F0 = lerp(kDielectricSpec.rgb, Ctint, metallic);
    half3 Cspec0 = lerp(specular * 0.8 * lerp(half3(1.0, 1.0, 1.0), Ctint, specularTint), Cdlin, metallic);
    half3 Csheen = lerp(half3(1.0, 1.0, 1.0), Ctint, sheenTint);

    //Diffuse

    half FL = SchlickFresnel(NoL), FV = SchlickFresnel(NoV);
    half Fd90 = 0.5 + 2 * LoH * LoH * roughness;
    half Fd = lerp(1, Fd90, FL) * lerp(1, Fd90, FV);


    half Fss90 = LoH * LoH * roughness;
    half Fss = lerp(1, Fss90, FL) * lerp(1, Fss90, FL);
    half ss = 1.25 * (Fss * (1 / (NoL + NoV) - 0.5) + 0.5);

    //specular
    half X, Y;
    ConvertAnisotropyToRoughness(roughness, anisotropic, X, Y);
    half XoH = saturate(dot(X, H));
    half YoH = saturate(dot(Y, H));
    half LoX = saturate(dot(L, X));
    half LoY = saturate(dot(L, Y));
    half Ds = GTR2_aniso(NoH, XoH, YoH, X, Y);
    half FH = SchlickFresnel(LoH);

    half3 Fs = lerp(Cspec0, half3(1.0, 1.0, 1.0), FH);
    half Gs = smithG_GGX_aniso(NoL, LoX, LoY, X, Y) * smithG_GGX_aniso(NoV, LoX, LoY, X, Y);

    //sheen
    half3 Fsheen = FH * sheen * Csheen;

    //Clear coat (ior = 1.5 -> F0 = 0.04)
    half Dr = GTR1(NoH, lerp(.1, .001, clearCoatGloss));
    half Fr = lerp(.04, 1, FH);
    half Gr = smithG_GGX(NoL, .25) * smithG_GGX(NoV, .25);

    return (1 / PI * lerp(Fd, ss, subsurface) * Cdlin + Fsheen)
        * (1 - metallic)
        + Gs * Fs * Ds + .25 * clearcoat * Gr * Fr * Dr;
}

float3 Unity_SpecularAnisoBRDF(half3 baseColor,half3 f90,half NoL, half NoV,half NoH,half ToL, half ToV, half ToH, half BoL,
                              half BoV,half BoH,half LoH,half metallic,half roughness, half anisotropic)
{
    half roughnessT,roughnessB;
    ConvertAnisotropyToRoughness(roughness, anisotropic, roughnessT, roughnessB);
    float DV = DV_SmithJointGGXAniso(ToH,BoH,NoH,ToV,BoV,NoV,ToL,BoL,NoL,roughnessT,roughnessB);
    float3 F0 = ComputeFresnel0(kDielectricSpec.rgb,baseColor,metallic);
    float3 F = F_Transm_Schlick(F0,f90,NoV);
    // BRDFspec = (D * V * F) / 4.0
    float3 brdf = DV * F / 4;
    return brdf;
}
#endif
