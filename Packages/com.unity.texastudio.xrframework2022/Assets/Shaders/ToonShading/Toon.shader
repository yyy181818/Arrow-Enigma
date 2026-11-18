Shader "TA Shaders/Toon/Toon_Lit"
{
    Properties
    {
        [Header(Base)]
        [Space(5)]
        [MainTexture]_BaseMap ("BaseMap", 2D) = "white" {}
        [Gradient]_GradientMap ("GradientMap", 2D) = "white" {}
        [Toggle()]_Is_LightColor_BaseColor ("Is LightColor BaseColor", Float) = 0
        [Toggle()]_Is_LightColor_1st ("Is LightColor 1st Color", Float) = 0
        [Toggle()]_Is_LightColor_2nd ("Is LightColor 2nd Color", Float) = 0
        _BaseColor ("Color", Color) = (1,1,1,1)
        _1st_Color ("1st Color", Color) = (1,1,1,1)
        _2nd_Color ("2nd Color", Color) = (1,1,1,1)
        _Set_1st_ShadePosition_var ("Set 1st ShadePosition", Range(0, 1)) = 0.5
        _Set_2nd_ShadePosition_var ("Set 2nd ShadePosition", Range(0, 1)) = 0.5
        _BaseColor_Step ("BaseColor_Step", Range(0, 1)) = 0.5
        _BaseShade_Feather ("Base/Shade_Feather", Range(0.0001, 1)) = 0.0001
        _ShadeColor_Step ("ShadeColor_Step", Range(0, 1)) = 0
        _1st2nd_Shades_Feather ("1st/2nd_Shades_Feather", Range(0.0001, 1)) = 0.0001
        _DirectStrength ("Direct Strength", Range(0, 16)) = 1
        _IndirectStrength ("Indirect Strength", Range(0, 1)) = 1
        [Space(10)]
        [Header(Shadow)]
        [Space(5)]
        _ShadowColor ("ShadowColor", Color) = (0.25,0.25,0.25,1)
        _ShadowStep ("ShadowStep", Range(0, 1)) = 0.25
        _ShadowStepSmooth ("ShadowStepSmooth", Range(0, 1)) = 0.15
        _ShadowStrength ("ShadowStrength", Range(-1, 1)) = 1
        [Toggle]_Set_SystemShadowsToBase ("Set SystemShadows To Base", float) = 0
        _ShadowsLevel ("Shadows Level", Range(-0.5,0.5)) = 0

        [Space(10)]
        [Header(Specular)]
        [Space(5)]
        _SpecularStep ("SpecularStep", Range(0, 1)) = 0.15
        _SpecularStepSmooth ("SpecularStepSmooth", Range(0, 1)) = 0.05
        [HDR]_SpecularColor ("SpecularColor", Color) = (1,1,1,1)
        [Space(10)]
        [Header(Rim)]
        [Space(5)]
        [Toggle(_ENABLE_RIM)]_EnableRim ("Enable Rim",float) = 0
        _RimStep ("RimStep", Range(0, 1)) = 0.6
        _RimStepSmooth ("RimStepSmooth",Range(0,1)) = 0.3
        _RimColor ("RimColor", Color) = (1,1,1,1)
        [Space(10)]
        [Header(Outline)]
        [Space(5)]
        [Toggle(_ENABLE_SMOOTHNORMALS)] _EnableSmoothNormals ("Enable Smooth Normals", Float) = 0
        [Space(5)]
        _OutlineWidth ("OutlineWidth", Range(0.0, 1.0)) = 0.015
        _OutlineColor ("OutlineColor", Color) = (0.0, 0.0, 0.0, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
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
            half _DirectStrength;
            half _IndirectStrength;

            half4 _ShadowColor;
            half _ShadowStep;
            half _ShadowStepSmooth;
            half _ShadowStrength;

            half _Set_SystemShadowsToBase;
            half _ShadowsLevel;

            half _SpecularStep;
            half _SpecularStepSmooth;
            half4 _SpecularColor;

            half _RimStepSmooth;
            half _RimStep;
            half4 _RimColor;

            float _OutlineWidth;
            half4 _OutlineColor;
        CBUFFER_END

        half _Cutoff;
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        
        TEXTURE2D(_GradientMap);
        SAMPLER(sampler_GradientMap);
        
        half Alpha(half albedoAlpha, half4 color, half cutoff)
        {
            #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
            half alpha = albedoAlpha * color.a;
            #else
                half alpha = color.a;
            #endif

            #if defined(_ALPHATEST_ON)
                clip(alpha - cutoff);
            #endif

            return alpha;
        }

        half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
        {
            return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
        }
        half3 SampleGradient(float halfLambert)
        {
            half3 gradient = SAMPLE_TEXTURE2D(_GradientMap, sampler_GradientMap, float2(halfLambert,0.08f));
            return gradient;
        }
        ENDHLSL
        Pass
        {
            Name "UniversalForward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma shader_feature _ENABLE_RIM
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb * _DirectStrength;

                Light light = GetMainLight(input.shadowCoord);
                half atten = light.distanceAttenuation * light.shadowAttenuation;

                half3 N = normalize(input.normalWS);
                half3 V = normalize(input.viewDirWS.xyz);
                half3 L = normalize(_MainLightPosition.xyz);
                half3 H = normalize(V + L);


                half NoH = saturate(dot(N, H));
                half NoL = dot(N, L) * 0.5 + 0.5;

                half specularNoH = smoothstep(1 - _SpecularStep * 0.05 - _SpecularStepSmooth * 0.05,
                                              1 - _SpecularStep * 0.05 + _SpecularStepSmooth * 0.05, NoH);
                half shadowNoL = smoothstep(_ShadowStep - _ShadowStepSmooth, _ShadowStep + _ShadowStepSmooth, NoL);

                half3 shadowColor = _ShadowColor.rgb * saturate(1 - shadowNoL) * (1 - _ShadowStrength);

                half shadow = MainLightRealtimeShadow(input.shadowCoord) * atten;

                half3 gradient1 = SampleGradient(NoL);
                half3 gradient2 = SampleGradient(1 - NoL);
                #if defined(_ENABLE_RIM)
                half NoV = saturate(dot(N, V));
                half3 rim = smoothstep(1 - _RimStep - _RimStepSmooth * 0.5, 1 - _RimStep + _RimStepSmooth * 0.5,
                                       0.5 - NoV) * _RimColor.rgb;
                #else
                half3 rim = 0;
                #endif

                half3 diffuse = _MainLightColor.rgb * albedo * shadowNoL * shadow;

                half3 specular = _SpecularColor.rgb * shadow * shadowNoL * specularNoH;

                half3 ambient = SampleSH(N) * albedo * _IndirectStrength;

                half3 finalColor = diffuse + rim + ambient + specular + shadowColor;

                // float3 Set_LightColor = light.color;
                //
                // float3 Set_BaseColor = lerp(_BaseColor.rgb, _BaseColor.rgb * Set_LightColor, _Is_LightColor_BaseColor);
                //
                // float3 Set_1st_ShadeColor = lerp(_1st_Color.rgb, _1st_Color.rgb * Set_LightColor, _Is_LightColor_1st);
                // float3 Set_2nd_ShadeColor = lerp((_2nd_Color.rgb), ((_2nd_Color.rgb) * Set_LightColor),
                //                                                  _Is_LightColor_2nd);
                // float _SystemShadowsLevel_var = (atten * 0.5) + 0.5 + _ShadowsLevel > 0.001
                //                         ? (atten * 0.5) + 0.5 + _ShadowsLevel
                //                         : 0.0001;
                // float Set_FinalShadowMask = saturate(
                //     1.0 + (lerp(NoL, NoL * saturate(_SystemShadowsLevel_var),
                //                                               _Set_SystemShadowsToBase) - (_BaseColor_Step -
                //         _BaseShade_Feather)) * ((1.0
                //         - _Set_1st_ShadePosition_var).r - 1.0)) / (
                //     _BaseColor_Step - (_BaseColor_Step - _BaseShade_Feather));
                // float3 Set_FinalBaseColor = lerp(Set_BaseColor,lerp(Set_1st_ShadeColor, Set_2nd_ShadeColor,saturate(1.0 + (NoL - (_ShadeColor_Step -_1st2nd_Shades_Feather)) * ((1.0 -
                //                                    _Set_2nd_ShadePosition_var).r - 1.0)
                //                                / (_ShadeColor_Step - (_ShadeColor_Step -
                //                                    _1st2nd_Shades_Feather)))),
                //                                         Set_FinalShadowMask); // Final Color
                // Set_FinalBaseColor *= diffuse;

                return half4(gradient1 * gradient2, 1.0);
            }
            ENDHLSL
        }

        //Outline
        Pass
        {
            Name "Outline"
            Cull Front
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ENABLE_SMOOTHNORMALS
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                #if defined(_ENABLE_SMOOTHNORMALS)
                float4 tangent : TANGENT;
                #endif
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 positionOS : TEXCOORD1;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.positionOS = v.vertex.xyz;
                float4 pos = TransformObjectToHClip(o.positionOS);
                #if defined(_ENABLE_SMOOTHNORMALS)
                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.tangent.xyz);
                #else
                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal.xyz);
                #endif
                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal)) * pos.w;
                float4 nearUpperRight = mul(unity_CameraInvProjection,
                                            float4(1, 1,UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);
                ndcNormal.x *= aspect;
                pos.xy += 0.01 * _OutlineWidth * ndcNormal.xy;
                o.pos = pos;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"

            ColorMask 0

            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}