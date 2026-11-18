Shader "TA Shaders/FX_Common"
{
    Properties
    {
        [HideInInspector][KeywordEnum(Alpha,PreMultiply,Additive,Soft Additive,Multiply,2x Multiply,Darken,Lighten,Screen,Linear Dodge)]_Blend ("Blend Mode", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_CullMode ("Cull Mode", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("BlendOp", Float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [HideInInspector][Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [IntRange]_Stencil ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Stencil Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0

        [Toggle]_AlphaTest ("AlphaTest", Float) = 0
        [Toggle]_BaseRtoA ("BaseRtoA", float) = 0
        [Toggle]_StencilToggle ("Stencil Toggle", float) = 0
        [Toggle(_CLAMPREPEAT_MAIN)] _ClampRepeat ("ClampRepeat", float) = 0
        [Toggle]_BaseDetailToggle ("Base Detail", float) = 0
        [Toggle]_MaskDetailToggle ("Base Detail", float) = 0
        [Toggle]_DissolveDetailToggle ("Disslove Detail", float) = 0
        [Toggle]_DistortionDetailToggle ("Distortion Detail", float) = 0
        [Toggle]_MaskRtoA ("MaskRtoA", float) = 0
        [Toggle]_DissolveRtoA ("DissloveRtoA", float) = 0
        [Toggle]_DistortionRtoA ("DistortionRtoA", float) = 0
        [Toggle]_UClamp ("U Clamp", float) = 0
        [Toggle]_VClamp ("V Clamp", float) = 0
        [Toggle]_UMirror ("U Mirror", float) = 0
        [Toggle]_VMirror ("V Mirror", float) = 0
        [Toggle]_MaskColorToggle("_Color_Mask", float) = 0
        [Toggle]_DissolveColorToggle("_Color_Mask", float) = 0
        [Toggle]_MaskUMirror ("U Clamp", float) = 0
        [Toggle]_MaskVMirror ("V Clamp", float) = 0
        [Toggle]_DissolveUMirror ("U Clamp", float) = 0
        [Toggle]_DissolveVMirror ("V Clamp", float) = 0
        [Toggle]_DistortionUMirror ("U Clamp", float) = 0
        [Toggle]_DistortionVMirror ("V Clamp", float) = 0
        [Toggle]_CustomMainOffset ("Custom Main Offset", float) = 0
        [Toggle]_CustomDissolveValue ("Custom Dissolve Value", float) = 0
        [Toggle]_CustomDistortionStrength ("Custom Distortion Strength", float) = 0
        [Toggle]_CustomMaskOffset ("Custom Mask Offset", float) = 0
        [Toggle]_CustomPolarOffset ("Custom Polar Offset", float) = 0
        [Toggle]_Remap ("Remap", float) = 0
        [Toggle]_VORtoA ("VORtoA", float) = 0
        [Toggle]_VOMaskRtoA ("VOMaskRtoA", float) = 0
        [Toggle]_VoUMirror("VO UMirror", float) = 0
        [Toggle]_VoVMirror("VO VMirror", float) = 0
        [Toggle]_VoMaskUMirror("VO Mask UMirror", float) = 0
        [Toggle]_VoMaskVMirror("VO Mask VMirror", float) = 0
        [Toggle]_VoUClamp("VO Clamp", float) = 0
        [Toggle]_VoVClamp("VO Clamp", float) = 0
        [Toggle]_VoMaskUClamp("VO Clamp", float) = 0
        [Toggle]_VoMaskVClamp("VO Clamp", float) = 0
        [Toggle]_VODetailToggle ("VO Detail", float) = 0
        [Toggle]_VOMaskDetailToggle ("VO Mask Detail", float) = 0
        [Toggle]_FresneltoA("Freshnel to A", float) = 0
        [Toggle]_InvFresnel("Inverts Fresnel", float) = 0

        [MainTexture]_BaseMap ("Base Map", 2D) = "white" {}
        [HideInInspector][HDR]_BaseColor ("Base Color", Color) = (1,1,1,1)
        _Rotation ("Rotation", Range(0,1)) = 0
        _Brightness ("Brightness", Range(1,10)) = 1
        _Saturation ("Saturation", Range(0,1)) = 0
        _GrayScale ("GrayScale", Range(0,1)) = 0
        _Offset ("Offset", Range(0,1)) = 0
        _Dispersion ("Dispersion", Range(256,1)) = 256
        _Contrast ("Contrast", Range(1,32)) = 1
        _HueOffset ("Hue Offset", Range(0,1)) = 0
        _BaseUVX ("U Speed", float) = 0
        _BaseUVY ("V Speed", float) = 0


        _MaskMap ("Mask Map", 2D) = "white" {}
        [HideInInspector][HDR]_MaskColor ("Mask Color", Color) = (1,1,1,1)
        _MaskStrength ("Mask Strength", Range(0,32)) = 1
        _MaskUVX ("U Speed", float) = 0
        _MaskUVY ("V Speed", float) = 0
        _MaskRotation ("Rotation", Range(0,1)) = 0

        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        [HDR]_DissolveColor ("Dissolve Color", Color) = (1,1,1,1)
        _DissolveValue ("Dissolve Value", Range(0,1)) = 0
        _DissolveWidth ("Dissolve Width", Range(0,1)) = 0.1
        _DissolveHardness ("Dissolve Hardness", Range(0,1.0)) = 0.5
        _DissolveUVX ("U Speed", float) = 0
        _DissolveUVY ("V Speed", float) = 0
        _DissolveRotation ("Rotation", Range(0,1)) = 0

        _DistortionMap ("Distortion Map", 2D) = "black" {}
        _DistortionUStrength ("Distortion Strength", Range(0,1)) = 0.1
        _DistortionVStrength ("Distortion Strength", Range(0,1)) = 0.1
        _DistortionUVX ("U Speed", float) = 0
        _DistortionUVY ("V Speed", float) = 0
        _DistortionRotation ("Rotation", Range(0,1)) = 0

        //Fresnel
        [HDR]_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        _FresnelPower ("Fresnel Power", float) = 1
        _FresnelScale ("Fresnel Scale", float) = 1
        _InvFresnelScale ("Inv Fresnel Scale", Range(0,20)) = 1
        //
        _VOTex ("VertexOffset Map", 2D) = "black" {}
        _VOStrength ("VertexOffset Strength", Range(0,32)) = 1
        _VOPower ("VertexOffset Power", Range(0,1)) = 0.6
        _VOUVX ("U Speed", float) = 0
        _VOUVY ("V Speed", float) = 0
        _VORotation ("Rotation", Range(0,1)) = 0
        _VOMaskMap ("VertexOffset Mask Map", 2D) = "white" {}
        _VOMaskRotation ("Rotation", Range(0,1)) = 0

        _PolarParameters ("Polar Parameters", vector) = (0.5,0.5,1,1)
        _PolarUOffset ("Polar U Offset", float) = 0
        _PolarVOffset ("Polar V Offset", float) = 0

        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _AlphaStrength ("Alpha Strength", Range(0,32)) = 1

        [Toggle(_SOFTPARTICLES)]_SoftParticles ("Soft Particles", float) = 0
        _SoftParticle ("Soft Particle Fade", Range(0.01, 10)) = 1

        [Toggle(_COLORPROCESS_ENABLE)]_ColorProcessEnable ("Color Process Enable", float) = 0
        [Toggle(_DISSOLVE_ENABLE)]_DissolveEnable ("Dissolve Enalbe", float) = 0
        [Toggle(_DISTORTION_ENABLE)]_DistortionEnable ("Distortion Enable", float) = 0
        [Toggle(_FRESNEL_ENABLE)]_FresnelEnable ("Fresnel Enable", float) = 0
        [Toggle(_MASK_ENABLE)]_MaskEnable ("Mask Enable", float) = 0
        [Toggle(_VERTEXOFFSET_ENABLE)]_VertexOffsetEnable ("VertexOffset Enable", float) = 0
        [Toggle(_VERTEXMASK_ENABLE)]_VertexMaskEnable ("VertexMask Enable", float) = 0
        [Toggle(_MAIN_POLAR_ENABLE)]_MainPolarEnable ("Polar Enable", float) = 0
        [Toggle(_DISSOLVE_POLAR_ENABLE)]_DissolvePolarEnable ("Dissolve Polar Enable", float) = 0
        _DissolvePolarParameters ("Dissolve Polar Parameters", vector) = (0.5,0.5,1,0)
        _DissolvePolarUOffset ("Dissolve Polar U Offset", float) = 0
        _DissolvePolarVOffset ("Dissolve Polar V Offset", float) = 0
        _DissolveCustomPolarOffset ("Dissolve Custom Polar Offset", float) = 0

        [Toggle(_DISTORTION_POLAR_ENABLE)]_DistortionPolarEnable ("Distortion Polar Enable", float) = 0
        _DistortionPolarParameters ("Distortion Polar Parameters", vector) = (0.5,0.5,1,0)
        _DistortionPolarUOffset ("Distortion Polar U Offset", float) = 0
        _DistortionPolarVOffset ("Distortion Polar V Offset", float) = 0
        _DistortionCustomPolarOffset ("Distortion Custom Polar Offset", float) = 0

        [Toggle(_MASK_POLAR_ENABLE)]_MaskPolarEnable ("Mask Polar Enable", float) = 0
        _MaskPolarParameters ("Mask Polar Parameters", vector) = (0.5,0.5,1,0)
        _MaskPolarUOffset ("Mask Polar U Offset", float) = 0
        _MaskPolarVOffset ("Mask Polar V Offset", float) = 0
        _MaskCustomPolarOffset ("Mask Custom Polar Offset", float) = 0

        //World Mask (vertical world-space fade)
        [Toggle(_WORLDMASK)]_WorldMask ("World Mask", float) = 0
        _WorldVerticalMask ("World Mask Height", float) = -10
        _WorldVerticalMaskIntense ("World Mask Intense", float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"= "Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True"
        }
        LOD 300
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        CBUFFER_START(UnityPerMaterial)
            //shader_feature
            float _FresnelEnable;
            float _MainPolarEnable;
            float _ColorProcessEnable;
            float _DissolveEnable;
            float _DistortionEnable;
            float _MaskEnable;
            float _VertexOffsetEnable;
            float _VertexMaskEnable;
            //Toggle
            half _BaseDetailToggle;
            half _MaskDetailToggle;
            half _DissolveDetailToggle;
            half _DistortionDetailToggle;
            half _BaseRtoA;
            half _MaskRtoA;
            half _UClamp;
            half _VClamp;
            half _UMirror;
            half _VMirror;
            half _DissolveRtoA;
            half _DistortionRtoA;
            half _Remap;
            half _VORtoA;
            half _VOMaskRtoA;
            half _VoUMirror;
            half _VoVMirror;
            half _VODetailToggle;
            half _VOMaskDetailToggle;
            half _VoMaskUMirror;
            half _VoMaskVMirror;
            half _VoUClamp;
            half _VoVClamp;
            half _VoMaskUClamp;
            half _VoMaskVClamp;
            half _FresneltoA;
            half _InvFresnel;
            half _StencilToggle;
            half _MaskColorToggle;
            half _DissolveColorToggle;
            half _CustomMainOffset;
            half _CustomDissolveValue;
            half _CustomDistortionStrength;
            half _CustomMaskOffset;
            half _CustomPolarOffset;
            //Main
            float4 _BaseMap_ST;
            float _Rotation;
            float _BaseUVX;
            float _BaseUVY;
            half4 _BaseColor;
            half _Brightness;
            half _Saturation;
            half _Dispersion;
            half _Contrast;
            half _GrayScale;
            half _HueOffset;
            half _Offset;
            half _AlphaCutoff;
            half _AlphaStrength;
            float _SoftParticle;

            //Mask
            float _MaskUVX;
            float _MaskUVY;
            half _MaskUMirror;
            half _MaskVMirror;
            half _MaskStrength;
            float _MaskRotation;
            half4 _MaskColor;
            float4 _MaskMap_ST;

            //Dissolve
            float4 _DissolveMap_ST;
            half4 _DissolveColor;
            float _DissolveUVX;
            float _DissolveUVY;
            float _DissolveRotation;
            half _DissolveValue;
            half _DissolveWidth;
            half _DissolveHardness;
            half _DissolveUMirror;
            half _DissolveVMirror;


            //Distortion
            float4 _DistortionMap_ST;
            float _DistortionUVX;
            float _DistortionUVY;
            float _DistortionUStrength;
            float _DistortionVStrength;
            float _DistortionRotation;
            half _DistortionUMirror;
            half _DistortionVMirror;

            //Fresnel
            half4 _FresnelColor;
            half _FresnelPower;
            half _FresnelScale;
            half _InvFresnelScale;
            //VertexOffset
            float4 _VOTex_ST;
            float _VOStrength;
            float _VOPower;
            float _VOUVX;
            float _VOUVY;
            float _VORotation;
            float4 _VOMaskMap_ST;
            float _VOMaskRotation;

            //Polar
            float4 _PolarParameters;
            float _PolarUOffset;
            float _PolarVOffset;

            float4 _DissolvePolarParameters;
            float _DissolvePolarUOffset;
            float _DissolvePolarVOffset;
            half _DissolveCustomPolarOffset;

            float4 _DistortionPolarParameters;
            float _DistortionPolarUOffset;
            float _DistortionPolarVOffset;
            half _DistortionCustomPolarOffset;

            float4 _MaskPolarParameters;
            float _MaskPolarUOffset;
            float _MaskPolarVOffset;
            half _MaskCustomPolarOffset;

            //World Mask
            float _WorldVerticalMask;
            float _WorldVerticalMaskIntense;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_MaskMap);
        SAMPLER(sampler_MaskMap);
        TEXTURE2D(_DissolveMap);
        SAMPLER(sampler_DissolveMap);
        TEXTURE2D(_DistortionMap);
        SAMPLER(sampler_DistortionMap);
        TEXTURE2D(_VOTex);
        SAMPLER(sampler_VOTex);
        TEXTURE2D(_VOMaskMap);
        SAMPLER(sampler_VOMaskMap);

        float2 Rotate(float2 UV, float Rotation)
        {
            Rotation = Rotation * (2.0 * PI);
            //Rotation = Rotation * (PI / 180.0f);
            UV -= float2(0.5, 0.5);
            float s = sin(Rotation);
            float c = cos(Rotation);
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix * 2 - 1;
            UV.xy = mul(UV.xy, rMatrix);
            UV += float2(0.5, 0.5);
            return UV;
        }

        float2 UVTransform(float2 UV, half4 Tex_ST, float2 OffsetSpeed, float Rotation, float toggle, float2 customData)
        {
            //UV = toggle == 0 ? UV * Tex_ST.xy + Tex_ST.zw : UV * Tex_ST.xy + (Tex_ST.zw + customData);
            UV = UV * Tex_ST.xy + Tex_ST.zw;
            UV = toggle == 1 ? UV * 1 + customData : UV;
            // 先旋转基础 UV，再叠加滚动偏移：
            // frac 重置时产生的是最终采样空间轴对齐的整数跳变 ((1,0)/(0,1))，
            // Repeat Wrap Mode 可完整抵消，旋转任意角度都不再闪。
            UV = Rotate(UV, Rotation);
            #if defined(_CLAMPREPEAT_MAIN)
            UV += (frac(_Time.y * OffsetSpeed) - 0.5) * Tex_ST.xy * 2;
            #else
            UV += frac(_Time.y * OffsetSpeed);
            #endif
            return UV;
        }

        half3 Saturation(half3 In, half Saturation)
        {
            float luma = dot(In, half3(0.2126729, 0.7151522, 0.0721750));
            return luma.xxx + Saturation.xxx * (In - luma.xxx);
        }

        half4 CalculateContrast(half contrastValue, half4 colorTarget)
        {
            half t = 0.5 * (1.0 - contrastValue);
            return mul(half4x4(contrastValue, 0, 0, t, 0, contrastValue, 0, t, 0, 0, contrastValue, t, 0, 0, 0, 1),
       colorTarget);
        }

        half OneMinus(half In)
        {
            return 1 - In;
        }

        float Remap(float In, float2 InMinMax, float2 OutMinMax)
        {
            return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }

        // 透视除法 + 把 z 统一到 [0,1]，抹平 D3D(reversed-Z, near=-1) 与 OpenGL(near=0) 差异
        float4 NormalizedScreenPos(float4 screenPos)
        {
            float4 ase_screenPosNorm = screenPos / screenPos.w;
            ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0)
                                                          ? ase_screenPosNorm.z
                                                          : ase_screenPosNorm.z * 0.5 + 0.5;
            return ase_screenPosNorm;
        }

        void UVClamp(half uClamp, half vClamp, half uMirror, half vMirror, inout float2 UV)
        {
            UV.x = uClamp == 1 ? saturate(UV.x) : UV.x;
            UV.y = vClamp == 1 ? saturate(UV.y) : UV.y;
            UV.x = uMirror == 1 ? OneMinus(UV.x) : UV.x;
            UV.y = vMirror == 1 ? OneMinus(UV.y) : UV.y;
        }

        float2 PolarCoordinates(float2 UV, float2 Center, float RadialScale, float LengthScale)
        {
            float2 delta = UV - Center;
            float radius = length(delta) * 2 * RadialScale;
            float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
            return float2(radius, angle);
        }

        half3 Hue(half3 In, float Offset)
        {
            half4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            half4 P = lerp(half4(In.bg, K.wz), half4(In.gb, K.xy), step(In.b, In.g));
            half4 Q = lerp(half4(P.xyw, In.r), half4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float E = 1e-10;
            half3 hsv = half3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);

            float hue = hsv.x + Offset;
            hsv.x = hue < 0? hue + 1: hue > 1? hue - 1: hue;
            half4 K2 = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            half3 P2 = abs(frac(hsv.xxx + K2.xyz) * 6.0 - K2.www);
            half3 Out = hsv.z * lerp(K2.xxx, saturate(P2 - K2.xxx), hsv.y);
            return Out;
        }
        ENDHLSL

        Pass
        {
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
            }

            Tags
            {
                "LightMode"="UniversalForward"
            }
            Blend [_SrcBlend] [_DstBlend]
            BlendOp [_BlendOp]
            Cull [_CullMode]
            ZTest [_ZTest]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma only_renderers gles gles3 glcore d3d11 vulkan
            #pragma shader_feature _CLAMPREPEAT_MAIN
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _SOFTPARTICLES
            #pragma shader_feature _COLORPROCESS_ENABLE
            #pragma shader_feature _DISSOLVE_ENABLE
            #pragma shader_feature _DISTORTION_ENABLE
            #pragma shader_feature _FRESNEL_ENABLE
            #pragma shader_feature _MASK_ENABLE
            #pragma shader_feature _VERTEXOFFSET_ENABLE
            #pragma shader_feature _VERTEXMASK_ENABLE
            #pragma shader_feature _MAIN_POLAR_ENABLE
            #pragma shader_feature _DISSOLVE_POLAR_ENABLE
            #pragma shader_feature _DISTORTION_POLAR_ENABLE
            #pragma shader_feature _MASK_POLAR_ENABLE
            #pragma shader_feature _WORLDMASK
            // GPU Instancing
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
                float4 CustomDataUV1 : TEXCOORD1; //Reserved for Custom Data1 MainTex UV, xy offset, z溶解值, w uv扭曲强度
                float4 CustomDataUV2 : TEXCOORD2; //Reserved for Custom Data2 Mask UV, xy offset, zw PolarUV offset
                float4 color : COLOR;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 CustomDataUV1 : TEXCOORD1;
                float4 CustomDataUV2 : TEXCOORD2;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD4;
                float3 normalWS : NORMAL;
                float3 viewDirWS : TEXCOORD3;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.uv = input.uv;

                float RemapValue = 1;
                float vMaskOffsetValue = _VertexOffsetEnable == 1 ? 1 : 0;
                #if defined(_VERTEXOFFSET_ENABLE)
                float2 vOffsetUV = output.uv.xy;
                vOffsetUV = UVTransform(vOffsetUV, _VOTex_ST, float2(_VOUVX, _VOUVY), _VORotation, 0, 0);
                UVClamp(_VoUClamp, _VoVClamp, _VoUMirror, _VoVMirror, vOffsetUV);
                float4 vOffset = SAMPLE_TEXTURE2D_LOD(_VOTex, sampler_VOTex, vOffsetUV, 0);
                float vOffsetValue = saturate(pow(_VORtoA == 1 ? abs(vOffset.r) : abs(vOffset.a), _VOPower));
                RemapValue *= _Remap == 1 ? Remap(vOffsetValue, float2(0, 1), float2(0.5, -0.5)) : vOffsetValue;
                #endif

                #if defined(_VERTEXMASK_ENABLE)
                float2 vMaskOffsetUV = output.uv.xy;
                vMaskOffsetUV = UVTransform(vMaskOffsetUV, _VOMaskMap_ST, float2(_VOUVX, _VOUVY), _VOMaskRotation, 0,0);
                UVClamp(_VoMaskUClamp, _VoMaskVClamp, _VoMaskUMirror, _VoMaskVMirror, vMaskOffsetUV);
                float4 vMaskOffset = SAMPLE_TEXTURE2D_LOD(_VOMaskMap, sampler_VOMaskMap, vMaskOffsetUV, 0);
                vMaskOffsetValue *= _VOMaskRtoA == 1 ? vMaskOffset.r : vMaskOffset.a;
                #endif

                float Offset = RemapValue * vMaskOffsetValue * _VOStrength;
                float3 offsetDir = Offset * input.normalOS.xyz;
                input.positionOS.xyz += offsetDir;

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.normalWS = TransformObjectToWorldDir(input.normalOS.xyz);
                output.viewDirWS = GetWorldSpaceViewDir(output.positionWS);

                output.color = input.color;
                output.CustomDataUV1 = input.CustomDataUV1;
                output.CustomDataUV2 = input.CustomDataUV2;


                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                //uv
                float2 uv = input.uv.xy;
                uv = UVTransform(uv, _BaseMap_ST, float2(_BaseUVX, _BaseUVY), _Rotation, _CustomMainOffset,
                                 input.CustomDataUV1.xy);
                UVClamp(_UClamp, _VClamp, _UMirror, _VMirror, uv);


                float2 PolarUV = 0;
                #if defined(_MAIN_POLAR_ENABLE)
                PolarUV = PolarCoordinates(input.uv.xy, float2(_PolarParameters.x, _PolarParameters.y),_PolarParameters.z, _PolarParameters.w);
                PolarUV = UVTransform(PolarUV, _BaseMap_ST, float2(_PolarUOffset, _PolarVOffset), 0, _CustomPolarOffset,
                 input.CustomDataUV2.zw);
                #endif

                //Distortion
                float2 DistortionBaseUV = 0;
                #if defined(_DISTORTION_ENABLE)
                float2 DistortionUV = input.uv.xy;
                DistortionUV = UVTransform(DistortionUV, _DistortionMap_ST, float2(_DistortionUVX, _DistortionUVY),_DistortionRotation, 0, 0);
                UVClamp(0, 0, _DistortionUMirror, _DistortionVMirror, DistortionUV);
                #if defined(_DISTORTION_POLAR_ENABLE)
                    float2 DistortionPolarUV = PolarCoordinates(DistortionUV, float2(_DistortionPolarParameters.x, _DistortionPolarParameters.y),_DistortionPolarParameters.z, _DistortionPolarParameters.w);
                    DistortionPolarUV = UVTransform(DistortionPolarUV, _DistortionMap_ST, float2(_DistortionPolarUOffset, _DistortionPolarVOffset), 0, _DistortionCustomPolarOffset,
                     input.CustomDataUV2.zw);
                    half4 Distortion = SAMPLE_TEXTURE2D(_DistortionMap, sampler_DistortionMap, DistortionPolarUV);
                #else
                    half4 Distortion = SAMPLE_TEXTURE2D(_DistortionMap, sampler_DistortionMap, DistortionUV);
                #endif
                
                float2 DistortionOffset = _CustomDistortionStrength == 1
                                                         ? float2(Distortion.x, Distortion.y) * input.CustomDataUV1.w
                                                         : float2(Distortion.x * _DistortionUStrength,Distortion.y * _DistortionVStrength);

                DistortionBaseUV += uv + DistortionOffset;
                
                #if defined(_MAIN_POLAR_ENABLE)
                    PolarUV += DistortionOffset;
                #endif
                #endif

                float2 baseUV = _MainPolarEnable == 1 ? PolarUV : _DistortionEnable ? DistortionBaseUV : uv;

                //Albedo
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
                half3 albedo = texColor.rgb * _BaseColor.rgb;

                //Color Process
                half3 finalColor = 1;
                float baseAlpha = _BaseRtoA ? texColor.r : texColor.a;
                #if defined(_COLORPROCESS_ENABLE)
                //亮度
                albedo *= _Brightness;
                half3 DispersionVar = float3(
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, ( uv + _Offset )).r, albedo.g,
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, ( uv - _Offset )).b);
                DispersionVar = _Offset > 0 ? DispersionVar : albedo;
                //half desaturateDot = dot( DispersionVar.rgb, float3( 0.299, 0.587, 0.114 ));
                //half3 desaturateDot = dot(DispersionVar, half3(.2125, .7154, .0721));
                //half3 saturation = Saturation(DispersionVar.rgb, 0);
                half desaturateDot = 0.2125 * DispersionVar.r + 0.7154 * DispersionVar.g + 0.0721 * DispersionVar.b;
                //去饱和度
                half3 saturationColor = half3(desaturateDot, desaturateDot, desaturateDot);
                half3 desaturateVar = lerp(DispersionVar.rgb, saturationColor, _Saturation);
                //灰度
                half brightness = Luminance(desaturateVar.rgb);
                brightness = floor(brightness * 256 / (256 / _Dispersion)) * (256 / _Dispersion) / 256;
                half3 grayColor = half3(brightness, brightness, brightness);
                //离散
                half r = floor(desaturateVar.r * 256 / (256 / _Dispersion)) * (256 / _Dispersion) / 256;
                half g = floor(desaturateVar.g * 256 / (256 / _Dispersion)) * (256 / _Dispersion) / 256;
                half b = floor(desaturateVar.b * 256 / (256 / _Dispersion)) * (256 / _Dispersion) / 256;
                half3 dispersionColor = half3(r, g, b);
                dispersionColor = _Dispersion < 256 ? dispersionColor : desaturateVar.rgb;
                half3 gray = lerp(dispersionColor, grayColor, _GrayScale);
                //gray = _GrayScale > 0 ? gray : dispersionColor.rgb;
                //half4 ContrastColor = float4(CalculateContrast(_Contrast, float4(gray, 1)).rgb, baseAlpha);
                //对比度
                half3 avgColor = half3(0.5, 0.5, 0.5);
                half3 ContrastColor = lerp(avgColor, gray, _Contrast);
                half3 HueColor = Hue(ContrastColor.rgb, _HueOffset);
                finalColor *= HueColor.rgb;
                #endif

                finalColor = _ColorProcessEnable == 1 ? finalColor : albedo;

                //Mask
                half maskAlpha = 1;
                #if defined(_MASK_ENABLE)
                float2 MaskUV = input.uv.xy;
                MaskUV = UVTransform(MaskUV, _MaskMap_ST, float2(_MaskUVX, _MaskUVY), _MaskRotation, _CustomMaskOffset,input.CustomDataUV2.xy);
                UVClamp(0, 0, _MaskUMirror, _MaskVMirror, MaskUV);
                #if defined(_MASK_POLAR_ENABLE)
                float2 MaskPolarUV = PolarCoordinates(MaskUV, float2(_MaskPolarParameters.x, _MaskPolarParameters.y),_MaskPolarParameters.z, _MaskPolarParameters.w);
                MaskPolarUV = UVTransform(MaskPolarUV, _BaseMap_ST, float2(_MaskPolarUOffset, _MaskPolarVOffset), 0, _MaskCustomPolarOffset,input.CustomDataUV2.zw);
                half4 mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, MaskPolarUV) * _MaskColor;
                #else
                half4 mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, MaskUV) * _MaskColor;
                #endif
                mask = saturate(mask * _MaskStrength);
                maskAlpha = _MaskRtoA == 1 ? mask.r : mask.a;

                finalColor.rgb = _MaskColorToggle == 1 ? finalColor.rgb * mask.rgb : finalColor.rgb;
                #endif


                //Dissolve
                half dissolveAlpha = 1;
                #if defined(_DISSOLVE_ENABLE)
                float2 DissolveUV = input.uv.xy;
                DissolveUV = UVTransform(DissolveUV, _DissolveMap_ST, float2(_DissolveUVX, _DissolveUVY),_DissolveRotation, 0, 0);
                UVClamp(0, 0, _DissolveUMirror, _DissolveVMirror, DissolveUV);
                #if defined(_DISSOLVE_POLAR_ENABLE)
                float2 DissolvePolarUV = PolarCoordinates(DissolveUV, float2(_DissolvePolarParameters.x, _DissolvePolarParameters.y),_DissolvePolarParameters.z, _DissolvePolarParameters.w);
                DissolvePolarUV = UVTransform(DissolvePolarUV, _DissolveMap_ST, float2(_DissolvePolarUOffset, _DissolvePolarVOffset), 0, _DissolveCustomPolarOffset,
                 input.CustomDataUV2.zw);
                half4 Dissolve = saturate(SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, DissolvePolarUV));
                #else
                half4 Dissolve = saturate(SAMPLE_TEXTURE2D(_DissolveMap, sampler_DissolveMap, DissolveUV));
                #endif
                
                half LocalDissolve_Value = _CustomDissolveValue == 1 ? input.CustomDataUV1.z : _DissolveValue;

                dissolveAlpha = _DissolveRtoA == 1 ? Dissolve.r : Dissolve.a;
                LocalDissolve_Value = Remap(LocalDissolve_Value, float2(0, 1), float2(-_DissolveWidth, 1.01));

                half small = smoothstep(LocalDissolve_Value + lerp(0, _DissolveWidth - 0.01, 1 - _DissolveHardness),
                                                                LocalDissolve_Value + _DissolveWidth, dissolveAlpha);
                //finalColor.rgb = lerp(_DissolveColor.rgb, finalColor.rgb, small);
                finalColor.rgb = _DissolveColorToggle == 1 ? finalColor.rgb * _DissolveColor.rgb : finalColor.rgb;
                dissolveAlpha *= small;
                #endif


                //Fresnel
                half alpha_fresnel = 1;
                half invFresnel = 1;
                #if defined(_FRESNEL_ENABLE)
                half NoV = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                half fresnel = pow(abs((1.0 - NoV) * _FresnelScale), _FresnelPower);
                half3 FresnelColor = fresnel * _FresnelColor.rgb;
                finalColor += FresnelColor;
                alpha_fresnel = _FresneltoA == 1 ? saturate(fresnel) : 1.0;
                invFresnel = _InvFresnel == 1 ? saturate(pow(abs(1.0 - fresnel), _InvFresnelScale)) : 1.0;
                #endif

                half alpha = saturate(
                    baseAlpha * maskAlpha * dissolveAlpha * input.color.a * alpha_fresnel * invFresnel *
                    _AlphaStrength);

                #if defined(_SOFTPARTICLES)
                // 软粒子：粒子离场景几何越近，alpha 越趋近 0，柔化硬相交边缘
                float4 screenPos = NormalizedScreenPos(input.screenPos);
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenPos.xy), _ZBufferParams);
                float thisDepth  = LinearEyeDepth(screenPos.z, _ZBufferParams);
                float depthFade  = saturate((sceneDepth - thisDepth) / max(_SoftParticle, 0.0001));
                alpha *= depthFade;
                #endif

                #if defined(_WORLDMASK)
                // 世界遮罩：按粒子世界空间 Y 高度做透明度渐隐（地面特效离地过高自动淡出）
                half worldMask = saturate(pow((input.positionWS.y - _WorldVerticalMask) * _WorldVerticalMaskIntense, 5));
                alpha *= worldMask;
                #endif

                finalColor *= input.color.rgb;

                #if defined(_ALPHAPREMULTIPLY_ON)
                finalColor.rgb *= alpha;
                #endif

                #if defined(_ALPHATEST_ON)
                clip(alpha - _AlphaCutoff);
                #endif

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    CustomEditor"IKASAMADUS.FX_ShaderGUI"
}