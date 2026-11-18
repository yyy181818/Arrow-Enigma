Shader "TA Shaders/Toon/Cloud"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Space(20)]
        [Header(Color)]
        [Space(5)]
        [Toggle()]_Is_LightColor_BaseColor ("Is LightColor BaseColor", Float) = 0
        [Toggle()]_Is_LightColor_1st ("Is LightColor 1st Color", Float) = 0
        [Toggle()]_Is_LightColor_2nd ("Is LightColor 2nd Color", Float) = 0
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _1st_Color ("1st Color", Color) = (1,1,1,1)
        _2nd_Color ("2nd Color", Color) = (1,1,1,1)
        _Set_1st_ShadePosition_var ("Set 1st ShadePosition", Range(0, 1)) = 0.5
        _Set_2nd_ShadePosition_var ("Set 2nd ShadePosition", Range(0, 1)) = 0.5
        _BaseColor_Step ("BaseColor_Step", Range(0, 1)) = 0.5
        _BaseShade_Feather ("Base/Shade_Feather", Range(0.0001, 1)) = 0.0001
        _ShadeColor_Step ("ShadeColor_Step", Range(0, 1)) = 0
        _1st2nd_Shades_Feather ("1st/2nd_Shades_Feather", Range(0.0001, 1)) = 0.0001
        _ColorIntensity ("Color Intensity", float) = 1
        [Toggle]_Set_SystemShadowsToBase ("Set SystemShadows To Base", float) = 0
        _ShadowsLevel ("Shadows Level", Range(-0.5,0.5)) = 0

        [Space(20)]
        [Header(Back SSS)]
        [Space(5)]
        _SSSColor ("SSS Color", Color) = (1,1,1,1)
        _BackSssStrength("Back Sss Strength",float) = 0.5
        _Rate("Rate",Range(-2 ,2)) = 0.5

        [Space(20)]
        [Header(Transparency)]
        [Space(5)]
        _EdgeNoiseMap("Edge Noise Map",2D) = "white" {}
        _EdgeStrength("Edge Strength",float) = 2
        _EdgeSmoothMin("Edge Smooth Min",float) = 0.5
        _EdgeSmoothMax("Edge Smooth Max",float) = 1
        _FresnelPower("Fresnel Power",Range(0,10)) = 5
        _AdditionalPower("Addtional Power",Range(0,10)) = 1
        _SmoothFresnelMin("Smooth Fresnel Min",float) = 0.5
        _SmoothFresnelMax("Smooth Fresnel Max",float) = 1
        [HideInInspector]_Fade("Fade",Range(0,1)) = 0.5

        [Space(20)]
        [Header(Color Adjustments)]
        [Space(5)]
        [Toggle(_ENALBE_COLOR_ADJUSTMENTS)]_ENABLE_COLOR_ADJUSTMENTS("Enable Color Adjustments",Float) = 0
        _Brightness("Brightness",Range(0,16)) = 1
        _Contrast("Contrast",float) = 1
        _Saturation("Saturation",float) = 1
        [Space(10)]
        [Header(Hue)]
        [Space(5)]
        [Toggle]_EnableHue("Enable Hue",Float) = 0
        _Hue("Hue",Range(0,360)) = 0
        [Space(10)]
        [Header(ACES Tone Mapping)]
        [Space(5)]
        [Toggle]_EnableACESToneMapping("Enable ACES Tone Mapping",Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector"="True"
        }
        LOD 100
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _1st_Color;
            half4 _2nd_Color;
            half _ColorIntensity;
            half _Is_LightColor_BaseColor;
            half _Is_LightColor_1st;
            half _Is_LightColor_2nd;
            half _BaseColor_Step;
            half _BaseShade_Feather;
            half _ShadeColor_Step;
            half _1st2nd_Shades_Feather;
            half _Set_1st_ShadePosition_var;
            half _Set_2nd_ShadePosition_var;

            half _Brightness;
            half _Contrast;
            half _Saturation;
            half _EnableHue;
            half _Hue;
            half _EnableACESToneMapping;


            half _Set_SystemShadowsToBase;
            half _ShadowsLevel;

            half4 _SSSColor;
            half _BackSssStrength;
            half _Rate;

            float4 _EdgeNoiseMap_ST;
            half _EdgeSmoothMin;
            half _EdgeSmoothMax;
            half _EdgeStrength;
            half _FresnelPower;
            half _AdditionalPower;
            half _SmoothFresnelMin;
            half _SmoothFresnelMax;

            half _Fade;
        CBUFFER_END

        TEXTURE2D(_EdgeNoiseMap);
        SAMPLER(sampler_EdgeNoiseMap);

        half3 ApplyHue(float3 aColor, float aHue)
        {
            float angle = radians(aHue);
            float3 k = float3(0.57735, 0.57735, 0.57735);
            float sinAngle, cosAngle;
            sincos(angle, sinAngle, cosAngle);

            return aColor * cosAngle + cross(k, aColor) * sinAngle + k * dot(k, aColor) * (1 - cosAngle);
        }

        half3 ACESToneMapping(half3 color, float adapted_lum)
        {
            const float A = 2.51f;
            const float B = 0.03f;
            const float C = 2.43f;
            const float D = 0.59f;
            const float E = 0.14f;

            color *= adapted_lum;
            return saturate((color * (A * color + B)) / (color * (C * color + D) + E));
        }
        ENDHLSL
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_CullMode]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma shader_feature _ENALBE_COLOR_ADJUSTMENTS
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 viewDirWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };


            Varyings vert(Attributes IN)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                output.uv = IN.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight(input.shadowCoord);
                half3 L = normalize(mainLight.direction);
                half3 lightColor = mainLight.color;
                float shadowAttenuation = 1;
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                shadowAttenuation = mainLight.shadowAttenuation;
                # endif
                half3 N = normalize(input.normalWS);
                half3 V = normalize(input.viewDirWS);
                half NoL = dot(N, L) * 0.5 + 0.5;
                half smoothNoL = saturate(pow(NoL, 2 - _Rate));
                half NoV = saturate(dot(V, N));
                half smoothNoV = saturate(pow(NoV, 2 - _Rate));

                half3 backLitDir = N * _BackSssStrength + L;
                half backSSS = saturate(dot(V, -backLitDir));
                backSSS = saturate(pow(backSSS, 2 + _Rate * 2) * 1.5);
                half shadow = saturate(lerp(shadowAttenuation, 1, (distance(input.positionWS, _WorldSpaceCameraPos.xyz) - 100) * 0.1));
                half finalLit = saturate(smoothNoV * 0.5 + shadow * saturate(smoothNoL + backSSS) * (1 - NoV * 0.5));

                half3 SSS = saturate(pow(finalLit, 2 + _Rate * 2) * 1.5) * _SSSColor.rgb;

                float3 Set_LightColor = lightColor.rgb;

                float3 Set_BaseColor = lerp(_BaseColor.rgb, _BaseColor.rgb * Set_LightColor, _Is_LightColor_BaseColor) * _ColorIntensity;

                float3 Set_1st_ShadeColor = lerp(_1st_Color.rgb, _1st_Color.rgb * Set_LightColor, _Is_LightColor_1st);
                float3 Set_2nd_ShadeColor = lerp(_2nd_Color.rgb, _2nd_Color.rgb * Set_LightColor,_Is_LightColor_2nd);
                float _SystemShadowsLevel_var = shadowAttenuation * 0.5 + 0.5 + _ShadowsLevel > 0.001? shadowAttenuation * 0.5 + 0.5 + _ShadowsLevel: 0.0001;
                float Set_FinalShadowMask = saturate(1.0 + (lerp(NoL, NoL * saturate(_SystemShadowsLevel_var),_Set_SystemShadowsToBase) - (_BaseColor_Step - _BaseShade_Feather)) * ((1.0- _Set_1st_ShadePosition_var).r - 1.0)) / (_BaseColor_Step - (_BaseColor_Step - _BaseShade_Feather));
                float3 Set_FinalBaseColor = lerp(Set_BaseColor,lerp(Set_1st_ShadeColor, Set_2nd_ShadeColor,saturate(1.0 + (NoL - (_ShadeColor_Step -_1st2nd_Shades_Feather)) * ((1.0 -_Set_2nd_ShadePosition_var).r - 1.0)/ (_ShadeColor_Step - (_ShadeColor_Step -_1st2nd_Shades_Feather)))),Set_FinalShadowMask); // Final Color

                Set_FinalBaseColor *= lightColor + SSS;

                #if defined(_ENALBE_COLOR_ADJUSTMENTS)
                Set_FinalBaseColor = _EnableHue ? ApplyHue(Set_FinalBaseColor, _Hue) : Set_FinalBaseColor;
                Set_FinalBaseColor *= _Brightness;
                half gray = 0.2125 * Set_FinalBaseColor.r + 0.7154 * Set_FinalBaseColor.g + 0.0721 * Set_FinalBaseColor.
                    b;
                half3 grayColor = half3(gray, gray, gray);
                Set_FinalBaseColor = lerp(grayColor, Set_FinalBaseColor, _Saturation);
                half3 avgColor = half3(0.5, 0.5, 0.5);
                Set_FinalBaseColor = lerp(avgColor, Set_FinalBaseColor, _Contrast);
                #endif

                Set_FinalBaseColor = _EnableACESToneMapping? ACESToneMapping(Set_FinalBaseColor, 1): Set_FinalBaseColor;
                half edgeNoise = SAMPLE_TEXTURE2D(_EdgeNoiseMap, sampler_EdgeNoiseMap,TRANSFORM_TEX(input.uv, _EdgeNoiseMap)).r * _EdgeStrength;
                half fresnel = 1 - pow(1.0 - NoV, _FresnelPower);
                fresnel = pow(fresnel, _AdditionalPower);
                half smoothFresnel = saturate(smoothstep(_SmoothFresnelMin, _SmoothFresnelMax, fresnel));
                half edge = 1 - saturate(smoothstep(_EdgeSmoothMin, _EdgeSmoothMax, 1 - smoothFresnel) * edgeNoise);
                half alpha = smoothFresnel * edge;
                //half tmp = step(_Fade, NoV);
                //col.a = col.a * tmp + (1 - tmp) * lerp(0, col.a, max(0, (NoV - 0.1) / (_Fade - 0.1)));
                //col.a *= smoothFresnel;
                return half4(Set_FinalBaseColor, alpha);
            }
            ENDHLSL
        }
    }
}