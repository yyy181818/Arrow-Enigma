//Madumpa URP Stylized Lit Shader Ver 1.2
//E-mail : mnpshino@gmail.com
//GitHub : https://github.com/madumpa/URP_StylizedLitShader
//WebSite : https://madumpa.com/
//Twitter : @madumpa



Shader "TA Shaders/Default/Stylized Lit"
{
    Properties
    {
        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        //StylizedPBR
        //[Header(StylizedDiffuse)]
        [Space(10)]
        _BrushTex("Brush Texture(R : Med Tone, G:Shadow, B:Reflect)", 2D) = "white"{}

        _MedBrushStrength ("Med Tone Brush Strength", Range(0,1)) = 0
        _ShadowBrushStrength ("Shadow Brush Strength", Range(0,1)) = 0
        _ReflBrushStrength ("Reflect Brush Strength", Range(0,1)) = 0

        [Space(10)]
        _MedColor("Med Tone Color", Color) = (1,1,1,1)
        _MedThreshold ("Med Tone Threshold", Range(0,1)) = 1
        _MedSmooth ("Med Tone Smooth", Range(0,0.5)) = 0.25

        [Space(10)]
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSmooth ("Shadow Smooth", Range(0,0.5)) = 0.25

        [Space(10)]
        _ReflectColor ("Reflect Color", Color) = (0,0,0,0)
        _ReflectThreshold ("Reflect Threshold", Range(0,1)) = 0
        _ReflectSmooth ("Reflect Smooth", Range(0,0.5)) = 0.25

        _GIIntensity("GI Intensity", Range(0,2)) = 1

        //[Header(StylizedReflection)]
        [Toggle] _GGXSpecular ("GGX Specular", float) = 0
        [LightDir]_SpecularLightOffset("Specular Light Offset", Vector) = (0,0,0,0)
        [Space(16)]
        _SpecularThreshold("Specular Threshold", Range(0.1,2)) = 0.5
        _SpecularSmooth ("Specular Smooth", Range(0,0.5)) = 0.5
        _SpecularIntensity("Specular Intensity", float) = 1

        [Space(10)]
        [Toggle] _DirectionalFresnel ("Directional Fresnel", float) = 0
        _FresnelThreshold("Fresnel Threshold", Range(0,1)) = 0.5
        _FresnelSmooth("Fresnel Smooth", Range(0,0.5) ) = 0.5
        _FresnelIntensity("Fresnel Intensity", float) = 1

        _ReflProbeIntensity("Non Metal Reflection Probe Intensity", float) = 1
        _MetalReflProbeIntensity ("Metal Reflection Probe Intensity", float) = 1

        [Space(30)]
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        _ReceiveShadows("Receive Shadows", Range(0,1)) = 1.0 //얘도 살려둠 
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections("EnvironmentReflections", Float) = 0.0

        // Planar Shadow
        [Toggle] _EnableClipShadow ("Enable Clip Shadow", Float) = 0
        [Toggle(_PLANARSHADOW_ENABLE)] _PlanarShadowEnable ("PlanarShadow Enable", Float) = 0
        [KeywordEnum(Global,Local,Planar)] _PlanarShadowMode ("PlanarShadow Mode", Float) = 0
        _PlanarShadowColor ("PlanarShadow Color", Color) = (0,0,0,1)
        _ShadowFalloff ("Shadow Falloff", Range(0,1)) = 0.1
        _GlobalHeight ("Global Ground Height", float) = -1
        _LocalHeight ("Local Ground Height", float) = 1
        _ShadowOffset ("Shadow Offset", float) = 1
        _ShadowIntensity ("Shadow Intensity", float) = 5

        [Toggle(_ENABLE_TONEMAP)] _EnableAcesToneMapping ("Enable ACES ToneMapping", Float) = 0
        [Toggle(_ENABLE_COLOR_PROCESSING)] _EnableColorProcessing ("Enable Color Processing", Float) = 0
        _Brightness ("Brightness", float) = 1
        _Contrast ("Contrast", float) = 1
        _Saturation ("Saturation", float) = 1
        [Toggle] _EnableHue ("Enable Hue", Float) = 0
        _Hue ("Hue", Range(0,360)) = 0

        // Vertex Sway (Wind)
        [Toggle(_SWAY)] _Sway ("Enable Sway", Float) = 0
        _SwayAmount ("Sway Strength (top)", Range(0, 2)) = 0.3
        _SwaySpeed ("Wind Speed", Range(0, 10)) = 1.5
        _SwayFrequency ("Wind Variation", Range(0, 5)) = 1.0
        _SwayHeight ("Model Height", Float) = 2.0
        [Toggle(_SWAY_VERTEXCOLOR)] _SwayByVertexColor ("Use Vertex Color Weight", Float) = 0

        // Alpha To Coverage (Cutout AA)
        [Toggle(_ALPHATOMASK)] _AlphaToMask ("Alpha To Mask (Cutout AA)", Float) = 0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]
            // Alpha-To-Coverage：Pass 级固定状态，无法按关键字切换；是否生效由 _ALPHATOMASK 控制——
            // 关时 fragment 输出 alpha=1（OutputAlpha）→ 全覆盖、硬边；开时输出真实 cutout alpha → MSAA 覆盖采样平滑边缘。
            AlphaToMask On

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            // #pragma prefer_hlslcc gles
            // #pragma exclude_renderers d3d11_9x
            // #pragma target 2.0

            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _OCCLUSIONMAP

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            #pragma shader_feature _ENABLE_TONEMAP
            #pragma shader_feature _ENABLE_COLOR_PROCESSING

            // Vertex Sway
            #pragma shader_feature _SWAY
            #pragma shader_feature _SWAY_VERTEXCOLOR

            // Alpha To Coverage (需 MSAA 开启)
            #pragma shader_feature _ALPHATOMASK

            // -------------------------------------
            // Universal Pipeline keywords
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            //#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            //#pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            // -------------------------------------
            // Unity defined keywords
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            // #pragma multi_compile _ LIGHTMAP_ON
            // #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog


            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "../ShaderLibrary/StylizedLit/StylizedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../ShaderLibrary/Sway.hlsl"

            // 修复：StylizedLit ForwardLit pass 未包含 URP 的 LitForwardPass.hlsl，
            // 导致 REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR 从未定义 -> tangentWS 插值被禁用 ->
            // 开启 _NORMALMAP 时片元读取不存在的字段而编译报错，且法线贴图静默失效。
            // 这里按 _NORMALMAP 显式定义，与 URP 标准 Lit 对齐（#ifndef 防重定义）。
            #ifndef REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
            #if defined(_NORMALMAP)
            #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
            #endif
            #endif

            #pragma multi_compile _ _USEBRUSHTEX_ON

            #ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
            #define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 color : COLOR;          // 顶点色权重（仅 _SWAY_VERTEXCOLOR 时使用 Blue 通道）
                float2 texcoord : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                float2 dynamicLightmapUV : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                float3 positionWS : TEXCOORD1;
                #endif
                float3 normalWS : TEXCOORD2;
                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                half4 tangentWS : TEXCOORD3; // xyz: tangent, w: sign
                #endif
                float3 viewDirWS : TEXCOORD4;
                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
                #else
                half fogFactor : TEXCOORD5;
                #endif
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord : TEXCOORD6;
                #endif

                float3 positionOS : TEXCOODR7;

                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);

                #if defined(DYNAMICLIGHTMAP_ON)
                float2 dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
                #endif

                float4 screenPos : TEXCOORD9;

                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
                inputData = (InputData)0;

                inputData.positionWS = input.positionWS;

                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                float sgn = input.tangentWS.w; // should be either +1 or -1
                float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
                inputData.tangentToWorld = tangentToWorld;
                inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
                #else
                inputData.normalWS = input.normalWS;
                #endif

                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = viewDirWS;

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                inputData.shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #else
                inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif
                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0),
                                             input.fogFactorAndVertexLight.x);
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                #else
                inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
                #endif

                #if defined(DYNAMICLIGHTMAP_ON)
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH,
                                                                            inputData.normalWS);
                #else
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                #endif
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

                #if defined(DEBUG_DISPLAY)
                #if defined(DYNAMICLIGHTMAP_ON)
                inputData.dynamicLightmapUV = input.dynamicLightmapUV;
                #endif
                #if defined(LIGHTMAP_ON)
                inputData.staticLightmapUV = input.staticLightmapUV;
                #else
                inputData.vertexSH = input.vertexSH;
                #endif
                #endif
            }

            ///////////////////////////////////////////////////////////////////////////////
            //                  Vertex and Fragment functions                            //
            ///////////////////////////////////////////////////////////////////////////////

            // Used in Standard (Physically Based) shader
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                float4 clipPos = vertexInput.positionCS;
                // #if UNITY_REVERSED_Z
                // clipPos.z += _ZTestAddValue;
                // clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                // #else
                // clipPos.z -= _ZTestAddValue;
                // clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                // #endif
                output.positionCS = clipPos;

                output.positionWS = vertexInput.positionWS;

                #if defined(_SWAY)
                    float swayWeight = TexaGetSwayWeight(input.positionOS.xyz, input.color, _SwayHeight);
                    float2 swayOffset = TexaGetSwayOffset(input.positionOS.xyz, swayWeight,
                                                         _SwayAmount, _SwaySpeed, _SwayFrequency);
                    vertexInput.positionWS.xz += swayOffset;
                    vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);
                    output.positionWS = vertexInput.positionWS;
                    output.positionCS = vertexInput.positionCS;
                #endif

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.normalWS = normalInput.normalWS;

                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                real sign = input.tangentOS.w * GetOddNegativeScale();
                half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                #endif
                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                output.tangentWS = tangentWS;
                #endif

                #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                #else
                output.positionOS = input.positionOS.xyz;
                #endif

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                output.shadowCoord = GetShadowCoord(vertexInput);
                #endif

                half3 vertexLight = VertexLighting(output.positionWS, normalInput.normalWS);

                half fogFactor = 0;
                #if !defined(_FOG_FRAGMENT)
                fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                #endif

                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);

                #if defined(DYNAMICLIGHTMAP_ON)
                output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy +
                    unity_DynamicLightmapST.zw;
                #endif
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
                #else
                output.fogFactor = fogFactor;
                #endif

                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                return output;
            }

            half LinearStep(half minValue, half maxValue, half In)
            {
                return saturate((In - minValue) / (maxValue - minValue));
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

            half3 ApplyHue(float3 aColor, float aHue)
            {
                float angle = radians(aHue);
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float sinAngle, cosAngle;
                sincos(angle, sinAngle, cosAngle);

                return aColor * cosAngle + cross(k, aColor) * sinAngle + k * dot(k, aColor) * (1 - cosAngle);
            }

            half3 DirectStylizedBDRF(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
            {
                #ifndef _SPECULARHIGHLIGHTS_OFF
                float3 halfDir = SafeNormalize(float3(lightDirectionWS) + float3(viewDirectionWS));

                float NoH = saturate(dot(normalWS, halfDir));
                half LoH = saturate(dot(lightDirectionWS, halfDir));

                // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
                // BRDFspec = (D * V * F) / 4.0
                // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
                // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
                // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
                // https://community.arm.com/events/1155

                // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
                // We further optimize a few light invariant terms
                // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
                float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;

                half LoH2 = LoH * LoH;
                half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);

                // On platforms where half actually means something, the denominator has a risk of overflow
                // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
                // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
                #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
                specularTerm = specularTerm - HALF_MIN;
                specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
                #endif

                half3 color = lerp(LinearStep(_SpecularThreshold - _SpecularSmooth,
                              _SpecularThreshold + _SpecularSmooth, specularTerm), specularTerm,
                   _GGXSpecular) * brdfData.specular * max(0, _SpecularIntensity) + brdfData.diffuse;
                return color;
                #else
                return brdfData.diffuse;
                #endif
            }

            half3 LightingStylizedPhysicallyBased(BRDFData brdfData, half3 radiance, half3 lightColor,
   half3 lightDirectionWS, half lightAttenuation,
   half3 normalWS,
   half3 viewDirectionWS)
            {
                return DirectStylizedBDRF(brdfData, normalWS, normalize(lightDirectionWS + _SpecularLightOffset.xyz),
           viewDirectionWS) * radiance;
            }

            half3 LightingStylizedPhysicallyBased(BRDFData brdfData, half3 radiance, Light light, half3 normalWS,
                               half3 viewDirectionWS)
            {
                return LightingStylizedPhysicallyBased(brdfData, radiance, light.color, light.direction,
                              light.distanceAttenuation * light.shadowAttenuation, normalWS,
                              viewDirectionWS);
            }

            //indirect Specular


            half3 EnvironmentBRDFCustom(BRDFData brdfData, half3 radiance, half3 indirectDiffuse,
                                                                half3 indirectSpecular, half fresnelTerm)
            {
                half3 c = indirectDiffuse * brdfData.diffuse * _GIIntensity;
                float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
                c += surfaceReduction * indirectSpecular * lerp(brdfData.specular * radiance, brdfData.grazingTerm,
                    fresnelTerm);
                return c;
            }


            half3 StylizedGlobalIllumination(BRDFData brdfData, half3 radiance, half3 bakedGI, half occlusion,
                       half3 normalWS, half3 viewDirectionWS, half metallic,
                       half ndotl)
            {
                half3 reflectVector = reflect(-viewDirectionWS, normalWS);
                half fresnelTerm = LinearStep(_FresnelThreshold - _FresnelSmooth, _FresnelThreshold += _FresnelSmooth,
                                                  1.0 - saturate(dot(normalWS, viewDirectionWS))) * max(
                    0, _FresnelIntensity) * ndotl;

                half3 indirectDiffuse = bakedGI * occlusion;
                half3 indirectSpecular =
                    GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, occlusion) * lerp(
                        max(0, _ReflProbeIntensity), max(0, _MetalReflProbeIntensity), metallic);

                return EnvironmentBRDFCustom(brdfData, radiance, indirectDiffuse, indirectSpecular, fresnelTerm);
            }


            half3 CalculateRadiance(Light light, half3 normalWS, half3 brush, half3 brushStrengthRGB)
            {
                half NdotL = dot(normalWS, light.direction) * 0.5f + 0.5f;
                #if _USEBRUSHTEX_ON
                half halfLambertMed = NdotL * lerp(0.5, brush.r, brushStrengthRGB.r) + 0.5;
                half halfLambertShadow = NdotL * lerp(0.5, brush.g, brushStrengthRGB.g) + 0.5;
                half halfLambertRefl = NdotL * lerp(0.5, brush.b, brushStrengthRGB.b) + 0.5;
                #else
                half halfLambertMed = NdotL * 0.5f + 0.5f;
                half halfLambertShadow = halfLambertMed;
                half halfLambertRefl = halfLambertMed;
                #endif
                half smoothMedTone = LinearStep(_MedThreshold - _MedSmooth, _MedThreshold + _MedSmooth, halfLambertMed);
                half3 MedToneColor = lerp(_MedColor.rgb, 1, smoothMedTone);
                half smoothShadow = LinearStep(_ShadowThreshold - _ShadowSmooth, _ShadowThreshold + _ShadowSmooth,
                                                     halfLambertShadow * (lerp(
                                                         1, light.distanceAttenuation * light.
                                                         shadowAttenuation,
                                                         _ReceiveShadows)));
                half3 ShadowColor = lerp(_ShadowColor.rgb, MedToneColor, smoothShadow);
                half smoothReflect = LinearStep(_ReflectThreshold - _ReflectSmooth, _ReflectThreshold + _ReflectSmooth,
                    halfLambertRefl);
                half3 ReflectColor = lerp(_ReflectColor.rgb, ShadowColor, smoothReflect);
                half3 radiance = light.color * ReflectColor; //lightColor * (lightAttenuation * NdotL);
                return radiance;
            }


            half4 UniversalFragmentStylizedPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
  half smoothness, half occlusion, half3 emission, half alpha,
  half2 uv)
            {
                BRDFData brdfData;
                InitializeBRDFData(albedo, metallic, specular, smoothness, alpha, brdfData);

                Light mainLight = GetMainLight(inputData.shadowCoord);
                #ifdef _LIGHT_LAYERS
                uint meshRenderingLayers = GetMeshRenderingLayer();
                bool mainLightMatches = IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers);
                #else
                bool mainLightMatches = true;
                #endif
                #if _USEBRUSHTEX_ON
                float3 brushTex = SAMPLE_TEXTURE2D(_BrushTex, sampler_BrushTex, uv * _BrushTex_ST.xy + _BrushTex_ST.zw).
                    rgb;
                float3 radiance = CalculateRadiance(mainLight, inputData.normalWS, brushTex,
                                 float3(_MedBrushStrength, _ShadowBrushStrength, _ReflBrushStrength));
                #else
                float3 radiance = CalculateRadiance(mainLight, inputData.normalWS, 0.5, float3(0, 0, 0));
                #endif

                #ifdef _LIGHT_LAYERS
                if (!mainLightMatches)
                    radiance = half3(0, 0, 0);
                #endif

                #ifdef _LIGHT_LAYERS
                if (mainLightMatches)
                #endif
                MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

                float ndotl = LinearStep(_ShadowThreshold - _ShadowSmooth, _ShadowThreshold + _ShadowSmooth,
                                                   dot(mainLight.direction, inputData.normalWS) * 0.5 +
                                                   0.5);

                half3 color = StylizedGlobalIllumination(brdfData, radiance, inputData.bakedGI, occlusion,
          inputData.normalWS, inputData.viewDirectionWS, metallic,
          lerp(1, ndotl, _DirectionalFresnel));
                #ifdef _LIGHT_LAYERS
                if (mainLightMatches)
                #endif
                color += LightingStylizedPhysicallyBased(brdfData, radiance, mainLight, inputData.normalWS,
                       inputData.viewDirectionWS);

                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
                    #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                    #endif
                    color += LightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS);
                }
                #endif

                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                color += inputData.vertexLighting * brdfData.diffuse;
                #endif

                color += emission;
                return half4(color, alpha);
            }


            // Used in Standard (Physically Based) shader
            half4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);

                half4 color = UniversalFragmentStylizedPBR(inputData, surfaceData.albedo, surfaceData.metallic,
                            surfaceData.specular, surfaceData.smoothness,
                            surfaceData.occlusion, surfaceData.emission,
                            surfaceData.alpha, input.uv);

                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                #if defined(_ALPHATOMASK)
                    // A2C：保留真实 cutout alpha 供输出合并阶段的 MSAA 覆盖采样使用。
                    // OutputAlpha 在 cutout（非透明表面）下会强制 alpha=1，会令 A2C 完全失效，故此处绕过。
                    color.a = surfaceData.alpha;
                #else
                    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
                #endif
                #if defined(_ENABLE_COLOR_PROCESSING)
                color.rgb *= _Brightness;
                color.rgb = _EnableHue ? ApplyHue(color.rgb, _Hue) : color.rgb;
                half gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
                half3 grayColor = half3(gray, gray, gray);
                color.rgb = lerp(grayColor, color.rgb, _Saturation);
                half3 avgColor = half3(0.5, 0.5, 0.5);
                color.rgb = lerp(avgColor, color.rgb, _Contrast);
                #endif

                #if defined(_ENABLE_TONEMAP)
                color.rgb = ACESToneMapping(color.rgb, 1.0);
                #endif

                return color;
            }
            #endif
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            // #pragma prefer_hlslcc gles
            // #pragma exclude_renderers d3d11_9x
            // #pragma target 2.0

            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "../ShaderLibrary/StylizedLit/StylizedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            // #pragma prefer_hlslcc gles
            // #pragma exclude_renderers d3d11_9x
            // #pragma target 2.0

            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "../ShaderLibrary/StylizedLit/StylizedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 3.0

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "../ShaderLibrary/StylizedLit/StylizedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            // #pragma prefer_hlslcc gles
            // #pragma exclude_renderers d3d11_9x

            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma shader_feature _SPECGLOSSMAP

            #include "../ShaderLibrary/StylizedLit/StylizedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"
            ENDHLSL
        }
        // Planar Shadows平面阴影
        Pass
        {
            Name "PlanarShadow"
            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }
            //用使用模板测试以保证alpha显示正确
            Stencil
            {
                Ref 0
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }
            //Cull[_CullMode]

            Blend [_SrcBlend] [_DstBlend]

            ZWrite [_ZWrite]

            Offset -1 ,-1

            HLSLPROGRAM
            #pragma target 3.0
            #pragma only_renderers gles gles3 glcore d3d11 vulkan

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma shader_feature _PLANARSHADOW_ENABLE
            #pragma shader_feature _PLANARSHADOWMODE_GLOBAL _PLANARSHADOWMODE_LOCAL _PLANARSHADOWMODE_PLANAR
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #include "../ShaderLibrary/StylizedLit/StylizedLitInput.hlsl"
            #include "../ShaderLibrary/Universal/PlanarShadowPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.StylizedLitSwayShaderGUI"
}