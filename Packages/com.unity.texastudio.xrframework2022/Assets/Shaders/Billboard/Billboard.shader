Shader "TA Shaders/Billboard"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("ZTest", Int) = 4
        [Toggle] _AlphaTest ("Alpha Clip", Float) = 0
        _Cutoff ("AlphaClip Threshold", Range(0.0, 1.0)) = 0.5
        [Space(8)]
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        [Toggle]_VerticalBillboard("是否在垂直方向旋转", float) = 1
        
        _Shrink ("Expand X", float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
            "IgnoreProjector" = "True"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _VerticalBillboard;
            float _Shrink;
            float _Cutoff;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        ENDHLSL
        Pass
        {
            Name "Billboard"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Off
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            //只有Mesh平面在XY空间才能生效
            Varyings vert(Attributes input)
            {
                Varyings output;
                ZERO_INITIALIZE(Varyings,output)
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 centerWS = unity_ObjectToWorld._m03_m13_m23;
                // 相机方向（世界空间，不再转物体空间）
                float3 normalDir = _WorldSpaceCameraPos - centerWS;
                normalDir.y = normalDir.y * _VerticalBillboard;
                normalDir = normalize(normalDir);

                float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 rightDir = normalize(cross(upDir, normalDir));
                upDir = normalize(cross(normalDir, rightDir));

                // 只取物体缩放（忽略旋转），保持原本 _Shrink 只影响 X 的语义
                float sx = length(unity_ObjectToWorld._m00_m10_m20);
                float sy = length(unity_ObjectToWorld._m01_m11_m21);
                float offsetX = input.positionOS.x * sx * _Shrink;
                float offsetY = input.positionOS.y * sy;

                float3 worldPos = centerWS + rightDir * offsetX + upDir * offsetY;
                output.positionCS = TransformWorldToHClip(worldPos);

                output.uv = input.uv;
                output.uv.x = (output.uv.x - 0.5f) * _Shrink + 0.5f;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                #if defined(_ALPHATEST_ON)
                clip(col.a -_Cutoff );
                #endif
                
                return half4(col.rgb,1);
            }
            ENDHLSL
        }
    }
}