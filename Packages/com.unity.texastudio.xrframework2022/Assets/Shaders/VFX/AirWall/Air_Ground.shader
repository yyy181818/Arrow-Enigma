//Created by Wans Wu
//2024,09,10
Shader "TA Shaders/VFX/Air/Ground"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        [Main(Base,_, off, off)] _Base ("Base", Float) = 0
        [Sub(Base)]_BaseMap("Base Map", 2D) = "white" {}
        [Sub(Base)][MainColor][HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        [Sub(Base)] _Speed("Flow速度", Float) = 0
        [Sub(Base)]_Power("Power", Range( 0 , 5)) = 2
        [Sub(Base)]_Alpha("Alpha", Float) = 0
        [Sub(Base)]_EgdeRange("EgdeRange",Vector) = (2,6,0,0)
        
        [Main(RampMap,_, off, off)] _RampMap ("渐变", Float) = 0
        [Space(10)]
        [Ramp(RampMap, RampMap, Assets.CommonArt.Texture, 512)] _Ramp ("渐变图", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull [_CullMode]
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
           struct Attributes
            {
                float4 positionOS   : POSITION;
               // float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                //float3 normalWS                 : TEXCOORD2;
                half4 tangentWS                : TEXCOORD3;
                float3 positionOS               : TEXCOORD4;
                float4 positionCS               : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO 
            };
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Ramp);
            SAMPLER(sampler_Ramp);
            //TEXTURE2D(_MulMap);
            //SAMPLER(sampler_MulMap);
           
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _MulMap_ST;
                float _Alpha;
                float4 _EgdeRange;
                float _Speed;
                half _Power;
            CBUFFER_END
                float4 _playPos;

           Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input); 
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                //VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.uv = input.texcoord;
                //output.normalWS = normalInput.normalWS;         
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                 output.positionOS = input.positionOS.xyz;
                return output;
            }

            float4 LitPassFragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv  = input.uv;
                float3 positionWS = input.positionWS;
                float3 playerPos = float3(_playPos.x,_playPos.y ,_playPos.z);
                float dis = abs(distance(positionWS,playerPos.xyz));
                float4 ramp = SAMPLE_TEXTURE2D(_Ramp,sampler_Ramp,half2(input.positionOS.y/_Alpha,1));
                float baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,(uv * _BaseMap_ST.xy + _BaseMap_ST.zw) -(_Speed)* _Time.y).r;
                //half MulMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv+ _Speed* _Time.y).g;
                float MulMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,(uv * _BaseMap_ST.xy + _BaseMap_ST.zw)+ _Speed* _Time.y).r;
                //baseMap.a = baseMap.r;
                baseMap.r *= 1 - smoothstep(_EgdeRange.x,_EgdeRange.y,dis);
                MulMap.r *= 1 - smoothstep(_EgdeRange.x,_EgdeRange.y,dis);
                //return half4( _BaseColor.xyz,baseMap.a *MulMap * _Alpha);
                return float4(  _BaseColor.rgb *ramp.rgb,saturate(baseMap.r*MulMap.r * (1 -input.positionOS.y+4)/_Alpha));
                //return  half4((1 -((input.positionOS.y+4))/_Alpha).xxx,1);
                //return 1;
            }   
            ENDHLSL
        }
    }
     FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}