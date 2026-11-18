#ifndef HAIR_LIGHTING_INCLUDED
#define HAIR_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

real Pow2(real x)
{
    return (x * x);
}

real Pow5(real x)
{
    return (x * x * x * x * x);
}

//--------------------基本数据准备--------------------
struct HairTempData
{
    float Alpha[3];
    float B[3];
    float VdotL;
    float SinThetaL;
    float SinThetaV;
    float SinThetaL_pos;
    float SinThetaV_pos;
    float CosThetaD;
    float CosThetaD_pos;
    float CosPhi;
    float CosHalfPhi;
    float n_prime;
    float n_prime_pos;
};

float Hair_g(float B, float Theta)
{
    return exp(-0.5 * Pow2(Theta) / (B * B)) / (sqrt(2 * PI) * B);
}

float Hair_F(float CosTheta)
{
    const float n = 1.55;
    const float F0 = Pow2((1 - n) / (1 + n));
    return F0 + (1 - F0) * Pow5(1 - CosTheta);
}

void HairBaseDataCal(out HairTempData HairTemp, float3 B, float3 V, float3 L, float roughness)
{
    const float Shift = 0.035;
    HairTemp.Alpha[0] = -Shift * 2; //R_shift
    HairTemp.Alpha[1] = Shift; //TT_shift
    HairTemp.Alpha[2] = Shift * 4; //TRT_shift

    float ClampedRoughness = clamp(roughness, 1 / 255.0f, 1.0f);
    HairTemp.B[0] = Pow2(ClampedRoughness); //R_roughness
    HairTemp.B[1] = Pow2(ClampedRoughness) / 2; //TT_roughness
    HairTemp.B[2] = Pow2(ClampedRoughness) * 2; //TRT_roughness

    //N是指向发根的方向
    HairTemp.VdotL = saturate(dot(V, L));
    HairTemp.SinThetaL = clamp(dot(B, L), -1.0f, 1.0f);
    HairTemp.SinThetaV = clamp(dot(B, V), -1.0f, 1.0f);
    // 用于 TT 和 TRT：取绝对值或钳制到非负
    HairTemp.SinThetaL_pos = saturate(dot(B, L));
    HairTemp.SinThetaV_pos = saturate(dot(B, V));
    HairTemp.CosThetaD = (cos(0.5 * abs(FastASin(HairTemp.SinThetaV) - FastASin(HairTemp.SinThetaL))));
    HairTemp.CosThetaD_pos = (cos(0.5 * abs(FastASin(HairTemp.SinThetaV_pos) - FastASin(HairTemp.SinThetaL_pos))));
    const float3 Lp = L - HairTemp.SinThetaL * B;
    const float3 Vp = V - HairTemp.SinThetaV * B;
    HairTemp.CosPhi = dot(Lp, Vp) * rsqrt(dot(Lp, Lp) * dot(Vp, Vp) + 1e-4);
    HairTemp.CosHalfPhi = sqrt(saturate(0.5 + 0.5 * HairTemp.CosPhi));
    HairTemp.n_prime = 1.19 / HairTemp.CosThetaD + 0.36 * HairTemp.CosThetaD;
    HairTemp.n_prime_pos = 1.19 / HairTemp.CosThetaD_pos + 0.36 * HairTemp.CosThetaD_pos;
}

//--------------------漫反射项计算--------------------
half3 KajiyaKayDiffuseAttenuation(float3 BaseColor, half atten, float3 L, float3 V, half3 B)
{
    // Use soft Kajiya Kay diffuse attenuation
    float KajiyaDiffuse = 1 - abs(dot(B, L));

    float3 FakeNormal = normalize(V - B * dot(V, B));
    B = FakeNormal;

    // Hack approximation for multiple scattering.
    float Wrap = 1;
    float BdotL = saturate((dot(B, L) + Wrap) / Pow2(1 + Wrap));
    float DiffuseScatter = (1 / PI) * lerp(BdotL, KajiyaDiffuse, 0.33);
    float Luma = Luminance(BaseColor);
    float3 ScatterTint = pow(BaseColor / Luma, 1 - (atten + 1) * 0.5);
    return sqrt(BaseColor) * DiffuseScatter * ScatterTint;
}

half3 DiffuseCal(Light Light, half atten, float3 BaseColor, float3 L, float3 V, half3 B)
{
    half3 kkDiffuseAtten = max(KajiyaKayDiffuseAttenuation(BaseColor, atten, L, V, B), 0.0);
    half3 lightColor = Light.color * PI;
    return kkDiffuseAtten * lightColor;
}

half3 SphereDiffuseCal(Light Light, half atten,half aoFactor, float3 baseColor, float3 posWS, float2 uv,
                       float thicknessScale, float thicknessBias)
{
    float3 diffuseNormal = float3(
        sin(posWS.x + uv.x * 6.283) * thicknessScale,
        thicknessBias + 0.2, // 略微向上的分量，模拟头顶隆起
        cos(posWS.z + uv.y * 6.283) * thicknessScale
    );
    float NdotL = saturate(dot(diffuseNormal, Light.direction));
    float3 diffuseTotal = baseColor * NdotL * aoFactor;
    half3 lightColor = Light.color * PI;
    return diffuseTotal * lightColor * atten;
}

//--------------------高光项计算--------------------
half3 R_Function(HairTempData HairTemp, float Specular)
{
    const float sa = sin(HairTemp.Alpha[0]);
    const float ca = cos(HairTemp.Alpha[0]);
    float Shift = 2 * sa * (ca * HairTemp.CosHalfPhi * sqrt(1 - Pow2(HairTemp.SinThetaV)) + sa * HairTemp.SinThetaV);

    float Mp = Hair_g(HairTemp.B[0] * sqrt(2.0) * HairTemp.CosHalfPhi, HairTemp.SinThetaL + HairTemp.SinThetaV - Shift);
    float Np = 0.25 * HairTemp.CosHalfPhi;
    float Fp = Hair_F(sqrt(saturate(0.5 + 0.5 * HairTemp.VdotL)));
    return Mp * Np * Fp * Specular * 2;
}

half3 TT_Function(HairTempData HairTemp, float3 BaseColor)
{
    //float a = 1 / HairTemp.n_prime;
    float a = 1 / max(HairTemp.n_prime_pos, 1e-5); // 防止除以0
    float h = HairTemp.CosHalfPhi * (1 + a * (0.6 - 0.8 * HairTemp.CosPhi));
    float3 Tp = pow(BaseColor, 0.5 * sqrt(1 - Pow2(h * a)) / HairTemp.CosThetaD_pos);

    float f = Hair_F(HairTemp.CosThetaD_pos * sqrt(saturate(1 - h * h)));
    float Fp = Pow2(1 - f);

    //float Mp = Hair_g(HairTemp.B[1], (HairTemp.SinThetaL) + (HairTemp.SinThetaV) - HairTemp.Alpha[1]);
    float Mp = Hair_g(HairTemp.B[1],
                      HairTemp.SinThetaL_pos + HairTemp.SinThetaV_pos - HairTemp.Alpha[1]); // 非负
    float Np = exp(-3.65 * HairTemp.CosPhi - 3.98);
    return Mp * Np * Fp * Tp;
}

half3 TRT_Function(HairTempData HairTemp, float3 BaseColor)
{
    //float Mp = Hair_g(HairTemp.B[2], (HairTemp.SinThetaL) + (HairTemp.SinThetaV) - HairTemp.Alpha[2]);
    float Mp = Hair_g(HairTemp.B[2],
                      HairTemp.SinThetaL_pos + HairTemp.SinThetaV_pos - HairTemp.Alpha[2]); // 非负
    float f = Hair_F(HairTemp.CosThetaD_pos * 0.5);
    float Fp = Pow2(1 - f) * f;

    float3 Tp = pow(BaseColor, 0.8 / HairTemp.CosThetaD_pos);

    float Np = exp(17 * HairTemp.CosPhi - 16.78);
    return Mp * Np * Fp * Tp;
}

half3 SpecularCal(HairTempData HairTemp, Light Light, half3 BaseColor, float Specular)
{
    //乘3和0.5操作是我自己增加的，源码中并没有
    half3 R = R_Function(HairTemp, Specular) * 3;
    half3 TT = TT_Function(HairTemp, BaseColor) * 0.5;
    half3 TRT = TRT_Function(HairTemp, BaseColor) * 3;
    half3 result = (R + TT + TRT) * Light.color * PI;
    return result;
}

//--------------------间接光计算--------------------
half3 IndirDiff(HairTempData HairTemp, Light Light, half atten, half3 BaseColor, half Specular, float3 V, float3 N,
                float3 B)
{
    float3 L = normalize(V - B * dot(V, B));
    half3 diffCal = DiffuseCal(Light, atten, BaseColor, L, V, B);
    half3 specCal = SpecularCal(HairTemp, Light, BaseColor, Specular);

    half3 RadianceSH = SampleSH(N);
    half3 indirDiffuse = (diffCal + specCal) * RadianceSH;
    return indirDiffuse;
}

//--------------------头发透光（Transmission）参数结构体--------------------
struct HairTransmissionParams
{
    half3 color; // 透光颜色（HDR）
    half power; // 透光边缘范围 (0-1)
    half scale; // 头发厚度 (0-1)
};

//--------------------头发透光计算函数--------------------
half3 HairTransmission(float3 L, float3 V, float3 B, float3 N, float3 BaseColor,
                       HairTransmissionParams transmissionParams, float roughness)
{
    // 1. 计算头发法线方向（垂直于发根和视线方向）
    float3 hairNormal = normalize(cross(B, V));

    // 2. 计算光线与法线的夹角（用于背光检测）
    float NdotL = dot(N, L);
    float backLight = saturate(-NdotL); // 背面光照（0-1）

    // 3. 计算透光边缘因子
    // 基于视线与法线的角度来控制透光扩散范围
    float VdotN = dot(V, N);
    float rimFactor = pow(1.0 - abs(VdotN), transmissionParams.power * 4.0 + 1.0);
    // rim 越大，边缘透光范围越广

    // 4. 计算光线与发根方向的夹角
    float BdotL = abs(dot(B, L));
    float BdotV = abs(dot(B, V));

    // 5. 计算透光强度
    float transmissionIntensity = backLight * rimFactor;

    // 6. 加入散射扩散效果（基于粗糙度）
    float scatterWidth = max(roughness * 0.5, 0.01);
    float scatter = exp(-2.0 * (1.0 - BdotV) / scatterWidth);

    // 7. 头发厚度衰减
    float thicknessAtten = exp(-transmissionParams.scale * 4.0);

    // 8. 综合计算
    float finalTransmission = transmissionIntensity * scatter * thicknessAtten;

    // 9. 颜色计算
    // 透光颜色 = 基础色 × 透光颜色参数
    float3 transmissionColor = BaseColor * transmissionParams.color;
    // 加入暖色调（模拟次表面散射）
    transmissionColor = lerp(transmissionColor, transmissionParams.color, 0.5);

    return finalTransmission * transmissionColor;
}

half3 HairMarschnerTransmission(half3 V, half3 L, half3 B, HairTransmissionParams transmissionParams)
{
    half3 H = normalize(V + L);
    half dotTH = dot(B, H);
    half sinTH = sqrt(1.0 - dotTH * dotTH);
    half mars = pow(max(0.0, sinTH), transmissionParams.power) * transmissionParams.scale * 0.5;
    half3 marsTransmission = mars * transmissionParams.color;
    return marsTransmission;
}

half3 HairDirectLight(half3 L, half3 V, half3 B, half3 N, half3 BaseColor,
                      half roughness, half Specular, Light light, half atten,
                      HairTransmissionParams transmissionParams,half aoFactor, float3 posWS, float2 uv,
                      float thicknessBias, float thicknessScale)
{
    HairTempData hairTemp;
    HairBaseDataCal(hairTemp, B, V, L, roughness);

    //half3 transmission = HairTransmission(L, V, B, N, BaseColor, transmissionParams, roughness);
    half3 transmission = HairMarschnerTransmission(V, L, B, transmissionParams);
    transmission *= light.color * atten;

    // 4. 合并
    half3 specular = SpecularCal(hairTemp, light, BaseColor, Specular);

    // 2. 计算漫反射
    #if defined(_USE_SPHERE_NORMAL)
    half3 diffuse = SphereDiffuseCal(light, atten, aoFactor, BaseColor, posWS, uv, thicknessScale, thicknessBias);
    #else
    half3 diffuse = DiffuseCal(light, atten, BaseColor, L, V, B);
    #endif

    half3 RadianceSH = SampleSH(N);

    half3 DirectLight = diffuse + specular;
    // 4. 合并结果
    half3 mainLightColor = DirectLight * RadianceSH + transmission;

    return mainLightColor;
}

half3 HairLightingFinal(half3 L, half3 V, half3 B, half3 N, half3 BaseColor,
                        half roughness, half Specular, Light light, half atten,
                        HairTransmissionParams transmissionParams,half aoFactor, float3 posWS, float2 uv,
                        float thicknessBias, float thicknessScale)
{
    half3 mainLightColor = HairDirectLight(L, V, B, N, BaseColor, roughness, Specular, light, atten, transmissionParams,
                                           aoFactor, posWS, uv, thicknessBias, thicknessScale);
    
    uint meshRenderingLayers = GetMeshRenderingLayer();
    half3 additionalLightsColor = 0;
    
    #if defined(_ADDITIONAL_LIGHTS)
    
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, posWS);

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
    {
        additionalLightsColor += HairDirectLight(L, V, B, N, BaseColor, roughness, Specular, light, atten,
                                                 transmissionParams,
                                                 aoFactor, posWS, uv, thicknessBias, thicknessScale);
    }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, posWS);

    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
    {
        additionalLightsColor += HairDirectLight(L, V, B, N, BaseColor, roughness, Specular, light, atten,
                                                 transmissionParams,
                                                 aoFactor, posWS, uv, thicknessBias, thicknessScale);
    }
    LIGHT_LOOP_END
    #endif

    return mainLightColor + additionalLightsColor;
}

half3 HairLighting(float2 uv, float3 posWS, half3 baseColor,half3 lightColor,half aoFactor, half3 L, half3 V,
                   half3 B, half3 N,
                   float thicknessBias,
                   float thicknessScale,half3 transmissionColor, half marschnerPower, half marschnerScale,
                   half noise, half _PrimaryShift, half _SecondaryShift, half specularPower, half specularScale,
                   half atten)
{
    float3 diffuseNormal = float3(
        sin(posWS.x + uv.x * 6.283) * thicknessScale,
        thicknessBias + 0.2, // 略微向上的分量，模拟头顶隆起
        cos(posWS.z + uv.y * 6.283) * thicknessScale
    );
    half NdotL = saturate(dot(diffuseNormal, L));
    half3 diffuseTotal = baseColor * NdotL * aoFactor;

    half3 H = normalize(V + L);
    half dotTH = dot(B, H);
    half sinTH = sqrt(1.0 - dotTH * dotTH);
    half mars = pow(max(0.0, sinTH), marschnerPower * 0.8) * marschnerScale * 0.5;
    half3 marsTransmission = mars * transmissionColor;

    half shiftVal = noise - 0.5;
    half3 t1 = normalize(B + (shiftVal + _PrimaryShift) * N);
    half3 t2 = normalize(B + (shiftVal + _SecondaryShift) * N);

    half dotTH1 = dot(t1, H);
    half dotTH2 = dot(t2, H);
    half sinTH1 = sqrt(1.0 - dotTH1 * dotTH1);
    half sinTH2 = sqrt(1.0 - dotTH2 * dotTH2);
    half spec1 = pow(max(0.0, sinTH1), specularPower * 0.8) * specularScale * 0.5;
    half spec2 = pow(max(0.0, sinTH2), specularPower) * specularScale;

    half3 specTotal = (spec1 * half3(0.3, 0.3, 0.3)) + (spec2 * half3(0.3, 0.3, 0.3)) + marsTransmission;
    return marsTransmission * lightColor * atten;
}
#endif
