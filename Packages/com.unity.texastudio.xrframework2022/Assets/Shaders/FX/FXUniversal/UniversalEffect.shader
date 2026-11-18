Shader"Effect/UniversalEffect"
{

    Properties
    {
        //[Header(BasicSettings)]

        //[KeywordEnum(Alpha,Add,LinearBurn,SoftAdditive,Multiply,2XMultiply,SoftMultiply)] _Blend("Blend mode", Float) = 0
        [KeywordEnum(Alpha,Add,PreMultiply)] _Blend("Blend mode", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("BlendOp", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Toggle]_ZWrite("ZWrite", Int) = 0


        //[Header(Color)]
        [MainColor][HDR]_Color("MainColor",Color) = (1.0,1.0,1.0,1.0)
        [HDR] _Add_Color("Add Color", Color) = (1.0, 1.0, 1.0, 1.0)

        _LerpValue("Lerp Value",Range(0.0, 1.0)) = 1
        //_MaskSmoothstepMin("MaskSmoothstepMin",Range(0.0, 1.0)) = 0
        //_MaskSmoothstepMax("MaskSmoothstepMax",Range(0.0, 1.0)) = 1
        //[Header(BaseMap)]
        [MainTexture]_BaseMap("Base Map", 2D) = "white"
        [Toggle(_NORMALMAP)] _NORMALMAP("NORMALMAP", Int) = 0
        _NormalMap("Normal Map", 2D) = "bump"
        _NormalScale("NormalScale",Float) = 1
        _N_Offset_Xspeed("Normal Offset Xspeed",Float) = 0
        _N_Offset_Yspeed("Normal Offset Yspeed",Float) = 0
        [Toggle(_RASALPHA_MAIN)] _RasAlpha_Main("_RasAlpha_Main", Int) = 0
        [Toggle(_CLAMPU_MAIN)] _ClampU_Main("_ClampU_Main", Int) = 0
        [Toggle(_CLAMPV_MAIN)] _ClampV_Main("_ClampV_Main", Int) = 0
        [Toggle(_CLAMPREPEAT_MAIN)] _ClampRepeat_Main("ClampRepeat_Main", Int) = 0
        _M_Rotation("Main Rotation",Float) = 0
        _M_Offset_Xspeed("Main Offset Xspeed",Float) = 0
        _M_Offset_Yspeed("Main Offset Yspeed",Float) = 0
        [Toggle(_CUSTOMDATA_MAIN)] _CustomData_Main("_CustomData_Main", Int) = 0
        _Main_Saturation("Main Saturation",Float) = 1
        _Main_Power("Main Power",Float) = 1.0

        //[Header(AddTexture)]
        _AddTex("Add Texture", 2D) = "white"
        [KeywordEnum(Add,Lerp,Multiply)] _AddBlend("AddBlend", Float) = 0

        _Add_Rotation("AddTexture Rotation",Float) = 0
        _Add_Offset_Xspeed("Add Offset Xspeed",Float) = 0
        _Add_Offset_Yspeed("Add Offset Yspeed",Float) = 0

        //[Header(MaskMap)]
        _MaskTex("Mask Texture", 2D) = "white"
        _MaskStrength("_MaskStrength",Float) = 1
        _Mask_Offset_Xspeed("Mask Offset Xspeed",Float) = 0
        _Mask_Offset_Yspeed("Mask Offset Yspeed",Float) = 0
        _MaskRot("WarpTexRotation",Float) = 0
        [Toggle(_RASALPHA_MASK)] _RasAlpha_Mask("_RasAlpha_Mask", Int) = 0
        [Toggle(_COLOR_MASK)] _Color_Mask("_Color_Mask", Int) = 0
        [Toggle(_CLAMPU_MASK)] _ClampU_Mask("_ClampU_Mask", Int) = 0
        [Toggle(_CLAMPV_MASK)] _ClampV_Mask("_ClampV_Mask", Int) = 0
        [Toggle(_CUSTOMDATA_MASK)] _CustomData_Mask("_CustomData_Mask", Int) = 0
        [Toggle] _EnableWorldMask ("Enable World Mask", float) = 0
        _WorldVerticalMask("WorldVerticalMask",Float) = -10
        _WorldVerticalMaskIntense("WorldVerticalMaskIntense",Float) = 1
        //[Header(DissolveTex)]
        _DissolveTex("Dissolve Texture",2D) = "white" {}
        _Dissolve_Offset_Xspeed("Dissolve Offset Xspeed",Float) = 0
        _Dissolve_Offset_Yspeed("Dissolve Offset Yspeed",Float) = 0
        _DissolveRot("DissolveTexRotation",Float) = 0
        _Dissolve_Value("Dissolve Value",Range(0.0, 1.0)) = 0
        _Dissolve_Width("_Dissolve_Width",Range(0.0, 1.0)) = 0.1
        [Toggle(_DISSOLVE_SOFT)] _Dissolve_Soft("_Dissolve_Soft", Int) = 1
        _Dissolve_SoftValue("_Dissolve_SoftValue",Range(0.01, 1.0)) = 0.8
        [HDR] _Dissolve_Color("Dissolve Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Toggle(_CUSTOMDATA_DISSOLVE)] _CustomData_Dissolve("_CustomData_Dissolve", Int) = 0

        //[Header(WarpTex)]
        _WarpTex("Warp Texture",2D) = "white" {}
        _Warp_Intensity("Warp Intensity",Float) = 0
        _Warp_Offset_Xspeed("Warp Offset Xspeed",Float) = 0
        _Warp_Offset_Yspeed("Warp Offset Yspeed",Float) = 0
        _WarpRot("WarpTexRotation",Float) = 0
        [Toggle(_WARP_MAIN)] _Warp_Main("_Warp_Main", Int) = 1
        [Toggle(_WARP_MASK)] _Warp_Mask("_ClampU_Mask", Int) = 0
        [Toggle(_WARP_DISSOLVE)] _Warp_Dissolve("_Warp_Dissolve", Int) = 0
        [Toggle(_CUSTOMDATA_WARP)] _CustomData_Warp("_CustomData_Warp", Int) = 0

        //Fresnel
        _FresnalPower("FresnalPower",Float) = 1
        [HDR]_FresnelColor("FresnelColor",Color) = (1.0,1.0,1.0,1.0)
        _FresnelBias("_FresnelBias",Float) = 0
        _FresnelScale("_FresnelScale",Float) = 0
        [Toggle(_FRESNEL_REV)] _Fresnel_Rev("_Fresnel_Rev", Int) = 0

        //[Header(FinalControl)]
        _Alpha("Alpha",Float) = 1
        _AlphaPower("Alpha Power",Float) = 1
        _Saturation("Saturation",Range(0.0, 1.0)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent""Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Blend[_SrcBlend][_DstBlend] //Blend [_SrcBlend] [_DstBlend] //SrcAlpha one；  SrcAlpha OneMinusSrcAlpha
            BlendOp[_BlendOp]
            Cull[_Cull]
            ZTest[_ZTest]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma only_renderers gles gles3 glcore d3d11 metal
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            #pragma multi_compile_instancing
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _RASALPHA_MAIN
            #pragma shader_feature _CLAMPU_MAIN
            #pragma shader_feature _CLAMPV_MAIN
            #pragma shader_feature _CLAMPREPEAT_MAIN
            #pragma shader_feature _CUSTOMDATA_MAIN

            #pragma shader_feature _ADDTEX
            #pragma shader_feature _ADDBLEND_ADD _ADDBLEND_LERP _ADDBLEND_MULTIPLY

            #pragma shader_feature _MASK
            #pragma shader_feature _RASALPHA_MASK
            #pragma shader_feature _CLAMPU_MASK
            #pragma shader_feature _CLAMPV_MASK
            #pragma shader_feature _CUSTOMDATA_MASK
            #pragma shader_feature _COLOR_MASK

            #pragma shader_feature _WARP
            #pragma shader_feature _WARP_MAIN
            #pragma shader_feature _WARP_MASK
            //#pragma shader_feature _WARP_ADD
            #pragma shader_feature _WARP_DISSOLVE
            #pragma shader_feature _CUSTOMDATA_WARP

            #pragma shader_feature _DISSOLVE
            #pragma shader_feature _DISSOLVE_SOFT
            #pragma shader_feature _CUSTOMDATA_DISSOLVE

            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _FRESNEL_REV
            //#pragma multi_compile_fog
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                half4 color : COLOR;
                #ifdef _NORMALMAP
                float4 tangentOS : TANGENT;
                #endif
                float4 uv : TEXCOORD0; //MainTex UV
                float4 uv1 : TEXCOORD1; //Reserved for Custom Data1 MainTex UV, xy offset, z溶解值, w uv扭曲强度
                float4 uv2 : TEXCOORD2; //Reserved for Custom Data2 Mask UV, xy offset 
                //float4 uv3           : TEXCOORD3; //AddTex UV
                //float4 uv4           : TEXCOORD4; //MaskTex UV
                //float4 uv5           : TEXCOORD5; //WarpTex UV
                //float4 uv6           : TEXCOORD6; //Dissolve UV
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4 color : COLOR;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 positionWS : TEXCOORD3; //positionWS.w is fogfactor
                float3 positionOS: TEXCOORD4;
                //float4 uv3           : TEXCOORD3;
                //float4 uv4           : TEXCOORD4;
                //float4 uv5           : TEXCOORD5;
                //float4 uv6           : TEXCOORD6;
                #ifdef _NORMALMAP
                half4 normalWS : TEXCOORD5; // xyz: normal, w: viewDir.x
                half4 tangentWS : TEXCOORD6; // xyz: tangent, w: viewDir.y
                half4 bitangentWS : TEXCOORD7; // xyz: bitangent, w: viewDir.z
                #else
                half3 normalWS : TEXCOORD5;
                half3 viewDirWS: TEXCOORD6;
                //half fogFactor : TEXCOORD7;

                #endif
            };


            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_AddTex);
            SAMPLER(sampler_AddTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_WarpTex);
            SAMPLER(sampler_WarpTex);
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseMap_ST, _AddTex_ST, _MaskTex_ST, _WarpTex_ST, _DissolveTex_ST, _NormalMap_ST;
                half4 _Color, _Add_Color, _FresnelColor, _Dissolve_Color;
                half _M_Offset_Xspeed, _M_Offset_Yspeed, _Add_Offset_Xspeed, _Add_Offset_Yspeed, _Mask_Offset_Xspeed,
  _Mask_Offset_Yspeed, _Warp_Offset_Xspeed,
  _Warp_Offset_Yspeed, _Dissolve_Offset_Xspeed,
  _Dissolve_Offset_Yspeed;
                half _N_Offset_Xspeed, _N_Offset_Yspeed;
                half _M_Rotation, _Add_Rotation, _MaskRot, _WarpRot, _DissolveRot;
                half _LerpValue;
                //half _MaskSmoothstepMin, _MaskSmoothstepMax;
                half _WorldVerticalMask, _WorldVerticalMaskIntense;
                half _MaskStrength;
                half _Warp_Intensity;
                half _Dissolve_Value, _Dissolve_Width, _Dissolve_SoftValue;
                half _Alpha, _AlphaPower, _Saturation;
                half _Main_Power, _Main_Saturation;
                half _FresnalPower, _FresnelBias, _FresnelScale;
                half _NormalScale;
                half _EnableWorldMask;
            CBUFFER_END

            float3 Saturation(float3 In, float Saturation)
            {
                float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
                return luma.xxx + Saturation.xxx * (In - luma.xxx);
            }

            float Remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float2 Rotate_Degrees(float2 UV, float2 Center, float Rotation)
            {
                Rotation = Rotation * (3.1415926f / 180.0f);
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5;
                rMatrix += 0.5;
                rMatrix = rMatrix * 2 - 1;
                UV.xy = mul(UV.xy, rMatrix);
                UV += Center;
                return UV;
            }

            float2 PolarCoordinates(float2 UV, float2 Center, float RadialScale, float LengthScale)
            {
                float2 delta = UV - Center;
                float radius = length(delta) * 2 * RadialScale;
                float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
                return float2(radius, angle);
            }

            float2 UVTransform(float2 UV, half4 Tex_ST, float2 OffsetSpeed, float2 Center, float Rotation)
            {
                //Till and Offset
                UV = UV * Tex_ST.xy + Tex_ST.zw;

                #ifdef _CLAMPREPEAT_MAIN
                UV += (frac(_Time.y * OffsetSpeed) - 0.5) * Tex_ST.xy * 2;
                #else
                UV += _Time.y * OffsetSpeed;
                #endif

                //Roate
                UV = Rotate_Degrees(UV, Center, Rotation);
                return UV;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                #ifdef _NORMALMAP
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normal.xyz, IN.tangentOS);
                #else
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normal.xyz);
                #endif

                OUT.positionWS.xyz = positionInputs.positionWS;
                OUT.positionOS = IN.positionOS.xyz;
                half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                float3x3 tangentTransform_World = float3x3(normalInputs.tangentWS.xyz, normalInputs.bitangentWS.xyz,
                               normalInputs.normalWS.xyz);
                float3 viewDirTS = TransformWorldToTangent(viewDirWS, tangentTransform_World);

                #ifdef _NORMALMAP
                OUT.normalWS = half4(normalInputs.normalWS, viewDirTS.x);
                OUT.tangentWS = half4(normalInputs.tangentWS, viewDirTS.y);
                OUT.bitangentWS = half4(normalInputs.bitangentWS, viewDirTS.z);
                #else
                OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                OUT.viewDirWS = viewDirWS;
                #endif
                OUT.color = IN.color;
                OUT.uv = IN.uv;
                OUT.uv1 = IN.uv1;
                OUT.uv2 = IN.uv2;
                OUT.positionWS.w = ComputeFogFactor(positionInputs.positionCS.z); //positionWS.w is fogfactor
                //OUT.uv3 = IN.uv3;
                //OUT.uv4 = IN.uv4;
                //OUT.uv5 = IN.uv5;
                //OUT.uv6 = IN.uv6;

                //Fresnel
                //#ifndef _NORMALMAP
                //
                //                float3 viewDir = TransformWorldToObject(GetCameraPositionWS()) - IN.positionOS.xyz;
                //                OUT.fresnel = saturate(_FresnelBias + pow((1.0 - saturate(dot(normalize(IN.normal), normalize(viewDir)))), _FresnalPower) * _FresnelScale);
                //#endif


                ////MainTex UV
                //OUT.uv.xy = UVTransform(IN.uv.xy, _BaseMap_ST, float2(_M_Offset_Xspeed, _M_Offset_Yspeed), float2(0.5, 0.5), _M_Rotation);
                //#if _CUSTOMDATA_MAIN
                //    OUT.uv.xy += OUT.uv1.xy;
                //#endif
                //#if _CLAMPU_MAIN
                //    OUT.uv.x = saturate(OUT.uv2.x);
                //#endif
                //#if _CLAMPV_MAIN
                //    OUT.uv.y = saturate(OUT.uv2.y);
                //#endif

                //#if _ADDTEX //AddTex UV           
                //      OUT.uv3.xy = UVTransform(IN.uv3.xy, _AddTex_ST, float2(_Add_Offset_Xspeed, _Add_Offset_Yspeed), float2(0.5, 0.5), _Add_Rotation);
                //#endif

                //#if _MASK //MaskTex UV

                //    OUT.uv4.xy = UVTransform(IN.uv4.xy, _MaskTex_ST, float2(_Mask_Offset_Xspeed, _Mask_Offset_Yspeed), float2(0.5, 0.5), _MaskRot);
                //    #if _CUSTOMDATA_MASK
                //                    OUT.uv4.xy += OUT.uv2.xy;
                //    #endif

                //    #if _CLAMPU_MASK
                //                    OUT.uv4.x = saturate(OUT.uv4.x);
                //    #endif

                //    #if _CLAMPV_MASK
                //                    OUT.uv4.y = saturate(OUT.uv4.y);
                //    #endif
                //#endif

                //#if _DISSOLVE //Dissolve UV         
                //    OUT.uv6.xy = UVTransform(IN.uv3.xy, _DissolveTex_ST, float2(_Dissolve_Offset_Xspeed, _Dissolve_Offset_Yspeed), float2(0.5, 0.5), _DissolveRot);
                //#endif

                //#if _WARP //WarpTex UV           
                //     OUT.uv5.xy = UVTransform(IN.uv5.xy, _WarpTex_ST, float2(_Warp_Offset_Xspeed, _Warp_Offset_Yspeed), float2(0.5, 0.5), _WarpRot);
                //#endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 MainUV = IN.uv.xy;
                float2 AddTexUV = IN.uv.xy;
                float2 MaskTexUV = IN.uv.xy;
                float2 DissolveUV = IN.uv.xy;
                float2 WarpTexUV = IN.uv.xy;

                //MainTex UV
                MainUV = UVTransform(MainUV, _BaseMap_ST, float2(_M_Offset_Xspeed, _M_Offset_Yspeed), float2(0.5, 0.5),
                                         _M_Rotation);
                #if _CUSTOMDATA_MAIN
                MainUV += IN.uv1.xy;
                #endif
                #if _CLAMPU_MAIN
                MainUV.x = saturate(MainUV.x);
                #endif
                #if _CLAMPV_MAIN
                MainUV.y = saturate(MainUV.y);
                #endif

                #if _ADDTEX //AddTex UV
                AddTexUV = UVTransform(AddTexUV, _AddTex_ST, float2(_Add_Offset_Xspeed, _Add_Offset_Yspeed),
       float2(0.5, 0.5), _Add_Rotation);
                #endif

                #if _MASK //MaskTex UV

                MaskTexUV = UVTransform(MaskTexUV, _MaskTex_ST, float2(_Mask_Offset_Xspeed, _Mask_Offset_Yspeed),
                       float2(0.5, 0.5), _MaskRot);
                #if _CUSTOMDATA_MASK
                MaskTexUV += IN.uv2.xy;
                #endif

                #if _CLAMPU_MASK
                MaskTexUV.x = saturate(MaskTexUV.x);
                #endif

                #if _CLAMPV_MASK
                MaskTexUV.y = saturate(MaskTexUV.y);
                #endif
                #endif

                #if _DISSOLVE //Dissolve UV
                DissolveUV = UVTransform(DissolveUV, _DissolveTex_ST,
                    float2(_Dissolve_Offset_Xspeed, _Dissolve_Offset_Yspeed), float2(0.5, 0.5), _DissolveRot);
                #endif

                #if _WARP //WarpTex UV
                WarpTexUV = UVTransform(WarpTexUV, _WarpTex_ST, float2(_Warp_Offset_Xspeed, _Warp_Offset_Yspeed),
                        float2(0.5, 0.5), _WarpRot);
                #endif

                #if _WARP
                half4 WarpTex_color = SAMPLE_TEXTURE2D(_WarpTex, sampler_WarpTex, WarpTexUV);
                half2 warpUV = half2(0, 0);
                #if _CUSTOMDATA_WARP
                warpUV = (WarpTex_color.xy) * IN.uv1.w;
                #else
                            warpUV = (WarpTex_color.xy ) * _Warp_Intensity;
                #endif
                #if _WARP_MAIN
                MainUV += warpUV;
                AddTexUV += warpUV;
                #endif
                #if _WARP_MASK
                MaskTexUV += warpUV;
                #endif
                #if _WARP_DISSOLVE
                DissolveUV += warpUV;
                #endif
                #endif

                //MainTex
                half4 MainTex_color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, MainUV);
                #if _RASALPHA_MAIN
                MainTex_color.a = MainTex_color.r;
                #endif
                //half4 finalColor = MainTex_color;
                half3 color = pow(MainTex_color.rgb, _Main_Power);
                half4 finalColor = half4(color, MainTex_color.a);

                finalColor.rgb = Saturation(finalColor.rgb, _Main_Saturation);
                finalColor *= _Color;

                #if _ADDTEX //AddTex

                half4 AddTex_color = SAMPLE_TEXTURE2D(_AddTex, sampler_AddTex, AddTexUV) * _Add_Color;
                //Blend
                #if _ADDBLEND_LERP
                        finalColor.rgb = lerp(finalColor.rgb, AddTex_color.rgb, _LerpValue);
                #elif _ADDBLEND_ADD
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb + AddTex_color.rgb, _LerpValue);
                #elif _ADDBLEND_MULTIPLY
                        finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * AddTex_color.rgb,_LerpValue);
                #endif
                #endif


                finalColor *= IN.color; //粒子染色


                #if _MASK
                half4 MaskTex_color = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, MaskTexUV);
                MaskTex_color = saturate(_MaskStrength * MaskTex_color);
                //MaskTex_color *= smoothstep(_MaskSmoothstepMin, _MaskSmoothstepMax, MaskTex_color);
                #if _RASALPHA_MASK
                finalColor.a *= MaskTex_color.r;
                #else
                        finalColor.a *= MaskTex_color.a;
                #endif
                #if _COLOR_MASK
                finalColor.rgb *= MaskTex_color.rgb;
                #endif
                #endif


                #if _DISSOLVE //DissolveTex

                half4 DissolveTex_color = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, DissolveUV);
                half DissolveTex_grey = min(DissolveTex_color.r, DissolveTex_color.a);

                #if _CUSTOMDATA_DISSOLVE
                half LocalDissolve_Value = IN.uv1.z;
                #else
                                        half LocalDissolve_Value = _Dissolve_Value;
                #endif

                #if _DISSOLVE_SOFT
                LocalDissolve_Value = Remap(LocalDissolve_Value, float2(0, 1), float2(-_Dissolve_Width, 1.01));
                half small = smoothstep(LocalDissolve_Value + lerp(0, _Dissolve_Width - 0.01, 1 - _Dissolve_SoftValue),
                            LocalDissolve_Value + _Dissolve_Width, DissolveTex_grey);
                finalColor.rgb = lerp(_Dissolve_Color.rgb, finalColor.rgb, small);
                finalColor.a *= small;

                #else
                                        LocalDissolve_Value = lerp(-_Dissolve_Width, 1, _Dissolve_Value);
                                        half DissolveRim = (step(DissolveTex_grey - LocalDissolve_Value, _Dissolve_Width) - step(DissolveTex_grey - LocalDissolve_Value, 0));
                                        //return  DissolveRim * _Dissolve_Color;
                                        finalColor.rgb = saturate(saturate(finalColor - DissolveRim) + DissolveRim * _Dissolve_Color);
                                        clip(DissolveTex_grey - LocalDissolve_Value);
                                        //return  DissolveRim * _Dissolve_Color;
                                        //return finalColor.rgb;
                #endif


                #endif

                #ifndef _NORMALMAP


                //float fresnel = saturate(IN.fresnel);

                float fresnel = saturate(
                    _FresnelBias + pow((1.0 - saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS)))),
    _FresnalPower) * _FresnelScale);

                #else
                float2 NormalUV = UVTransform(IN.uv.xy, _NormalMap_ST, float2(_N_Offset_Xspeed, _N_Offset_Yspeed),
                float2(0.5, 0.5), 0);
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, NormalUV), _NormalScale);
                half3 viewDirTS = float3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w); //viewDirWorldSpace
                float fresnel = saturate(
                    _FresnelBias + pow((1.0 - saturate(dot(normalize(normalTS), normalize(viewDirTS)))), _FresnalPower)
                    * _FresnelScale);
                #endif


                #if _FRESNEL_REV
                finalColor.a = finalColor.a * (1 - fresnel);
                #else
                finalColor.rgb = lerp(finalColor.rgb, _FresnelColor.rgb, fresnel);
                #endif

                half worldMask = _EnableWorldMask == 1
                 ? saturate(pow((IN.positionWS.y - _WorldVerticalMask) * _WorldVerticalMaskIntense, 5))
                 : 1;
                finalColor.a *= worldMask;

                finalColor.a = saturate(finalColor.a * _Alpha);
                finalColor.rgb = pow(finalColor.rgb, _AlphaPower);
                //finalColor.rgb = Saturation(finalColor.rgb, _Saturation);
                //finalColor.rgb = MixFog(finalColor.rgb, IN.positionWS.w); //positionWS.w is fogfactor
                #if defined(_ALPHAPREMULTIPLY_ON)
                finalColor.rgb *= finalColor.a;
                #endif
                return finalColor;
            }
            ENDHLSL
        }
    }
    CustomEditor "UniversalEffect"
}