// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "TA Shaders/Skybox/Cubemap"
{
    Properties
    {
        [Toggle] _UseTint ("Use Tint", Float) = 0
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _SkyColor ("Sky Color",Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
        [Toggle(_GLOBAL_LIGHT)] _UseGlobalLight ("Use Global Light", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull Off ZWrite Off

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma shader_feature_local_fragment _GLOBAL_LIGHT 
            
            #include "UnityCG.cginc"
            
            samplerCUBE _Tex;
            half4 _Tex_HDR;
            half4 _Tint;
            half4 _SkyColor;
            half _UseTint;
            half _Exposure;
            float _Rotation;
            half _UseGlobalLight;
            half _Light;

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 tex = texCUBE(_Tex, i.texcoord);
                half3 c = DecodeHDR(tex, _Tex_HDR);
                half3 baseColor = _UseTint ? _Tint.rgb : _SkyColor.rgb;
                c = c * baseColor.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                #if defined(_GLOBAL_LIGHT)
                c = c * (1 - _Light);
                #endif
                return half4(c, 1);
            }
            ENDCG
        }
    }


    Fallback Off

}