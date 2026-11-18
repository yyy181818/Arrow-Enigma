//Created by Wans Wu
#ifndef STANDARD_LIGHTING_INCLUDE
#define STANDARD_LIGHTING_INCLUDE
#include "LightingCommon.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


//BRDF
float3 StandardBRDF(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 L, float3 V, float3 N,
                    float3 LightColor, float Shadow)
{
    float a2 = Pow4(Roughness);
    float3 H = normalize(V + L);
    float NoH = saturate(dot(N, H));
    float NoV = saturate(abs(dot(N, V)) + 1e-5); // Both side 
    float NoL = saturate(dot(N, L));
    float VoH = saturate(dot(V, H));
    float3 Radiance = NoL * LightColor * Shadow * PI;

    float3 DiffuseTerm = Diffuse_Lambert(DiffuseColor) * Radiance;
    // Generalized microfacet specular
    float D = D_GGX_UE4(a2, NoH);
    float Vis = Vis_SmithJointApprox(a2, NoV, NoL);
    float3 F = F_Schlick_UE4(SpecularColor, VoH);
    half3 SpecularTerm = ((D * Vis) * F) * Radiance;
    float3 DirectLighting = (DiffuseTerm + SpecularTerm);

    return DirectLighting;
}
void IndirectLighting_float_Opti(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 N,
                            float3 V,
                            float Occlusion, half HeightLight, float2 lightUV, float3 lDir, half _LightIntensitiy,
                            out float3 IndirectLighting)
{
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 Irradiance = float3(0, 0, 0);

    #if defined(LIGHTMAP_ON)
        // 1. 采样并解码光照贴图
        float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
        Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));

        #if defined(DIRLIGHTMAP_COMBINED)
            // 2. 处理方向性
            float4 direction = SAMPLE_TEXTURE2D(unity_LightmapInd, samplerunity_Lightmap, lightUV);
            
            // 将方向从 [0,1] 映射到 [-1,1]
            // 注意：Unity 有时只在 xy 存储方向，z 可能是其他数据，但在 standard decode 中通常如下：
            half3 LightDir = direction.xyz * 2.0f - 1.0f;
            
            // 计算简单的半兰伯特漫反射项，用于模拟烘焙出的方向感
            half halfLambert = saturate(dot(N, LightDir)) * 0.5 + 0.5;
            
            // --- 修复点 1：移除除以 direction.w 的操作 ---
            // 直接应用方向性衰减即可
            Irradiance *= halfLambert;
            
            // --- 修复点 2：移除 BlinnPhong 叠加 ---
            // 烘焙光照不应该产生视线相关的高光，那是反射探针的工作。
            // 仅仅应用强度控制
            Irradiance *= _LightIntensitiy;
        #else
            // 如果不是方向性光照贴图，直接应用强度
            Irradiance *= _LightIntensitiy;
        #endif

    #else
        // 如果没有光照贴图，采样球谐光照 (Light Probe)
        Irradiance = SampleSH(N) * _LightIntensitiy;
    #endif

    // 计算漫反射部分
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;

    // 计算镜面反射部分 (反射探针)
    half3 R = reflect(-V, N);
    half3 SpeucularLD = GlossyEnvironmentReflection_revert(R, Roughness, DiffuseAO);
    half3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    half SpecularOcclusion = GetSpecularOcclusion(NoV, Roughness * Roughness, Occlusion);
    half3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    
    half3 IndirectSpecular = SpeucularLD * SpecularDFG * SpecularAO;

    // 最终合成
    IndirectLighting = IndirectDiffuse + IndirectSpecular;
}
//IndirectLighting

void IndirectLighting_float(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 N,
                            float3 V,
                            float Occlusion,half HeightLight, float2 lightUV, float3 lDir,half _LightIntensitiy,
                            out float3 IndirectLighting)
{
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 Irradiance = float3(0, 0, 0);
    #if defined(LIGHTMAP_ON)
    float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
    Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));
    #if defined(DIRLIGHTMAP_COMBINED)
    float4 direction = SAMPLE_TEXTURE2D(unity_LightmapInd, samplerunity_Lightmap, lightUV);
    half3 LightDir = direction * 2.0f - 1.0f;
    half halfLambert = saturate(dot(N, LightDir)) * 0.5 + 0.5;
    Irradiance = Irradiance * halfLambert / max(1e-4, direction.w);
    //Irradiance = smoothstep(0,_dayNight,Irradiance);
    half BlinnPhong = pow(max(0, saturate(dot(N, normalize(lDir + V)))), 20);
    Irradiance = Irradiance * _LightIntensitiy + Irradiance * BlinnPhong * HeightLight;
    #endif
    #else
    Irradiance = SampleSH(N) * _LightIntensitiy;
    #endif
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;
    half3 R = reflect(-V, N);
    half3 SpeucularLD = GlossyEnvironmentReflection_revert(R, Roughness, DiffuseAO);
    half3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    half SpecularOcclusion = GetSpecularOcclusion(NoV, Roughness * Roughness, Occlusion);
    half3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    half3 IndirectSpecular = SpeucularLD * SpecularDFG * SpecularAO;
    IndirectLighting = IndirectDiffuse + IndirectSpecular;
}

void IndirectLighting_DayandNight(float3 DiffuseColor, float3 SpecularColor, float4 _LightVector, float Roughness,
                                  float3 WorldPos, float3 N, float3 V,
                                  float Occlusion,half HeightLight, float2 lightUV, float3 lDir,half _LightIntensitiy,
                                  out float3 IndirectLighting)
{
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 Irradiance = float3(0, 0, 0);
    #if defined(LIGHTMAP_ON)
    float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
    Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));
    #if defined(DIRLIGHTMAP_COMBINED)
    float4 direction = SAMPLE_TEXTURE2D(unity_LightmapInd, samplerunity_Lightmap, lightUV);
    half3 LightDir = direction * 2.0f - 1.0f;
    half halfLambert = saturate(dot(N, LightDir)) * 0.5 + 0.5;
    Irradiance = lerp(RangeRemap(0, _LightVector.z, Irradiance), Irradiance, _LightVector.w);
    Irradiance = clamp(Irradiance, 0, _LightVector.y);
    Irradiance = Irradiance * halfLambert / max(1e-4, direction.w);
    Irradiance = pow(Irradiance, _LightVector.x);
    half BlinnPhong = pow(max(0, saturate(dot(N, normalize(lDir + V)))), 20);
    Irradiance = Irradiance * _LightIntensitiy + Irradiance * BlinnPhong * HeightLight * _LightIntensitiy;
    #endif
    #else
    Irradiance = SampleSH(N) * _LightIntensitiy;
    #endif
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;
    half3 R = reflect(-V, N);
    half3 SpeucularLD = GlossyEnvironmentReflection_revert(R, Roughness, DiffuseAO);
    half3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    half SpecularOcclusion = GetSpecularOcclusion(NoV, Roughness * Roughness, Occlusion);
    half3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    half3 IndirectSpecular = SpeucularLD * SpecularDFG * SpecularAO;
    IndirectLighting = IndirectDiffuse + IndirectSpecular;
    //IndirectLighting = Irradiance1;
}

void IndirectLighting_DayandNight_Opti(float3 DiffuseColor, float3 SpecularColor, float4 _LightVector, float Roughness,
                                       float3 WorldPos, float3 N, float3 V,
                                       float Occlusion, half HeightLight, float2 lightUV, float3 lDir,
                                       half _LightIntensitiy, out float3 IndirectLighting)
{
    float NoV = saturate(abs(dot(N, V)) + 1e-5);

    // 1. 多重反弹 AO (这会增加暗部的亮度，如果 DiffuseColor 很白，这里会更白，建议先保留)
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);

    float3 Irradiance = float3(0, 0, 0);

    #if defined(LIGHTMAP_ON)
    float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
    Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));

    #if defined(DIRLIGHTMAP_COMBINED)
    float4 direction = SAMPLE_TEXTURE2D(unity_LightmapInd, samplerunity_Lightmap, lightUV);

    // 修复 1: 移除除以 direction.w 的操作，这非常危险且不稳定
    // Irradiance = Irradiance * halfLambert / max(1e-4,direction.w); 

    // 你的自定义重映射逻辑 (假设 RangeRemap 是外部定义的)
    // 注意：_LightVector.x 如果是 gamma 值，建议放在最后
    // Irradiance = lerp(RangeRemap(0, _LightVector.z, Irradiance), Irradiance, _LightVector.w);
    // Irradiance = clamp(Irradiance, 0, _LightVector.y);
    // Irradiance = pow(Irradiance, _LightVector.x);

    // 修复 2 (核心): 
    // 烘焙光照贴图主要是漫反射(Diffuse)。
    // 绝对不要在这里把 BlinnPhong (高光) 乘进去再叠加！
    // 高光应该由反射探针 (Reflection Probe) 或实时光提供。
    // 如果你非常想模拟烘焙方向带来的高光，必须非常微弱，且不能乘以 Irradiance 本身。

    // 原错误代码：Irradiance = Irradiance * _LightIntensitiy + Irradiance * BlinnPhong * HeightLight * _LightIntensitiy;

    // 修正后：仅应用光照强度
    Irradiance *= _LightIntensitiy;
    #else
    Irradiance *= _LightIntensitiy;
    #endif
    #else
    // 假如没有光照贴图，使用球谐光照
    Irradiance = SampleSH(N) * _LightIntensitiy;
    #endif

    // 漫反射部分 = 辐照度 * 材质漫反射颜色 * AO
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;

    // 修复 3: 反射探针采样逻辑修正 (见下方函数修复)
    half3 R = reflect(-V, N);
    half3 SpeucularLD = GlossyEnvironmentReflection_revert(R, Roughness, DiffuseAO);
    // 这里传 DiffuseAO 进去做 Specular Occlusion 是种近似，可以用

    half3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);

    // Specular Occlusion 计算
    half SpecularOcclusion = GetSpecularOcclusion(NoV, Roughness * Roughness, Occlusion);

    // 注意：这里又做了一次 AOMultiBounce 给 Specular？通常 SpecularAO 不需要 MultiBounce，直接乘即可。
    // 如果你坚持要用，保持原样，但通常直接 * SpecularOcclusion 就够了。
    half3 IndirectSpecular = SpeucularLD * SpecularDFG * SpecularOcclusion;

    IndirectLighting = IndirectDiffuse + IndirectSpecular;
}

void IndirectLighting_simple(float3 DiffuseColor, float3 N, float3 V, float Occlusion, out float3 IndirectLighting)
{
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    half3 Irradiance = SampleSH(N);
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;
    IndirectLighting = IndirectDiffuse;
}

void IndirectLighting_simpleLightmap(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 N, float3 V,
                                     float Occlusion,half3 lDir,half2 lightUV,half HegihtLight,half _LightIntensitiy,
                                     half _ReflectIntensitiy, out float3 IndirectLighting)
{
    half NoV = saturate(abs(dot(N, V)) + 1e-5);
    half3 Irradiance = half3(0, 0, 0);
    #if defined(LIGHTMAP_ON)
    float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
    Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));
    #else
    Irradiance = SampleSH(N);
    #endif
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO * _LightIntensitiy;
    //float3 IndirectDiffuse =  DiffuseColor * DiffuseAO;
    //float3 IndirectDiffuse =  DiffuseColor ;
    half BlinnPhong = pow(saturate(dot(N, normalize(lDir + V))), 30);
    half3 R = reflect(-V, N);
    half3 SpeucularLD = GlossyEnvironmentReflection_revert(R, Roughness, DiffuseAO) * _ReflectIntensitiy;
    half3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    half SpecularOcclusion = GetSpecularOcclusion(NoV, Roughness * Roughness, Occlusion);
    half3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    half3 IndirectSpecular = SpeucularLD * SpecularDFG * SpecularAO;
    IndirectSpecular = IndirectSpecular + IndirectSpecular * BlinnPhong * HegihtLight;
    IndirectLighting = IndirectSpecular + IndirectDiffuse;
    //IndirectLighting =IndirectSpecular * BlinnPhong * HegihtLight;
}

void IndirectLighting_OnlyLightmap(float3 DiffuseColor, float3 N, float Occlusion,half2 lightUV,half _LightIntensitiy,
                                   out float3 IndirectLighting)
{
    half3 Irradiance = half3(0, 0, 0);
    #if defined(LIGHTMAP_ON)
    float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
    Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));
    #else
    Irradiance = SampleSH(N) * _LightIntensitiy;
    #endif
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;
    IndirectLighting = IndirectDiffuse;
}

void IndirectLighting_customLight(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 N,
                                  float3 V,
                                  float Occlusion, float2 lightUV, float3 lDir,half _LightIntensitiy, float HeightLight,
                                  float4 encodedIrradiance, out float3 IndirectLighting)
{
    IndirectLighting = half3(0, 0, 0);
    #ifndef SHADERGRAPH_PREVIEW
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    //half3 Irradiance = half3(0,0,0);
    //#if defined(_CUSTOMLIGHTMAP)
    half3 Irradiance = DecodeLightmap(encodedIrradiance,
                                      float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h)) *
        _LightIntensitiy;
    half BlinnPhong = pow(max(0, dot(N, normalize(lDir + V))), 20);
    Irradiance = Irradiance * _LightIntensitiy + Irradiance * BlinnPhong * HeightLight;
    //#else
    //encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap,samplerunity_Lightmap,lightUV);
    //Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h)) * _LightIntensitiy ;
    //#else
    //Irradiance = SampleSH(N);
    //Irradiance = 1;
    //#endif

    //SH
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;
    //IBL
    half3 R = reflect(-V, N);
    float3 SpecularG = GlossyEnvironmentReflection(R, Roughness, Occlusion);
    float3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    float SpecularOcclusion = GetSpecularOcclusion(NoV, Pow2(Roughness), Occlusion);
    float3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    float3 IndirectSpecular = SpecularG * SpecularDFG * SpecularAO;
    IndirectLighting = IndirectDiffuse + IndirectSpecular;
    #endif
}

void IndirectLighting_customReflection(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos,
                                       float3 N, float3 V,
                                       float Occlusion, float2 lightUV,half4 encodedIrradiance_re,
                                       half4 ReflectionCubeMap_HDR, out float3 IndirectLighting)
{
    IndirectLighting = half3(0, 0, 0);
    #ifndef SHADERGRAPH_PREVIEW
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    half3 Irradiance = half3(0, 0, 0);
    #if defined(LIGHTMAP_ON)
    float4 encodedIrradiance = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, lightUV);
    Irradiance = DecodeLightmap(encodedIrradiance, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h));
    #else
    Irradiance = SampleSH(N);
    //Irradiance = 1;
    #endif

    //SH
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO;
    //IBL
    half3 R = reflect(-V, N);
    float3 SpecularG = GlossyEnvironmentReflection_custom(R, Roughness, Occlusion, encodedIrradiance_re,
                                                          ReflectionCubeMap_HDR);
    float3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    float SpecularOcclusion = GetSpecularOcclusion(NoV, Pow2(Roughness), Occlusion);
    float3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    float3 IndirectSpecular = SpecularG * SpecularDFG * SpecularAO;
    IndirectLighting = IndirectDiffuse + IndirectSpecular;
    #endif
}

void fogLinear(float Distance, float Start, float End, out float fog)
{
    fog = (1 - saturate((End - Distance) / (End - Start)));
}

void IndirectLightingHair_float(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 N,
                                float3 V,
                                float Occlusion, float EnvRotation, out float3 IndirectLighting)
{
    IndirectLighting = half3(0, 0, 0);
    #ifndef SHADERGRAPH_PREVIEW
    //float NoV = saturate(abs(dot(N,V))+1e-5);
    //SH
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float3 RadianceSH = SampleSH(N);
    float3 IndirectDiffuse = RadianceSH * DiffuseColor * DiffuseAO;

    //IBL
    // half3 R = reflect(-V,N);
    // R = RotateDirection(R,EnvRotation);
    // float3 SpecularG = GlossyEnvironmentReflection(R,Roughness,Occlusion);
    // float3 SpecularDFG = EnvBRDFApprox(SpecularColor,Roughness,NoV);
    // float SpecularOcclusion = GetSpecularOcclusion(NoV,Pow2(Roughness),Occlusion);
    // float3 SpecularAO = AOMultiBounce(SpecularColor,SpecularOcclusion);
    // float3 IndirectSpecular = SpecularG * SpecularDFG * SpecularAO;
    IndirectLighting = IndirectDiffuse;
    #endif
}

void IndirectLightingSSS_float(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 N,
                               float3 V,
                               float Occlusion, float EnvRotation,half3 lutMap,half4 MetallicSmoothness,
                               out float3 IndirectLightingSSS)
{
    IndirectLightingSSS = half3(0, 0, 0);
    half NoV = saturate(abs(dot(N, V)) + 1e-5);
    //SH
    half3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    half3 RadianceSH = SampleSH(N);
    half3 IndirectDiffuse = RadianceSH * DiffuseColor * DiffuseAO;
    half3 IndirectDiffuseSSS = lutMap * IndirectDiffuse;
    half skin = 1 - MetallicSmoothness.a;
    half3 Diffuse = lerp(IndirectDiffuse, IndirectDiffuseSSS * 0.5 + 0.5, skin);
    //IBL
    half3 R = reflect(-V, N);
    R = RotateDirection(R, EnvRotation);
    half3 SpecularG = GlossyEnvironmentReflection(R, Roughness, Occlusion);
    half3 SpecularDFG = EnvBRDFApprox(SpecularColor, Roughness, NoV);
    half SpecularOcclusion = GetSpecularOcclusion(NoV, Pow2(Roughness), Occlusion);
    half3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    half3 IndirectSpecular = SpecularG * SpecularDFG * SpecularAO;
    IndirectLightingSSS = Diffuse + IndirectSpecular;
}

void SSSuv_float(float3 DiffuseColor, float3 N, float3 WorldPos,
                 float Occlusion,half LutOffset,half sssPower, out float2 uvLut)
{
    uvLut = half2(0, 0);
    //SH
    half3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    half3 RadianceSH = SampleSH(N);
    half3 IndirectDiffuse = RadianceSH * DiffuseColor * DiffuseAO;
    half IndirectSSS = IndirectDiffuse.x;
    half Curvature = 0;
    GetCurvature_float(LutOffset, sssPower, N, WorldPos, Curvature);
    uvLut = half2(IndirectSSS, Curvature);
}

//DirectLighting
void DirectLighting_float(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 N,
                          float3 V,
                          out float3 DirectLighting)
{
    DirectLighting = half3(0, 0, 0);
    #ifndef SHADERGRAPH_PREVIEW
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    float4 positionCS = TransformWorldToHClip(WorldPos);
    float4 ShadowCoord = ComputeScreenPos(positionCS);
    #else
    float4 ShadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif
    float4 ShadowMask = float4(1.0, 1.0, 1.0, 1.0);
    //mainLight
    float3 DirectLight_MainLight = half3(0, 0, 0);
    {
        Light light = GetMainLight(ShadowCoord, WorldPos, ShadowMask);
        float3 L = light.direction;
        float3 LightColor = light.color;
        float Shadow = light.shadowAttenuation;
        DirectLight_MainLight = StandardBRDF(DiffuseColor, SpecularColor, Roughness, L, V, N, LightColor, Shadow);
    }
    //AddLight
    float3 DirectLight_AddLight = half3(0, 0, 0);
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIntex = 0; lightIntex < pixelLightCount; ++lightIntex)
    {
        Light light = GetAdditionalLight(lightIntex, WorldPos, ShadowMask);
        half3 L = light.direction;
        half3 LightColor = light.color;
        half Shadow = light.shadowAttenuation * light.distanceAttenuation;
        DirectLight_AddLight += StandardBRDF(DiffuseColor, SpecularColor, Roughness, L, V, N, LightColor, Shadow);
    }

    DirectLighting = DirectLight_MainLight + DirectLight_AddLight;
    #endif
}

void DirectLightingMoblie_float(float3 DiffuseColor, float3 SpecularColor, float Roughness, float SpecShininess,
                                float3 N, float3 L, float3 H,half3 hightColor,half3 darkColor,
                                out float3 DirectLighting)
{
    DirectLighting = half3(0, 0, 0);
    #ifndef SHADERGRAPH_PREVIEW
    //direct diffuse
    half lambert = max(0.0, dot(N, L));
    half3 diffuse = lerp(darkColor, hightColor, lambert);
    half3 DirectDiffuse = diffuse * DiffuseColor;

    //direct specular
    half NdotH = dot(N, H);
    half smoothness = 1.0 - Roughness;
    half shininess = lerp(1, SpecShininess, smoothness);
    half binPhong = pow(max(0.0, NdotH), shininess * smoothness);
    half3 DirectSpec = binPhong * SpecularColor;

    DirectLighting = DirectDiffuse;
    #endif
}

void WaterDirectLighting_float(float3 DiffuseColor, float3 SpecularColor, float Roughness, float3 WorldPos, float3 lDir,
                               float3 N, float3 V,
                               out float3 DirectLighting)
{
    DirectLighting = half3(0, 0, 0);
    #ifndef SHADERGRAPH_PREVIEW
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    float4 positionCS = TransformWorldToHClip(WorldPos);
    float4 ShadowCoord = ComputeScreenPos(positionCS);
    #else
    float4 ShadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif
    float4 ShadowMask = float4(1.0, 1.0, 1.0, 1.0);
    //mainLight
    float3 DirectLight_MainLight = half3(0, 0, 0);
    {
        Light light = GetMainLight(ShadowCoord, WorldPos, ShadowMask);
        float3 L = lDir;
        float3 LightColor = light.color;
        float Shadow = light.shadowAttenuation;
        DirectLight_MainLight = StandardBRDF(DiffuseColor, SpecularColor, Roughness, L, V, N, LightColor, Shadow);
    }

    DirectLighting = DirectLight_MainLight;
    #endif
}

//cloth
void IndirectLighting_cloth(float3 DiffuseColor, float3 SheenColor, float Roughness, float SheenRoughness,
                            float ClothDFG, float SheenDFG, float3 WorldPos, float3 N, float3 V,
                            float Occlusion,half HeightLight, float3 lDir, float EnvRotation,
                            out float3 IndirectLighting)
{
    IndirectLighting = half3(0, 0, 0);
    //#ifndef SHADERGRAPH_PREVIEW 
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    //SH
    float3 RadianceSH = SampleSH(N);
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);

    //half halfLambert = saturate(dot(N,lDir)) * 0.5 + 0.5;
    half3 Irradiance = RadianceSH;
    half BlinnPhong = pow(max(0, dot(N, normalize(lDir + V))), 20);


    float3 IndirectDiffuse = Irradiance * DiffuseColor * DiffuseAO + Irradiance * BlinnPhong * HeightLight * SheenColor;
    //#if defined(_SH_OFF)
    //IndirectDiffuse = half3(0,0,0);
    //#endif
    //IBL
    half3 R = reflect(-V, N);
    R = RotateDirection(R, EnvRotation);
    float3 SpecularG = GlossyEnvironmentReflection_revert(R, Roughness, 1.0f);
    float3 SpecularDFG = ClothDFG * 0.04f;
    float SpecularOcclusion = GetSpecularOcclusion(NoV, Pow2(Roughness), 1.0f);
    float3 SpecularAO = AOMultiBounce(float3(0.04, 0.04, 0.04), SpecularOcclusion);
    float3 IndirectSpecular = SpecularG * SpecularDFG * SpecularAO;
    //#if defined(_IBL_OFF)
    //IndirectSpecular = half3(0,0,0);
    //#endif

    float3 SheenSpecularG = GlossyEnvironmentReflection_revert(R, SheenRoughness, 1.0f);
    float3 SheenSpecularDFG = SheenColor * SheenDFG;
    float SheenOcclusion = GetSpecularOcclusion(NoV, Pow2(SheenRoughness), 1.0f);
    float3 SheenAO = AOMultiBounce(SheenColor, SheenOcclusion);
    float3 SheenSpecular = SheenSpecularG * SheenSpecularDFG * SheenAO;
    //#if defined(_IBL_OFF)
    //IndirectSpecular = half3(0,0,0);
    //#endif
    float sheenScaling = 1.0 - max(max(SheenColor.r, SheenColor.g), SheenColor.b) * SheenDFG;
    IndirectDiffuse *= sheenScaling;
    IndirectSpecular *= sheenScaling;

    IndirectLighting = IndirectDiffuse + IndirectSpecular + SheenSpecular;
    //IndirectLighting = RadianceSH  ;
    //#endif
}

void IndirectLighting_skin(float3 DiffuseColor, float3 SpecularColor, float Lobe0Roughness, float Lobe1Roughness,
                           float LobeMix, float3 WorldPos, float3 N, float3 V, float Occlusion, float EnvRotation,
                           out float3 IndirectLighting)
{
    float AverageRoughness = (Lobe0Roughness + Lobe1Roughness) * 0.5;
    IndirectLighting = half3(0, 0, 0);
    //#ifndef SHADERGRAPH_PREVIEW 
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    //float MainlightShadow = clamp(GetMainLightShadow(WorldPos),0.35,1.0);
    //AO
    float3 DiffuseAO = AOMultiBounce(DiffuseColor, Occlusion);
    float SpecularOcclusion = GetSpecularOcclusion(NoV, Pow2(AverageRoughness), Occlusion);
    float3 SpecularAO = AOMultiBounce(SpecularColor, SpecularOcclusion);
    //SH
    float3 RadianceSH = SampleSH(N);
    float3 IndirectDiffuse = RadianceSH * DiffuseColor * DiffuseAO;
    //#if defined(_SH_OFF)
    //IndirectDiffuse = half3(0,0,0);
    //#endif
    //IBL
    half3 R = reflect(-V, N);
    R = RotateDirection(R, EnvRotation);
    half3 SpecularLobe0 = SpecularIBL(R, WorldPos, Lobe0Roughness, SpecularColor, NoV);
    half3 SpecularLobe1 = SpecularIBL(R, WorldPos, Lobe1Roughness, SpecularColor, NoV);
    half3 DualLobe = lerp(SpecularLobe0, SpecularLobe1, 1.0 - LobeMix);
    float3 IndirectSpecular = DualLobe * SpecularAO;
    //#if defined(_IBL_OFF)
    //IndirectSpecular = half3(0,0,0);
    //#endif
    //ClearCoat
    //half3 R_ClearCoat = reflect(-V,ClearCoatNormal);
    //float NoV_ClearCoat = saturate(abs(dot(ClearCoatNormal,V)) + 1e-5);
    //half3 ClearCoatLobe = SpecularIBL(R_ClearCoat,WorldPos,ClearCoatRoughness,float3(0.04,0.04,0.04),NoV_ClearCoat);
    //half3 IndirectClearCoat = ClearCoatLobe * ClearCoat * SpecularAO;
    //float3 EnergyLoss = F_Schlick_UE4( float3(0.04,0.04,0.04), NoV_ClearCoat ) * ClearCoat;
    //IndirectDiffuse = IndirectDiffuse ;
    //IndirectSpecular = IndirectSpecular ;
    IndirectLighting = IndirectDiffuse + IndirectSpecular;
    //#endif
}

float3 SkinBRDF(float3 DiffuseColor, float Lobe0Roughness,
                float3 L, float3 V, float3 N, float3 LightColor,
                half3 DiffuseIrradiance)
{
    float a2 = Pow4(Lobe0Roughness);
    float3 H = normalize(V + L);
    float NoH = saturate(dot(N, H));
    float NoV = saturate(abs(dot(N, V)) + 1e-5); // Both side 
    float NoL = dot(N, L);
    float VoH = saturate(dot(V, H));
    float3 DiffuseTerm = Diffuse_Lambert(DiffuseColor) * DiffuseIrradiance * PI * (NoL * 0.5 + 0.5);

    // Generalized microfacet specular
    float NoL_Specular = saturate(NoL);
    half3 SpecularIrradiance = NoL_Specular * LightColor;
    // Generalized microfacet specular
    float D = D_GGX_UE4(a2, NoH);
    float Vis = Vis_SmithJointApprox(Lobe0Roughness, NoV, NoL);
    float3 F = F_Schlick_UE4(0.04, VoH);
    float3 SpecularBRDF = (D * Vis) * F;
    half3 SpecularTerm = SpecularBRDF * SpecularIrradiance * PI;

    float3 DirectLighting = DiffuseTerm + SpecularTerm;
    return DirectLighting;
}

// void ScenceDisappear(float3 positionWS,float4 center,float scenceContrl,float noise,float noiseOffset,float edgeWidth,half4 edgeColor,out float dissoloveAlpha,out float4 outColor)
// {
//    	float Radius = scenceContrl;
//    	float circleMask = 1 -  saturate(distance(positionWS,center) / (Radius * 100));
//    	float noiseCircleMask = circleMask * noise;
//    	float dissoloveEdge  =  step(( noiseCircleMask - edgeWidth),noiseOffset);
//    	float dissoloveOffset = step(noiseOffset,noiseCircleMask);
//    	dissoloveAlpha = dissoloveEdge ;
//    	outColor = dissoloveEdge * dissoloveOffset * edgeColor;
// }
// void ScenceAppear(float3 positionWS,float4 center,float scenceContrl,float noise,float noiseOffset,float edgeWidth,half4 edgeColor,out float dissoloveAlpha,out float4 outColor)
//     {
//    		float Radius = scenceContrl;
//    		float circleMask = 1 -  saturate(distance(positionWS,center) / (Radius * 100));
//    		float noiseCircleMask = circleMask * noise;
//    		float dissoloveEdge  =  step(( noiseCircleMask - edgeWidth),noiseOffset);
//    		float dissoloveOffset = step(noiseOffset,noiseCircleMask);
//    		dissoloveAlpha = saturate(dissoloveOffset) ;
//    		outColor = dissoloveEdge * dissoloveOffset * edgeColor;
//     }
#endif
