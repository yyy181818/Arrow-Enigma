 //Created by Wans Wu
//2024,09,27
Shader "URP/Environment/Galaxy"
{
    Properties
    {
        _GalaxyAlpha ("Alpha", Range(0, 1)) = 1.00
        _ColorMask  ("_ColorMask ", 2D) = "white" {}
        _ColorMaskDisturbNoise ("Noise", 2D) = "white" {}
        _ColorMask_Disturb ("NoiseTilling", Range(0, 1)) = 0.23
        _ColorMaskDisturbNoise_Speed ("NoiseSpeed", Range(0, 0.1)) = 0.005
        [HideInInspector]_ColorMaskDisturbNoise_ST ("_ColorMaskDisturbNoise_ST", Vector) = (3.50, 1.00, 0.00, 0.00)
        [Space(20)]
        _CenterColor ("Color", Color) = (1.00, 0.00, 0.00, 1.00)
        _CenterColorIntensity ("Intensity", Range(0, 20)) = 18.00
        _CenterBandNoise ("Noise ", 2D) = "white" {}
        [HideInInspector]_CenterBandNoise_ST ("_CenterBandNoise_ST", Vector) = (4.35, 1.10, 0.00, 0.00 )
        _CenterBandNoise_Bias ("NoiseBias", Range(-0.1, 0)) = -0.05 
        _CenterBandNoise_Scale ("NoiseScale", Range(0, 2)) = 1.60
        [Space(20)]
        _BesideColor  ("Color ", Color) = (0.00, 0.74265, 0.03585, 1.00)
        _BesideColor_Intensity ("Intensity", Range(0, 20)) = 10.82
        _BesideColorAddition ("AdditionColor", Color) = (0.14706, 0.40, 1.00, 1.00)
        _BesideColorAddition_Intensity ("AdditionIntensity", Range(0, 10)) = 5.00
        _BesideBandNoise ("Noise", 2D) = "white" {}
        [HideInInspector]_BesideBandNoise_ST ("_BesideBandNoise_ST", Vector) = (1.00, 0.40, 0.00, 0.00                     )
        _BesideBandNoise_Speed ("NoiseSpeed", Range(-0.1, 0)) = -0.005
        _BesideBandNoise_Bias ("NoiseBias", Range(0, 1)) = 0.38
        
        [Space(20)]
        _StarMap ("StarsMap", 2D) = "white" {}
        [HideInInspector]_StarMap_ST ("_StarMap_ST", Vector) = (8.00, 6.00, 0.00, 0.00)
        _StarIntensity ("StarIntensity", Range(0, 2)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="AlphaTest" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull Front
            ZWrite Off
             Blend DstColor One, DstColor One
            BlendOp Add
            HLSLPROGRAM
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO 
            };
            
            #define _UV_ST float4(1.00, 1.00, 0.00, 0.00)          
            TEXTURE2D (_ColorMaskDisturbNoise) ;
            SAMPLER(sampler_ColorMaskDisturbNoise);
            TEXTURE2D (_ColorMask) ;
            SAMPLER(sampler_ColorMask);
            TEXTURE2D (_CenterBandNoise) ;
            SAMPLER(sampler_CenterBandNoise);
            TEXTURE2D (_BesideBandNoise);
            SAMPLER(sampler_BesideBandNoise);
            TEXTURE2D (_StarMap);
            SAMPLER(sampler_StarMap);

            CBUFFER_START(UnityPerMaterial)
            float4 _BesideBandNoise_ST; 
            float4 _BesideColor;
            float _BesideBandNoise_Speed; 
            float _BesideBandNoise_Bias; 
            float _BesideColor_Intensity; 
            float4 _ColorMask_ST; 
            float4 _ColorMaskDisturbNoise_ST;
            float _ColorMaskDisturbNoise_Speed; 
            float _ColorMask_Disturb;
            float4 _CenterBandNoise_ST; 
            float _CenterBandNoise_Bias;
            float _CenterBandNoise_Scale; 
            float _StarIntensity; 
            float4 _StarMap_ST; 
            float4 _CenterColor; 
            float4 _BesideColorAddition;
            float _BesideColorAddition_Intensity; 
            float _GalaxyAlpha; 
            float _CenterColorIntensity;
            CBUFFER_END
            half _Light;

            Varyings LitPassVertex (Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = (input.texcoord * _UV_ST.xy) + _UV_ST.zw;
                return output;
            }
            
            
            half4 LitPassFragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                //BesideBandNoise
                float2 besideNoiseUV = (_Time.yy * _BesideBandNoise_Speed) + (input.uv * _BesideBandNoise_ST.xy) + _BesideBandNoise_ST.zw;
                float besideBandNoise = SAMPLE_TEXTURE2D(_BesideBandNoise,sampler_BesideBandNoise, besideNoiseUV).x;
                float3 besideColor = (besideBandNoise + _BesideBandNoise_Bias) * _BesideColor.xyz * _BesideColor_Intensity;
                
                 //ColorMaskDisturbNoise
                float2 ColorMasknoiseAUV = (_Time.yy * _ColorMaskDisturbNoise_Speed) + (input.uv * _ColorMaskDisturbNoise_ST.xy) + _ColorMaskDisturbNoise_ST.zw;
                float colorMaskDisturbNoise = SAMPLE_TEXTURE2D(_ColorMaskDisturbNoise,sampler_ColorMaskDisturbNoise, ColorMasknoiseAUV).x;

                // ColorMask
                float2 colorMaskUV = (colorMaskDisturbNoise * _ColorMask_Disturb) + (input.uv * _ColorMask_ST.xy) + _ColorMask_ST.zw;
                float2 colorMask = SAMPLE_TEXTURE2D(_ColorMask,sampler_ColorMask, colorMaskUV).xy;

                //Star
                float2 starUV = (input.uv * _StarMap_ST.xy) + _StarMap_ST.zw;
                float starMap = SAMPLE_TEXTURE2D(_StarMap,sampler_StarMap, starUV).x * _StarIntensity * 1000;;
                float starColor = starMap * colorMask.r;

                 //CenterBandNoise
                float2 CenternoiseUV = (input.uv * _CenterBandNoise_ST.xy) + _CenterBandNoise_ST.zw;
                float centerNoise = SAMPLE_TEXTURE2D(_CenterBandNoise,sampler_CenterBandNoise, CenternoiseUV).x;
                float centerAreaReverse = lerp(1.0, (centerNoise + _CenterBandNoise_Bias) * _CenterBandNoise_Scale, colorMask.g);
                
                float besideArea = centerAreaReverse * colorMask.r;
                float centerArea = 1.0 - centerAreaReverse;
                float3 finalbesideColor = (besideColor * besideArea) + (starColor);
                float3 centerColor = _CenterColor.xyz * _CenterColorIntensity * centerArea;

                float3 combineColor = ((centerColor * besideArea) + finalbesideColor +(besideArea * _BesideColorAddition.xyz * _BesideColorAddition_Intensity))* _GalaxyAlpha;
                float4 finalColor = 0;
                finalColor.xyz = combineColor;
                finalColor.w = 1.0;
                return finalColor * (1 -  _Light);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}
