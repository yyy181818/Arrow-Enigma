Shader "TA Shaders/UI/UI(Default)"
{
    Properties
    {
        //[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        //[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 10
        //[HideInInspector][Enum(Off, 0, On, 1)]_ZWriteMode ("ZWrite", float) = 0
        //[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTest", Float) = 8
        
        
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Space(10)]
        [Toggle(_BLUR_ON)] _BlurOn ("Blur On", Float) = 0
        _BlurSize ("Blur Size", Range(0, 8)) = 0

        [Space(10)]
        [Toggle(UI_CLIP_RECT)] _UIClipRect("Use Clip Rect", Float) = 0
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0


        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
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
            float4 _Color;
            float4 _MainTex_ST;
            float _BlurSize;
            float4 _MainTex_TexelSize;
        CBUFFER_END

        float4 _TextureSampleAdd;
        float4 _ClipRect;

        inline float UnityGet2DClipping(in float2 position, in float4 clipRect)
        {
            float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
            return inside.x * inside.y;
        }
        ENDHLSL
        Pass
        {
            Cull Off
            //Blend [_SrcBlend] [_DstBlend]
            Blend One OneMinusSrcAlpha
            //Blend SrcAlpha OneMinusSrcAlpha,One Zero
            ColorMask [_ColorMask]
            Offset -1, -1
            ZWrite Off
            //ZTest [_ZTestMode]
            ZTest [unity_GUIZTestMode]
            Lighting Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature _BLUR_ON
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 texcoord : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float3 worldPosition : TEXCOORD1;
                //half4 mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = TransformObjectToWorld(v.vertex.xyz);
                o.positionCS = TransformWorldToHClip(o.worldPosition);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.color = v.color * _Color;

                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0 / alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision) * invAlphaPrecision;


                #if defined(_BLUR_ON)
                float blurSizeX = _BlurSize * _MainTex_TexelSize.x;
                float blurSizeY = _BlurSize * _MainTex_TexelSize.y;

                half4 color = half4(0, 0, 0, 0);
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord + float2(x * blurSizeX, y * blurSizeY)) / 25 + _TextureSampleAdd *IN.color;
                    }
                }
                #else
                half4 color = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                #endif

                #ifdef UI_CLIP_RECT
					color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
					clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
            ENDHLSL
        }
    }
}