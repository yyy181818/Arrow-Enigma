Shader "TA Shaders/Particle/Base"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0.0
        [Space(10)]
        [MainTexture]_BaseMap("Base Map", 2D) = "white" {}
        [MainColor][HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        _Light("颜色亮度", Range( 0 , 5))= 1
        _Alpha("整体透明度", Range( 0 , 1)) = 1

        _Emission("Emission", Float) = 0
        [Header(VertColor)]
        [Space(10)]
        [Toggle(_VERTCOLOR)] _VertColor ("VertColor", Float) = 1
        [Space(20)]
        [Header(Depth)]
        [Space(10)]
        [Toggle(_DEPTH)] _Depth ("Depth", Float) = 1
        _SoftParticle("Soft Particle Fade", Float) = 1
        [Space(10)]
        [Header(SphereMask)]
        [Space(10)]
        [Toggle(_USESPHEREMASK_ON)] _SphereMask ("SphereMask", Float) = 0
        [Toggle(_SPHERE_MASK_INVERT)] _SphereMaskInvert("SphereMask Invert", float) = 0
        [Space(10)]
        [NoScaleOffset]_DissolveNoiseMap("Dissolve Noise Map", 2D) = "white" {}
        _DissolveCenter("Sphere Center", Vector) = (0,0,0,0)
        _NoiseTilling("Noise Tilling", Float) = 1
        _NoiseSpeed("Noise Speed", Float) = 0
        _Wave0Offset("Wave0 Offset", Float) = 0
        _Wave0Width("Wave0 Width", Float) = 0.1
        _Wave1ffset("Wave1 Offset", Float) = 0
        _Wave1Width("Wave1 Width", Float) = 0.1
        _Wave2ffset("Wave2 Offset", Float) = 0
        _Wave2Width("Wave2 Width", Float) = 0.1
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _Wave2Color("Wave2 Color", Color) = (1,1,1,1)
        _scenceContrl("Sphere Radius", Float) = 3
        
        
        [KeywordEnum(Orientation,Regular,_REGULAR,FacingCamera,_FACINGCAMERA,FacingCameraLock,_FACINGCAMERALOCK)] _OrientationMode("公告板模式", Float) = 0


    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent+50" "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull [_CullMode]
            ZWrite Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #pragma shader_feature_local _DEPTH
            #pragma shader_feature_local _VERTCOLOR
            #pragma shader_feature_local _REGULAR _FACINGCAMERA _FACINGCAMERALOCK

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma instancing_options renderinglayer
            //#pragma instancing_options procedural:vertInstancingSetups

            #pragma shader_feature _USESPHEREMASK_ON
            #pragma shader_feature_local_fragment _SPHERE_MASK_INVERT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            //#include "../ShaderLibrary/LightingCommon.hlsl"
            #include "Packages/com.unity.texastudio.xrframework2022/Assets/Shaders/ShaderLibrary/LightingCommon.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                
                #if defined(_USESPHEREMASK_ON)
                float3 normalOS     : NORMAL;
                #endif
                
                //float4 tangentOS    : TANGENT;
                float2 texcoord : TEXCOORD0;
                float4 vertexColor : COLOR;
                
                //UNITY_VERTEX_INPUT_INSTANCE_ID
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                #if defined(_USESPHEREMASK_ON)
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
                #endif
                
                // half4 tangentWS                : TEXCOORD3;
                float3 positionOS : TEXCOORD4;
                float4 positionCS : SV_POSITION;
                float4 vertexColor : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };

            //TEXTURE2D(_NoiseMap);
            //SAMPLER(sampler_NoiseMap);
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;

                float _SoftParticle;
                float _Emission;
                half4  _DissolveCenter;
                half   _NoiseSpeed;
                half   _NoiseTilling;
                half   _Wave0Offset;   half _Wave0Width;
                half   _Wave1ffset;    half _Wave1Width;
                half   _Wave2ffset;    half _Wave2Width;
                half4  _EdgeColor;
                half4  _Wave2Color;
                float  _scenceContrl;
                half   _Light;
                half   _Alpha;
            CBUFFER_END
            
            TEXTURE2D(_DissolveNoiseMap);
            SAMPLER(sampler_DissolveNoiseMap);
       

            float4 NormalizedScreenPos(float4 screenPos)
            {
                float4 ase_screenPosNorm = screenPos / screenPos.w;
                ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0)
                                                                      ? ase_screenPosNorm.z
                                                                      : ase_screenPosNorm.z * 0.5 + 0.5;
                return ase_screenPosNorm;
            }

            // half Fresnel(float3 NormalWS, float3 Vdir,half Bias,half Scale,half Power)
            // {
            //     half result = saturate(pow(1 - saturate(dot(NormalWS, Vdir)), Power)) * Scale + Bias;
            //     return result;
            // }

            void SphereMask_Height(TEXTURE2D_PARAM(Dissolve, sampler_Dissolve),
                                   float3 worldPos, float3 center, float noiseTilling,
                                   float noiseSpeed, half3 n, half3 v,
                                   half waveOffset0, half waveWidth0, // 改为 float
                                   half waveOffset1, half waveWidth1, // 改为 float
                                   half waveOffset2, half waveWidth2, // 改为 float
                                   half4 edgeColor, half4 waveColor2, float control,
                                   out half3 EdgeColor, out half3 DarkColor,
                                   out half Alpha)
            {
                // 采样噪声 (UV计算可以用 float)
                float2 noiseUV = float2((worldPos.xz * noiseTilling) + _Time.y * noiseSpeed);
                half noise = SAMPLE_TEXTURE2D(Dissolve, sampler_Dissolve, noiseUV).r;

                // 核心距离计算 (必须 float)
                float dist = distance(center, worldPos);

                // Fresnel 这种视觉效果可以用 half 没关系，为了保险先 float
                half fresnelVal = Fresnel(n, v, 0, 2, 1);

                #if defined(_SPHERE_MASK_INVERT)
                // --- 场景 2 (显示) 逻辑 ---
                // 只有在 Sphere 内部才显示，外部 Clip 掉

                // 计算距离场：(dist - control)
                // 如果 dist > control (圈外)，值 > 0
                // 如果 dist < control (圈内)，值 < 0
                float DistanceField = dist + fresnelVal - control + 3.0; // +3 是你的原始偏移

                // WAVE0 (Alpha)
                // 如果 DistanceField 是大的正数 (圈外)，saturate(...) = 1, Alpha = 0 -> Clip
                // 如果 DistanceField 是负数 (圈内)，saturate(...) = 0, Alpha = 1 -> 显示
                float waveVal = (DistanceField - noise - waveOffset0) / max(0.001, waveWidth0);
                half waveAlpha = 1.0 - saturate(waveVal);

                Alpha = saturate(waveAlpha);
                clip(Alpha - 0.01);

                // WAVE1 (Edge)
                // 边缘发光
                half edgeVal = (DistanceField - noise + waveOffset1) / (-max(0.001, waveWidth1));
                EdgeColor = (1.0 - saturate(edgeVal)) * edgeColor.rgb;
                DarkColor = half3(1, 1, 1);

                #else
                // --- 场景 1 (消失) 逻辑 ---
                // 只有在 Sphere 外部才显示，内部 Clip 掉

                // 原始逻辑： -(dist ... - control)
                // 简化逻辑： control - dist
                // 如果 control > dist (圈内)，DistanceField > 0
                float DistanceField = -(dist + fresnelVal - control);

                // WAVE0
                float waveVal = (DistanceField - noise - waveOffset0) / max(0.001, waveWidth0);
                half waveAlpha = 1.0 - saturate(waveVal);
                Alpha = saturate(waveAlpha);
                clip(Alpha - 0.01);

                // WAVE2 (Edge)
                half edgeVal = (DistanceField - noise + waveOffset1) / (-max(0.001, waveWidth1));
                half EdgeFactor = 1.0 - saturate(edgeVal);
                EdgeColor = EdgeFactor * edgeColor.rgb;

                // WAVE3 (Darkening)
                float darkVal = (DistanceField - waveOffset2 - 1.0) / max(0.001, waveWidth2);
                half3 darkFactor = lerp(waveColor2.rgb, half3(1, 1, 1), 1.0 - saturate(darkVal));
                DarkColor = darkFactor + EdgeColor; // 叠加边缘
                #endif
            }

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                //billibord
                float3 positionOS = float3(0, 0, 0);
                float3 objectScale = float3(
                    length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                    length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                    length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)));
                float3 objectPos = GetAbsolutePositionWS(UNITY_MATRIX_M._m03_m13_m23);
                
                #if defined (_REGULAR)
                positionOS = input.positionOS.xyz;
                #elif defined (_FACINGCAMERA)
                positionOS = mul(GetWorldToObjectMatrix(),
                    float4(
                        mul(UNITY_MATRIX_I_V, float4(input.positionOS.xyz * objectScale, 0)).xyz +
                        objectPos, 1));
                #elif defined(_FACINGCAMERALOCK)
                float3 Camera_Direction = -1 * mul(UNITY_MATRIX_M,
                                                           transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz);
                float2 Camera_DirectionXZ = normalize(float2(Camera_Direction.x, Camera_Direction.z));
                float RotationAngle = atan2(Camera_DirectionXZ.x, Camera_DirectionXZ.y);
                float3 RotationPos = input.positionOS * objectScale;
                float3 RotationAxis = float3(0, 1, 0);
                positionOS = RotateAroundAxis(RotationPos, half3(0, 0, 0), half3(0, 1, 0), RotationAngle) / objectScale;
                #endif
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.uv = input.texcoord;
                output.positionCS = vertexInput.positionCS;
                
                #if defined(_USESPHEREMASK_ON)
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                #endif
                
                output.positionOS = input.positionOS.xyz;
                output.vertexColor = input.vertexColor;
                
                return output;
            }

            float4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.uv;
                /*float3 positionWS = input.positionWS;
                half3 vDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
                half3 nDirWS = normalize(input.normalWS);
                float2 flowUV = uv +  float2(0.1,0.1) * _Time.y;
                half4 noiseMap = SAMPLE_TEXTURE2D(_NoiseMap,sampler_NoiseMap,flowUV);*/
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                float4 baseColor = baseMap * _BaseColor;
                
                //顶点色
                #if defined (_VERTCOLOR)
                float3 finalColor = baseColor.rgb * _BaseColor.rgb * input.vertexColor.rgb;
                float alpha = baseMap.a * _BaseColor.a * input.vertexColor.w;
                #else
                float3 finalColor = baseColor.rgb * _BaseColor.rgb;
                float alpha = baseMap.a * _BaseColor.a;
                #endif
                
                // 软粒子：用正确裁剪空间算屏幕坐标，按“粒子与场景的深度差”平滑淡出
                #if defined (_DEPTH)
                float4 screenPos = NormalizedScreenPos(ComputeScreenPos(TransformObjectToHClip(input.positionOS)));
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenPos.xy), _ZBufferParams);
                float thisDepth  = LinearEyeDepth(screenPos.z, _ZBufferParams);
                float depthFade  = saturate((sceneDepth - thisDepth) / max(_SoftParticle, 0.0001));
                alpha *= depthFade;
                #endif
                
                //自发光
                finalColor = finalColor + _Emission * finalColor;
                
                #if defined(_USESPHEREMASK_ON)
                half3 EdgeColor, DarkColor;
                half dissolveAlpha;
                SphereMask_Height(TEXTURE2D_ARGS(_DissolveNoiseMap, sampler_DissolveNoiseMap), input.positionWS,
                    _DissolveCenter, _NoiseTilling, _NoiseSpeed, input.normalWS,
                    GetWorldSpaceNormalizeViewDir(input.positionWS), _Wave0Offset, _Wave0Width, _Wave1ffset, _Wave1Width, _Wave2ffset, _Wave2Width,
                    _EdgeColor, _Wave2Color, _scenceContrl, EdgeColor, DarkColor, dissolveAlpha);
                finalColor = finalColor * DarkColor + EdgeColor;
                 alpha *= dissolveAlpha;
                #endif
                
                return float4(finalColor * _Light, alpha * _Alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}