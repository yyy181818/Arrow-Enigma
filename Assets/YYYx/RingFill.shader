Shader "Custom/RingFill"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0,1)) = 0
        _StartAngle ("Start Angle", Range(0,360)) = 90
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define TWO_PI 6.28318530718

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Progress;
                float _StartAngle;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.positionOS = input.positionOS.xyz;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float angle = atan2(i.positionOS.y, i.positionOS.x);
                angle = angle < 0 ? angle + TWO_PI : angle;

                float start = _StartAngle * TWO_PI / 360.0;
                float d = start - angle;
                d = fmod(d, TWO_PI);
                if (d < 0) d += TWO_PI;

                float t = d / TWO_PI;
                if (t > _Progress) discard;
                return _Color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
