// VAT "Rigid-Body Dynamics" shader
// 光照模型: 与 TA Shaders/Default/Lit_New 完全一致 (复用其自定义 PBR 函数链)
//   InitLitSurfaceData -> InitLitInputData -> CustomUniversalFragmentPBR
// 结构上额外加入的部分: VAT 顶点动画 (位置/法线/切线由 VAT 纹理驱动)
//   - 解码函数统一放在 ShaderLibrary/VAT/VATDecode.hlsl, 所有 pass 共用
//   - Forward 顶点管线逐行复刻 Lit_New 的 CommonLitVertex, 仅在最前面插入 VAT 解码

Shader "TA Shaders/VAT/Rigid-Body Dynamics"
{
    Properties
    {
        // ===== Lit_New 基础属性 (光照模型一致) =====
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
        [KeywordEnum(NMR,MRO,None)] _TextureMode("混合贴图模式", Float) = 0
        _CombineMap("混合贴图", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0
        _Roughness("Roughness", Range(0,1)) = 0.5
        [Toggle(_OCCLUSIONMAP)] _Occlusion("开启后将应用AO效果", Float) = 1
        _OcclusionStrength("AO Strength", Range(0,1)) = 1.0
        [Space(10)]
        [Header(Normal Settings)]
        [Space(10)]
        [Toggle(_NORMALMAP)] _Normal("Use Normal Map", Float) = 1
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0,8)) = 1
        [Space(10)]
        [Header(Emission Settings)]
        [Space(10)]
        [Toggle(_EMISSION)] _Emission("Emission", Float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionMap("Emission Map", 2D) = "white" {}
        [Toggle] _EnableSampleRGB("Enable Sample RGB", Float) = 0
        [Channel] _EmissionChannel("Emission Channel", Vector) = (0,0,0,0)
        [Space(10)]
        _DirectStrength("Direct Strength", Range(0,32)) = 1
        _InDirectStrength("InDirect Strength", Range(0,32)) = 1
        _LightMapIntensity("LightMap Intensity", Range(0,2)) = 1
        [HideInInspector] _LightMapColor("LightMap Tint Color", Color) = (1,1,1,1)
        _SpecularColor("Specular Color", Color) = (1,1,1,1)

        [Toggle(_RECEIVE_SHADOWS_OFF)] _ReceiveShadows("Receive Shadows off", Float) = 1

        // ---- 清漆/镭射 (取消下面三处注释即可启用) ----
        //[Toggle(_ENABLE_CLEARCOAT_ON)] _EnableClearCoat("Enable ClearCoat", Float) = 0
        //_ClearCoat("Clear Coat", Range(0,1)) = 0
        //_Iridescence("Iridescence", Range(0,1)) = 0
        //_IridescenceThickness("Iridescence Thickness", Range(0,1)) = 1

        // ===== VAT 顶点动画属性 (额外结构) =====
        [Header(VAT Settings)]
        [Space(10)]
        [NoScaleOffset] _PosTex("Position Texture", 2D) = "white" {}
        [NoScaleOffset] _RotTex("Rotation Texture", 2D) = "white" {}
        [NoScaleOffset] _ColTex("Color Texture", 2D) = "white" {}
        _NumOfFrames("Number Of Frames", float) = 24.0
        _HoudiniFPS("Houdini FPS", float) = 24.0
        _PlaySpeed("Play Speed", Range(-10, 10)) = 1.0
        _VATScale("Scale", Range(0, 1)) = 0.01
        [ToggleUI] _PivotMode("Pivot Mode (1=Piece,0=Object)", Float) = 1.0
        [ToggleUI] _AutoPlay("Auto Play", Float) = 1.0
        _ManualFrame("Manual Frame", Range(0, 1000)) = 0.0
        [ToggleUI] _Interpolate("Interpolate", Float) = 1.0
        [ToggleUI] _UseColTex("Use Vertex Color Texture", Float) = 0.0
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
            // ---- Lit_New 参数 ----
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
            half4  _LightMapColor;   // 与共享 InputData.hlsl 中 bakedGI 乘法保持一致; VAT 默认白(无影响)
            float  _Cutoff;
            half4  _EmissionColor;
            half4  _EmissionChannel;
            half   _EnableSampleRGB;
            half4  _SpecularColor;
            // ---- VAT 参数 (额外) ----
            float  _NumOfFrames;
            float  _HoudiniFPS;
            float  _PlaySpeed;
            float  _VATScale;
            float  _PivotMode;
            float  _AutoPlay;
            float  _ManualFrame;
            float  _Interpolate;
            float  _UseColTex;
        CBUFFER_END

        TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_CombineMap);     SAMPLER(sampler_CombineMap);
        TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_EmissionMap);    SAMPLER(sampler_EmissionMap);
        // ---- VAT 纹理 ----
        TEXTURE2D(_PosTex);         SAMPLER(sampler_PosTex);
        TEXTURE2D(_RotTex);         SAMPLER(sampler_RotTex);
        TEXTURE2D(_ColTex);         SAMPLER(sampler_ColTex);

        #include "../../ShaderLibrary/Surface/Sampling.hlsl"
        ENDHLSL

        // ==================================================================
        // Pass 1 — Forward Lit (Lit_New 光照模型 + VAT 顶点动画)
        // ==================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_CullMode]
            ZWrite [_ZWrite]
            ZTest [_ZTest]

            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers gles3 d3d11 vulkan
            #pragma vertex VATForwardLitVert
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
            // VAT 动态网格不烘焙 lightmap, 移除 LIGHTMAP 相关变体避免 vertexSH/staticLightmapUV 冲突
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "../../ShaderLibrary/Core/Input.hlsl"
            #include "../../ShaderLibrary/Core/Vertex.hlsl"
            #include "../../ShaderLibrary/Surface/Setup.hlsl"
            #include "../../ShaderLibrary/Lighting/BRDF.hlsl"
            #include "../../ShaderLibrary/Lighting/Lighting.hlsl"
            #include "../../ShaderLibrary/Lighting/GI.hlsl"
            #include "../../ShaderLibrary/Lighting/InputData.hlsl"
            #include "../../ShaderLibrary/Lighting/PBR.hlsl"
            #include "../../ShaderLibrary/VAT/VATDecode.hlsl"

            // ---- 顶点输入: Lit_New 的 Attributes 之上, 增加 VAT 通道 ----
            // VAT 动态网格不烘焙 lightmap, 因此不占用 staticLightmapUV(TEXCOORD1),
            // 还原原始通道映射: uvSamp=uv2(TEXCOORD1), uvPivotA=uv3(TEXCOORD2), uvPivotB=uv4(TEXCOORD3)
            struct VATAttributes
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS: TANGENT;
                float2 uvSamp   : TEXCOORD1;
                float2 uvPivotA : TEXCOORD2;
                float2 uvPivotB : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ---- 输出: 与 Lit_New 的 Varyings_forward_base 完全一致, 仅多一个 colVAT ----
            struct Varyings_VAT
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
            #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                half4   tangentWS               : TEXCOORD3;
            #endif
            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                half4   fogFactorAndVertexLight : TEXCOORD5;
            #else
                half    fogFactor               : TEXCOORD5;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4  shadowCoord             : TEXCOORD6;
            #endif
                half3  vertexSH               : TEXCOORD7;   // VAT 动态网格不烘焙 lightmap, 直接走 Vertex SH
                float3  colVAT                  : TEXCOORD8;   // VAT 逐帧顶点色
                float4  positionCS              : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ---- 顶点: VAT 解码 + Lit_New CommonLitVertex 同款管线 ----
            Varyings_VAT VATForwardLitVert(VATAttributes input)
            {
                Varyings_VAT output = (Varyings_VAT)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // ============ VAT 解码 (额外结构) ============
                float3 piecePivot = VATDecodePiecePivot(input.uvPivotA, input.uvPivotB) * _VATScale;
                float3 pivotVec   = input.vertex.xyz - piecePivot;

                float frameF, frameN, interp;
                VATComputeFrame(_AutoPlay, _ManualFrame, _PlaySpeed, _HoudiniFPS,
                                _NumOfFrames, _Interpolate, frameF, frameN, interp);

                float2 uvA = input.uvSamp; uvA.y -= (frameF - 1.0) / _NumOfFrames;
                float2 uvB = input.uvSamp; uvB.y -= (frameN - 1.0) / _NumOfFrames;

                float4 pT0 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvA, 0);
                float4 pT1 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvB, 0);
                float4 rT0 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvA, 0);
                float4 rT1 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvB, 0);
                float4 cT0 = SAMPLE_TEXTURE2D_LOD(_ColTex, sampler_ColTex, uvA, 0);
                float4 cT1 = SAMPLE_TEXTURE2D_LOD(_ColTex, sampler_ColTex, uvB, 0);

                float4 quat = VATDecodeQuat(rT0, pT0);
                float4 qN   = VATDecodeQuat(rT1, pT1);
                if (dot(quat, qN) < 0.0) qN = -qN;
                quat = normalize(lerp(quat, qN, interp));

                float3 posOffset = lerp(pT0.rgb, pT1.rgb, interp) * _VATScale;
                float3 posOS = (_PivotMode > 0.5)
                            ? (piecePivot + posOffset + VATRotateVec(pivotVec, quat))
                            : (posOffset + VATRotateVec(pivotVec, quat));

                float3 nrmOS = VATRotateVec(input.normalOS, quat);
                float4 tanOS = input.tangentOS;
                tanOS.xyz = VATRotateVec(input.tangentOS.xyz, quat);

                output.colVAT = lerp(cT0.rgb, cT1.rgb, interp);
                // ============ VAT 解码结束 ============

                // ---- 以下与 Lit_New CommonLitVertex 完全一致 ----
                VertexPositionInputs vertexInput = GetVertexPositionInputs(posOS);
                VertexNormalInputs   normalInput = GetVertexNormalInputs(nrmOS, tanOS);

                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = 0;
            #if !defined(_FOG_FRAGMENT)
                fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            #endif

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = normalInput.normalWS;

            #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                real sign = tanOS.w * GetOddNegativeScale();
                half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                output.tangentWS = tangentWS;
            #endif

                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            #else
                output.fogFactor = fogFactor;
            #endif

                output.positionWS = vertexInput.positionWS;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                output.shadowCoord = GetShadowCoord(vertexInput);
            #endif
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            // ---- 片元: 与 Lit_New 完全一致的 PBR 函数链 ----
            half4 frag(Varyings_VAT i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                SurfaceData surfaceData;
                InitLitSurfaceData(i.uv, surfaceData);

                // VAT 逐帧顶点色 (可选, 与 Lit_New 函数链无冲突)
                if (_UseColTex > 0.5)
                    surfaceData.albedo *= i.colVAT;

                // 将 Varyings_VAT 折叠回 Lit_New 的 Varyings_forward_base 供 InputData 使用
                Varyings_forward_base fb = (Varyings_forward_base)0;
                fb.uv         = i.uv;
                fb.positionWS = i.positionWS;
                fb.normalWS   = i.normalWS;
            #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                fb.tangentWS  = i.tangentWS;
            #endif
            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                fb.fogFactorAndVertexLight = i.fogFactorAndVertexLight;
            #else
                fb.fogFactor = i.fogFactor;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                fb.shadowCoord = i.shadowCoord;
            #endif
                fb.vertexSH = i.vertexSH;   // VAT 不烘焙 lightmap, 始终走 Vertex SH
                fb.positionCS = i.positionCS;

                InputData inputData;
                InitLitInputData(fb, surfaceData.normalTS, inputData);

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
        // Pass 2 — ShadowCaster (随 VAT 动画, 保证阴影贴合)
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
            #pragma vertex VATShadowVert
            #pragma fragment VATShadowFrag
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../ShaderLibrary/VAT/VATDecode.hlsl"

            struct VATShadowAttr
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uvSamp     : TEXCOORD1;
                float2 uvPivotA   : TEXCOORD2;
                float2 uvPivotB   : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 VATShadowVert(VATShadowAttr input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 piecePivot = VATDecodePiecePivot(input.uvPivotA, input.uvPivotB) * _VATScale;
                float3 pivotVec   = input.positionOS.xyz - piecePivot;

                float frameF, frameN, interp;
                VATComputeFrame(_AutoPlay, _ManualFrame, _PlaySpeed, _HoudiniFPS,
                                _NumOfFrames, _Interpolate, frameF, frameN, interp);

                float2 uvA = input.uvSamp; uvA.y -= (frameF - 1.0) / _NumOfFrames;
                float2 uvB = input.uvSamp; uvB.y -= (frameN - 1.0) / _NumOfFrames;

                float4 pT0 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvA, 0);
                float4 pT1 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvB, 0);
                float4 rT0 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvA, 0);
                float4 rT1 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvB, 0);

                float4 quat = VATDecodeQuat(rT0, pT0);
                float4 qN   = VATDecodeQuat(rT1, pT1);
                if (dot(quat, qN) < 0.0) qN = -qN;
                quat = normalize(lerp(quat, qN, interp));

                float3 posOS = (_PivotMode > 0.5)
                            ? (piecePivot + lerp(pT0.rgb, pT1.rgb, interp) * _VATScale + VATRotateVec(pivotVec, quat))
                            : (lerp(pT0.rgb, pT1.rgb, interp) * _VATScale + VATRotateVec(pivotVec, quat));

                return TransformWorldToHClip(TransformObjectToWorld(posOS));
            }

            half4 VATShadowFrag() : SV_Target { return 0; }
            ENDHLSL
        }

        // ==================================================================
        // Pass 3 — DepthOnly (随 VAT 动画)
        // ==================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex VATDepthVert
            #pragma fragment VATDepthFrag
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../ShaderLibrary/VAT/VATDecode.hlsl"

            struct VATDepthAttr
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uvSamp     : TEXCOORD1;
                float2 uvPivotA   : TEXCOORD2;
                float2 uvPivotB   : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 VATDepthVert(VATDepthAttr input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 piecePivot = VATDecodePiecePivot(input.uvPivotA, input.uvPivotB) * _VATScale;
                float3 pivotVec   = input.positionOS.xyz - piecePivot;

                float frameF, frameN, interp;
                VATComputeFrame(_AutoPlay, _ManualFrame, _PlaySpeed, _HoudiniFPS,
                                _NumOfFrames, _Interpolate, frameF, frameN, interp);

                float2 uvA = input.uvSamp; uvA.y -= (frameF - 1.0) / _NumOfFrames;
                float2 uvB = input.uvSamp; uvB.y -= (frameN - 1.0) / _NumOfFrames;

                float4 pT0 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvA, 0);
                float4 pT1 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvB, 0);
                float4 rT0 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvA, 0);
                float4 rT1 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvB, 0);

                float4 quat = VATDecodeQuat(rT0, pT0);
                float4 qN   = VATDecodeQuat(rT1, pT1);
                if (dot(quat, qN) < 0.0) qN = -qN;
                quat = normalize(lerp(quat, qN, interp));

                float3 posOS = (_PivotMode > 0.5)
                            ? (piecePivot + lerp(pT0.rgb, pT1.rgb, interp) * _VATScale + VATRotateVec(pivotVec, quat))
                            : (lerp(pT0.rgb, pT1.rgb, interp) * _VATScale + VATRotateVec(pivotVec, quat));

                return TransformWorldToHClip(TransformObjectToWorld(posOS));
            }

            half4 VATDepthFrag() : SV_Target { return 0; }
            ENDHLSL
        }

        // ==================================================================
        // Pass 4 — DepthNormals (随 VAT 动画, 供 SSAO / 贴花 / 法线相关效果)
        // ==================================================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex VATDepthNormalsVert
            #pragma fragment VATDepthNormalsFrag
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../ShaderLibrary/VAT/VATDecode.hlsl"

            struct VATDN_Attr
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uvSamp     : TEXCOORD1;
                float2 uvPivotA   : TEXCOORD2;
                float2 uvPivotB   : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VATDN_V2F
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VATDN_V2F VATDepthNormalsVert(VATDN_Attr input)
            {
                VATDN_V2F output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 piecePivot = VATDecodePiecePivot(input.uvPivotA, input.uvPivotB) * _VATScale;
                float3 pivotVec   = input.positionOS.xyz - piecePivot;

                float frameF, frameN, interp;
                VATComputeFrame(_AutoPlay, _ManualFrame, _PlaySpeed, _HoudiniFPS,
                                _NumOfFrames, _Interpolate, frameF, frameN, interp);

                float2 uvA = input.uvSamp; uvA.y -= (frameF - 1.0) / _NumOfFrames;
                float2 uvB = input.uvSamp; uvB.y -= (frameN - 1.0) / _NumOfFrames;

                float4 pT0 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvA, 0);
                float4 pT1 = SAMPLE_TEXTURE2D_LOD(_PosTex, sampler_PosTex, uvB, 0);
                float4 rT0 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvA, 0);
                float4 rT1 = SAMPLE_TEXTURE2D_LOD(_RotTex, sampler_RotTex, uvB, 0);

                float4 quat = VATDecodeQuat(rT0, pT0);
                float4 qN   = VATDecodeQuat(rT1, pT1);
                if (dot(quat, qN) < 0.0) qN = -qN;
                quat = normalize(lerp(quat, qN, interp));

                float3 posOS = (_PivotMode > 0.5)
                            ? (piecePivot + lerp(pT0.rgb, pT1.rgb, interp) * _VATScale + VATRotateVec(pivotVec, quat))
                            : (lerp(pT0.rgb, pT1.rgb, interp) * _VATScale + VATRotateVec(pivotVec, quat));
                float3 nrmOS = VATRotateVec(input.normalOS, quat);

                output.positionCS = TransformWorldToHClip(TransformObjectToWorld(posOS));
                output.normalWS   = TransformObjectToWorldNormal(nrmOS);
                return output;
            }

            half4 VATDepthNormalsFrag(VATDN_V2F i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return half4(NormalizeNormalPerPixel(i.normalWS), 0.0);
            }
            ENDHLSL
        }

        // ==================================================================
        // Pass 5 — Meta (lightmap 烘焙, 使用基础姿态, 与 Lit_New 一致)
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
            #include "../../ShaderLibrary/Surface/Setup.hlsl"

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
