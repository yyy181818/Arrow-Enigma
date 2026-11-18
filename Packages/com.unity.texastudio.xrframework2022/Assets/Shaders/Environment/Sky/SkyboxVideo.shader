// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "TA Shaders/Skybox/Video" {
Properties {
    [MainTexture]_MainTex ("Base (RGB)", 2D) = "white" {}
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _BaseColor("Base Color", Color) = (1,1,1,1)
    _AddColor("Add Color", Color) = (0,0,0,0)
    [Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 1.0
    [Toggle(_FIXSEAM)]_FixSeam("Fix Seam",float) = 0
    //_MySampler ("My Sampler", SamplerState) = {} 
}

SubShader {
    Tags { "RenderType"="Opaque" }
   // LOD 100
    Cull[_CullMode]

    Pass {
//        		    Stencil
//				{
//					Ref 1
//					Comp Equal
//				}
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            //#pragma multi_compile_fog
            #pragma shader_feature_local _FIXSEAM

            #include "UnityCG.cginc"
            #define SamplerState_Trilinear SAMPLER(sampler_trilinear)

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
               // UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _AddColor;
            float4 _BaseColor;
            float _Light;
            float _Exposure;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
               // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                #if defined (_FIXSEAM)
                float4 col = tex2D(_MainTex, float2(1-i.texcoord.x-0.001,i.texcoord.y));
                #else
                float4 col = tex2D(_MainTex, float2(1-i.texcoord.x,i .texcoord.y));
                #endif
                
                //UNITY_APPLY_FOG(i.fogCoord, col);
               // UNITY_OPAQUE_ALPHA(col.a);
                return (col * _BaseColor + _AddColor) * _Exposure *(1 - _Light);
            }
        ENDCG
    }
}

}
