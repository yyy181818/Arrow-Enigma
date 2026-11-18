//Created by Wans Wu
//2024,09,03
Shader "TA Shaders/Transparent/Unlit/FresnelandMatcap"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTes("Ztest", Float) = 4.0
        _Tiliing ("全局Tiliing", vector) = (1, 1, 0, 0)
        [MainColor][HDR] _BaseColor("Base颜色", Color) = (1,1,1,1)
        _Alpha("透明度", Range(0.0, 1.0)) =1.0
        
        _FresnelScale("Scale", Range(0.0, 2.0)) = 1.0
        _FresnelPower("Power", Range(0.0, 10.0)) = 1.0
        _FresnelBias("Bias", Range(0.0, 10.0)) = 1.0
        
        [Toggle(_MATCAP)] _Matcap ("Matcap", Float) = 0
        _MatcapTex("MatcapTex", 2D) = "black" {}
        [HDR]_MatcapColor("颜色", Color) = (0,0,0,1)

        [Toggle(_WIREFRAME)]_WireFrame1 ("WireFrames", Float) = 0
        _WireframeThickness("WireframeThickness", Float) = 1.0
        _WireframeScale("WireframeScale", Float) = 0.0
        [HDR]_FrameColor("发光颜色", Color) = (0,0,0,1)
        _WireFlowSpeed("FlowSpeed", Float) = 0.1
        _WireFlowNoiseScale("NoiseScale", Float) = 50
        
    }

    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull[_CullMode]
            ZWrite On
            ZTest [_ZTes]
            
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile_fog
            #pragma shader_feature_local _MATCAP
            #pragma shader_feature_local _WIREFRAME

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
            #include "../ShaderLibrary/StandardLighting.hlsl"
            #include "../ShaderLibrary/SceneChange.hlsl"
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                 float4 tangentOS    : TANGENT;
                //#if defined (_WIREFRAME)
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                //#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
                half4 tangentWS                : TEXCOORD3;
                float4 positionOS               : TEXCOORD4;
                #if defined (_WIREFRAME)
                float4 uv2                      : TEXCOORD5;
                float4 uv3                      : TEXCOORD6;
                #endif
                float4 positionCS               : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO 
            };
            #if defined (_MATCAP)
            TEXTURE2D(_MatcapTex);
            SAMPLER(sampler_MatcapTex);
            #endif
            
            CBUFFER_START(UnityPerMaterial)
            float4 _ReflectionCubeMap_HDR;
            half4 _BaseColor;
            //half4 _AddColor;
            half4 _Tiliing;
            //half _Alpha1;
            half _Alpha;
            half4 _FresnelColor;
            half _FresnelScale;
            half _FresnelPower;
            half _FresnelBias;
            half4 _MatcapColor;
            half _WireframeThickness;
            half _WireframeScale;
            half4 _FrameColor;
            half _WireFlowSpeed;
            half _WireFlowNoiseScale;
            CBUFFER_END
            float _Light;
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                 UNITY_SETUP_INSTANCE_ID(input); 
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;  
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                output.positionOS = input.positionOS;
                #if defined (_WIREFRAME)
                output.uv2 = input.texcoord2;
                output.uv3 = input.texcoord3;
                #endif
                return output;
            }
            
            float4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3  vDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3  nDirWS = normalize(input.normalWS);
                float3 BaseColor = _BaseColor.rgb ;
                half RimMask = Fresnel(nDirWS,vDirWS,_FresnelBias,_FresnelScale,_FresnelPower);
                float csz = input.positionCS.z * input.positionCS.w; 
                real fogFactor = ComputeFogFactor(csz);
                float4 color = float4(BaseColor,saturate( _Alpha * RimMask));
                #if defined (_MATCAP)
                // MatCap
                half3 nDirVS = TransformWorldToViewDir(nDirWS);
                float2 uv_MatCap = 0.5 * nDirVS.xy + 0.5;
                half3 matCapCol = _MatcapColor * SAMPLE_TEXTURE2D(_MatcapTex,sampler_MatcapTex, uv_MatCap).rgb;
                half3 senceColor = SampleSH(nDirWS);
                color = half4((color + matCapCol)* senceColor ,saturate((  _Alpha * matCapCol.r +RimMask) ));
                #endif
                #if defined (_WIREFRAME)
                half WireFrameMask = 0;
                float3 TriCenter = float3(0,0,0);
                half TriRandom = 0;
                half trisize = 0;
                ReadTriInfo(input.uv3,input.uv2,_WireframeThickness * 0.01,0,0.5,WireFrameMask,TriCenter,trisize,TriRandom);
                WireFrameMask = WireFrameMask  * RimMask;
                half4 WireFrameColor =  WireFrameMask *_FrameColor + saturate(simple3Dnoise(TriCenter + _Time.y * _WireFlowSpeed,_WireFlowNoiseScale)) * RimMask ;
                color = float4(color.xyz + +WireFrameColor.xyz,WireFrameColor.r *_Alpha);
                #endif
                color.rgb = MixFog(color.rgb,fogFactor) * (1 - _Light);
                return color;
                //return float4(((1-(input.positionOS.xxx *7+1)) ),1);
            }
             ENDHLSL
            }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}
