//Created by Wans Wu
Shader "URP/Skybox/CubemapAddFog_DayNight"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0.0
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex1 ("Cubemap1   (HDR)", Cube) = "grey" {}
        [NoScaleOffset] _Tex2 ("Cubemap2   (HDR)", Cube) = "grey" {}
        _Change("Change", Range(0, 1)) = 0.0
        _offset("CubemapOffset(XYZ)",Vector) = (0,0,0,0)
        _Saturate("Saturate", Range(0, 1)) = 1.0
        [Space(20)]
        [Toggle(_FOGCUSTOM)] _Fog("Enable Fog", float) = 0
        _FogIntensity ("Intensity", Float) = 1.0
        _FogColor ("FogColor", Color) = (0,0,0,1)
        _FogDistanceStart ("DistanceStart", Float) = 1.0
        _FogDistanceEnd ("DistanceEnd", Float) = 1.0
        _FogHeightStart ("HeightStart", Float) = -100
        _FogHeightEnd ("HeightEnd", Float) = 100
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull [_CullMode]
        //ZWrite On

        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            //#pragma multi_compile_fog
            #pragma shader_feature_local _FOGCUSTOM

            #define unity_ColorSpaceDouble half4(4.59479380, 4.59479380, 4.59479380, 2.0)

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            samplerCUBE _Tex1;
            samplerCUBE _Tex2;
            half4 _Tex1_HDR;
            half4 _Tex2_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            half _Light;
            half _Saturate;
            half _Change;
            float3 _offset;

            //fog
            float _FogDistanceStart;
            float _FogDistanceEnd;
            float _FogHeightStart;
            float _FogHeightEnd;
            float _SunFogRange;
            float _SunFogIntensity;
            float _FogIntensity;
            float4 _FogColor;
            float4 _SunFogColor;


            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }


            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 positionOS : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            float LinearFog(float Distance, float Start, float End)
            {
                return 1 - (clamp((End - Distance) / (End - Start), 0, 1));
                //return 1 - clamp(exp2(-Distance),0,1);
            }


            float3 MixFog_Skybox(float3 positionWS, float3 positionCamera, float FogDistanceStart, float FogDistanceEnd,
                                 float FogHeightStart, float FogHeightEnd, float3 vDir, float3 lDir, float SunFogRange,
                                 float SunFogIntensity, float FogIntensity, float3 FogColor, float3 SunFogColor,
                                 float3 color)
            {
                float distanceCam = distance(positionWS, positionCamera);
                float FogLinear = LinearFog(distanceCam, FogDistanceStart * 100000, FogDistanceEnd * 100000);
                float FogHeight = 1 - LinearFog(dot(positionWS,half3(0, 1, 0)), FogHeightStart * 100000,
          FogHeightEnd * 100000);
                float finalFog = clamp(FogLinear * FogHeight * FogIntensity, 0, 1);
                float3 finalColor = lerp(color, FogColor, finalFog);
                //float3 finalColor = finalFog.xxx;
                return finalColor;
            }

            inline half3 DecodeHDR(half4 data, half4 decodeInstructions, int colorspaceIsGamma)
            {
                // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
                half alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

                // If Linear mode is not supported we can skip exponent part
                if (colorspaceIsGamma)
                    return (decodeInstructions.x * alpha) * data.rgb;

                #   if defined(UNITY_USE_NATIVE_HDR)
                return decodeInstructions.x * data.rgb;
                #   else
                return (decodeInstructions.x * pow(alpha, decodeInstructions.y)) * data.rgb;
                #   endif
            }

            inline half3 DecodeHDR(half4 data, half4 decodeInstructions)
            {
                #if defined(UNITY_COLORSPACE_GAMMA)
                return DecodeHDR(data, decodeInstructions, 1);
                #else
                return DecodeHDR(data, decodeInstructions, 0);
                #endif
            }


            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(rotated);
                o.vertex = vertexInput.positionCS;
                o.positionWS = mul(unity_ObjectToWorld, rotated).xyz;
                float3 offset = RotateAroundYInDegrees(v.vertex.xyz, _offset);
                o.texcoord = offset;
                o.positionOS = v.vertex;
                //#if UNITY_REVERSED_Z 
                // o.vertex.z = o.vertex.w * 0.000001f;
                //#else
                //o.vertex.z = o.vertex.w * 0.999999f;
                //#endif
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 tex1 = texCUBE(_Tex1, i.texcoord);
                half4 tex2 = texCUBE(_Tex2, i.texcoord);
                float3 positionWS = i.positionWS;
                float3 vDirWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                half3 c1 = DecodeHDR(tex1, _Tex1_HDR);
                half3 c2 = DecodeHDR(tex2, _Tex2_HDR);
                half3 c = lerp(c1, c2, _Change);
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                half dotFactor = dot(c, float3(0.299, 0.587, 0.114));
                c = lerp(c, dotFactor.xxx, 1 - _Saturate);
                c *= _Exposure * (1 - _Light);
                #if defined (_FOGCUSTOM)
                c.rgb = MixFog_Skybox(i.positionWS, _WorldSpaceCameraPos, _FogDistanceStart, _FogDistanceEnd,
                    _FogHeightStart, _FogHeightEnd, vDirWS, 1, _SunFogRange, _SunFogIntensity,
                    _FogIntensity, _FogColor.rgb * _FogColor.rgb, _SunFogColor.rgb, c);
                #else
                c.rgb = c.rgb;
                #endif
                // float csz = i.vertex.z * i.vertex.w; 
                // real fogFactor = ComputeFogFactor(csz);
                // c.rgb = MixFog(c.rgb,fogFactor);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}