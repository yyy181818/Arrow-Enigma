Shader "TA Shaders/Particle/Advance"
{
    Properties
    {
        [Enum(Off, 0, On, 1)]_ZWriteMode ("ZWrite", float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        [Toggle(_VERTCOLOR)] _VertColor ("VertColor", Float) = 1
        [MainTexture]_BaseMap ("主贴图", 2D) = "white" {}
        [MainColor][HDR]_BaseColor("颜色",Color) = (1,1,1,1)
        [Toggle]_MixAlpha("RGB混合Alpha",float)=0
        [Toggle]_MixRGB("RGB乘以Alpha(预乘透明模式)",float)=0
        //遮罩
        [Header(Mask)]
        [Space(10)]
        [Toggle(_USEMASK_ON)] _Mask ("Mask", Float) = 0
        [Enum(R,0,A,1)]_MaskChannel("遮罩通道",float) = 0
        _MaskTex("遮罩图",2D) = "white"{}
        //[SubToggle(Mask)]_MaskU2("特效控制z",float)=0
        _MaskMinMaxSacle("最小值(X)最大值(Y)强度(Z)",Vector)=(0,1,1,0)

        //扭曲
        [Header(Noise)]
        [Space(10)]
        [Toggle(_USENOISE_ON)] _Twist ("扭曲", Float) = 0
        _NoiseTex("扭曲图",2D) = "white"{}
        [Toggle(Twist)]_NoiseU2("特效控制w",float)=0
        _NoiseScale("扭曲强度X",float) = 1

        //溶解
        [Header(Dissolve)]
        [Space(10)]
        [Toggle(_USEDISSOLVE_ON)] _Dissolve ("溶解", Float) = 0
        _DissolveTex("溶解贴图",2D) = "white" {}
        [Toggle]_DissolvelU2("特效使用UV2",float)=0
        _DissolveInt("溶解值",Range(0,1)) = 0
        _DissolveHardness("边缘硬度",Range(0,1)) = 1
        _DissolveEdg("边缘宽度",Range(0,1)) = 0
        [HDR]_DissolveColor("边缘颜色",Color) = (1,1,1,1)
        [Toggle(_USELIFETIME)] _UseLifetime ("粒子系统生命周期控制溶解", float) = 0

        //纹理流动
        [Header(Flow)]
        [Space(10)]
        [Toggle(_FLOWMAP_ON)] _Flow ("流动", Float) = 0
        _MainMaskFlow("主贴图(xy)遮罩(zw)",Vector) = (0,0,0,0)
        _NoiseDissFlow("扭曲(xy)溶解(zw)",Vector) = (0,0,0,0)

        _uvRotate("主贴图(x)遮罩(y)扭曲(z)溶解(w)",Vector) = (0,0,0,0)

        //Fresnel
        [Header(Fresnel)]
        [Space(10)]
        [Toggle(_FRESNEL_ON)] _Fresnel ("Fresnel", Float) = 0
        [HDR]_FresnelColor("颜色",Color) = (0,0,0,0)
        _FresnelScale("Scale", Range(0.0, 2.0)) = 1.0
        _FresnelPower("Power", Range(0.0, 10.0)) = 3
        _FresnelBias1("Bias", Range(0.0, 10.0)) = 0.0

        [HDR]_InnerFresnelColor("反向Fresnel颜色",Color) = (0,0,0,0)
        _InnerFresnelScale("反向FresnelScale", Range(0.0, 2.0)) = 1.0
        _InnerFresnelPower("反向FresnelPower", Range(0.0, 10.0)) = 3
        _InnerFresnelBias1("反向FresnelBias", Range(0.0, 10.0)) = 0.0

        //其它
        [Header(BlendMode)]
        [Space(10)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", float) = 5.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("Dst Blend", float) = 10.0
        //[Enum(Off,0,On,1)]_ZWriteMode("ZWrite Mode", Int) = 0
        //[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("ZTest Mode", Int) = 4
        [HideInInspector]_PartcleAdvanceAlpha("PartcleAdvanceAlpha", float) = 0
        
        [Header(Breathing)]
        [Space(10)]
        [Toggle(_BREATHING)] _Breathing ("呼吸效果(Alpha)", Float) = 0
        _Speed ("呼吸速度", float) = 0
        //[Main(GlobalLight, _GLOBALLGIHT, on)] _GlobalLight ("GlobalLight", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline""Queue"="Transparent+55"
        }
        LOD 100

        HLSLINCLUDE
        //#include "../ShaderLibrary/CustomLitInputParticle.hlsl"
        #include "Packages/com.unity.texastudio.xrframework2022/Assets/Shaders/ShaderLibrary/CustomLitInputParticle.hlsl"
        //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


        TEXTURE2D (_BaseMap);
        SAMPLER (sampler_BaseMap);

        #ifdef _USEMASK_ON
        TEXTURE2D (_MaskTex);
        SAMPLER (sampler_MaskTex);
        #endif

        #ifdef _USENOISE_ON
        TEXTURE2D (_NoiseTex);
        SAMPLER (sampler_NoiseTex);
        #endif

        #ifdef _USEDISSOLVE_ON
        TEXTURE2D (_DissolveTex);
        SAMPLER (sampler_DissolveTex);
        #endif
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags
            {
                "LightMode"="UniversalForward"
            }
            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZWrite [_ZWriteMode]
            ZTest [_ZTestMode]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _USEMASK_ON
            #pragma shader_feature_local _USENOISE_ON
            #pragma shader_feature_local _USEDISSOLVE_ON
            #pragma shader_feature_local _FLOWMAP_ON
            #pragma shader_feature_local _USELIFETIME
            #pragma shader_feature_local _FRESNEL_ON
            //#pragma shader_feature_local_fragment _GLOBALLGIHT
            #pragma shader_feature_local _VERTCOLOR
            #pragma shader_feature_local_fragment _BREATHING
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"


            struct AttributesParticle
            {
                float4 positionOS : POSITION;
                #if defined (_USELIFETIME)
                float4 texcoords : TEXCOORD0;
                #else
                float2 texcoords : TEXCOORD0;
                #endif
                #if defined(_VERTCOLOR)
                half4 color : COLOR;
                #endif

                float4 uv2 : TEXCOORD1;
                #if defined (_FRESNEL_ON)
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsParticle
            {
                #ifdef _USEMASK_ON
                float4 uv : TEXCOORD0;
                #else
                float2 uv : TEXCOORD0;
                #endif

                float4 uv2 : TEXCOORD1;

                #if defined(_USENOISE_ON) || defined(_USEDISSOLVE_ON)
                float4 noiseDissUV : TEXCOORD2;
                #endif
                #if defined(_USEDISSOLVE_ON)
                float2 dissCoord : TEXCOORD3;
                #endif

                float4 vertex : SV_POSITION;
                #if defined(_VERTCOLOR)
                half4 color : COLOR;
                #endif

                #if defined (_FRESNEL_ON)
                float3 normalWS : TEXCOORD4;
                float3 positionWS : TEXCOORD5;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };


            VaryingsParticle vert(AttributesParticle v)
            {
                VaryingsParticle o = (VaryingsParticle)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.vertex = vertexInput.positionCS;
                #ifdef _FLOWMAP_ON
                float2 mainFlow = float2(_MainMaskFlow.x, _MainMaskFlow.y) * _Time.y;
                float2 maskFlow = float2(_MainMaskFlow.z, _MainMaskFlow.w) * _Time.y;
                float2 noiseFlow = float2(_NoiseDissFlow.x, _NoiseDissFlow.y) * _Time.y;
                float2 dissFlow = float2(_NoiseDissFlow.z, _NoiseDissFlow.w) * _Time.y;
                #endif

                o.uv2 = v.uv2;

                o.uv.xy = TRANSFORM_TEX(v.texcoords, _BaseMap);
                //o.uv.xy = v.uv;
                #ifdef _FLOWMAP_ON
                o.uv.xy += frac(mainFlow);
                #endif

                #ifdef _USEMASK_ON
                o.uv.zw = TRANSFORM_TEX(v.texcoords * _MaskTex_ST.xy + _MaskTex_ST.zw, _MaskTex);
                #ifdef _FLOWMAP_ON
                o.uv.zw += frac(maskFlow);
                #endif
                #endif
                #if defined(_USENOISE_ON) || defined(_USEDISSOLVE_ON)
                o.noiseDissUV = float4(1, 1, 1, 1);
                #endif

                #ifdef _USENOISE_ON
                o.noiseDissUV.xy = TRANSFORM_TEX(v.texcoords, _NoiseTex);
                #ifdef _FLOWMAP_ON
                o.noiseDissUV.xy += frac(noiseFlow);
                #endif
                #endif

                #ifdef _USEDISSOLVE_ON
                half hard = _DissolveHardness * 0.99f; //防止1-导致分母为0
                half edg = _DissolveEdg + 1;
                #if defined (_USELIFETIME)
                float DissolveInt = lerp(lerp(_DissolveInt, 1, v.texcoords.z), o.uv2.z, _DissolvelU2);
                #else
                float DissolveInt = lerp(_DissolveInt, o.uv2.z, _DissolvelU2);
                #endif
                DissolveInt = 1 - DissolveInt;
                o.dissCoord.x = (DissolveInt * edg + 1 - hard) * (2 - hard) - edg;
                o.dissCoord.y = DissolveInt * edg * (2 - hard) - 1;

                o.noiseDissUV.zw = TRANSFORM_TEX(v.texcoords, _DissolveTex);
                #ifdef _FLOWMAP_ON
                o.noiseDissUV.zw += frac(dissFlow);
                #endif
                #endif
                #if defined (_FRESNEL_ON)
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                //VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.normalWS = normalInput.normalWS;
                o.positionWS = vertexInput.positionWS;
                #endif
                #if defined(_VERTCOLOR)
                o.color = v.color;
                #endif

                return o;
            }

            half4 frag(VaryingsParticle i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                #if defined (_FRESNEL_ON)
                float3 vDirWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float3 nDirWS = normalize(i.normalWS);
                #endif

                float2 mainUV = i.uv.xy - float2(0.5, 0.5);
                half2x2 RotX = half2x2(cos(_uvRotate.x), -sin(_uvRotate.x), sin(_uvRotate.x), cos(_uvRotate.x));
                mainUV = mul(RotX, mainUV);
                mainUV += float2(0.5, 0.5);
                #ifdef _USENOISE_ON
                float2 noiseUV = i.noiseDissUV.xy - float2(0.5, 0.5);
                half2x2 RotZ = half2x2(cos(_uvRotate.z), -sin(_uvRotate.z), sin(_uvRotate.z), cos(_uvRotate.z));
                noiseUV = mul(RotZ, noiseUV);
                noiseUV += float2(0.5, 0.5);
                half4 noiseTex = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV);
                half noise = (noiseTex.r);
                _NoiseScale = lerp(_NoiseScale, i.uv2.y, _NoiseU2);
                mainUV += noiseTex.r * _NoiseScale;
                #endif

                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, mainUV);
                #if defined(_VERTCOLOR)
                float4 col = baseMap * _BaseColor * i.color;
                #else
                float4 col = baseMap * _BaseColor;
                #endif

                #ifdef _USEMASK_ON
                float2 maskUV = i.uv.zw - float2(0.5, 0.5);
                half2x2 RotY = half2x2(cos(_uvRotate.y), -sin(_uvRotate.y), sin(_uvRotate.y), cos(_uvRotate.y));
                maskUV = mul(RotY, maskUV);
                maskUV += float2(0.5, 0.5);
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskUV);
                half mask = maskTex.r * (1 - _MaskChannel) + maskTex.a * (_MaskChannel);
                //_MaskScale = lerp(_MaskScale,i.uv2.x,_MaskU2);
                mask = lerp(0, mask, _MaskMinMaxSacle.z);
                mask = smoothstep(_MaskMinMaxSacle.x, _MaskMinMaxSacle.y, mask);
                col *= mask;
                #endif

                #ifdef _USEDISSOLVE_ON
                float2 dissolveUV = i.noiseDissUV.zw - float2(0.5, 0.5);
                half2x2 RotW = half2x2(cos(_uvRotate.w), -sin(_uvRotate.w), sin(_uvRotate.w), cos(_uvRotate.w));
                dissolveUV = mul(RotW, dissolveUV);
                dissolveUV += float2(0.5, 0.5);
                half4 dissTex = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, dissolveUV);
                half dissMask = pow(dissTex.r, 1 / 2.2f);
                half hard = _DissolveHardness * 0.99f; //防止1-导致分母为0

                half dissColorLerp = saturate((dissMask + i.dissCoord.x) / (1 - hard));
                col.rgb = lerp(_DissolveColor, col.rgb, dissColorLerp);

                half dissAlpha = saturate((dissMask + i.dissCoord.y) / (1 - hard));
                col.a *= dissAlpha;
                #endif
                #if defined (_FRESNEL_ON)
                half RimMask = saturate(pow(1 - saturate(dot(nDirWS, vDirWS)), _FresnelPower)) * _FresnelScale +
                    _FresnelBias1;
                half4 RimMaskColor = RimMask * _FresnelColor;

                half InnerRimMask = saturate(pow(saturate(dot(nDirWS, vDirWS)), _InnerFresnelPower)) *
                    _InnerFresnelScale + _InnerFresnelBias1;
                half4 InnerRimMaskColor = InnerRimMask * _InnerFresnelColor;
                col.a *= RimMask + InnerRimMask;
                col.rgb *= RimMaskColor.rgb + InnerRimMaskColor.rgb;
                #endif

                half mixAlpha = lerp(1, baseMap.r, _MixAlpha);
                col.a *= mixAlpha;
                col.rgb = _MixRGB ? col.rgb * col.a : col.rgb;
                #if defined(_BREATHING)
                half b = sin(_Time.y * _Speed) * 0.5f + 0.5f;
                col.a = lerp(0, col.a, b);
                #endif

                // #if defined (_GLOBALLGIHT)
                // return half4(col.rgb * (1 - _Light), saturate(col.a)) * (1 - _PartcleAdvanceAlpha);
                // #else
                // return half4(col.rgb, saturate(col.a)) * (1 - _PartcleAdvanceAlpha);
                // #endif

                return half4(col.rgb * (1 - _Light), saturate(col.a)) * (1 - _PartcleAdvanceAlpha);
            }
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
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            //#include "../ShaderLibrary/CustomLitInputParticle.hlsl"
            #include "Packages/com.unity.texastudio.xrframework2022/Assets/Shaders/ShaderLibrary/CustomLitInputParticle.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesDepthOnlyPass.hlsl"
            ENDHLSL
        }


    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}