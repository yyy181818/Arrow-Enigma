// URP 水晶/宝石 shader
// 特性：屏幕空间折射（能看穿到后面）、Fresnel 边缘反射、三通道色散模拟火彩、内部自发光
// 适用：URP 14.x（Wheel of Riddles 项目用的版本）
// 用法：把这段存成 Crystal.shader，材质 Shader 选 Universal Render Pipeline/Crystal

Shader "Universal Render Pipeline/Crystal"
{
    Properties
    {
        [Header(基础)]
        _BaseMap("颜色贴图", 2D) = "white" {}
        _BaseColor("基础色", Color) = (0.85, 0.08, 0.18, 1)
        [MainTexture] _BaseMap_ST("贴图缩放偏移", Vector) = (1, 1, 0, 0)

        [Header(透明与折射)]
        _RefractionStrength("折射强度", Range(0, 1)) = 0.35
        _RefractOffset("折射偏移", Range(-0.1, 0.1)) = 0.02
        _Dispersion("色散强度", Range(0, 1)) = 0.25

        [Header(Fresnel 边缘)]
        _RimColor("边缘颜色", Color) = (1, 0.55, 0.62, 1)
        _RimPower("边缘锐利度", Range(0.5, 8)) = 3
        _RimIntensity("边缘亮度", Range(0, 5)) = 2.5

        [Header(表面高光)]
        _Smoothness("光滑度", Range(0, 1)) = 0.97
        _SpecColor("高光颜色", Color) = (1, 1, 1, 1)
        _SpecIntensity("高光强度", Range(0, 5)) = 2

        [Header(自发光)]
        _EmissionColor("自发光颜色", Color) = (1.2, 0.15, 0.25, 1)
        _EmissionMap("自发光贴图", 2D) = "black" {}
        [MainColor] _EmissionColorMain("自发光强度", Range(0, 3)) = 1.2

        [Header(法线)]
        _BumpMap("法线贴图", 2D) = "bump" {}
        _BumpScale("凹凸强度", Range(0, 3)) = 1

        [Header(深度淡出)]
        _FadeNear("淡出起始距离", Float) = 0.5
        _FadeFar("淡出结束距离", Float) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "CrystalForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            TEXTURE2D(_BaseMap);   SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_BumpMap);   SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _EmissionColor;
                float4 _RimColor;
                float4 _SpecColor;
                float _RefractionStrength;
                float _RefractOffset;
                float _Dispersion;
                float _RimPower;
                float _RimIntensity;
                float _Smoothness;
                float _SpecIntensity;
                float _EmissionColorMain;
                float _BumpScale;
                float _FadeNear;
                float _FadeFar;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 screenPos   : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float  fogCoord    : TEXCOORD5;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.screenPos = ComputeScreenPos(posInputs.positionCS);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                OUT.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            // 屏幕空间采样：拿背后的颜色，模拟折射
            float4 SampleRefraction(float2 screenUV, float3 normalWS, float3 viewDirWS, float offsetScale)
            {
                // 用法线在屏幕空间的偏移量决定折射方向
                float2 screenNormal = normalWS.xy * offsetScale;

                // 三个通道用略微不同的偏移，做出色散
                float3 dist = screenNormal;

                // 采样偏移方向：视线与法线越平行，折射越强
                float parallel = saturate(abs(dot(viewDirWS, normalWS)));
                float2 shift = screenNormal * lerp(1.0, 0.2, parallel);

                float2 baseUV = screenUV + shift * _RefractionStrength;

                float4 r = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV + dist * _Dispersion * 0.02);
                float4 g = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV);
                float4 b = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, baseUV - dist * _Dispersion * 0.02);

                return float4(r.r, g.g, b.b, 1.0);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 距离淡出，太近时隐掉，避免糊脸
                float distToCam = distance(_WorldSpaceCameraPos, IN.positionWS);
                float fade = 1.0 - saturate((distToCam - _FadeNear) / max(_FadeFar - _FadeNear, 0.001));
                fade = saturate(fade);
                clip(fade - 0.01);

                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionWS);

                // 法线贴图
                #ifdef _NORMALMAP
                half3x3 tangentToWorld = half3x3(
                    normalize(IN.tangentOS.xyz),
                    normalize(cross(IN.tangentOS.xyz, normalWS)),
                    normalWS);
                half4 bumpSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv);
                half3 bumpedNormalWS = normalize(TransformTangentToWorld(bumpSample, tangentToWorld));
                normalWS = normalize(lerp(normalWS, bumpedNormalWS, _BumpScale));
                #endif

                float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w, 0.0001);

                // 屏幕空间折射：看穿玉体
                float4 behind = SampleRefraction(screenUV, normalWS, viewDirWS, _RefractOffset);

                // 基础颜色
                float4 albedoTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 baseCol = albedoTex.rgb * _BaseColor.rgb;

                // Fresnel：正对时弱，掠射时强
                float fresnel = pow(saturate(1.0 - dot(normalWS, viewDirWS)), _RimPower);

                // 直接光高光
                Light mainLight = GetMainLight(IN.shadowCoord);
                half3 lightDir = normalize(mainLight.direction);
                half3 halfVec = normalize(lightDir + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfVec)), lerp(8, 256, _Smoothness));
                half3 specular = _SpecColor.rgb * spec * _SpecIntensity * mainLight.color;

                // 自发光（贴图 + 颜色）
                half4 emisTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv);
                half3 emission = _EmissionColor.rgb * _EmissionColorMain * (0.4 + emisTex.rgb);

                // 合成：折射背景为主体，加染色，加边缘发光
                half3 col = behind.rgb * baseCol * 1.4;
                col += baseCol * mainLight.color * 0.25;      // 玉体染色
                col += specular;
                col += emission;
                col += _RimColor.rgb * fresnel * _RimIntensity; // 边缘火彩

                // 边缘更亮更白，模拟宝石棱角
                col = lerp(col, _RimColor.rgb * 1.5, saturate(fresnel * 0.6));

                // Alpha：中央透，边缘实
                half alpha = saturate(_BaseColor.a * 0.35 + fresnel * 0.65);
                alpha *= albedoTex.a;
                alpha = max(alpha, fresnel);   // 边缘完全不透
                alpha *= fade;

                col = MixFog(col, IN.fogCoord);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}