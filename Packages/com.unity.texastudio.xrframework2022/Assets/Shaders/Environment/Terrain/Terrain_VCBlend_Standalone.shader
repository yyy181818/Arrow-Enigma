// ============================================================================
// Terrain Vertex-Color Blend (Standalone)
//
// 完全独立版本：只依赖 URP 自带的 Core.hlsl / Lighting.hlsl / Shadows.hlsl，
// 不引用任何自定义 ShaderLibrary，不使用 LWGUI 自定义面板。
//
// 混合原理（与原 MaterialBlend_VecterColor 一致）：
//   对齐 Unity 内置地形 shader 的 Layer 结构：Albedo + Normal + Mask(RGBA)
//     Mask 通道打包（与 Unity TerrainLayer.MaskMap 一致）：
//       R = Metallic   G = AO   B = Height   A = Smoothness
//   1. 每层从各自 Splat 取 Albedo(RGB)；从 Mask 取 MRO+H
//   2. layerFactor = 顶点色通道 + CheapContrast(高度对比, Mask.B)
//        R -> Layer1    G -> Layer2    B -> Layer3
//   3. top = max(所有 factor) - 混合过渡宽度
//   4. weight = max(factor - top, 0)，再归一化
//   5. Albedo / Normal / Metallic / AO / Smoothness 按 weight 线性混合
//   6. 每层金属度/光滑度 = 该层常量(_LayerN_Metallic/_LayerN_Smoothness) × Mask 对应通道；
//      未拖 Mask 时直接用该层常量；最终光滑度再 × 顶点色A（白=光滑，黑=粗糙）
//   7. Metallic 最终 = 层混合(_LayerN_Metallic × Mask.R)；地形默认各层 0 → 整体非金属
//
// 与原版的差异（都是为了开箱可用 / 修 bug）：
//   - Splat 默认贴图 "white"（原版 "black" 会让未赋图的层直接变纯黑）
//   - 法线默认 "bump"（原版 "black" 会让 UnpackNormalScale 出 NaN）
//   - 三层 BaseColor 给了不同默认色，不贴图也能立刻看出混合分布
//   - 内置 Debug 下拉：可直出顶点色 / 混合权重 / 世界法线
//   - CBUFFER 提到 HLSLINCLUDE，四个 Pass 布局一致 -> SRP Batcher 兼容
// ============================================================================
Shader "TA Shaders/Environment/Terrain/VCBlend_Standalone"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("剔除模式", Float) = 2
        _BlendContrast ("混合过渡宽度", Range(0.001, 1)) = 0.1

        [Space(12)][Header(Layer 1   Vertex Color R)][Space(4)]
        _Splat0 ("L1 Albedo (RGB)", 2D) = "white" {}
        _Layer1_BaseColor ("L1 颜色", Color) = (0.36, 0.55, 0.24, 1)
        _Layer1_addColor ("L1 叠加色", Color) = (0, 0, 0, 0)
        [Normal] _Layer1_Normal ("L1 法线图", 2D) = "bump" {}
        _Layer1_NormalIntensity ("L1 法线强度", Range(0, 4)) = 1
        _Layer1_Mask ("L1 Mask (R=Metallic G=AO B=Height A=Smooth)", 2D) = "white" {}
        _Layer1_Tilling ("L1 平铺", Float) = 1
        _Layer1_HeightContrast ("L1 高度对比 (混合锐度)", Range(0, 1)) = 0
        _Layer1_Metallic ("L1 金属度 (无 Mask 时直接生效)", Range(0, 1)) = 0
        _Layer1_Smoothness ("L1 光滑度 (无 Mask 时直接生效)", Range(0, 1)) = 0.5

        [Space(12)][Header(Layer 2   Vertex Color G)][Space(4)]
        _Splat1 ("L2 Albedo (RGB)", 2D) = "white" {}
        _Layer2_BaseColor ("L2 颜色", Color) = (0.52, 0.37, 0.22, 1)
        _Layer2_addColor ("L2 叠加色", Color) = (0, 0, 0, 0)
        [Normal] _Layer2_Normal ("L2 法线图", 2D) = "bump" {}
        _Layer2_NormalIntensity ("L2 法线强度", Range(0, 4)) = 1
        _Layer2_Mask ("L2 Mask (R=Metallic G=AO B=Height A=Smooth)", 2D) = "white" {}
        _Layer2_Tilling ("L2 平铺", Float) = 1
        _Layer2_HeightContrast ("L2 高度对比 (混合锐度)", Range(0, 1)) = 0
        _Layer2_Metallic ("L2 金属度 (无 Mask 时直接生效)", Range(0, 1)) = 0
        _Layer2_Smoothness ("L2 光滑度 (无 Mask 时直接生效)", Range(0, 1)) = 0.5

        [Space(12)][Header(Layer 3   Vertex Color B)][Space(4)]
        [Toggle(_LAYER3)] _Layer3Enable ("启用 Layer3", Float) = 1
        _Splat2 ("L3 Albedo (RGB)", 2D) = "white" {}
        _Layer3_BaseColor ("L3 颜色", Color) = (0.70, 0.70, 0.74, 1)
        _Layer3_addColor ("L3 叠加色", Color) = (0, 0, 0, 0)
        [Normal] _Layer3_Normal ("L3 法线图", 2D) = "bump" {}
        _Layer3_NormalIntensity ("L3 法线强度", Range(0, 4)) = 1
        _Layer3_Mask ("L3 Mask (R=Metallic G=AO B=Height A=Smooth)", 2D) = "white" {}
        _Layer3_Tilling ("L3 平铺", Float) = 1
        _Layer3_HeightContrast ("L3 高度对比 (混合锐度)", Range(0, 1)) = 0
        _Layer3_Metallic ("L3 金属度 (无 Mask 时直接生效)", Range(0, 1)) = 0
        _Layer3_Smoothness ("L3 光滑度 (无 Mask 时直接生效)", Range(0, 1)) = 0.5

        [Space(12)][Header(Sampling)][Space(4)]
        _Sample ("UV 采样方式", Float) = 1
        _WorldUVScale ("World 采样缩放", Float) = 0.1

        [Space(12)][Header(Debug)][Space(4)]
        _Debug ("调试显示", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        // --------------------------------------------------------------------
        // 所有 Pass 共享：URP Core + 统一的 UnityPerMaterial（SRP Batcher 要求
        // 每个 Pass 的 cbuffer 布局完全一致）
        // --------------------------------------------------------------------
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Splat0_ST;
            float4 _Splat1_ST;
            float4 _Splat2_ST;
            float4 _Layer1_Normal_ST;
            float4 _Layer2_Normal_ST;
            float4 _Layer3_Normal_ST;
            float4 _Layer1_Mask_ST;
            float4 _Layer2_Mask_ST;
            float4 _Layer3_Mask_ST;

            half4  _Layer1_BaseColor;
            half4  _Layer2_BaseColor;
            half4  _Layer3_BaseColor;
            half4  _Layer1_addColor;
            half4  _Layer2_addColor;
            half4  _Layer3_addColor;

            float  _Layer1_Tilling;
            float  _Layer2_Tilling;
            float  _Layer3_Tilling;
            half   _Layer1_NormalIntensity;
            half   _Layer2_NormalIntensity;
            half   _Layer3_NormalIntensity;
            half   _Layer1_HeightContrast;
            half   _Layer2_HeightContrast;
            half   _Layer3_HeightContrast;
            half   _Layer1_Metallic;
            half   _Layer2_Metallic;
            half   _Layer3_Metallic;
            half   _Layer1_Smoothness;
            half   _Layer2_Smoothness;
            half   _Layer3_Smoothness;

            half   _BlendContrast;
            float  _WorldUVScale;
            float  _CullMode;
            float  _Layer3Enable;
            float  _Sample;
            float  _Debug;
        CBUFFER_END
        ENDHLSL

        // ====================================================================
        // Forward Lit
        // ====================================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_CullMode]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex   VCBlendVert
            #pragma fragment VCBlendFrag

            // ---- 材质关键字 ----
            #pragma shader_feature_local _LAYER3
            #pragma shader_feature_local _SAMPLE_UV _SAMPLE_WORLD
            #pragma shader_feature_local_fragment _DEBUG_OFF _DEBUG_VERTEXCOLOR _DEBUG_WEIGHT _DEBUG_NORMAL

            // ---- URP 光照关键字 ----
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS        : POSITION;
                float3 normalOS          : NORMAL;
                float4 tangentOS         : TANGENT;
                float2 texcoord          : TEXCOORD0;
                float2 staticLightmapUV  : TEXCOORD1;
                float2 dynamicLightmapUV : TEXCOORD2;
                float4 color             : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float4 positionWSAndFog : TEXCOORD1;   // xyz = positionWS, w = fogFactor
                half3  normalWS         : TEXCOORD2;
                half4  tangentWS        : TEXCOORD3;   // xyz = tangent, w = sign
                half4  color            : TEXCOORD4;   // 顶点色 RGBA（混合权重来源）
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
                #ifdef DYNAMICLIGHTMAP_ON
                float2 dynamicLightmapUV : TEXCOORD6;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_Splat0);        SAMPLER(sampler_Splat0);
            TEXTURE2D(_Splat1);        SAMPLER(sampler_Splat1);
            TEXTURE2D(_Splat2);        SAMPLER(sampler_Splat2);
            TEXTURE2D(_Layer1_Normal); SAMPLER(sampler_Layer1_Normal);
            TEXTURE2D(_Layer2_Normal); SAMPLER(sampler_Layer2_Normal);
            TEXTURE2D(_Layer3_Normal); SAMPLER(sampler_Layer3_Normal);
            TEXTURE2D(_Layer1_Mask);   SAMPLER(sampler_Layer1_Mask);
            TEXTURE2D(_Layer2_Mask);   SAMPLER(sampler_Layer2_Mask);
            TEXTURE2D(_Layer3_Mask);   SAMPLER(sampler_Layer3_Mask);

            // 原版的 CheapContrast：把 0~1 的高度重映射到 [-contrast, contrast] 再 clamp
            half CheapContrast(half contrast, half x)
            {
                return clamp(lerp(-contrast, contrast, x), 0.0h, 1.0h);
            }

            Varyings VCBlendVert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(v.normalOS, v.tangentOS);

                o.positionCS       = posInputs.positionCS;
                o.positionWSAndFog = float4(posInputs.positionWS, ComputeFogFactor(posInputs.positionCS.z));
                o.normalWS         = half3(nrmInputs.normalWS);

                real tangentSign = v.tangentOS.w * GetOddNegativeScale();
                o.tangentWS = half4(half3(nrmInputs.tangentWS.xyz), half(tangentSign));

                o.uv    = v.texcoord;
                o.color = half4(v.color);

                OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, o.staticLightmapUV);
                #ifdef DYNAMICLIGHTMAP_ON
                o.dynamicLightmapUV = v.dynamicLightmapUV * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                OUTPUT_SH(o.normalWS.xyz, o.vertexSH);

                return o;
            }

            half4 VCBlendFrag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 positionWS = i.positionWSAndFog.xyz;
                half   fogFactor  = half(i.positionWSAndFog.w);

                // ---------------- 采样 UV ----------------
                #if defined(_SAMPLE_WORLD)
                    float2 baseUV = positionWS.xz * _WorldUVScale;
                #else
                    float2 baseUV = i.uv;
                #endif

                // ---------------- TBN ----------------
                half3 nWS = normalize(i.normalWS);
                half3 tWS = normalize(i.tangentWS.xyz);
                half3 bWS = normalize(cross(nWS, tWS) * i.tangentWS.w);
                half3x3 TBN = half3x3(tWS, bWS, nWS);

                // ---------------- Layer 1 (顶点色 R) ----------------
                float2 uv1  = baseUV * _Layer1_Tilling;
                half4  tex1 = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv1);
                half3  col1 = tex1.rgb * _Layer1_BaseColor.rgb + _Layer1_addColor.rgb;
                half3  nrm1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer1_Normal, sampler_Layer1_Normal, uv1), _Layer1_NormalIntensity);
                nrm1 = normalize(mul(nrm1, TBN));
                half4  m1   = SAMPLE_TEXTURE2D(_Layer1_Mask, sampler_Layer1_Mask, uv1);
                half   h1   = CheapContrast(_Layer1_HeightContrast, m1.b);

                // ---------------- Layer 2 (顶点色 G) ----------------
                float2 uv2  = baseUV * _Layer2_Tilling;
                half4  tex2 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, uv2);
                half3  col2 = tex2.rgb * _Layer2_BaseColor.rgb + _Layer2_addColor.rgb;
                half3  nrm2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer2_Normal, sampler_Layer2_Normal, uv2), _Layer2_NormalIntensity);
                nrm2 = normalize(mul(nrm2, TBN));
                half4  m2   = SAMPLE_TEXTURE2D(_Layer2_Mask, sampler_Layer2_Mask, uv2);
                half   h2   = CheapContrast(_Layer2_HeightContrast, m2.b);

                // ---------------- Layer 3 (顶点色 B) ----------------
                #if defined(_LAYER3)
                float2 uv3  = baseUV * _Layer3_Tilling;
                half4  tex3 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, uv3);
                half3  col3 = tex3.rgb * _Layer3_BaseColor.rgb + _Layer3_addColor.rgb;
                half3  nrm3 = UnpackNormalScale(SAMPLE_TEXTURE2D(_Layer3_Normal, sampler_Layer3_Normal, uv3), _Layer3_NormalIntensity);
                nrm3 = normalize(mul(nrm3, TBN));
                half4  m3   = SAMPLE_TEXTURE2D(_Layer3_Mask, sampler_Layer3_Mask, uv3);
                half   h3   = CheapContrast(_Layer3_HeightContrast, m3.b);
                #endif

                // ---------------- 权重计算（核心混合逻辑）----------------
                half f1 = i.color.r + h1;
                half f2 = i.color.g + h2;

                #if defined(_LAYER3)
                    half f3   = i.color.b + h3;
                    half top  = max(max(f1, f2), f3) - _BlendContrast;
                    half w1   = max(f1 - top, 0.0h);
                    half w2   = max(f2 - top, 0.0h);
                    half w3   = max(f3 - top, 0.0h);
                    half wSum = w1 + w2 + w3 + 1e-4h;
                    w1 /= wSum; w2 /= wSum; w3 /= wSum;

                    half3 albedo    = col1 * w1 + col2 * w2 + col3 * w3;
                    half3 normalWS  = normalize(nrm1 * w1 + nrm2 * w2 + nrm3 * w3);
                    half3 weightDbg = half3(w1, w2, w3);
                    // 每层的金属度/光滑度 = 该层常量 × Mask 对应通道（未拖 Mask 时 Mask=white，即直接用该层常量）
                    half  met = w1 * (_Layer1_Metallic * m1.r) + w2 * (_Layer2_Metallic * m2.r) + w3 * (_Layer3_Metallic * m3.r);
                    half  ao  = w1 * m1.g + w2 * m2.g + w3 * m3.g;
                    half  sm  = w1 * (_Layer1_Smoothness * m1.a) + w2 * (_Layer2_Smoothness * m2.a) + w3 * (_Layer3_Smoothness * m3.a);
                #else
                    half top  = max(f1, f2) - _BlendContrast;
                    half w1   = max(f1 - top, 0.0h);
                    half w2   = max(f2 - top, 0.0h);
                    half wSum = w1 + w2 + 1e-4h;
                    w1 /= wSum; w2 /= wSum;

                    half3 albedo    = col1 * w1 + col2 * w2;
                    half3 normalWS  = normalize(nrm1 * w1 + nrm2 * w2);
                    half3 weightDbg = half3(w1, w2, 0.0h);
                    half  met = w1 * (_Layer1_Metallic * m1.r) + w2 * (_Layer2_Metallic * m2.r);
                    half  ao  = w1 * m1.g + w2 * m2.g;
                    half  sm  = w1 * (_Layer1_Smoothness * m1.a) + w2 * (_Layer2_Smoothness * m2.a);
                #endif

                // ---------------- 调试直出（不走光照）----------------
                #if defined(_DEBUG_VERTEXCOLOR)
                    return half4(i.color.rgb, 1.0h);
                #elif defined(_DEBUG_WEIGHT)
                    return half4(weightDbg, 1.0h);
                #elif defined(_DEBUG_NORMAL)
                    return half4(normalWS * 0.5h + 0.5h, 1.0h);
                #endif

                // ---------------- 顶点色 A × 层光滑度 → 最终光滑度 ----------------
                // 顶点色 A：白=光滑，黑=粗糙；与 Unity 默认 smoothness 贴图语义一致
                half smoothness = saturate(sm * i.color.a);
                half roughness  = max(1.0h - smoothness, 0.001h);

                // ---------------- 走 URP 标准 PBR ----------------
                SurfaceData surface = (SurfaceData)0;
                surface.albedo              = albedo;
                surface.specular            = half3(0.0h, 0.0h, 0.0h);
                surface.metallic            = met;
                surface.smoothness          = smoothness;
                surface.normalTS            = half3(0.0h, 0.0h, 1.0h);
                surface.emission            = half3(0.0h, 0.0h, 0.0h);
                surface.occlusion           = ao;
                surface.alpha               = 1.0h;
                surface.clearCoatMask       = 0.0h;
                surface.clearCoatSmoothness = 0.0h;

                InputData inputData = (InputData)0;
                inputData.positionWS      = positionWS;
                inputData.positionCS      = i.positionCS;
                inputData.normalWS        = normalWS;               // 已在世界空间混合完毕
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(positionWS);
                inputData.shadowCoord     = TransformWorldToShadowCoord(positionWS);
                inputData.fogCoord        = fogFactor;
                inputData.vertexLighting  = half3(0.0h, 0.0h, 0.0h);
                inputData.tangentToWorld  = TBN;
                #if defined(DYNAMICLIGHTMAP_ON)
                    inputData.bakedGI = SAMPLE_GI(i.staticLightmapUV, i.dynamicLightmapUV, i.vertexSH, normalWS);
                #else
                    inputData.bakedGI = SAMPLE_GI(i.staticLightmapUV, i.vertexSH, normalWS);
                #endif
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                inputData.shadowMask              = SAMPLE_SHADOWMASK(i.staticLightmapUV);

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, fogFactor);
                color.a   = 1.0h;
                return color;
            }
            ENDHLSL
        }

        // ====================================================================
        // Shadow Caster（自包含，不依赖 LitInput）
        // ====================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex   ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 GetShadowPositionHClip(ShadowAttributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        // ====================================================================
        // Depth Only（自包含）
        // ====================================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex   DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings DepthVert(DepthAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthFrag(DepthVaryings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }

        // ====================================================================
        // Depth Normals（自包含，供 SSAO / 深度法线特效使用）
        // ====================================================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex   DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_instancing

            struct DNAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DNVaryings
            {
                float4 positionCS : SV_POSITION;
                half3  normalWS   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DNVaryings DepthNormalsVert(DNAttributes input)
            {
                DNVaryings output = (DNVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS   = half3(TransformObjectToWorldNormal(input.normalOS));
                return output;
            }

            half4 DepthNormalsFrag(DNVaryings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0h);
            }
            ENDHLSL
        }

        // ====================================================================
        // Meta（向光照烘焙器提供表面 Albedo / Emission）
        // 作用：Baked / Mixed 主光下，让地形自身直接光能被烘焙进 Lightmap；
        //       仅 Realtime 主光时此 Pass 不触发，不影响正常光照。
        // ====================================================================
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma exclude_renderers gles gles3 glcore
            #pragma vertex   MetaVert
            #pragma fragment MetaFrag

            #pragma shader_feature_local _LAYER3
            #pragma shader_feature_local _SAMPLE_UV _SAMPLE_WORLD

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct MetaAttributes
            {
                float4 positionOS : POSITION;
                float2 uv0        : TEXCOORD0;
                float2 uv1        : TEXCOORD1;   // static lightmap uv
                float2 uv2        : TEXCOORD2;   // dynamic lightmap uv
                float4 color      : COLOR;
            };

            struct MetaVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 color      : TEXCOORD2;
            };

            TEXTURE2D(_Splat0);        SAMPLER(sampler_Splat0);
            TEXTURE2D(_Splat1);        SAMPLER(sampler_Splat1);
            TEXTURE2D(_Splat2);        SAMPLER(sampler_Splat2);
            TEXTURE2D(_Layer1_Normal); SAMPLER(sampler_Layer1_Normal);
            TEXTURE2D(_Layer2_Normal); SAMPLER(sampler_Layer2_Normal);
            TEXTURE2D(_Layer3_Normal); SAMPLER(sampler_Layer3_Normal);
            TEXTURE2D(_Layer1_Mask);   SAMPLER(sampler_Layer1_Mask);
            TEXTURE2D(_Layer2_Mask);   SAMPLER(sampler_Layer2_Mask);
            TEXTURE2D(_Layer3_Mask);   SAMPLER(sampler_Layer3_Mask);

            half CheapContrast(half contrast, half x)
            {
                return clamp(lerp(-contrast, contrast, x), 0.0h, 1.0h);
            }

            MetaVaryings MetaVert(MetaAttributes v)
            {
                MetaVaryings o = (MetaVaryings)0;
                o.positionCS = UnityMetaVertexPosition(v.positionOS, v.uv1, v.uv2, unity_LightmapST, unity_DynamicLightmapST);
                o.uv         = v.uv0;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.color      = half4(v.color);
                return o;
            }

            // 与 ForwardLit 一致的减式混合，只取 albedo 给烘焙器
            half3 ComputeBlendAlbedo(MetaVaryings i)
            {
                #if defined(_SAMPLE_WORLD)
                    float2 baseUV = i.positionWS.xz * _WorldUVScale;
                #else
                    float2 baseUV = i.uv;
                #endif

                float2 uv1  = baseUV * _Layer1_Tilling;
                half4  tex1 = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv1);
                half3  col1 = tex1.rgb * _Layer1_BaseColor.rgb + _Layer1_addColor.rgb;

                float2 uv2  = baseUV * _Layer2_Tilling;
                half4  tex2 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, uv2);
                half3  col2 = tex2.rgb * _Layer2_BaseColor.rgb + _Layer2_addColor.rgb;

                half f1 = i.color.r;
                half f2 = i.color.g;

                #if defined(_LAYER3)
                    float2 uv3  = baseUV * _Layer3_Tilling;
                    half4  tex3 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, uv3);
                    half3  col3 = tex3.rgb * _Layer3_BaseColor.rgb + _Layer3_addColor.rgb;
                    half f3   = i.color.b;
                    half top  = max(max(f1, f2), f3) - _BlendContrast;
                    half w1   = max(f1 - top, 0.0h);
                    half w2   = max(f2 - top, 0.0h);
                    half w3   = max(f3 - top, 0.0h);
                    half wSum = w1 + w2 + w3 + 1e-4h;
                    w1 /= wSum; w2 /= wSum; w3 /= wSum;
                    return col1 * w1 + col2 * w2 + col3 * w3;
                #else
                    half top  = max(f1, f2) - _BlendContrast;
                    half w1   = max(f1 - top, 0.0h);
                    half w2   = max(f2 - top, 0.0h);
                    half wSum = w1 + w2 + 1e-4h;
                    w1 /= wSum; w2 /= wSum;
                    return col1 * w1 + col2 * w2;
                #endif
            }

            half4 MetaFrag(MetaVaryings i) : SV_Target
            {
                half3 albedo = ComputeBlendAlbedo(i);

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo        = albedo;
                metaInput.Emission      = half3(0.0h, 0.0h, 0.0h);
                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "IKASAMADUS.TerrainVCBlendShaderGUI"
}
