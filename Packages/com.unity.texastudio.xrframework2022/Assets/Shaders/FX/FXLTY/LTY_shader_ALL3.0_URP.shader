// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LTY/Shader3.0/ALL3.0_URP"
{
    Properties
    {
        [HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
        [HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
        _AlphaClipThreshold ("AlphaClipThreshold", Range(0,1)) = 0.5
        [Enum(Add,1,Alpha,10)]_Dst("BlendMode", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 0
        [Enum(Less or Equal,4,Always,8)]_ZTestMode("ZTest", Float) = 4
        [Enum(ON,1,OFF,0)]_Zwrite("Zwrite", Float) = 0
        [Toggle] _AlphaTest ("Alpha Clip", Float) = 0
        _OffsetFactor ("Offset Factor", Float) = 0
        _OffsetUnits ("Offset Units", Float) = 0
        _ZTestAddValue ("ZTest Add Value", float) = 0
        _QueueOffset("QueueOffset", Range(-50,50)) = 0
        
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [IntRange]_StencilRef ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Stencil Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("Stencil ZFail", Float) = 0
        
        //_SoftParticle("SoftParticle", Range( 0 , 10)) = 0
        [Header(Main_Tex)]_Main_Tex("Main_Tex", 2D) = "white" {}
        [HDR]_Main_Tex_Color("Main_Tex_Color", Color) = (1,1,1,1)
        [Enum(R,0,A,1)]_Main_Tex_A_R("Main_Tex_A_R", Float) = 0
        [Enum(Normal,0,Custom,1)]_Main_Tex_Custom_ZW("Main_Tex_Custom_ZW", Float) = 0
        [Enum(Repeat,0,Clmap,1)]_Main_Tex_ClampSwitch("Main_Tex_ClampSwitch", Float) = 0
        _Main_Tex_Rotator("Main_Tex_Rotator", Range( 0 , 360)) = 0
        _Main_Tex_U_speed("Main_Tex_U_speed", Float) = 0
        _Main_Tex_V_speed("Main_Tex_V_speed", Float) = 0
        [Header(Remap_Tex)]_Remap_Tex("Remap_Tex", 2D) = "white" {}
        [Enum(OFF,0,ON,1)]_Remap_Tex_Followl_Main_Tex("Remap_Tex_Followl_Main_Tex", Float) = 0
        [Enum(R,0,A,1)]_Remap_Tex_A_R("Remap_Tex_A_R", Float) = 0
        _Remap_Tex_Desaturate("Remap_Tex_Desaturate", Range( 0 , 1)) = 0
        [Enum(Repeat,0,Clmap,1)]_Remap_Tex_ClampSwitch("Remap_Tex_ClampSwitch", Float) = 0
        _Remap_Tex_Rotator("Remap_Tex_Rotator", Range( 0 , 360)) = 0
        _Remap_Tex_U_speed("Remap_Tex_U_speed", Float) = 0
        _Remap_Tex_V_speed("Remap_Tex_V_speed", Float) = 0
        [Header(Mask_Tex)]_Mask_Tex("Mask_Tex", 2D) = "white" {}
        [Enum(Repeat,0,Clmap,1)]_Mask_Tex_ClampSwitch("Mask_Tex_ClampSwitch", Float) = 0
        [Enum(R,0,A,1)]_Mask_Tex_A_R("Mask_Tex_A_R", Float) = 0
        _Mask_Tex_Rotator("Mask_Tex_Rotator", Range( 0 , 360)) = 0
        _Mask_Tex_U_speed("Mask_Tex_U_speed", Float) = 0
        _Mask_Tex_V_speed("Mask_Tex_V_speed", Float) = 0
        [Header(Dissolve_Tex)]_Dissolve_Tex("Dissolve_Tex", 2D) = "white" {}
        [Toggle(_DISSOLVE_SWITCH_ON)] _Dissolve_Switch("Dissolve_Switch", Float) = 0
        [Enum(R,0,A,1)]_Dissolve_Tex_A_R("Dissolve_Tex_A_R", Float) = 0
        [Enum(OFF,0,ON,1)]_Dissolve_Tex_Custom_X("Dissolve_Tex_Custom_X", Float) = 0
        _Dissolve_Tex_Rotator("Dissolve_Tex_Rotator", Range( 0 , 360)) = 0
        _Dissolve_Tex_smooth("Dissolve_Tex_smooth", Range( 0.5 , 1)) = 0.5
        _Dissolve_Tex_power("Dissolve_Tex_power", Range( 0 , 1)) = 0
        _Dissolve_Tex_U_speed("Dissolve_Tex_U_speed", Float) = 0
        _Dissolve_Tex_V_speed("Dissolve_Tex_V_speed", Float) = 0
        [Header(Distortion_Tex)]_Distortion_Tex("Distortion_Tex", 2D) = "white" {}
        [Enum(OFF,0,ON,1)]_Distortion_Switch("Distortion_Switch", Float) = 0
        _Distortion_Tex_Power("Distortion_Tex_Power", Float) = 0
        _Distortion_Tex_U_speed("Distortion_Tex_U_speed", Float) = 0
        _Distortion_Tex_V_speed("Distortion_Tex_V_speed", Float) = 0
        [HDR][Header(Fresnel)]_Fresnel_Color("Fresnel_Color", Color) = (1,1,1,1)
        [Toggle(_FRESNEL_SWITCH_ON)] _Fresnel_Switch("Fresnel_Switch", Float) = 0
        [Enum(Fresnel,0,Bokeh,1)]_Fresnel_Bokeh("Fresnel_Bokeh", Float) = 0
        _Fresnel_scale("Fresnel_scale", Float) = 1
        _Fresnel_power("Fresnel_power", Float) = 5
        [Header(Wpo_Tex)]_WPO_Tex("WPO_Tex", 2D) = "white" {}
        [Toggle(_WPO_SWITCH_ON)] _WPO_Switch("WPO_Switch", Float) = 0
        [Enum(Normal,0,Custom,1)]_WPO_CustomSwitch_Y("WPO_CustomSwitch_Y", Float) = 0
        _WPO_tex_power("WPO_tex_power", Float) = 0
        _WPO_Tex_Direction("WPO_Tex_Direction", Vector) = (1,1,1,0)
        _WPO_Tex_U_speed("WPO_Tex_U_speed", Float) = 0
        [ASEEnd]_WPO_Tex_V_speed("WPO_Tex_V_speed", Float) = 0

    }

    SubShader
    {


        Tags
        {
            "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"
        }

        Cull [_CullMode]
        AlphaToMask Off

        HLSLINCLUDE
        
        #pragma target 3.0
        
        #pragma only_renderers gles gles3 glcore d3d11 metal
        ENDHLSL


        Pass
        {

            Name "Forward"
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            Blend SrcAlpha [_Dst], One OneMinusSrcAlpha
            ZWrite [_Zwrite]
            ZTest [_ZTestMode]
            Offset [_OffsetFactor],[_OffsetFactor]
            ColorMask RGBA


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            #pragma shader_feature _WPO_SWITCH_ON
            #pragma shader_feature _FRESNEL_SWITCH_ON
            #pragma shader_feature _DISSOLVE_SWITCH_ON
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            
            struct VertexInput
            {
                float4 vertex : POSITION;
                #if defined(_FRESNEL_SWITCH_ON) || defined(_WPO_SWITCH_ON)
                float3 ase_normal : NORMAL;
                #endif
                float4 ase_texcoord1 : TEXCOORD1;
                float4 ase_texcoord : TEXCOORD0;
                float4 ase_color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
                #if defined(_FRESNEL_SWITCH_ON)
                float3 worldPos : TEXCOORD0;
                #endif
    //             #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				// float4 shadowCoord : TEXCOORD1;
    //             #endif
    //             #ifdef ASE_FOG
				// float fogFactor : TEXCOORD2;
    //             #endif
                float4 ase_color : COLOR;
                float4 ase_texcoord3 : TEXCOORD3;
                float4 ase_texcoord4 : TEXCOORD4;
                #if defined(_FRESNEL_SWITCH_ON)
                float4 ase_texcoord5 : TEXCOORD5;
                #endif
                float4 ase_texcoord6 : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Main_Tex_ST;
                float4 _Mask_Tex_ST;
                half4 _Fresnel_Color;
                float4 _WPO_Tex_ST;
                float4 _WPO_Tex_Direction;
                half4 _Main_Tex_Color;
                float4 _Distortion_Tex_ST;
                float4 _Dissolve_Tex_ST;
                float4 _Remap_Tex_ST;
                float _Dissolve_Tex_Rotator;
                half _Dissolve_Tex_A_R;
                half _Dissolve_Tex_power;
                half _Dissolve_Tex_Custom_X;
                float _Remap_Tex_U_speed;
                float _Remap_Tex_V_speed;
                float _Zwrite;
                float _Remap_Tex_Rotator;
                half _Remap_Tex_ClampSwitch;
                half _Remap_Tex_Desaturate;
                half _Main_Tex_A_R;
                //half _SoftParticle;
                float _Mask_Tex_U_speed;
                float _Mask_Tex_V_speed;
                float _Mask_Tex_Rotator;
                float _Mask_Tex_ClampSwitch;
                float _Remap_Tex_Followl_Main_Tex;
                float _Dissolve_Tex_V_speed;
                half _Fresnel_Bokeh;
                half _Dissolve_Tex_smooth;
                float _ZTestMode;
                float _Dst;
                float _CullMode;
                half _WPO_tex_power;
                half _WPO_CustomSwitch_Y;
                float _WPO_Tex_U_speed;
                float _WPO_Tex_V_speed;
                float _Main_Tex_U_speed;
                float _Main_Tex_V_speed;
                float _Main_Tex_Custom_ZW;
                float _Distortion_Tex_Power;
                float _Distortion_Tex_U_speed;
                float _Distortion_Tex_V_speed;
                float _Distortion_Switch;
                float _Main_Tex_Rotator;
                float _Main_Tex_ClampSwitch;
                half _Fresnel_power;
                half _Fresnel_scale;
                half _Mask_Tex_A_R;
                float _Dissolve_Tex_U_speed;
                half _Remap_Tex_A_R;
                half _AlphaClipThreshold;
                float _ZTestAddValue;
            CBUFFER_END

            sampler2D _WPO_Tex;
            sampler2D _Main_Tex;
            sampler2D _Distortion_Tex;
            sampler2D _Dissolve_Tex;
            sampler2D _Remap_Tex;
            //uniform float4 _CameraDepthTexture_TexelSize;
            sampler2D _Mask_Tex;


            VertexOutput VertexFunction(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                #ifdef _WPO_SWITCH_ON
                float4 texCoord395 = v.ase_texcoord1;
                texCoord395.xy = v.ase_texcoord1.xy * float2(1, 1) + float2(0, 0);
                float lerpResult398 = lerp(_WPO_tex_power, texCoord395.y, _WPO_CustomSwitch_Y);
                float2 appendResult386 = (float2(_WPO_Tex_U_speed, _WPO_Tex_V_speed));
                 float2 uv_WPO_Tex = v.ase_texcoord.xy * _WPO_Tex_ST.xy + _WPO_Tex_ST.zw;
                float2 panner390 = (1.0 * _Time.y * appendResult386 + uv_WPO_Tex);
                float4 staticSwitch408 = (lerpResult398 * float4(v.ase_normal, 0.0) * tex2Dlod(
                    _WPO_Tex, float4(panner390, 0, 0.0)) * _WPO_Tex_Direction);
                #else
				float4 staticSwitch408 = (0.0).xxxx;
                #endif
                float4 Vertex117 = staticSwitch408;
                #if defined(_FRESNEL_SWITCH_ON)
                float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
                o.ase_texcoord5.xyz = ase_worldNormal;
                #endif
                float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
                float4 screenPos = ComputeScreenPos(ase_clipPos);
                o.ase_texcoord6 = screenPos;

                o.ase_color = v.ase_color;
                o.ase_texcoord3.xy = v.ase_texcoord.xy;
                o.ase_texcoord4 = v.ase_texcoord1;

                //setting value to unused interpolator channels and avoid initialization warnings
                o.ase_texcoord3.zw = 0;
                #if defined(_FRESNEL_SWITCH_ON)
                o.ase_texcoord5.w = 0;
                #endif
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
                #else
                float3 defaultVertexValue = float3(0, 0, 0);
                #endif
                float3 vertexValue = Vertex117.rgb;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
                #else
                v.vertex.xyz += vertexValue;
                #endif
                //v.ase_normal = v.ase_normal;

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float4 positionCS = TransformWorldToHClip(positionWS);
                #if UNITY_REVERSED_Z
                positionCS.z += _ZTestAddValue;
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z -= _ZTestAddValue;
                positionCS.z = max(positionCS.z,positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                #if defined(_FRESNEL_SWITCH_ON)
                o.worldPos = positionWS;
                #endif
    //             #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				// VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				// vertexInput.positionWS = positionWS;
				// vertexInput.positionCS = positionCS;
				// o.shadowCoord = GetShadowCoord( vertexInput );
    //             #endif
    //             #ifdef ASE_FOG
				// o.fogFactor = ComputeFogFactor( positionCS.z );
    //             #endif
                o.clipPos = positionCS;
                return o;
            }

            VertexOutput vert(VertexInput v)
            {
                return VertexFunction(v);
            }

            half4 frag(VertexOutput IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                #if defined(_FRESNEL_SWITCH_ON)
                float3 WorldPosition = IN.worldPos;
      //           #endif
      //           float4 ShadowCoords = float4(0, 0, 0, 0);
      //
      //           #if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
      //           #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						// ShadowCoords = IN.shadowCoord;
      //           #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						// ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
      //           #endif
                #endif
                float2 appendResult298 = (float2((_Main_Tex_U_speed * _TimeParameters.x),
                                                 (_TimeParameters.x * _Main_Tex_V_speed)));
                float2 uv_Main_Tex = IN.ase_texcoord3.xy * _Main_Tex_ST.xy + _Main_Tex_ST.zw;
                float4 texCoord288 = IN.ase_texcoord4;
                texCoord288.xy = IN.ase_texcoord4.xy * float2(1, 1) + float2(0, 0);
                float2 appendResult301 = (float2(texCoord288.z, texCoord288.w));
                float2 lerpResult308 = lerp((frac(appendResult298) + uv_Main_Tex), (uv_Main_Tex + appendResult301),
                                                                 _Main_Tex_Custom_ZW);
                float2 appendResult283 = (float2(_Distortion_Tex_U_speed, _Distortion_Tex_V_speed));
                float2 uv_Distortion_Tex = IN.ase_texcoord3.xy * _Distortion_Tex_ST.xy + _Distortion_Tex_ST.zw;
                float2 panner286 = (1.0 * _Time.y * appendResult283 + uv_Distortion_Tex);
                float lerpResult309 = lerp(
                    0.0, (_Distortion_Tex_Power * (-0.5 + (tex2D(_Distortion_Tex, panner286).r - 0.0) * (0.5 - -0.5) / (
                        1.0 - 0.0))), _Distortion_Switch);
                float2 ONE214 = (lerpResult308 + lerpResult309);
                float cos321 = cos(((_Main_Tex_Rotator * PI) / 180.0));
                float sin321 = sin(((_Main_Tex_Rotator * PI) / 180.0));
                float2 rotator321 = mul(ONE214 - float2(0.5, 0.5), float2x2(cos321, -sin321, sin321, cos321)) + float2(
                    0.5, 0.5);
                float2 lerpResult411 = lerp(rotator321, saturate(rotator321), _Main_Tex_ClampSwitch);
                half4 tex2DNode1 = tex2D(_Main_Tex, lerpResult411);
                #if defined(_FRESNEL_SWITCH_ON)
                half3 ase_worldViewDir = (_WorldSpaceCameraPos.xyz - WorldPosition);
                ase_worldViewDir = normalize(ase_worldViewDir);
                half3 ase_worldNormal = IN.ase_texcoord5.xyz;
                half dotResult343 = saturate(dot(ase_worldViewDir, ase_worldNormal));
                half smoothstepResult438 = smoothstep(0.0, 1.0, (1.0 - saturate(abs(dotResult343))));
                half temp_output_363_0 = (pow(smoothstepResult438, _Fresnel_power) * (_Fresnel_scale * 1));
                half lerpResult370 = lerp(temp_output_363_0, (1.0 - temp_output_363_0), _Fresnel_Bokeh);
                half staticSwitch372 = lerpResult370;
                #else
				half staticSwitch372 = 1.0;
                #endif
                
                #ifdef _DISSOLVE_SWITCH_ON
                float2 appendResult342 = (float2(_Dissolve_Tex_U_speed, _Dissolve_Tex_V_speed));
                float2 uv_Dissolve_Tex = IN.ase_texcoord3.xy * _Dissolve_Tex_ST.xy + _Dissolve_Tex_ST.zw;
                float cos430 = cos(((_Dissolve_Tex_Rotator * PI) / 180.0));
                float sin430 = sin(((_Dissolve_Tex_Rotator * PI) / 180.0));
                float2 rotator430 = mul(uv_Dissolve_Tex - float2(0.5, 0.5), float2x2(cos430, -sin430, sin430, cos430)) +
                    float2(0.5, 0.5);
                float2 panner346 = (1.0 * _Time.y * appendResult342 + rotator430);
                half4 tex2DNode353 = tex2D(_Dissolve_Tex, panner346);
                half lerpResult417 = lerp(tex2DNode353.r, tex2DNode353.a, _Dissolve_Tex_A_R);
                half five300 = texCoord288.x;
                half lerpResult351 = lerp(_Dissolve_Tex_power, five300, _Dissolve_Tex_Custom_X);
                half smoothstepResult368 = smoothstep((1.0 - _Dissolve_Tex_smooth), _Dissolve_Tex_smooth,
                        saturate(((lerpResult417 + 1.0) - (lerpResult351 * 2.0))));
                half staticSwitch371 = smoothstepResult368;
                #else
				half staticSwitch371 = 1.0;
                #endif
                #if defined(_FRESNEL_SWITCH_ON)
                half4 four222 = (staticSwitch372 * staticSwitch371 * _Fresnel_Color);
                #else
                half4 four222 = 1;
                #endif
                float2 appendResult420 = (float2(_Remap_Tex_U_speed, _Remap_Tex_V_speed));
                float2 uv_Remap_Tex = IN.ase_texcoord3.xy * _Remap_Tex_ST.xy + _Remap_Tex_ST.zw;
                float2 panner421 = (1.0 * _Time.y * appendResult420 + uv_Remap_Tex);
                float2 temp_cast_0 = (0.0).xx;
                float2 GAM_MAIN_Move446 = appendResult301;
                float2 lerpResult448 = lerp(temp_cast_0, GAM_MAIN_Move446, _Remap_Tex_Followl_Main_Tex);
                float cos399 = cos(((_Remap_Tex_Rotator * PI) / 180.0));
                float sin399 = sin(((_Remap_Tex_Rotator * PI) / 180.0));
                float2 rotator399 = mul((panner421 + lerpResult448) - float2(0.5, 0.5),
                                             float2x2(cos399, -sin399, sin399, cos399)) +
                    float2(0.5, 0.5);
                float2 lerpResult433 = lerp(rotator399, saturate(rotator399), _Remap_Tex_ClampSwitch);
                half4 tex2DNode406 = tex2D(_Remap_Tex, (lerpResult309 + lerpResult433));
                half3 desaturateInitialColor441 = tex2DNode406.rgb;
                half desaturateDot441 = dot(desaturateInitialColor441, float3(0.299, 0.587, 0.114));
                half3 desaturateVar441 = lerp(desaturateInitialColor441, desaturateDot441.xxx, _Remap_Tex_Desaturate);
                half3 appendResult425 = (float3(desaturateVar441));
                half3 Gam258 = appendResult425;

                half lerpResult409 = lerp(tex2DNode1.r, tex2DNode1.a, _Main_Tex_A_R);
                #if defined(_FRESNEL_SWITCH_ON)
                half Three217 = (staticSwitch372 * _Fresnel_Color.a);
                #else
                half Three217 = 1;
                #endif
                // float4 screenPos = IN.ase_texcoord6;
                // float4 ase_screenPosNorm = screenPos / screenPos.w;
                // ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0)
                //     ? ase_screenPosNorm.z
                //     : ase_screenPosNorm.z * 0.5 + 0.5;
                // float screenDepth393 = LinearEyeDepth(
                //     SHADERGRAPH_SAMPLE_SCENE_DEPTH(ase_screenPosNorm.xy), _ZBufferParams);
                // float distanceDepth393 = abs(
                //     (screenDepth393 - LinearEyeDepth(ase_screenPosNorm.z, _ZBufferParams)) / (_SoftParticle));
                // float soft219 = saturate(distanceDepth393);
                float2 appendResult332 = (float2(_Mask_Tex_U_speed, _Mask_Tex_V_speed));
                float2 uv_Mask_Tex = IN.ase_texcoord3.xy * _Mask_Tex_ST.xy + _Mask_Tex_ST.zw;
                float2 panner334 = (1.0 * _Time.y * appendResult332 + uv_Mask_Tex);
                float cos335 = cos(((_Mask_Tex_Rotator * PI) / 180.0));
                float sin335 = sin(((_Mask_Tex_Rotator * PI) / 180.0));
                float2 rotator335 = mul(panner334 - float2(0.5, 0.5), float2x2(cos335, -sin335, sin335, cos335)) +
                    float2(0.5, 0.5);
                float2 lerpResult413 = lerp(rotator335, saturate(rotator335), _Mask_Tex_ClampSwitch);
                half4 tex2DNode327 = tex2D(_Mask_Tex, lerpResult413);
                half lerpResult453 = lerp(tex2DNode327.r, tex2DNode327.a, _Mask_Tex_A_R);
                half two215 = lerpResult453;
                half lerpResult432 = lerp(tex2DNode406.r, tex2DNode406.a, _Remap_Tex_A_R);

                half3 BakedAlbedo = 0;
                half3 BakedEmission = 0;
                half3 Color = (IN.ase_color * (_Main_Tex_Color * tex2DNode1) * four222 * float4(Gam258, 0.0)).rgb;
                // float Alpha = (IN.ase_color.a * _Main_Tex_Color.a * lerpResult409 * Three217 * soft219 * two215 *
                //     lerpResult432);
                half Alpha = (IN.ase_color.a * _Main_Tex_Color.a * lerpResult409 * Three217 * two215 *
                    lerpResult432 * staticSwitch371);
                //half AlphaClipThreshold = 0.5;
                half AlphaClipThresholdShadow = 0.5;

                #ifdef _ALPHATEST_ON
					clip( Alpha - _AlphaClipThreshold );
                #endif
                
                return half4(Color, Alpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "LTY_ShaderGUI_URP"
    Fallback "Hidden/InternalErrorShader"
}