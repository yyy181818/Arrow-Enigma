Shader "TA Shaders/MatCap"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 6
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", float) = 0
        [Toggle] _AlphaTest ("Alpha Clip", Float) = 0
        _Cutoff ("Cut off", Range(0,1)) = 0.5
        [Space(10)]
        [Header(Base Color)]
        [Space(5)]
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Color Blendmode)]
        [Space(5)]
        [Enum(Add,0,Multiply,1)] _ColorBlendMode ("Color Blendmode", Float) = 0
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range(0,1)) = 1
        [Header(Normal Settings)]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(1, 16)) = 1
        [Header(MatCap Settings)]
        _MatCap ("MatCap", 2D) = "white" {}
        _Rotation ("Matcap Rotation", Range(0, 360)) = 0
        _MatCapCenterScale ("MatCap Center Scale", float) = 1
        _MatcapSampleLod ("MatCap SampleLOD", Range(0,1)) = 1
        _MatCapStrength ("MatCap Strength", Range(0, 8)) = 1
        [Space(10)]
        [Header(Color Settings)]
        [Space(5)]
        _Brightness("Brightness", Range(0,8)) = 1 //调整亮度
        _Saturation("Saturation", Range(0,32)) = 1 //调整饱和度
        _Contrast("Contrast", Range(0,32)) = 1 //调整对比度
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half _ColorBlendMode;
            half4 _BaseColor;
            half _NormalScale;
            float _MatCapCenterScale;
            half _MatCapStrength;
            half _MatcapSampleLod;
            half _Cutoff;
            half _Alpha;
            float _Rotation;
            half _Brightness;
            half _Saturation;
            half _Contrast;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);
        TEXTURE2D(_MatCap);
        SAMPLER(sampler_MatCap);
        ENDHLSL

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_CullMode]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionVS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
            };

            Varyings vert(Attributes IN)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS);
                output.positionVS = vertexInput.positionVS;
                output.positionCS = vertexInput.positionCS;
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = IN.uv;
                return output;
            }

            float2x2 Rotation(half Angle)
            {
                Angle = PI / 180 * Angle;
                half cosAngle = cos(Angle);
                half sinAngle = sin(Angle);
                float2x2 rot = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                return rot;
            }

            float2 MatCapUV(float3 N,float3 viewPos)
            {
                float3 viewNorm = mul((float3x3)UNITY_MATRIX_V, N);
                float3 viewDir = normalize(viewPos);
                float3 viewCross = cross(viewDir, viewNorm);
                viewNorm = float3(-viewCross.y, viewCross.x, 0.0);
                float2 matCapUV = viewNorm.xy * 0.5 * _MatCapCenterScale + 0.5;
                return matCapUV;
            }

            float3 NormalStrength(float3 In, float Strength)
            {
                return float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                half alpha = albedo.a * _Alpha;
                half3 normalTangent = UnpackNormal(
                    SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half3x3 TBN = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                half3 normalWS = normalize(mul(normalTangent, TBN));
                normalWS = NormalStrength(normalWS, _NormalScale);
                float2 matcapUV = MatCapUV(normalWS, input.positionVS);
                matcapUV = mul(Rotation(_Rotation), matcapUV - 0.5) + 0.5;
                float3 spmatCap = SAMPLE_TEXTURE2D_LOD(_MatCap, sampler_MatCap, matcapUV.xy,
                        8 * (1.0 - _MatcapSampleLod)).
                    rgb * _MatCapStrength;;
                
                half3 finalColor = _ColorBlendMode ? albedo.rgb * spmatCap : albedo.rgb + spmatCap * 2 - 1;
                
                finalColor *= _Brightness;
                //saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
                half gray = 0.2125 * finalColor.r + 0.7154 * finalColor.g + 0.0721 * finalColor.b;
                half3 grayColor = half3(gray, gray, gray);
                //根据Saturation在饱和度最低的图像和原图之间差值
                finalColor = lerp(grayColor, finalColor, _Saturation);
                //contrast对比度：首先计算对比度最低的值
                half3 avgColor = half3(0.5, 0.5, 0.5);
                //根据Contrast在对比度最低的图像和原图之间差值
                finalColor = lerp(avgColor, finalColor, _Contrast);
                #if defined(_ALPHATEST_ON)
                clip(alpha - _Cutoff);
                #endif

                return half4(finalColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}