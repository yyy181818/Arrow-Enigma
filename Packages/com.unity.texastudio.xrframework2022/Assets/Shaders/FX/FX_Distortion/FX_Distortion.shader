Shader "TA Shaders/FX/FX_Distortion"
{
    Properties
    {
        [_MainTexture]_MainTex ("Base Map", 2D) = "white" {}
        _NoiseMap ("Noise Texture", 2D) = "white" {}
        _Distortion ("Distortion", Range(0, 1)) = 0.5
        _DistortionSpeed ("Distortion Speed", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        half _DistortionSpeed;
        half _Distortion;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D(_NoiseMap);
        SAMPLER(sampler_NoiseMap);
        ENDHLSL
        Pass
        {
            Cull Off
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half2 disUV = i.uv + frac(_Time.y * _DistortionSpeed);
                half4 noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, disUV);
                half2 dis = noise.xy * _Distortion;
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv += dis);
                return col;
            }
            ENDHLSL
        }
    }
}