Shader "TA Shaders/VFX/Player/Star"
{
    Properties
    {
        [Space(10)]
    	[Main(Global, _, off, off)] _Global ("Global", Float) = 1
    	[Sub(Global)]_Alpha("透明度", Range( 0 , 1)) = 1
        [Main(Fresnel, _, off, off)] _Fresnel ("Fresnel", Float) = 1
		[Sub(Fresnel)]_Power("Power",  Range( 0 ,10 )) = 5
		[Sub(Fresnel)]_Bias("Bias",  Range( 0 , 1)) = 0
		[Sub(Fresnel)]_Scale("强度",  Range( 0 , 1)) = 1
    	
		[Space(10)]
        [Main(Star, _, off, off)] _Star1 ("星星", Float) = 1
		[Sub(Star)]_Star("星星图", 2D) = "white" {}
		//[Tex(Star)]_StarNoise("噪声图", 2D) = "white" {}
		[Sub(Star)]_NoiseTiliing("流动Tiliing(XY)", Vector) = (1,1,0,0)
		[Sub(Star)][HDR]_StarColor("颜色", Color) = (0,0,0,0)
		[Sub(Star)]_Intensity("亮度", Float) = 1
		[Sub(Star)]_StarSpeed("流动速度", Range( 0 , 1)) = 0.1
		[Sub(Star)]_StarAlpha("星星透明度", Range( 0 , 2)) = 0
    	
    	[Space(10)]
        [Main(Dissolve, _DISSOLVE,  off)] _Dissolve1 ("Dissolve", Float) = 0
    	[Space(10)]
    	[Tex(Dissolve)]_Noise("噪声图", 2D) = "white" {}
    	[Sub(Dissolve)]_DissolveTiliing("Tiliing(XY)", Vector) = (1,1,0,0)
    	[Sub(Dissolve)]_DissolveAmout("DissolveControl", Float) = 0.89
    	[Sub(Dissolve)]_DissolveOffset("DissolveOffset", Float) = -0.4
    	[Sub(Dissolve)]_DissolveSpread("DissolveSpread", Float) = 0.67
    	[Sub(Dissolve)][HDR]_DissolveColor("DissolveColor", Color) = (1,1,1,1)
    	[Sub(Dissolve)]_DissolveEdgeOffset("DissloveEdgeOffset", Float) = -0.94
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
           Tags{"LightMode" = "UniversalForward"}
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest Less
			
            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _DISSOLVE
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../../ShaderLibrary/LightingCommon.hlsl"
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD1;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float4 positionWS               : TEXCOORD1;
                float4 normalWS                 : TEXCOORD2;
                half4 tangentWS                : TEXCOORD3;
                float3 positionOS               : TEXCOORD4;
                float4 positionCS               : SV_POSITION;
            	 UNITY_VERTEX_OUTPUT_STEREO 
            };
            TEXTURE2D(_Star);
            SAMPLER(sampler_Star);
            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);
           
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
            half _Alpha;
            CBUFFER_END
            //dissolve
            #if defined (_DISSOLVE)
            float _DissolveAmout;
            float _DissolveOffset;
            float _DissolveSpread;
            float _DissolveEdgeOffset;
            half4 _DissolveColor;
            float4 _DissolveTiliing;
            #endif

           Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
            	UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.uv = input.texcoord;
                output.normalWS.xyz = normalInput.normalWS;         
                output.positionWS.xyz = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;
           		output.positionOS = input.positionOS;
                return output;
            }

            float4 LitPassFragment (Varyings input) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv  = input.uv;
                 float3 positionWS = input.positionWS.xyz;
                // float3 postionOS = input.positionOS;
                half3 vDirWS = GetWorldSpaceNormalizeViewDir(positionWS.xyz);
                half3 nDirWS = normalize(input.normalWS.xyz);
            	//half mask = SAMPLE_TEXTURE2D(_Mask,sampler_Mask,uv).r;
            	//star
            	float3 starMap = SAMPLE_TEXTURE2D(_Star,sampler_Star,uv * _Star_ST.xy + _Star_ST.zw).rgb;
            	half2 starNoiseUV = uv * _NoiseTiliing.xy +  _Time.y * _StarSpeed;
            	float noiseMap = SAMPLE_TEXTURE2D(_Star,sampler_Star,starNoiseUV).r;
            	float3 starAlpha = starMap.rgb * noiseMap * _StarAlpha;
            	float3 starColor =  starMap * noiseMap * _StarColor.rgb;

            	//fresnel
            	float fresnel = (pow(1.0 - saturate(dot(nDirWS,vDirWS)), _Power ) * _Scale + _Bias );
            	float fresnelStar = fresnel* starAlpha.g;
            	float finalAlpha = clamp(fresnelStar +pow(fresnel,3)   ,0,1);
            	float3 finalColor = starColor * _Intensity * 10 +pow(fresnel,2) * _StarColor.rgb;

            	//dissolve
            	#if defined (_DISSOLVE)
            	float3 objToWS = mul( GetObjectToWorldMatrix(), float4( float3( 0,0,0 ), 1 ) ).xyz;
            	float dissolveRange = (((1-(positionWS - objToWS).y) - _DissolveAmout ) / _DissolveSpread)- _DissolveOffset;
            	float dissocvleValve = smoothstep(0.5,1,dissolveRange);
				float dissolveEdge = pow(1 - distance(dissolveRange,_DissolveEdgeOffset),3);
            	
            	//float dissolveMap = simple3Dnoise(positionWS  ,100);
            	float dissolveMap = SAMPLE_TEXTURE2D(_Noise,sampler_Noise,uv * _DissolveTiliing.xy +_DissolveTiliing.zw).r;
            	float dissloveEmiss = step(0.95,dissolveEdge -dissolveMap);
            	float dissolveAplha = (dissocvleValve + noiseMap) *  dissocvleValve;
            	float3 dissolveColor = saturate(dissolveEdge+noiseMap)  * _DissolveColor + dissloveEmiss * _DissolveColor;
            	return float4(finalColor +dissolveColor,saturate( dissolveAplha * finalAlpha +dissloveEmiss)* _Alpha);
            	//return float4(dissolveEdge.xxx,1);
            	#else
            	return float4(finalColor,saturate(finalAlpha * _Alpha));
            	#endif
            	
            }   
            ENDHLSL
        }
    }
     FallBack "Hidden/Universal Render Pipeline/FallbackError"
	 CustomEditor "LWGUI.LWGUI"
}