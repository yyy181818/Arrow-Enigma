Shader "TA Shaders/UI/UI(Outline)"
{
    Properties
    {
        [Enum(Off, 0, On, 1)]_ZWriteMode ("ZWrite", float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTest", Float) = 8
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", float) = 0.01

        _ShadowOutlineColor ("Shadow Outline Color", Color) = (0,0,0,1)
        _ShadowOutlineWidth ("Shadow Outline Width", float) = 0.01

        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
        [Toggle(UI_CLIP_RECT)] _UIClipRect("Use Clip Rect", Float) = 0
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _ShadowOutlineColor;
            float _ShadowOutlineWidth;
            half4 _OutlineColor;
            float _OutlineWidth;
            float4 _MainTex_TexelSize;
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            half4 _Color;
            float4 _MainTex_ST;
        CBUFFER_END

        inline float UnityGet2DClipping(in float2 position, in float4 clipRect)
        {
            float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
            return inside.x * inside.y;
        }
        ENDHLSL
        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]
            Offset -1, -1
            ZWrite Off
            ZTest [unity_GUIZTestMode]
            Lighting Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                half4 color : COLOR;
                float3 worldPosition : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half IsInRect(float2 pPos, float2 pClipRectMin, float2 pClipRectMax)
            {
                pPos = step(pClipRectMin, pPos) * step(pPos, pClipRectMax);
                return pPos.x * pPos.y;
            }

            half SampleAlpha(int pIndex, Varyings IN, float width, half4 outlineColor)
            {
                const half sinArray[12] = {0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5};
                const half cosArray[12] = {1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866};
                float2 pos = IN.texcoord + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * width;
                return IsInRect(pos, IN.uv1, IN.uv2) * (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pos) +
                    _TextureSampleAdd).a * outlineColor.a;
            }

            Varyings vert(Attributes IN)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = IN.vertex;
                o.vertex = TransformObjectToHClip(IN.vertex);
                o.normal = IN.normal;
                o.tangent = IN.tangent;
                o.texcoord = TRANSFORM_TEX(IN.texcoord.xy, _MainTex);
                o.uv1 = IN.uv1;
                o.uv2 = IN.uv2;
                o.uv3 = IN.uv3;
                o.color = IN.color * _Color;

                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;


                if (IN.uv3.x != -1 && _OutlineWidth > 0 || (IN.uv3.x == -1 && _ShadowOutlineWidth > 0))
                {
                    color.a *= IsInRect(IN.texcoord, IN.uv1, IN.uv2);
                    half4 outlineColor = IN.uv3.x == -1 ? _ShadowOutlineColor : _OutlineColor;

                    half4 val = half4(outlineColor.rgb, 0);

                    float outlineWidth = IN.uv3.x == -1 ? _ShadowOutlineWidth : _OutlineWidth;
                    for (int i = 0; i < 12; i++)
                    {
                        val.a += SampleAlpha(i, IN, outlineWidth, outlineColor);
                    }
                    color = val * (1 - color.a) + color * color.a;
                    color.a = saturate(color.a);
                    color.a *= IN.color.a;
                }

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDHLSL
        }
    }
}