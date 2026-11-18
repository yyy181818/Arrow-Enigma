Shader "TA Shaders/Lit"
{
    Properties
    {
        [Toggle(_SURFACE_TYPE_TRANSPARENT)] _Surface("半透明是否开启", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", Float) = 10
        [Space(10)]
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("CullMode", float) = 2
        [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Space(10)]
        [Header(Base Settings)]
        [Space(10)]
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1,1,1,1)
        _BaseColorStrength("Base Color Strength", Range(0,32)) = 1
        _TillingOffset("Tilling Offset", Vector) = (1,1,0,0)
        [Space(10)]
        [Header(AlphaTest Settings)]
        [Space(10)]
        [Toggle(_ALPHATEST_ON)] _AlphaClip("Clip是否开启", Float) = 0
        [Enum(A,0,R,1)] _AlphaChannel("Alpha使用的通道", Float) = 0
        _Cutoff("Cut off", Range(0,1)) = 0.5
        _Alpha("整体透明度", Range(0,1)) = 1
        [Space(10)]
        [Header(CombineMap Settings)]
        [Space(10)]
        [KeywordEnum(NMR,MRO,None)] _TextureMode("混合贴图模式", Float) = 1
        _CombineMap("混合贴图", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0
        _Roughness("Roughness", Range(0,1)) = 0.5
        [Toggle(_OCCLUSIONMAP)] _Occlusion("开启后将应用AO效果", Float) = 1
        _OcclusionStrength("AO Strength", Range(0,1)) = 1.0
        [Space(10)]
        [Header(Normal Settings)]
        [Space(10)]
        [Toggle(_NORMALMAP)] _Normal("Use Normal Map", Float) = 0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0,8)) = 1
        [Space(10)]
        [Header(Emission Settings)]
        [Space(10)]
        [Toggle(_EMISSION)] _EnableEmission("Emission", Float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionMap("Emission Map", 2D) = "white" {}
        [Toggle] _EnableSampleRGB("Enable Sample RGB", Float) = 0
        [Channel] _EmissionChannel("Emission Channel", Vector) = (0,0,0,0)
        [Space(10)]
        _DirectStrength("Direct Strength", Range(0,32)) = 1
        _InDirectStrength("InDirect Strength", Range(0,32)) = 1
        _LightMapIntensity("LightMap Intensity", Range(0,2)) = 1
        _LightMapColor("LightMap Tint Color", Color) = (1,1,1,1)
        _SpecularColor("Specular Color", Color) = (1,1,1,1)

        [Toggle(_RECEIVE_SHADOWS_OFF)] _ReceiveShadows("Receive Shadows off", Float) = 1

        // ---- 清漆/镭射 (取消下面三处注释即可启用) ----
        //[Toggle(_ENABLE_CLEARCOAT_ON)] _EnableClearCoat("Enable ClearCoat", Float) = 0
        //_ClearCoat("Clear Coat", Range(0,1)) = 0
        //_Iridescence("Iridescence", Range(0,1)) = 0
        //_IridescenceThickness("Iridescence Thickness", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _TillingOffset;
            float4 _BaseMap_ST;
            half4  _BaseColor;
            half   _BaseColorStrength;
            half   _AlphaChannel;
            half   _Alpha;
            half   _Metallic;
            half   _Roughness;
            half   _OcclusionStrength;
            half   _BumpScale;
            half   _DirectStrength;
            half   _InDirectStrength;
            float  _LightMapIntensity;
            half4  _LightMapColor;
            float  _Cutoff;
            half4  _EmissionColor;
            half4  _EmissionChannel;
            half   _EnableSampleRGB;
            half4  _SpecularColor;
            // ClearCoat
            //half   _ClearCoat;
            //half   _Iridescence;
            //half   _IridescenceThickness;
        CBUFFER_END

        TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_CombineMap);     SAMPLER(sampler_CombineMap);
        TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_EmissionMap);    SAMPLER(sampler_EmissionMap);

        #include "../ShaderLibrary/Surface/Sampling.hlsl"
        ENDHLSL

        // ==================================================================
        // Pass 1 — Forward Lit
        // ==================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_CullMode]
            ZWrite [_ZWrite]
            ZTest [_ZTest]

            Blend [_SrcBlend] [_DstBlend]   // 半透明时取消注释, SrcBlend=5 DstBlend=10

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers gles3 d3d11 vulkan
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local_fragment _TEXTUREMODE_NMR _TEXTUREMODE_MRO _TEXTUREMODE_NONE
            //#pragma shader_feature_local_fragment _ENABLE_CLEARCOAT_ON

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "../ShaderLibrary/Core/Input.hlsl"
            #include "../ShaderLibrary/Core/Vertex.hlsl"
            #include "../ShaderLibrary/Surface/Setup.hlsl"
            #include "../ShaderLibrary/Lighting/BRDF.hlsl"
            #include "../ShaderLibrary/Lighting/Lighting.hlsl"
            #include "../ShaderLibrary/Lighting/GI.hlsl"
            #include "../ShaderLibrary/Lighting/InputData.hlsl"
            #include "../ShaderLibrary/Lighting/PBR.hlsl"

            Varyings_forward_base vert(Attributes_forward_base input)
            {
                return CommonLitVertex(input);
            }

            half4 frag(Varyings_forward_base i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                SurfaceData surfaceData;
                InitLitSurfaceData(i.uv, surfaceData);
                
                InputData inputData;
                InitLitInputData(i, surfaceData.normalTS, inputData);

                half4 color = CustomUniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
#if defined(_SURFACE_TYPE_TRANSPARENT)
                color.a = surfaceData.alpha;
#endif
                return color;
            }
            ENDHLSL
        }

        // ==================================================================
        // Pass 2 — ShadowCaster
        // ==================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // ==================================================================
        // Pass 3 — DepthOnly
        // ==================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // ==================================================================
        // Pass 4 — Meta (lightmap baking)
        // ==================================================================
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex UniversalVertexMeta
            #pragma fragment MetaFrag

            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _TEXTUREMODE_NMR _TEXTUREMODE_MRO _TEXTUREMODE_NONE
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
            #include "../ShaderLibrary/Surface/Setup.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv0        : TEXCOORD0;
                float2 uv1        : TEXCOORD1;
                float2 uv2        : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings UniversalVertexMeta(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2,
                    unity_LightmapST, unity_DynamicLightmapST);
                output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
                return output;
            }

            half4 MetaFrag(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitLitSurfaceData(input.uv, surfaceData);

                MetaInput metaInput;
                metaInput.Albedo   = surfaceData.albedo;
                metaInput.Emission = surfaceData.emission;

                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
