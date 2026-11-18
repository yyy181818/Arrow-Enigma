Shader "TA Shaders/Character/SimpleHair"
{
    Properties
    {
        [Toggle]_AlphaTest ("AlphaTest", Float) = 0
        _Cutoff ("Cutoff", Range(0,1)) = 0.5
        [MainTexture]_BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]_BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseColorInt ("Base Color Intensity", Range(0,8)) = 1
        [Toggle(_USE_SPHERE_NORMAL)]_SphereNormal ("Sphere Normal Diffuse", float) = 0
        _CombineMap ("Metal&Roughness&AO(xyz)", 2D) = "white"{}
        [Gamma]_Metallic ("Metallic", Range(0,1)) = 1
        _Roughness ("Roughness", Range(0,1)) = 1
        _Specular ("Specular", Color) = (0.5,0.5,0.5,1)
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,8)) = 1
        [Toggle(_EMISSION)]_Emission ("Emission", float) = 0
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _OcclusionStrength ("Occlusion", Range(0,1)) = 1
        _NoiseMap ("Noise Map", 2D) = "white" {}
        _NoiseShift ("Noise Shift", float) = 1

        [HDR] _TransmissionColor("Transmission Color", Color) = (0.8, 0.5, 0.3, 1.0)
        [HideInInspector]_TransmissionRim("Transmission Rim", Range(0.0, 1.0)) = 0.2
        [HideInInspector]_TransmissionThickness("Transmission Thickness", Range(0.0, 1.0)) = 0.5
        _TransmissionPower("Transmission Power", Range(0.0, 1000.0)) = 0.2
        _TransmissionScale("Transmission Scale", Range(0.0, 2.0)) = 0.5
        _ThicknessBias ("Thickness Bias",Range(0,0.5)) = 0.5
        _ThicknessScale ("Thickness Scale", Range(0.0, 2.0)) = 2
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #pragma shader_feature_local_fragment _ALPHATEST_ON
        #pragma shader_feature_local_fragment _USE_SPHERE_NORMAL
        #pragma shader_feature_local_fragment _EMISSION

        //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        //  #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        // #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        //#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
        //#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        //#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        //#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        //#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        //#pragma multi_compile_fragment _ _LIGHT_COOKIES
        //#pragma multi_compile _ _LIGHT_LAYERS
        //#pragma multi_compile _ _FORWARD_PLUS
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "../ShaderLibrary/Hair/HairLighting2026.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            half _Cutoff;
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _BaseColorInt;
            half _Metallic;
            half _Roughness;
            half _BumpScale;
            half4 _Specular;
            half4 _EmissionColor;
            half _OcclusionStrength;
            half _NoiseShift;
            half4 _TransmissionColor;
            half _TransmissionRim;
            half _TransmissionThickness;
            half _TransmissionPower;
            half _TransmissionScale;
            half _ThicknessBias;
            half _ThicknessScale;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_CombineMap);
        SAMPLER(sampler_CombineMap);
        TEXTURE2D(_BumpMap);
        SAMPLER(sampler_BumpMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);

        TEXTURE2D(_NoiseMap);
        SAMPLER(sampler_NoiseMap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
            half4 tangentOS : TANGENT;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float2 uv : TEXCOORD0;
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD1;
            float3 normalWS : TEXCOORD2;
            half3 tangentWS : TEXCOORD3;
            half3 bitangentWS : TEXCOORD4;
            float3 viewDirWS : TEXCOORD5;
            float4 shadowCoord : TEXCOORD6;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionWS = vertexInput.positionWS;
            output.positionCS = vertexInput.positionCS;
            VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
            output.normalWS = normalInput.normalWS;
            output.tangentWS = normalInput.tangentWS;
            output.bitangentWS = normalInput.bitangentWS;
            output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
            output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            return output;
        }

        half AlphaClip(half alpha, half secondPassCutOff,half output)
        {
            float tempCutOff = _Cutoff * 0.5 + 0.005 - alpha;
            clip(_Cutoff * 0.5 - abs(tempCutOff));

            float tempCutOff2 = saturate(alpha / max(_Cutoff, 0.01));
            clip(tempCutOff2 - secondPassCutOff);
            output = tempCutOff2;
        }

        half4 frag(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
            half3 baseColor = baseMap.rgb * _BaseColor.rgb;
            baseColor *= _BaseColorInt;
            half alpha = baseMap.a * _BaseColor.a;

            half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, input.uv).g * _NoiseShift;

            AlphaDiscard(alpha, _Cutoff);

            half3 normalWS = normalize(input.normalWS);
            half3 tangentWS = normalize(input.tangentWS);
            half3 bitangenWS = normalize(input.bitangentWS);
            half3 binormalWS = normalize(bitangenWS + normalWS * (noise - 0.5));
            half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
            float3x3 TBN = float3x3(tangentWS, binormalWS, normalWS);
            normalWS = TransformTangentToWorldDir(normalTS, TBN, true);


            half3 emission = 0.0;
            #if defined(_EMISSION)
            half3 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;
            emission = emissionMap * _EmissionColor;
            #endif

            half3 combine = SAMPLE_TEXTURE2D(_CombineMap, sampler_CombineMap, input.uv).xyz;
            half roughness = combine.y * _Roughness;

            Light mainLight = GetMainLight(input.shadowCoord, input.positionWS, 1);
            half3 lightDirWS = normalize(mainLight.direction);
            half3 viewDirWS = normalize(input.viewDirWS);

            //float3 lightDirWS = normalize(viewDirWS - binormalWS * dot(viewDirWS, binormalWS));
            half atten = mainLight.shadowAttenuation;
            half specular = _Specular.r;

            HairTransmissionParams transParams;
            transParams.color = _TransmissionColor.rgb;
            transParams.power = _TransmissionPower;
            transParams.scale = _TransmissionScale;
            HairTempData hairTempData;

            HairBaseDataCal(hairTempData, binormalWS, viewDirWS, lightDirWS, roughness);

            //half3 hairDiffuse = DiffuseCal(mainLight, atten, baseColor, lightDirWS, viewDirWS, binormalWS);
            // half3 hairSpecular = SpecularCal(hairTempData, mainLight, baseColor, specular);
            //
            // half3 directColor = hairDiffuse + hairSpecular;
            // half3 RadianceSH = SampleSH(normalWS);
            // half3 indirectColor = IndirDiff(hairTempData, mainLight, atten, baseColor, specular, viewDirWS,
            //                                 normalWS, binormalWS);
            half occlusion = LerpWhiteTo(combine.b, _OcclusionStrength);
            half3 c = HairLightingFinal(lightDirWS, viewDirWS, binormalWS, normalWS, baseColor,
                                        roughness, specular, mainLight, atten,
                                        transParams, occlusion, input.positionWS, input.uv,
                                        _ThicknessBias,
                                        _ThicknessScale);

            half4 col = (half4(c, alpha));
            return saturate(col);
        }

        half4 hairTransparentFrag(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
            half3 baseColor = baseMap.rgb * _BaseColor.rgb;
            baseColor *= _BaseColorInt;
            half alpha = baseMap.a * _BaseColor.a;

            half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, input.uv).g * _NoiseShift;

            half3 normalWS = normalize(input.normalWS);
            half3 tangentWS = normalize(input.tangentWS);
            half3 bitangenWS = normalize(input.bitangentWS);
            half3 binormalWS = normalize(bitangenWS + normalWS * (noise - 0.5));
            half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
            float3x3 TBN = float3x3(tangentWS, binormalWS, normalWS);
            normalWS = TransformTangentToWorldDir(normalTS, TBN, true);


            half3 emission = 0.0;
            #if defined(_EMISSION)
            half3 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;
            emission = emissionMap * _EmissionColor;
            #endif

            half3 combine = SAMPLE_TEXTURE2D(_CombineMap, sampler_CombineMap, input.uv).xyz;
            half roughness = combine.y * _Roughness;

            Light mainLight = GetMainLight(input.shadowCoord, input.positionWS, 1);
            half3 lightDirWS = normalize(mainLight.direction);
            half3 viewDirWS = normalize(input.viewDirWS);

            //float3 lightDirWS = normalize(viewDirWS - binormalWS * dot(viewDirWS, binormalWS));
            half atten = mainLight.shadowAttenuation;
            half specular = _Specular.r;

            HairTransmissionParams transParams;
            transParams.color = _TransmissionColor.rgb;
            transParams.power = _TransmissionPower;
            transParams.scale = _TransmissionScale;

            HairTempData hairTempData;
            HairBaseDataCal(hairTempData, binormalWS, viewDirWS, lightDirWS, roughness);

            //half3 hairDiffuse = DiffuseCal(mainLight, atten, baseColor, lightDirWS, viewDirWS, binormalWS);
            // half3 hairSpecular = SpecularCal(hairTempData, mainLight, baseColor, specular);
            //
            // half3 directColor = hairDiffuse + hairSpecular;
            // half3 RadianceSH = SampleSH(normalWS);
            // half3 indirectColor = IndirDiff(hairTempData, mainLight, atten, baseColor, specular, viewDirWS,
            // normalWS, binormalWS);
            half occlusion = LerpWhiteTo(combine.b, _OcclusionStrength);
            half3 c = HairLightingFinal(lightDirWS, viewDirWS, binormalWS, normalWS, baseColor,
                roughness, specular, mainLight, atten,
                transParams, occlusion, input.positionWS, input.uv,
                _ThicknessBias, _ThicknessScale);
            AlphaDiscard(alpha, 0.01);
            half4 col = (half4(c, alpha));
            return saturate(col);
        }
        ENDHLSL
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Cull Off
            ZWrite On
            ZTest LEqual
            //Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Name "Hair Transparent"
            Tags
            {
                "LightMode" = "HairTransparent"
            }
            ZWrite Off
            ZTest Less
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha,One OneMinusSrcAlpha
            //Blend SrcAlpha OneMinusSrcAlpha, OneMinusDstAlpha One
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment hairTransparentFrag
            ENDHLSL
        }
    }
}