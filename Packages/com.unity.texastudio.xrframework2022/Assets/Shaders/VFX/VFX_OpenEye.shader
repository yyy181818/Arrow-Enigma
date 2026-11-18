Shader "TA Shaders/VFX/EyeOpen"
{
    Properties
    {
        [Main(PBRIntput, _, off, off)] _PBRIntput ("基础属性", Float) = 1
        [Sub(PBRIntput)]_BaseMap("Base Map", 2D) = "white" {}
        [Sub(PBRIntput)]_BaseColor("Base Color", Color) = (1,1,1,1)
        [Sub(PBRIntput)]_Alpha("Alpha", Range( 0 , 1)) = 0
        
        [Sub(PBRIntput)]_Progress("_Progress", Range( 0 , 1)) = 0
        [Sub(PBRIntput)]_ArchHeight("_ArchHeight", Range( 0 , 1)) = 0
        //[Sub(PBRIntput)]_Intensity1("_Intensity", Range( 0 , 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
           Tags{"LightMode" = "UniversalForward"}
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
           HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
           struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
                half4 tangentWS                : TEXCOORD3;
                float3 positionOS               : TEXCOORD4;
                float4 positionCS               : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO 
            };
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
           
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Alpha;
            half _Progress;
            half _ArchHeight;
            half _Intensity1;
            CBUFFER_END  
            

           Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input); 
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.uv = input.texcoord;
                output.normalWS = normalInput.normalWS;         
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.positionOS = input.positionOS;
                return output;
            }

            half4 LitPassFragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half2 uv  = input.uv;
                 //float3 positionWS = input.positionWS;
                // float3 postionOS = input.positionOS;
                //  half3 vDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
                //  half3 nDirWS = normalize(input.normalWS);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv);
                half3 baseColor = baseMap.rgb;

                 // 上眼皮与下眼皮边界
                float upBorder = 0.5 + _Progress * (0.5 + _ArchHeight);
                float downBorder = 0.5 - _Progress * (0.5 + _ArchHeight);
                upBorder -= _ArchHeight * pow(uv.x - 0.5, 2);
                downBorder += _ArchHeight * pow(uv.x - 0.5, 2);
                
                // 可视区域
                float visableUp = distance(upBorder,uv.y)*(1 - step(upBorder, uv.y));
                float visableUpDown = distance(downBorder,uv.y) * (step(downBorder, uv.y));
                float visibleV = visableUp * visableUpDown;
                //float visibleV = smoothstep((1 - step(upBorder, uv.y)),-uv.y,_Intensity1) * smoothstep((step(downBorder, uv.y)),-uv.y,_Intensity1);
                //float visibleV = (1 - smoothstep(0,upBorder, uv.y)) * (smoothstep(0,downBorder, uv.y));
                half alpha = baseMap.a * (1-visibleV );
                half3 finalColor = baseColor * _BaseColor * visibleV ;
                half finalAlpha = saturate(alpha * _Alpha);
                return half4(finalColor,finalAlpha);
                //return half4(distance(upBorder,uv.y)*(1 - step(upBorder, uv.y)) .xxx,1);
            }   
            ENDHLSL
        }
    }
     FallBack "Hidden/Universal Render Pipeline/FallbackError"
     CustomEditor "LWGUI.LWGUI"
}