Shader "TA Shaders/VFX/Player/PreDepth"
{
    Properties
    {
        //[Space(10)]
       [Main(Fresnel, _, off, off)] _Fresnel ("Fresnel", Float) = 1
		[Sub(Fresnel)]_Power("Power",  Range( 0 ,10 )) = 5
		[Sub(Fresnel)]_Bias("Bias",  Range( 0 , 1)) = 0
		[Sub(Fresnel)]_Scale("强度",  Range( 0 , 1)) = 1
		//[Space(10)]
//        [Main(Star, _, off, off)] _Star1 ("星星", Float) = 1
//		[Sub(Star)]_Star("星星图", 2D) = "white" {}
//		//[Tex(Star)]_StarNoise("噪声图", 2D) = "white" {}
//		[Sub(Star)]_NoiseTiliing("流动Tiliing(XY)", Vector) = (1,1,0,0)
//		[Sub(Star)][HDR]_StarColor("颜色", Color) = (0,0,0,0)
//		[Sub(Star)]_Intensity("亮度", Float) = 1
//		[Sub(Star)]_StarSpeed("流动速度", Range( 0 , 1)) = 0.1
//		[Sub(Star)]_StarAlpha("透明度", Range( 0 , 2)) = 0
    	//[Sub(Star)]_Mask("Mask", 2D) = "white" {}
    	
		//[Space(10)]
		//[Tex]_Mask("Mask", 2D) = "white" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
           //Tags{"LightMode" = "UniversalForward"}
            Cull Off
             Blend SrcAlpha One, One One
			ZWrite On
			//ZTest Less
			
           HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
           struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1;
                float3 normalWS                 : TEXCOORD2;
                half4 tangentWS                : TEXCOORD3;
                float3 positionOS               : TEXCOORD4;
                float4 positionCS               : SV_POSITION;
            	 UNITY_VERTEX_OUTPUT_STEREO 
            };
            TEXTURE2D(_Star);
            SAMPLER(sampler_Star);
            //TEXTURE2D(_StarNoise);
           // SAMPLER(sampler_StarNoise);
			 TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);
           
           
            CBUFFER_START(UnityPerMaterial)
           half4 _Star_ST;
           half _StarSpeed;
           half4 _NoiseTiliing;
           half _StarAlpha;
           half4 _StarColor;
           half _Power;
           half _Scale;
           half _Bias;
           half _Intensity;
            CBUFFER_END  
            

           Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
            	UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
               UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.uv = input.texcoord;
                output.normalWS = normalInput.normalWS;         
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
                 output.positionOS = input.positionOS;
                return output;
            }

            float4 LitPassFragment (Varyings input) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
               // float2 uv  = input.uv;
                 float3 positionWS = input.positionWS;
                // float3 postionOS = input.positionOS;
                half3 vDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
                half3 nDirWS = normalize(input.normalWS);
            	//half mask = SAMPLE_TEXTURE2D(_Mask,sampler_Mask,uv).r;
            	//star
            	//float3 starMap = SAMPLE_TEXTURE2D(_Star,sampler_Star,uv * _Star_ST.xy + _Star_ST.zw).rgb;
            	//half2 starNoiseUV = uv * _NoiseTiliing.xy +  _Time.y * _StarSpeed;
            	//float noiseMap = SAMPLE_TEXTURE2D(_Star,sampler_Star,starNoiseUV).r;
            	//float3 starAlpha = starMap.rgb * noiseMap * _StarAlpha;
            	//float3 starColor =  starMap * noiseMap * _StarColor.rgb;

            	//fresnel
            	float fresnel = (pow(1.0 - saturate(dot(nDirWS,vDirWS)), _Power ) * _Scale + _Bias ) ;
            	//float fresnelStar = fresnel* starAlpha.g;
            	float finalAlpha = clamp(fresnel   ,0,1);
            	//float3 finalColor = starColor * _Intensity * 10 +pow(fresnel,2) * mask;
            	clip(finalAlpha-0.9);
               return float4(float3(0,0,0),0);
            	//return float4(mask.xxx,1);
            
            }   
            ENDHLSL
        }
//Pass
//        {
//            Name "DepthOnly"
//            Tags{"LightMode" = "DepthOnly"}
//
//            ZWrite On
//            ColorMask 0
//            Cull[_Cull]
//
//            HLSLPROGRAM
//            #pragma exclude_renderers gles gles3 glcore
//            #pragma target 4.5
//
//            #pragma vertex DepthOnlyVertex
//            #pragma fragment DepthOnlyFragment
//
//            // -------------------------------------
//            // Material Keywords
//            #pragma shader_feature_local_fragment _ALPHATEST_ON
//            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//
//            //--------------------------------------
//            // GPU Instancing
//            #pragma multi_compile_instancing
//            #pragma multi_compile _ DOTS_INSTANCING_ON
//
//            #include "../ShaderLibrary/CustomLitInput.hlsl"
//            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
//            ENDHLSL
//        }
    }
     FallBack "Hidden/Universal Render Pipeline/FallbackError"
	 CustomEditor "LWGUI.LWGUI"
}