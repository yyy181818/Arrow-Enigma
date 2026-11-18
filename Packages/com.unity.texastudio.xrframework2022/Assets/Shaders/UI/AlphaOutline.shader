Shader "TA Shaders/UI/AlphaOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // 边框控制
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        _OutlineSoftness ("Outline Softness", Range(0, 1)) = 0.5
        
        [KeywordEnum(Sobel, Fwidth)] _EdgeMethod ("Edge Detection Method", Float) = 0
        
        [Toggle(ENABLE_SWEEP)] _EnableSweep ("Enable Radial Sweep", Float) = 0
        _SweepCount ("Sweep Count", int) = 2
        _SweepParams ("Sweep Params (Smooth x-y, Speed z-w)", Vector) = (0.5,1,1,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "AlphaOutline"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ ENABLE_SWEEP
            #pragma multi_compile_local _ _EDGEMETHOD_SOBEL _EDGEMETHOD_FWIDTH

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4 mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            // 边框参数
            half4 _OutlineColor;
            half _OutlineWidth;
            half _OutlineSoftness;

            // 扫描参数
            int _SweepCount;
            float4 _SweepParams;

            float2 VectorToRadialValue(float2 uv)
            {
                float2 p = uv * 2.0 - 1.0;
                float r = length(p);
                float angle = atan2(p.y, p.x);
                float normalizedAngle = angle / (2.0 * UNITY_PI) + 0.5;
                return float2(normalizedAngle, r);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw,
                                 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Sobel 边缘检测
            half EdgeSobel(v2f IN, half currentAlpha)
            {
                float2 texelSize = _MainTex_TexelSize.xy;

                half alphaL = tex2D(_MainTex, IN.texcoord + float2(-texelSize.x, 0)).a;
                half alphaR = tex2D(_MainTex, IN.texcoord + float2(texelSize.x, 0)).a;
                half alphaD = tex2D(_MainTex, IN.texcoord + float2(0, -texelSize.y)).a;
                half alphaU = tex2D(_MainTex, IN.texcoord + float2(0, texelSize.y)).a;
                half alphaUL = tex2D(_MainTex, IN.texcoord + float2(-texelSize.x, texelSize.y)).a;
                half alphaUR = tex2D(_MainTex, IN.texcoord + float2(texelSize.x, texelSize.y)).a;
                half alphaDL = tex2D(_MainTex, IN.texcoord + float2(-texelSize.x, -texelSize.y)).a;
                half alphaDR = tex2D(_MainTex, IN.texcoord + float2(texelSize.x, -texelSize.y)).a;

                half gradX = (alphaUL + 2 * alphaL + alphaDL) - (alphaUR + 2 * alphaR + alphaDR);
                half gradY = (alphaUL + 2 * alphaU + alphaUR) - (alphaDL + 2 * alphaD + alphaDR);

                half edge = sqrt(gradX * gradX + gradY * gradY);
                edge *= currentAlpha;
                return edge;
            }

            // Fwidth 边缘检测
            half EdgeFwidth(v2f IN, half currentAlpha)
            {
                half edge = fwidth(currentAlpha);
                return edge;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0 / alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision) * invAlphaPrecision;

                // 采样原图
                half4 texColor = tex2D(_MainTex, IN.texcoord);
                half4 color = IN.color * texColor;

                // ========== 边缘检测 ==========
                half currentAlpha = texColor.a;
                half edge = 0;

                #if defined(_EDGEMETHOD_SOBEL)
                edge = EdgeSobel(IN, currentAlpha);
                // 边缘 mask
                half isEdge = smoothstep(0.5 - _OutlineSoftness * 0.5, 0.5 + _OutlineSoftness * 0.5, edge);
                half edgeWidth = _OutlineWidth * 50;
                half edgeMask = saturate(isEdge * edgeWidth);
                #elif defined(_EDGEMETHOD_FWIDTH)
                edge = EdgeFwidth(IN, currentAlpha);
                half edgeMask = smoothstep(0, _OutlineWidth * 10, edge);
                #else
                edge = EdgeSobel(IN, currentAlpha);
                half isEdge = smoothstep(0.5 - _OutlineSoftness * 0.5, 0.5 + _OutlineSoftness * 0.5, edge);
                half edgeWidth = _OutlineWidth * 50;
                half edgeMask = saturate(isEdge * edgeWidth);
                #endif

                
                #ifdef ENABLE_SWEEP
                float2 offset = _Time.y * _SweepParams.zw;
                float2 radialUV = VectorToRadialValue(IN.texcoord) + offset;
                float sweepValue = 1 - frac(radialUV * _SweepCount);
                float sweepMask = smoothstep(_SweepParams.x, _SweepParams.y, sweepValue);
                edgeMask *= sweepMask;
                #endif
                
                half4 outline = _OutlineColor * edgeMask;
                color.rgb = lerp(color.rgb, outline.rgb, outline.a * edgeMask);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return half4(color);
            }
            ENDCG
        }
    }
}