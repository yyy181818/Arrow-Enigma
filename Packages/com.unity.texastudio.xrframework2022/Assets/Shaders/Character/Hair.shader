Shader "TA Shaders/Character/Hair"
{
    Properties
    {
         /*************************************/
        /***********Surface Options***********/
        /*************************************/
        // Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _ZTest("ZTest", Int) = 4
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0
        [Enum(Flip, 0, Mirror, 1, None, 2)] _DoubleSidedNormalMode("Double sided normal mode", Float) = 1
        [HideInInspector] _DoubleSidedConstants("Double-Sided Constants", Vector) = (1.0, 1.0, 1.0, 0.0)
        // Alpha Clipping
        [ToggleUI]  _AlphaCutoffEnable("Alpha Cutoff Enable", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [ToggleUI]  _UseShadowThreshold("_UseShadowThreshold", Float) = 0.0
        _AlphaCutoffShadow("_AlphaCutoffShadow", Range(0.0, 1.0)) = 0.5
        // Geometric AA
        [ToggleUI] _EnableGeometricSpecularAA("EnableGeometricSpecularAA", Float) = 0.0
        _SpecularAAScreenSpaceVariance("SpecularAAScreenSpaceVariance", Range(0.0, 1.0)) = 0.1
        _SpecularAAThreshold("SpecularAAThreshold", Range(0.0, 1.0)) = 0.2

        /************************************/
        /***********Surface Inputs***********/
        /************************************/
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _BumpScale("Scale", Range(0.0, 8.0)) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _AmbientOcclusionMap("AmbientOcclusionMap", 2D) = "white" {}
        _AORemapMin("AORemapMin", Range(0.0, 1.0)) = 0.0
        _AORemapMax("AORemapMax", Range(0.0, 1.0)) = 1.0

        //Specular Block
        _SmoothnessMaskMap("SmoothnessMaskMap", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5   
        _SmoothnessRemapMin("SmoothnessRemapMin", Range(0.0, 1.0)) = 0.0
        _SmoothnessRemapMax("SmoothnessRemapMax", Range(0.0, 1.0)) = 1.0

        _SpecularColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecularTintColor("SpecularTint", Color) = (0.2, 0.2, 0.2)
        _SpecularMultiplier("SpecularMultiplier", Range(0.0, 1.0)) = 1.0
        _SpecularShift("SpecularShift", Range(0.0, 1.0)) = 1.0
        _SecondarySpecularMultiplier("SpecularMultiplier", Range(0.0, 1.0)) = 1.0
        _SecondarySpecularShift("SpecularShift", Range(0.0, 1.0)) = 1.0
        [HDR] _TransmissionColor("TransmissionColor", Color) = (0.2, 0.2, 0.2)
        _TransmissionIntensity("TransmissionIntensity", Range(0.0, 1.0)) = 1.0

        _StaticLightColor("Static Light Color", Color) = (1,1,1,1)
        _StaticLightVector("Static Light Vector", Vector) = (0.0, 0.0, 0.0, 1.0)

        /************************************/
        /**********Advanced Options**********/
        /************************************/
        [ToggleUI]  _CastShadows("Cast Shadows", Float) = 0.0
        [ToggleUI]  _ReceiveShadows("Receive Shadows", Float) = 1.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleUI] _StaticSpecularHighlight("Static Specular Highlight", Float) = 0.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections("EnvironmentReflections", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    
    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="3.0"}
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _DOUBLESIDED_ON
            #pragma shader_feature_local_fragment _ENABLE_GEOMETRIC_SPECULAR_AA

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _AO_MAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_MASK

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _STATICSPECULARHIGHLIGHT_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

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
            //#pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            // #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "../ShaderLibrary/Hair/HairInput.hlsl"
            #include "../ShaderLibrary/Hair/HairForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest[_ZTest]
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SHADOW_CUTOFF

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "../ShaderLibrary/Hair/HairInput.hlsl"
            #include "../ShaderLibrary/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ZTest[_ZTest]
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SHADOW_CUTOFF

            #include "../ShaderLibrary/Hair/HairInput.hlsl"
            #include "../ShaderLibrary/UniversalDepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
//        Pass
//        {
//            Name "DepthNormals"
//            Tags{"LightMode" = "DepthNormals"}
//
//            ZWrite On
//            ZTest[_ZTest]
//            Cull[_Cull]
//
//            HLSLPROGRAM
//            #pragma only_renderers gles gles3 glcore d3d11 vulkan
//            #pragma target 3.0
//
//            #pragma vertex DepthNormalsVertex
//            #pragma fragment DepthNormalsFragment
//
//            // -------------------------------------
//            // Material Keywords
//            #pragma shader_feature_local _NORMALMAP
//            #pragma shader_feature_local_fragment _ALPHATEST_ON
//
//            //--------------------------------------
//            // GPU Instancing
//            #pragma multi_compile_instancing
//
//            #include "ShaderLibrary/Hair/HairInput.hlsl"
//            #include "ShaderLibrary/UniversalDepthNormalsPass.hlsl"
//            ENDHLSL
//        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma target 3.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _DOUBLESIDED_ON
            #pragma shader_feature_local_fragment _ENABLE_GEOMETRIC_SPECULAR_AA

            #pragma shader_feature_local_fragment _AO_MAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_MASK

            #include "../ShaderLibrary/Hair/HairInput.hlsl"
            #include "../ShaderLibrary/Hair/HairMetaPass.hlsl"

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.HairGUI"
}
