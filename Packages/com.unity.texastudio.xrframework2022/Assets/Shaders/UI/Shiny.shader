Shader "TA Shaders/UI/Shiny"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)


        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _ShinyColor ("Shiny Color", Color) = (1,1,1,1)
        _ShinyWidth ("Shiny Width", Range(0.01, 1)) = 0.2
        _ShinyAngle ("Shiny Angle", Range(0,180)) = 45
        _ShinyStrength ("Shiny Strength", Range(0,2)) = 1
        _ShinyOuterStrength ("Shiny Outer Strength", Range(0,2)) = 0.3
        _Softness ("Softness", Range(0.5, 5)) = 1.5
        _CoreWidth ("Core Width", Range(0.1, 1)) = 0.5
        [Toggle] _Reverse ("Reverse Direction", Float) = 0

        _LoopTime ("Loop Time", Float) = 1
        _Interval ("Interval", Float) = 3
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            fixed4 _ShinyColor;
            float _ShinyWidth;
            float _ShinyAngle;
            float _ShinyStrength;
            float _ShinyOuterStrength;
            float _Softness;
            float _CoreWidth;
            float _LoopTime;
            float _Interval;
            half _Reverse;

            float inFlash(half2 uv)
            {
                float brightness = 0;

                float angleInRad = 0.0174444 * _ShinyAngle;
                float tanInverseInRad = 1.0 / tan(angleInRad);

                //            float currentTime = _Time.y - _BeginTime;  
                float currentTime = _Time.y;
                float totalTime = _Interval + _LoopTime;
                float currentTurnStartTime = (int)((currentTime / totalTime)) * totalTime;
                float currentTurnTimePassed = currentTime - currentTurnStartTime - _Interval;

                float dir = (_Reverse > 0.5) ? -1.0 : 1.0;
                float k = tanInverseInRad * dir;

                bool onLeft = (k > 0);
                float xBottomFarLeft = onLeft ? 0.0 : k;
                float xBottomFarRight = onLeft ? (1.0 + k) : 1.0;

                float percent = currentTurnTimePassed / _LoopTime;
                float xBottomRightBound = xBottomFarLeft + percent * (xBottomFarRight - xBottomFarLeft);
                float xBottomLeftBound = xBottomRightBound - _ShinyWidth;

                float xProj = uv.x + uv.y * k;

                float center = (xBottomLeftBound + xBottomRightBound) * 0.5;
                float dist = abs(xProj - center);
                
                float halfWidth = _ShinyWidth * 0.5;
                
                float sigma1 = halfWidth * _Softness;
                float glowOuter = exp(-dist * dist / (sigma1 * sigma1));
                
                float sigma2 = sigma1 * _CoreWidth;
                float glowCore = exp(-dist * dist / (sigma2 * sigma2));
                
                brightness = glowOuter * _ShinyOuterStrength + glowCore;

                return brightness;
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
                                 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(
                                     pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0 / alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision) * invAlphaPrecision;

                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                float brightness = inFlash(IN.texcoord);
                color.rgb += _ShinyColor.rgb * brightness * _ShinyStrength;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}