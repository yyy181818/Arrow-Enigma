//Created by Wans Wu
//2024,02,07
Shader "TA Shaders/VFX/PivotVetex"
{
    Properties
    {
        [Space(10)]
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        _Tiliing ("全局Tiliing", vector) = (1, 1, 0, 0)
       
        [Main(PBRIntput, _, off, off)] _PBRIntput ("PBR 属性", Float) = 1
        [Tex(PBRIntput,_BaseColor)][MainTexture] _BaseMap("Base(RGB)", 2D) = "white" {}
        [HideInInspector][MainColor] _BaseColor("Base颜色", Color) = (1,1,1,1)
        
        [Tex(PBRIntput)] _MRAMap("MRA", 2D) = "white" {}
        [Sub(PBRIntput)]_Metallic("金属度(R)",Range(0.0, 1.0)) = 1.0
        [Sub(PBRIntput)]_Smoothness("光滑度(G)", Range(0.0, 1.0)) = 1.0
        [Sub(PBRIntput)]_OcclusionStrength("AO(R)", Range(0.0, 1.0)) = 0.0
        
        [Main(Normal, _, off, off)] _normal ("法线", Float) = 1  
        [Tex(Normal,_BumpScale)]_BumpMap("法线贴图", 2D) = "bump" {}
        [HideInInspector]_BumpScale("法线强度", Float) = 1.0
                
        [Main(Emission,_, off, off)] _Emission ("发光", Float) = 0
        [Sub(Emission)][HDR]_EmissionColor("发光颜色", Color) = (0,0,0,1)
        [Sub(Emission)]_EmissionStrength("发光强度", Range(0.0, 1.0)) =1.0
        [SubToggle(Emission,_VERTEXCOLOREMISS)]_VertexColorEmiss("内侧发光",Float) = 0.0
        
        [Main(Effect,_, off, off)] _Effect ("Effect", Float) = 0
        [SubTitle(Effect,Global)]
        [Sub(Effect)]_Contrl("总体控制", Float) = 1.0
        [Sub(Effect)]_Range("范围", Float) = 5.0
        [Sub(Effect)]_EffectRandom("Random", Range(0.0, 1.0)) = 1.0
        [Sub(Effect)]_VertexExpand("顶点外扩", Float) = 0.0
        
        [SubTitle(Effect,Dispersion)]
        [Sub(Effect)]_Dispersion("Dispersion", Float) = 1.7
        [Sub(Effect)]_DispersionRandom("DispersionRandom", Float) = 4.0
        [Sub(Effect)]_DispersionCenter("Center", Vector) = (0,0,0,0)
        [Sub(Effect)]_DispersionDir("Dir", Vector) = (1,0,1,0)
        
        [SubTitle(Effect,Lift)]
        [Sub(Effect)]_DispersionLift("Lift", Float) = 10
        [Sub(Effect)]_LiftRandom("LiftRandom", Float) = 4.0
        
        [SubTitle(Effect,SelfRotaion)]
        [Sub(Effect)]_SelfRotate("SelfRotate", Float) = 15
        [Sub(Effect)]_SelfRotateRandom("SelfRotateRandom", Float) = 1
        
        [SubTitle(Effect,GlobalRotaion)]
        [Sub(Effect)]_GlobalRotate("GlobalRotate", Float) = 15
        [Sub(Effect)]_GlobalRotateRandom("GlobalRotateRandom", Float) = 1
        
        
        [Main(Lighting, _, off, off)] _Lighting ("光照", Float) = 1
        [Sub(Lighting)]_LightIntensitiy("烘培光照强度",Range(0.0, 2.0)) = 1.0
        [Sub(Lighting)]_LightVector("Day&Night",Vector) = (1.0,1.0,0,0)
        [SubToggle(Lighting,_ENVIRONMENTREFLECTIONS_OFF)]_EnvironmentReflections("环境反射", Float) = 0.0
    }

    SubShader
    {
        Tags{"RenderType" = "Opaque" "Queue"="Geometry" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit""IgnoreProjector" = "True"}
        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull[_CullMode]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
         
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local _DAYNIGHT
            #pragma shader_feature_local _VERTEXCOLOREMISS
            #pragma multi_compile_fog
          

            //realtimeLight
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOF
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
            #include "../../ShaderLibrary/StandardLighting.hlsl"
            #include "../../ShaderLibrary/VertexFunction.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                float2 texcoord3     : TEXCOORD2;
                float2 texcoord4     : TEXCOORD3;
                float4 vertexColor      :COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 uv                       : TEXCOORD0;
                float4 positionWS               : TEXCOORD1;
                float4 normalWS                 : TEXCOORD2;
                half4  tangentWS                : TEXCOORD3;    
                float4 positionCS               : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO 
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MRAMap);
            SAMPLER(sampler_MRAMap);
            CBUFFER_START(UnityPerMaterial)
            //float4 _ReflectionCubeMap_HDR;
            float4 _BaseMap_ST;
            half4 _MainTex;
            half4 _NormalMap_ST;
            float4 _DetailAlbedoMap_ST;
            half4 _BaseColor;
            half4 _AddColor;
            half4 _SpecColor;
            half4 _EmissionColor;
            half _Cutoff;
            float _Smoothness;
            half _Metallic;
            half _BumpScale;
            half _Parallax;
            half _ClearCoatMask;
            half _ClearCoatSmoothness;
            half _DetailAlbedoMapScale;
            half _DetailNormalMapScale;
            half _Surface;
            half _EmissionStrength;
            half _LightIntensitiy;
            half _SpecShininess;
            half4 _Tiliing;
            half _OcclusionStrength;
            half _HeightLight;
            
            CBUFFER_END
            half _Light;
            #if defined (_DAYNIGHT)
            half4 _LightVector;
            #endif

            //effect
            half _Contrl;
            half _Range;
            half _EffectRandom;

            half _Dispersion;
            half _DispersionRandom;
            float4 _DispersionCenter;
            float4 _DispersionDir;

            half _DispersionLift;
            half _LiftRandom;

            half _VertexExpand;

            half _SelfRotate;
            half _SelfRotateRandom;

            half _GlobalRotate;
            half _GlobalRotateRandom;
            
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                //Pivot Painter
                half customAlpha = 0;
                half random = 0;
                float3 Xvector = float3(0,0,0);
                float3 PivotPos = float3(0,0,0);
                GetPivotPainterData_float(input.texcoord3,input.texcoord4,half4(1,1,1,1),customAlpha,random,Xvector,PivotPos);

                //EffectMask
                half EffectMask = saturate(((PivotPos.y + (random * _EffectRandom)) / _Range + 1) - (_Contrl * 2));
                output.normalWS.w = EffectMask;

                //Dispersion
                float DispersionRandom = _Dispersion + random * _DispersionRandom;
                float3 DispersionOffset = normalize(PivotPos - _DispersionCenter) * _DispersionDir * DispersionRandom * EffectMask;

                //Lift
                float LiftRandom = random * _LiftRandom + _DispersionLift;
                float3 Lift = float3(0,1,0) *  LiftRandom * EffectMask;

                //ScaliingPos
                float3 OrigitlPos = input.positionOS.xyz + input.normalOS * _VertexExpand * 0.01;
                float3 ScallingPos = lerp(OrigitlPos,PivotPos,EffectMask);

                //SelfRotation
                float selfRotationAngle = (_SelfRotate + random * _SelfRotateRandom ) * EffectMask;
                float3 selfRotaion = RotateAroundAxis_Flus(ScallingPos,PivotPos,Xvector,selfRotationAngle);
                float3 selfRotaionNormal =RotateAroundAxis_Flus(input.normalOS,half3(0,0,0),Xvector,selfRotationAngle);

                //GlobalRotation
                float3 posForRotaion = DispersionOffset + selfRotaion + Lift;
                float GlobalRotationAngle = (random * _GlobalRotateRandom + _GlobalRotate) * EffectMask;
                float3 GlobalRotaion = RotateAroundAxis(posForRotaion,half3(0,0,0),half3(0,1,0),GlobalRotationAngle);
                float3 GlobalRotaionNormal = RotateAroundAxis(selfRotaionNormal,half3(0,0,0),half3(0,1,0),GlobalRotationAngle);
                
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(GlobalRotaion.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(GlobalRotaionNormal, input.tangentOS);
                output.uv.xy = input.texcoord;
                output.normalWS.xyz = normalInput.normalWS;         
                real sign = input.tangentOS.w * GetOddNegativeScale();
                half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                output.positionCS = vertexInput.positionCS;

                output.tangentWS = tangentWS;
                output.positionWS.xyz = vertexInput.positionWS;
                output.positionWS.w = input.vertexColor.w;
                output.uv.zw = input.staticLightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
                return output;
            }
            
            float4 LitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv  = input.uv.xy *  _Tiliing.xy + _Tiliing.zw;
                float3 positionWS = input.positionWS;
                float3  vDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3  nDirWS = normalize(input.normalWS.xyz);
                half3  tDirWS = normalize(input.tangentWS.xyz);
                half3  bDirWS = normalize(cross(nDirWS,tDirWS) * input.tangentWS.w);
                half3x3  TBN  = half3x3(tDirWS,bDirWS,nDirWS);
                float4 ShadowCoord = TransformWorldToShadowCoord(positionWS);
                half4  ShadowMask = float4(1.0,1.0,1.0,1.0);
                Light light = GetMainLight(ShadowCoord,positionWS,ShadowMask);
                float3 LightDir = light.direction;

                //Color
                half4  BaseColorAlpha = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv) ;
                //float BaseAlpha = pow(abs(BaseColorAlpha.a),1/2.2);
                half3 BaseColor = BaseColorAlpha.rgb *_BaseColor.rgb;
                float3 nDirTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,uv),_BumpScale);
                nDirWS = normalize(mul(nDirTS,TBN));

                //Emiss
                half4 EffectEmissonColor = input.normalWS.w * input.normalWS.w *  _EmissionColor ;
                #if defined (_VERTEXCOLOREMISS)
                half4 VertexColorEmiss = (1-input.positionWS.w) * _EmissionColor * 0.5;
                half4 EmissonColor = _EmissionColor * _EmissionStrength + EffectEmissonColor +VertexColorEmiss;
                #else
                half4 EmissonColor = _EmissionColor * _EmissionStrength + EffectEmissonColor;
                #endif
                
                //MRA
                half3 MRAmap = SAMPLE_TEXTURE2D(_MRAMap,sampler_MRAMap,uv).rgb ;
                float Metallic = saturate(_Metallic * MRAmap.r) ;
                half  Occlusion =saturate( lerp( 1,0 + MRAmap.b,_OcclusionStrength ));
                float Roughness = saturate( (1 - _Smoothness * (1-MRAmap.g)));
                
                float csz = input.positionCS.z * input.positionCS.w; 
                real fogFactor = ComputeFogFactor(csz);
                half3 DiffuseColor = lerp(BaseColor,half3(0.0,0.0,0.0),Metallic);
                half3 SpecularColor = lerp(half3(0.04,0.04,0.04),BaseColor,Metallic);
                Roughness = max(saturate(Roughness),0.1f);

                //realtimeLight
                half3  DirectLighting = half3(0,0,0);
                DirectLighting_float(DiffuseColor,SpecularColor,Roughness,positionWS,nDirWS,vDirWS,DirectLighting);
                float3  IndirectLighting = float3(0,0,0);
                #if defined (_DAYNIGHT)
                IndirectLighting_DayandNight(DiffuseColor,SpecularColor,_LightVector,Roughness,positionWS,nDirWS,vDirWS,Occlusion,1,input.staticLightmapUV,LightDir,_LightIntensitiy  ,IndirectLighting);
                #else
                IndirectLighting_float(DiffuseColor,SpecularColor,Roughness,positionWS,nDirWS,vDirWS,Occlusion,1,input.uv.zw,LightDir,_LightIntensitiy  ,IndirectLighting);
                #endif
                float4 color = float4((DirectLighting+IndirectLighting+ EmissonColor.rgb) ,1);
                color.rgb = MixFog(color.rgb,fogFactor)* (1 - _Light);
                return color;
            }
              ENDHLSL
            }
Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            #pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "../../ShaderLibrary/CustomLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL
        }
     

        

    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"

    }
