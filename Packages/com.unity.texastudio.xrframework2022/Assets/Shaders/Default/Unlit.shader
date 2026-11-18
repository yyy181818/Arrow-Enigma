Shader "TA Shaders/Unlit"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("Cull", float) = 2
        
        [Toggle]_AlphaTest ("Alpha Clip", float) = 0
        _Cutoff ("Cut off", Range(0.0,1.0)) = 0.5
        [Space(10)]
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
        [Toggle(_EMISSION)]_Emission ("Emission", float) = 0
        _EmissionMap ("Emission Map", 2D) = "white"{}
        [HDR]_EmissionColor ("Emission Color", Color) = (0,0,0,0)
        
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Name "Unlit"
            Cull [_CullMode]
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_fog
            
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            #if defined(_EMISSION)
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            #endif
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _EmissionColor;
                float _Cutoff;
            CBUFFER_END
            half _Light;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #if defined(_FOG_FRAGMENT)
                output.fogCoord = vertexInput.positionVS.z;
                #else
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                #endif
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half2 uv = input.uv;
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half3 color = texColor.rgb * _BaseColor.rgb;
                half alpha = texColor.a * _BaseColor.a;

                alpha = AlphaDiscard(alpha, _Cutoff);
                color = AlphaModulate(color, alpha);
                #if defined(_EMISSION)
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb;
                #else
                half3 emission = 0;
                #endif
                
                half4 finalColor = half4(color + emission, alpha);

                #if defined(_FOG_FRAGMENT)
                #if (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
                float viewZ = -input.fogCoord;
                float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
                half fogFactor = ComputeFogFactorZ0ToFar(nearToFarZ);
                #else
                half fogFactor = 0;
                #endif
                #else
                half fogFactor = input.fogCoord;
                #endif
                half GlobalLight = 1 - _Light;
                finalColor.rgb = MixFog(finalColor.rgb, fogFactor);
                finalColor.rgb *= GlobalLight;
                return finalColor;
            }
            ENDHLSL
        }
    }
}