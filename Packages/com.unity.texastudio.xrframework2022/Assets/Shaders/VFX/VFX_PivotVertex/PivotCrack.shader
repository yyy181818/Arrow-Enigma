//Created by Wans Wu
//2025,07,21
Shader "TA Shaders/VFX/PivotCrack"
{
    Properties
    {
        [Space(10)]
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 2.0
        _Tiliing ("全局Tiliing", vector) = (1, 1, 0, 0)
       
        [Main(PBRIntput, _, off, off)] _PBRIntput ("PBR 属性", Float) = 1
        [Tex(PBRIntput,_BaseColor)][MainTexture] _BaseMap("Base(RGB)", 2D) = "white" {}
        [HideInInspector][MainColor] _BaseColor("Base颜色", Color) = (1,1,1,1)
        //[Sub(PBRIntput)] _AddColor("叠加 颜色", Color) = (0,0,0,0)
        
        [Tex(PBRIntput)] _MRAMap("MSA", 2D) = "white" {}
        [Sub(PBRIntput)]_Metallic("金属度(R通道)",Range(0.0, 1.0)) = 1.0
        [Sub(PBRIntput)]_Smoothness("光滑度(G通道)", Range(0.0, 1.0)) = 1.0
        [Sub(PBRIntput)]_HeightLight("高光度", Range(0.0, 1.0)) = 1.0
        [Sub(PBRIntput)]_OcclusionStrength("AO(B通道)", Range(0.0, 1.0)) = 0.0
        
        [Main(Normal, _, off, off)] _normal ("法线", Float) = 1  
        [Tex(Normal,_BumpScale)]_BumpMap("法线贴图", 2D) = "bump" {}
        [HideInInspector]_BumpScale("法线强度", Float) = 1.0
                
        [Main(Emission,_EMISSION, off)] _Emission ("发光", Float) = 0
        [Tex(Emission,_EmissionColor)]_EmissionMap("发光贴图", 2D) = "white" {}
        [HideInInspector][HDR]_EmissionColor("发光颜色", Color) = (0,0,0,1)
        [Sub(Emission)]_EmissionStrength("发光强度", Range(0.0, 1.0)) =1.0
        [SubToggle(Emission,_VERTEXCOLOREMISS)]_VertexColorEmiss("内侧发光",Float) = 0.0
        [Sub(Emission)][HDR]_InnerEmissionColor("内侧发光颜色", Color) = (0,0,0,1)
        
        [Main(Lighting, _, off, off)] _Lighting ("光照", Float) = 1
        [Sub(Lighting)]_LightIntensitiy("烘培光照强度",Range(0.0, 2.0)) = 1.0
        [SubToggle(Lighting,_ENVIRONMENTREFLECTIONS_OFF)]_EnvironmentReflections("环境反射", Float) = 0.0
        
        [Main(Crack, _, off, off)] _Crack ("Crack", Float) = 1  
        [Sub(Crack)]_Rotate("Rotate", Float) = 2.0
        [Sub(Crack)]_EffectOffset("Offset", Float) = 2.0
        [Sub(Crack)]_EffectRange("Range", Float) = 1.25
        [Sub(Crack)]_EffectHardness("Hardness", Float) = 1
        [Sub(Crack)]_Scalling("Scalling",Range(0.0, 1.0)) = 1
        [Sub(Crack)]_XvextorOffset("XvextorOffset", Float) = 1
        [Sub(Crack)]_LiftOffset("LiftOffset", Vector) = (0,0,0,0)
        [Sub(Crack)]_FloatingFreq("FloatingFreq", Float) = 1
        [Sub(Crack)]_FloatingWave("FloatingWave", Vector) = (0,0,0,0)
        [Sub(Crack)]_VerticalMin("VerticalMin", Float) = 0
        [Sub(Crack)]_VerticalMax("VerticalMax", Float) = 1
        [Sub(Crack)]_PlayerPositionfloat("CrackCenter", Vector) = (0,0,0,0)
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
            #pragma shader_feature_local _EMISSION
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
            //#include "../../ShaderLibrary/SceneChange.hlsl"
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
            #if defined (_EMISSION)
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            #endif
            CBUFFER_START(UnityPerMaterial)
            //float4 _ReflectionCubeMap_HDR;
            half4 _BaseColor;
            half _Metallic;
            half _Smoothness;
            half _HeightLight;
            half _OcclusionStrength;
            half _BumpScale;
            half4 _EmissionColor;
            half _EmissionStrength;
            float4 _Tiliing;
            half _LightIntensitiy;
            //crack
            float _Rotate;
            float _EffectOffset;
            float _EffectRange;
            float _EffectHardness;
            float _Scalling;
            float _XvextorOffset;
            float4 _LiftOffset;
            float _FloatingFreq;
            float4 _FloatingWave;
            float _VerticalMin;
            float _VerticalMax;
            float4 _PlayerPositionfloat;
            half4 _InnerEmissionColor;
            CBUFFER_END
            half _Light;
            
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                //pivot painter
                float outside ;
                float3 Xvextor ;
                float3 PovitPosition ;
                half randomper ;
                GetPivotPainterData_float(input.texcoord3,input.texcoord4,input.vertexColor,outside,randomper,Xvextor,PovitPosition);
                PovitPosition = normalize(PovitPosition);
                float PovitRandom = RandomRange(input.texcoord3,0,1);
                //VertivlaMask
                float VertivlaMask = smoothstep(_VerticalMin,_VerticalMax,PovitPosition.y);
                //effectMask
                float playerDis = distance(_PlayerPositionfloat,mul(GetObjectToWorldMatrix(),float4( PovitPosition, 1 )).xyz)+_EffectOffset;
                float effectMask = 1 - clamp(pow(clamp(inverseLerp(0,_EffectRange,playerDis),0,1),_EffectHardness),0,1);
                float3 rotationAngle = _Rotate * PovitRandom * effectMask * VertivlaMask;
                //Lift
                float3 Lift = (Xvextor * _XvextorOffset + _LiftOffset * PovitRandom) * effectMask * VertivlaMask;
                //Floating
                float PovitRandom1 = RandomRange(input.texcoord3.xy,-10,10);
                float3 Floating = sin((PovitRandom1 + _Time.y) * _FloatingFreq) * _FloatingWave * (effectMask * effectMask) * 0.01 * VertivlaMask;
               //scalling
                float3 scallingPos = lerp(PovitPosition,input.positionOS.xyz,_Scalling);

                float3 rotationAxis = normalize(cross(PovitPosition,Xvextor));
                float3 normal = RotateAroundAxis(input.normalOS.xyz,half3(0,0,0),rotationAxis,rotationAngle);
                float3 positonOS = RotateAroundAxis(scallingPos,PovitPosition,rotationAxis,rotationAngle) + Lift + Floating;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positonOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(normal, input.tangentOS);
                output.uv.xy = input.texcoord;
                output.normalWS.xyz = normalInput.normalWS;
                output.normalWS.w = outside;
                real sign = input.tangentOS.w * GetOddNegativeScale();
                half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                output.positionCS = vertexInput.positionCS;
                output.tangentWS = tangentWS;
                output.positionWS.xyz = vertexInput.positionWS;
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
                float3  nDirWS = normalize(input.normalWS);
                half3  tDirWS = normalize(input.tangentWS.xyz);
                half3  bDirWS = normalize(cross(nDirWS,tDirWS) * input.tangentWS.w);
                half3x3  TBN  = half3x3(tDirWS,bDirWS,nDirWS);
                float4 ShadowCoord = TransformWorldToShadowCoord(positionWS);
                half4  ShadowMask = float4(1.0,1.0,1.0,1.0);
                Light light = GetMainLight(ShadowCoord,positionWS,ShadowMask);
                float3 LightDir = light.direction;
                half4  BaseColorAlpha = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv) ;
                //float BaseAlpha = pow(abs(BaseColorAlpha.a),1/2.2);
                half3 BaseColor = BaseColorAlpha.rgb *_BaseColor.rgb ;
                float3 nDirTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,uv),_BumpScale);
                nDirWS = normalize(mul(nDirTS,TBN));
                half4 EmissonMap = 0;
                 #if defined (_EMISSION)
                    EmissonMap = SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,uv)* _EmissionColor * _EmissionStrength;
                    #if defined (_VERTEXCOLOREMISS)
                    half4 VertexColorEmiss = (1-input.normalWS.w) * _InnerEmissionColor ;
                    EmissonMap = EmissonMap  +VertexColorEmiss;
                    #endif
                
                 #else
                    EmissonMap = half4(0,0,0,0);
                 #endif
                 half4 EmissonColor = EmissonMap;
                half3 MRAmap = SAMPLE_TEXTURE2D(_MRAMap,sampler_MRAMap,uv).rgb ;
                
                float Metallic = saturate(_Metallic * MRAmap.r) ;
                half  Occlusion =saturate( lerp( 1,0 + MRAmap.b,_OcclusionStrength ));
                float Roughness = saturate( (1 - _Smoothness * (1-MRAmap.g)));
                
                float csz = input.positionCS.z * input.positionCS.w; 
                real fogFactor = ComputeFogFactor(csz);
                half3 DiffuseColor = lerp(BaseColor,half3(0.0,0.0,0.0),Metallic);
                half3 SpecularColor = lerp(half3(0.04,0.04,0.04),BaseColor,Metallic);
                Roughness = max(saturate(Roughness),0.1f);
                float3  IndirectLighting = float3(0,0,0);
                IndirectLighting_float(DiffuseColor,SpecularColor,Roughness,positionWS,nDirWS,vDirWS,Occlusion,_HeightLight,input.uv.zw,LightDir,_LightIntensitiy  ,IndirectLighting);
                float4 color = float4((IndirectLighting+ EmissonColor.rgb) ,1);
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

            #include "../../ShaderLibrary/CustomLitInputGeneral.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL
        }
     

        

    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "LWGUI.LWGUI"

    }
