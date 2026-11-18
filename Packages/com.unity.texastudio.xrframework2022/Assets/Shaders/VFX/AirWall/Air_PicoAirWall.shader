//Created by Wans Wu
//2024,11,07
Shader "TA Shaders/VFX/Air/PicoAirWall"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        _Tiliing ("全局Tiliing", vector) = (1, 1, 0, 0)
        [Main(PBRIntput, _, off, off)] _PBRIntput ("基础属性", Float) = 1
        [Tex(PBRIntput,_BaseColor)][MainTexture] _BaseMap("Base贴图", 2D) = "white" {}
        [HideInInspector][MainColor] _BaseColor("Base颜色", Color) = (1,1,1,1)
        [Sub(PBRIntput)] _AddColor("叠加 颜色", Color) = (0,0,0,0)
        [Sub(PBRIntput)] _Alpha("透明度", Range(0.0, 1.0)) =1.0
        [Main(Emission, _EMISSION, off)] _Emission ("发光", Float) = 0
        [Tex(Emission,_EmissionColor)]_EmissionMap("发光贴图", 2D) = "white" {}
        [HideInInspector][HDR]_EmissionColor("发光颜色", Color) = (0,0,0,1)
        [Sub(Emission)][HDR]_EmissionAddColor("Add颜色", Color) = (0,0,0,1)
        [Sub(Emission)]_EmissionStrength("发光强度", Range(0.0, 1.0)) =1.0
    }

    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull[_CullMode]
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature_local _EMISSION
            #pragma multi_compile_fog

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 texcoord     : TEXCOORD0;
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float4 positionCS               : SV_POSITION;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            //#if defined (_EMISSION)
           // TEXTURE2D(_EmissionMap);
           // SAMPLER(sampler_EmissionMap);
            //#endif
            CBUFFER_START(UnityPerMaterial)
            float4 _ReflectionCubeMap_HDR;
            half4 _BaseColor;
            half4 _AddColor;
            // half4 _EmissionColor;
            // half4 _EmissionAddColor;
            // half _EmissionStrength;
            half4 _Tiliing;
            half _Alpha;
            CBUFFER_END
            float _Light;
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.uv = input.texcoord;
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                return output;
            }
            
            float4 LitPassFragment(Varyings input) : SV_Target
            {
                float2 uv  = input.uv *  _Tiliing.xy + _Tiliing.zw;
                float4 BaseColorAlpha = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv) ;
                float3 BaseColor = BaseColorAlpha.rgb *_BaseColor.rgb  *BaseColorAlpha.a+ _AddColor.rgb;
                  float4 EmissonMap = 0;
                // #if defined (_EMISSION)
                //    EmissonMap = (SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,uv)* _EmissionColor  + _EmissionAddColor)* _EmissionStrength;
                // #else
                //    EmissonMap = half4(0,0,0,0);
                // #endif
                //float4 EmissonColor = EmissonMap;
                float csz = input.positionCS.z * input.positionCS.w; 
                real fogFactor = ComputeFogFactor(csz);
                float4 color = half4(BaseColor ,BaseColorAlpha.a * _Alpha * saturate(1- input.positionWS.y + 0.3));
                color.rgb = MixFog(color.rgb,fogFactor) * (1 - _Light);
                return color;
            }
             ENDHLSL
            }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"
}
