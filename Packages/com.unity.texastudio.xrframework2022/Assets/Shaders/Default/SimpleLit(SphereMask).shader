Shader "TA Shaders/Default/SimpleLit(SphereMask)"
{
    Properties
    {
        [Header(Settings)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [Toggle] _AlphaTest ("Alpha Test", Float) = 0
        _Cutoff ("Cutoff", Range(0, 1)) = 0.5
        [Space(10)]
        [Header(BaseColor)]
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _ColorIntensity ("Color Intensity", Range(0, 16)) = 1
        [Space(10)]
        [Header(Noise)]
        _CutoutMap ("Cutout Map", 2D) = "white" {}
        [HDR]_EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _NoiseWidth ("Noise Width", Range(0, 1)) = 0.1
        _NoiseStrength ("Noise Strength", Range(0, 16)) = 0.5
        _NoiseSpeed ("Noise Speed", Range(0, 1)) = 0.1
        [Space(10)]
        [Header(SphereMask)]
        [Toggle] _SphereMaskToggle ("SphereMask Enable", Float) = 0
        [Toggle] _Invert ("Invert", Float) = 0
        //[HideInInspector]_MaskValue ("Mask Value", Range(0, 1)) = 0
        //[Enum(Sphere, 0, Plane, 1)]_SphereMaskType ("SphereMask Type", float) = 0
        [Toggle] _UseSecondPositionToggle ("Use Second Position Enable", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue" = "Transparent" "RenderPipeine" = "UniversalPipeline" "IgnoreProjector"="True"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _ColorIntensity;
            float4 _MainTex_ST;
            half _NoiseWidth;
            half _NoiseStrength;
            half _NoiseSpeed;
            half _Cutoff;
            float4 _CutoutMap_ST;
            half4 _EdgeColor;
            half _Invert;
            half _UseSecondPositionToggle;
        CBUFFER_END

        float3 _MaskPosition;
        half3 _MaskNormal;

        float3 _MaskSecondPosition;
        half3 _MaskSecondNormal;

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutoutMap);
        SAMPLER(sampler_CutoutMap);
        ENDHLSL

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature _SPHEREMASKTOGGLE_ON

            // GPU Instancing
            #pragma multi_compile_instancing

            #define SphereMaskSmooth(x) x = saturate(x * x * (3 - 2 * x));

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // float2 SphereMask_HeightField(float3 vertexWorldPosition, float noise, float3 maskPosition, float radius,
            //     float intensity, float noiseStrength, float edgeSize, float smooth)
            // {
            //     //Distance
            //     float d = distance(maskPosition, vertexWorldPosition);
            //     float d1 = distance(maskPosition, vertexWorldPosition);
            //     //Noise
            //     float r = radius;
            //     radius += noise * noiseStrength;
            //
            //     float shape = 1 - saturate(max(0, d - radius + edgeSize) / edgeSize);
            //     float shape1 = 1 - saturate(max(0, d1 - r + edgeSize) / edgeSize);
            //     //Smooth
            //     shape = pow(shape, smooth + 0.01);
            //     shape1 = pow(shape1, smooth + 0.01);
            //     //Fade
            //     shape *= intensity;
            //     shape1 *= intensity;
            //     return float2(shape, shape1);
            // }

            float2 SphereMask_PlaneMask(float3 planePosition, float3 planeNormal, float3 vertexPositionWS, float noise,
                float noiseStrength)
            {
                float edge = dot(planeNormal, planePosition - vertexPositionWS);
                float edge2 = dot(planeNormal, planePosition - vertexPositionWS);
                edge += noise * noiseStrength;
                edge = saturate(edge / max(0.01, _NoiseWidth) / 0.8);
                edge2 = saturate(edge2 / max(0.01, _NoiseWidth) / 0.8);
                    SphereMaskSmooth(edge);
                    SphereMaskSmooth(edge2);
                return float2(edge, edge2);
            }

            float4 GetMask(float3 positionWS, float2 uv)
            {
                float4 output = float4(0, 0, 0, 1);

                #if defined(_SPHEREMASKTOGGLE_ON)
                float2 noiseUV = TRANSFORM_TEX(uv, _CutoutMap);
                noiseUV += _Time.y * _NoiseSpeed;
                half noise = SAMPLE_TEXTURE2D(_CutoutMap, sampler_CutoutMap, noiseUV).r;
                half2 SphereMask_Plane = _UseSecondPositionToggle == 0
                    ? SphereMask_PlaneMask(_MaskPosition, _MaskNormal, positionWS, noise,
                        _NoiseStrength)
                    : SphereMask_PlaneMask(_MaskSecondPosition, _MaskSecondNormal, positionWS, noise,
                        _NoiseStrength);
                half3 MaskColor = (SphereMask_Plane.x - SphereMask_Plane.y) * _EdgeColor.rgb;
                half mask = _Invert == 1 ? 1 - SphereMask_Plane.x : SphereMask_Plane.x;
                output = float4(MaskColor, mask);
                #endif

                return output;
            }

            Varyings vert(Attributes IN)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, output);
                output.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                output.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 albedo = color.rgb * _Color.rgb * _ColorIntensity;
                half alpha = color.a * _Color.a;
                Light mainLight = GetMainLight(input.shadowCoord);
                half shadow = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                half3 lightDir = mainLight.direction;
                half3 normal = normalize(input.normalWS);
                half NoL = max(0, dot(normal, lightDir)) * 0.5 + 0.5;
                half3 diffuse = albedo * mainLight.color * NoL * shadow;
                half3 finalColor = diffuse;
                half4 mask = GetMask(input.positionWS, input.uv);
                #if defined(_SPHEREMASKTOGGLE_ON)
                //Cutout
                float dissolveValue1 = alpha - _Cutoff;
                float dissolveEdgeWidth1 = 0.1 * (_Cutoff < 0.1 ? _Cutoff * 10 : 1);
                //Geometric
                float dissolveValue2 = mask.a;
                float dissolveEdgeWidth2 = _NoiseWidth;
                float xyzAxisCoef = 0.998;
                //Lerp
                float lerpValue = (mask.a - _NoiseWidth * xyzAxisCoef);
                lerpValue = lerpValue > 0 ? 1 : 0;
                float dissolveValue = lerp(dissolveValue2, dissolveValue1, lerpValue);
                float dissolveEdgeWidth = lerp(dissolveEdgeWidth2, dissolveEdgeWidth1, lerpValue);
                float edgeGradient = saturate(dissolveValue) * (1.0 / saturate(dissolveEdgeWidth));
                //float invertGradient = 1 - edgeGradient;
                mask.a += edgeGradient;
                mask.a = saturate(mask.a);
                #endif
                finalColor += mask.rgb;
                alpha *= mask.a;

                #if defined(_ALPHATEST_ON)
                    clip(alpha - _Cutoff);
                #endif

                return half4(finalColor,alpha);
            }
            ENDHLSL
        }
    }
}