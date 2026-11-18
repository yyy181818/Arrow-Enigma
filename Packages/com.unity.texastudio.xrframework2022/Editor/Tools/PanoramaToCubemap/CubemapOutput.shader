Shader "Hidden/CubeMap/CubemapOutput"
{
    Properties
    {
        _BaseMap ("Cube Map", Cube) = "white" {}
        _index ("Face Index", Int) = 0
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
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        CBUFFER_START(UnityPerMaterial)
            int _index;
        CBUFFER_END

        TEXTURECUBE(_BaseMap);
        SAMPLER(sampler_BaseMap);

        #define _PI    3.141592653589793
        #define TWOPI 6.283185307179587

        // 顶点位置转换到裁剪空间位置
        // 只考虑xy,通过第一行代码得到，
        // 索引0, x: 0<<1=00, 00&10=00 y: 00&10=0，所以0号位对应坐标为0,0
        // 索引1, x: 1<<1=10, 10&10=10 y: 01&10=0, 所以1号位对应坐标为2,0
        // 索引2, x: 2<<1=100, 100&010=000 y:10&10=10,所以2号位对应坐标为0,2
        // *2-1映射后，顶点的坐标为
        // v0:-1，-1
        // v1:3，-1
        // v2:-1，3
        // float4 GetFullScreenTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
        // {
        //     float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
        //     return float4(uv * 2.0 - 1.0, z, 1.0);
        // }
        inline half3 LinearToGammaSpace(half3 linRGB)
        {
            linRGB = max(linRGB, half3(0.h, 0.h, 0.h));
            // An almost-perfect approximation from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
            return max(1.055h * pow(linRGB, 0.416666667h) - 0.055h, 0.h);

            // Exact version, useful for debugging.
            //return half3(LinearToGammaSpaceExact(linRGB.r), LinearToGammaSpaceExact(linRGB.g), LinearToGammaSpaceExact(linRGB.b))
        }
        ENDHLSL
        Pass
        {
            Cull Off
            Blend Off
            ZWrite Off
            ZTest Always
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma shader_feature _REPAIR_ON
            struct Attributes
            {
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                #if defined(_REPAIR_ON)
                IN.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
                #else
                //IN.uv = (output.positionCS.xy / output.positionCS.w + 1) * 0.5 + 0.5;
                IN.uv = output.positionCS.xy / output.positionCS.w * 0.5 + 0.5;
                #ifdef UNITY_UV_STARTS_AT_TOP
                     IN.uv.y = 1 - IN.uv.y;
                #endif
                #endif
                output.uv = IN.uv * float2(TWOPI, _PI);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float theta = input.uv.y;
                float phi = input.uv.x;
                float3 unit = float3(0, 0, 0);

                unit.x = sin(phi) * sin(theta) * -1;
                unit.y = cos(theta) * -1;
                unit.z = cos(phi) * sin(theta) * -1;
                half4 color = SAMPLE_TEXTURECUBE(_BaseMap, sampler_BaseMap, unit);
                //color.rgb = LinearToGammaSpace(color.rgb);
                color = Gamma20ToLinear(color);
                return color;
            }
            ENDHLSL
        }
    }
}