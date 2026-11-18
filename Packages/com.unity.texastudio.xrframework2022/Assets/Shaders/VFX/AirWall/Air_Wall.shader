//Created by Wans Wu
//2024,10,12
Shader "TA Shaders/VFX/Air/Wall"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        [Main(Base,_, off, off)] _Base ("Base", Float) = 0
        [Sub(Base)][MainColor][HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        [Sub(Base)]_Alpha("Alpha", Float) = 0
        
        [Main(RampMap,_, off, off)] _RampMap ("渐变", Float) = 0
        [Space(10)]
        [Ramp(RampMap, RampMap, Assets.CommonArt.Texture, 256)] _Ramp ("渐变图", 2D) = "white" { }
        [Sub(RampMap)]_Offset("Offset", Float) = 0
        [Sub(RampMap)]_Speard("Speard", Float) = 1
        
        [Main(WireFrame, _WIREFRAME, off)] _WireFrame1 ("WireFrames", Float) = 0
        [Sub(WireFrame)]_WireframeThickness("WireframeThickness", Float) = 1.0
        [Sub(WireFrame)]_WireframeScale("WireframeScale", Float) = 0.0
        [Sub(WireFrame)][HDR]_FrameColor("发光颜色", Color) = (0,0,0,1)
        
        [Main(EdgeFade, _EDGEFADE, off)] _EdgeFade ("边缘Fade", Float) = 0
        [Sub(EdgeFade)]_EdgeFadeDistance("Distance",Float) = 3
        [Sub(EdgeFade)]_EdgeFadeOffset("Offset",Range(0,10)) = 0.1
        
        [Main(GlobalLight, _GLOBALLGIHT, off)] _GlobalLight ("GlobalLight", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull [_CullMode]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _WIREFRAME
            #pragma shader_feature_local _EDGEFADE
            #pragma shader_feature_local _GLOBALLGIHT
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "../../ShaderLibrary/StandardLighting.hlsl"
            //#include "../../ShaderLibrary/SceneChange.hlsl"
            #include "Packages/com.unity.texastudio.xrframework2022/Assets/Shaders/ShaderLibrary/StandardLighting.hlsl"
            #include "Packages/com.unity.texastudio.xrframework2022/Assets/Shaders/ShaderLibrary/SceneChange.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                #if defined (_WIREFRAME)
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
                half4 tangentWS                : TEXCOORD3;
                float3 positionOS               : TEXCOORD4;
                #if defined (_WIREFRAME)
                float4 uv2                      : TEXCOORD5;
                float4 uv3                      : TEXCOORD6;
                #endif
                float4 positionCS               : SV_POSITION;
                #if defined (_EDGEFADE)
                float4 scrPos : TEXCOORD5;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO 
            };
            TEXTURE2D(_Ramp);
            SAMPLER(sampler_Ramp);
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _Alpha;
            
            //frame
            #if defined (_WIREFRAME)
            half _RimPower;
            half _RimScale;
            half _WireframeThickness;
            half _WireframeScale;
            half4 _FrameColor;
            #endif
            half _Offset;
            half _Speard;
            half _EdgeFadeDistance;
            half _EdgeFadeOffset;
            CBUFFER_END  
            half _Light;
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
                output.positionOS = input.positionOS.xyz;
                #if defined (_WIREFRAME)
                output.uv2 = input.texcoord2;
                output.uv3 = input.texcoord3;
                #endif
                #if defined (_EDGEFADE)
                output.scrPos = ComputeScreenPos(output.positionCS);
                #endif
                return output;
            }

            half4 LitPassFragment (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 ramp = SAMPLE_TEXTURE2D(_Ramp,sampler_Ramp,half2(input.positionOS.y * _Speard+_Offset,1));
                float finalAlpha = saturate(_Alpha * ramp.r);
                //EdgeFade
                #if defined (_EDGEFADE)
                float4 ndcPos1 = input.scrPos/ input.scrPos.w;
                ndcPos1.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ndcPos1.z : ndcPos1.z * 0.5 + 0.5;
                float depth1 = LinearEyeDepth(SampleSceneDepth(ndcPos1.xy),_ZBufferParams) ;
                float edgeFadeDistance = lerp(_EdgeFadeDistance,_EdgeFadeDistance * 5,finalAlpha);
                float distanceDepth = saturate( abs( ( depth1 - LinearEyeDepth( ndcPos1.z,_ZBufferParams ) ) / ( edgeFadeDistance ) )+(1-max(0.1,_EdgeFadeOffset)));
                finalAlpha = finalAlpha * distanceDepth;
                #endif
                #if defined (_WIREFRAME)
                half3  vDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3  nDirWS = normalize(input.normalWS);
                half RimMask = 1- Fresnel(nDirWS,vDirWS,-0.1,1,5);
                half WireFrameMask = 0;
                float3 TriCenter = float3(0,0,0);
                half TriRandom = 0;
                half trisize = 0;
                ReadTriInfo(input.uv3,input.uv2,_WireframeThickness * 0.01,0,0.5,WireFrameMask,TriCenter,trisize,TriRandom);
                WireFrameMask = WireFrameMask  * RimMask;
                return half4(  _BaseColor.rgb *ramp.rgb+WireFrameMask * _FrameColor,saturate(_Alpha * ramp.r));
                #else
                return half4(  _BaseColor.rgb *ramp.rgb * (1 - _Light), finalAlpha);
                #endif
            }   
            ENDHLSL
        }
    }
     FallBack "Hidden/Universal Render Pipeline/FallbackError"
}