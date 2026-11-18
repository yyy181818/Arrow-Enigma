//Created by Wans Wu
//2024,05,09
Shader "TA Shaders/VFX/Bubble"
{
    Properties 
    {
        [Main(Blast, _, off, off)] _Blast ("Blast", Float) = 1  
        [Sub(Blast)][HDR][MainColor] _BaseColor("Base颜色", Color) = (1,1,1,1)
        [Sub(Blast)][NoScaleOffset] _SDF("SDF", 2D) = "white" {}
        [Sub(Blast)]_DisplayFrame("PlayFrame",Float) = 0
        [Sub(Blast)]_AllFrames("AllFrames",Float) = 0
        
        [Main(Flow, _, off, off)] _Flow ("Flow", Float) = 1  
        [Sub(Flow)]_FlowMap("FlowMap", 2D) = "white" {}
        [Sub(Flow)]_FlowMapStrength("FlowMapStrength", Float) = 1
        [Sub(Flow)]_FlowMppSpeed("FlowMppSpeed", Float) = 1
        
        [Main(Normal, _, off, off)] _Normal ("Normal", Float) = 1  
        [Sub(Normal)][Normal] _NormalMap("NormalMap", 2D) = "bump" {}
        [Sub(Normal)]_NormalStrength("NormalStrength", Range(0,2)) = 1

        [Main(Spectrogram, _, off, off)] _Spectrogram ("Spectrogram", Float) = 1  
        [Sub(Spectrogram)]_spectrum("Spectrum", 2D) = "white" {}
        [Sub(Spectrogram)]_MinThickNess("MinThickNess", Range(0,1)) = 0.1
        [Sub(Spectrogram)]_MaxThickNess("MaxThickNess", Range(0,1)) = 1
        [Sub(Spectrogram)]_ThickNessWarp("ThickNessWarp", Range(0,1)) = 1
        [Sub(Spectrogram)]_SpectrogramStrength("SpectrogramStrength", Range(0,10)) = 1
        
        [Main(Transparency, _, off, off)] _Transparency ("Transparency", Float) = 1  
        [Sub(Transparency)]_FresnelPartial("FresnelPartial", Range(0,1)) = 1
        [Sub(Transparency)]_TransparencyAdjust("TransparencyAdjust", Range(-1,1)) = 1
        [Sub(Transparency)]_TransparencyScale("TransparencyScale", Range(0,5)) = 1
        [Sub(Transparency)]_CubeMap("CubeMap", CUBE) = "white" {}
    }
     SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FlowMap_ST;
                float4 _NormalMap_ST;
                float _FlowMapStrength;
                float _FlowMppSpeed;
                float _TimeSpeed;
                float _TimeOffset;
                float _DisplayFrame;
                float _NormalStrength;
                float _ThickNessWarp;
                float _MinThickNess;
                float _MaxThickNess;
                float _AllFrames;
                float _TransparencyScale;
                float _TransparencyAdjust;
                float _FresnelPartial;
                float _SpectrogramStrength;
                half4 _BaseColor;
            CBUFFER_END

            TEXTURE2D(_SDF);  
            SAMPLER(sampler_SDF);
            TEXTURE2D(_FlowMap);  
            SAMPLER(sampler_FlowMap);
            TEXTURE2D(_NormalMap); 
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_spectrum); 
            SAMPLER(sampler_spectrum);
            TEXTURECUBE(_CubeMap); 
            SAMPLER(sampler_CubeMap);

            struct Attributes 
            {
                float4 positionOS: POSITION;
                float3 normalOS: NORMAL;
                float4 tangent : TANGENT;
                float4 vertexColor: COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings 
            {
                float4 positionCS: SV_POSITION;
                float4 positionWS: TEXCOORD4;
                float2 uv: TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                half4 vertexColor: COLOR;
            };

            float Remap(float value, float inMin, float inMax, float outMin, float outMax)
            {
                return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
            }

            float Scale(float inVlue, float para)
            {
                if (para == 0)
                {
                    return inVlue;
                }
                return (1 - 1 / (1 + para * inVlue)) * (1 + 1 / para);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs posInput = GetVertexPositionInputs(v.positionOS);
                o.positionCS = posInput.positionCS;
                o.positionWS.xyz = posInput.positionWS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangent);
                o.normalWS = normalInput.normalWS;
                o.tangentWS = normalInput.tangentWS;
                o.bitangentWS = normalInput.bitangentWS;
                
                o.positionWS.w = Remap(v.positionOS.y, -0.5, 0.5, _MaxThickNess, _MinThickNess);
                o.uv = v.uv;
                o.vertexColor = v.vertexColor;
                return o;
            }

            half4 frag(Varyings i) : SV_Target 
            {
                half4 OUT;
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(i.positionWS);

                // 用 flowmap 扰动 normal，然后转到世界空间
                float2 flowMap = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, (i.uv*_FlowMap_ST.xy)+ _FlowMap_ST.zw +float2( _Time.x*_FlowMppSpeed,0)).xy * 2 - 1;
                float3 normal1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv*_NormalMap_ST.xy+_NormalMap_ST.zw + flowMap*_FlowMapStrength*0.01*_SinTime.x)).xyz;
                float3 normal2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv*_NormalMap_ST.xy+_NormalMap_ST.zw + flowMap*_FlowMapStrength*0.01*_CosTime.x)).xyz;
                float3 normal = lerp(normal1, normal2, _SinTime.y * 0.5 + 0.5);
                float3x3 WS2TS = float3x3(i.tangentWS, i.bitangentWS, i.normalWS);
                float3 normalWS = NLerp(i.normalWS, mul(normal, WS2TS), _NormalStrength);

                // 使用光的入射角和厚度采样光谱图
                float fresnel = 1 - pow(dot(viewDirWS, normalWS), 1);
                // 入射光和薄膜的夹角 0-1
                float angle = acos(fresnel) * INV_PI * 2;

                float thickNess = lerp(i.positionWS.w, i.positionWS.w * normal.x, _ThickNessWarp); // 切线空间的法线x作为厚度扰动，乘上由上到下的厚度变化
                float3 spectrogram = SAMPLE_TEXTURE2D(_spectrum, sampler_spectrum, float2(thickNess,angle)).rgb*_SpectrogramStrength;

                // specular
                float3 reflectDirWS = reflect(-viewDirWS, normalWS);
                half3 specCol1 = SAMPLE_TEXTURECUBE_LOD(_CubeMap, sampler_CubeMap, reflectDirWS, 0).rgb;
                half3 specCol2 = SAMPLE_TEXTURECUBE_LOD(_CubeMap, sampler_CubeMap, float3(reflectDirWS.xy,reflectDirWS.z), 0).rgb;
                half3 specCol = lerp(specCol1, specCol2, thickNess);
                
                float luminance = Scale(Luminance(specCol.rgb), _TransparencyAdjust) * _TransparencyScale + 0.1;
                
                float transparency = lerp(saturate(luminance*luminance*luminance),saturate(luminance*luminance*luminance)*fresnel,  _FresnelPartial);
                float3 col = transparency + spectrogram +specCol;
                
                // SDF
                half sdf = 1 - SAMPLE_TEXTURE2D(_SDF, sampler_SDF, i.uv).x;
                half sdfThreshold = _DisplayFrame / _AllFrames;
                half mask = smoothstep(sdfThreshold - 0.02, sdfThreshold + 0.02, sdf);
                return float4(col * _BaseColor.rgb,transparency*mask);
                }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}