// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TA Shaders/Water" {
    Properties {
    	
		[HDR]_BaseColor("Base Color", Color) = (0.51171875,0.8125,1,1)
    	[HDR]_Specular("Specular", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0, 1)) = 0.8
        _Roughness ("Roughness", Range(0, 1)) = 0.888
    	
		[NoScaleOffset]_WaterColorTexture ("WaterColorTexture", 2D) = "white" {}
    	
		_WaterAlphaPower("WaterAlphaPower", Range(0, 1)) = 0.7863253
        

        _Normal_A_Texture ("NormalMap", 2D) = "bump" {}
        _Normal_A_Scale ("Normal_A_Scale", float ) = 5
        _NormalA_Time_U ("NormalA_Time_U", Float ) = 0.1
        _NormalA_Time_V ("NormalA_Time_V", Float ) = 0.1
		

        

//        [NoScaleOffset]_CausticsTexture ("CausticsTexture", 2D) = "white" {}
//        _CausticsScale ("CausticsScale(int)", int ) = 10
//        _CausticsPower ("CausticsPower", Range(0, 1)) = 1
//        _CausticsReflect ("CausticsReflect", Range(0, 1)) = 0

//		[NoScaleOffset]_FoamTexture("FoamTexture", 2D) = "white" {}
//		[NoScaleOffset]_FoamAlphaTexture("FoamAlphaTexture", 2D) = "white" {}
//		_FoamScale("FoamScale(int)", int) = 10
//		_FoamAlpha("FoamAlpha", Range(0, 1)) = 1
//		_FoamAlphaReflect("FoamAlphaReflect", Range(0, 1)) = 0


        //[HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
        	"RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
    	HLSLINCLUDE
    	
    	ENDHLSL
        Pass {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #define UNITY_PASS_FORWARDBASE
            // #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            // #define _GLOSSYENV 1

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
//            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
//            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
//            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            //#pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0

			CBUFFER_START(UnityPerMaterial)
				half _Metallic;
	            half _Roughness;
	            half _WaterAlphaPower;
				//half _Cutoff;
	            half4 _BaseColor;
            half4 _Specular;
	            float4 _WaterColorTexture_ST;
	            float4 _Normal_A_Texture_ST;
	            float _NormalA_Time_U;
				float _NormalA_Time_V;
				half _Normal_A_Scale;
	   //          float4 _CausticsTexture_ST;
	   //          int _CausticsScale;
				// half _CausticsPower;
				// half _CausticsReflect;
	   //          float4 _FoamTexture_ST;
				// int _FoamScale;
				// half _FoamAlpha;
				// half _FoamAlphaReflect;
	   //          float4 _FoamAlphaTexture_ST;
            CBUFFER_END
             TEXTURE2D(_WaterColorTexture);
             SAMPLER(sampler_WaterColorTexture);
			 

             TEXTURE2D(_Normal_A_Texture);
             SAMPLER(sampler_Normal_A_Texture);
            
             // TEXTURE2D(_CausticsTexture);
             // SAMPLER(sampler_CausticsTexture);
            
             // TEXTURE2D(_FoamTexture);
             // SAMPLER(sampler_FoamTexture);
             //
             // TEXTURE2D(_FoamAlphaTexture);
             // SAMPLER(sampler_FoamAlphaTexture);
            
            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
            	UNITY_VERTEX_INPUT_INSTANCE_ID    
            };
            
            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
	            UNITY_VERTEX_INPUT_INSTANCE_ID
	            UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput vert (VertexInput v)
            {
            	
                
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
			    UNITY_TRANSFER_INSTANCE_ID(v, o);
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				//no Scroll tex - uv0
				o.uv0.xy = TRANSFORM_TEX(v.texcoord0.xy, _WaterColorTexture);
               

				//base UV
				float2 UV = TRANSFORM_TEX(v.texcoord1.xy, _Normal_A_Texture);
				o.uv1.xy = UV;
				
				//Normal Map - uv1
				float NATU = fmod(_Time.x * _NormalA_Time_U, 1);
				float NATV = fmod(_Time.x * _NormalA_Time_V, 1);
				o.uv1.xy = (UV + float2(NATU, NATV)) * _Normal_A_Scale;


				//Normal Map - uv2
				o.uv2.xy = (UV - float2(NATU, NATV)) * _Normal_A_Scale;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
            	VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal,v.tangent);
                o.normalDir = normalInput.normalWS.xyz;
                o.tangentDir = normalInput.tangentWS.xyz;
                o.bitangentDir = normalInput.bitangentWS.xyz;
                o.positionWS = vertexInput.positionWS;
                o.positionCS = vertexInput.positionCS;
                return o;
            }
            half3 DiffuseAndSpecularFromMetallic (half3 albedo, half metallic,out half3 specColor, out half oneMinusReflectivity)
            {
				specColor = lerp(kDielectricSpec.rgb, albedo, metallic);
				oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
				return albedo * oneMinusReflectivity;
			}
			half Pow5(half x)
            {
	            return x * (x * x) * (x * x);
            }
            half4 frag(VertexOutput i) : SV_TARGET
            {
            	
            	UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            	
                half3 normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, normalDir);
                half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);

				//Normal AB UV
                half4 _Normal_A_Texture_var = SAMPLE_TEXTURE2D(_Normal_A_Texture,sampler_Normal_A_Texture, i.uv1.xy);
                half4 _Normal_B_Texture_var = SAMPLE_TEXTURE2D(_Normal_A_Texture,sampler_Normal_A_Texture, i.uv2.xy);
            	
				//Light
            	float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
            	Light mainLight = GetMainLight(shadowCoord);
            	
				//NormalMap
                half3 NormalMap = _Normal_A_Texture_var.rgb * _Normal_B_Texture_var.rgb;
                half3 normalLocal = NormalMap;
                half3 normalDirection = normalize(mul( normalLocal, tangentTransform ));
                half3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                half3 lightDirection = normalize(mainLight.direction.xyz);
                half3 lightColor = mainLight.color.rgb;
                half3 halfDirection = normalize(viewDirection+lightDirection);
            	
                float Pi = 3.141592654;
            	
				// Gloss:
                half gloss = _Roughness;
                half perceptualRoughness = 1.0 - _Roughness;
                half roughness = perceptualRoughness * perceptualRoughness;
                //half specPow = exp2( gloss * 10.0 + 1.0 );
            	//Ambient
            	half3 ambient = _GlossyEnvironmentColor.rgb;
            	//GI
            	half3 giDiffuse = SampleSH(normalDir) * ambient;
            	half3 giSpecular = GlossyEnvironmentReflection(viewReflectDirection, viewDirection, perceptualRoughness,1);
            	
				//Specular
                half NdotL = saturate(dot( normalDirection, lightDirection ));
                half LdotH = saturate(dot(lightDirection, halfDirection));
                half3 specularColor = _Specular.rgb;
                half specularMonochrome;
                half4 _MainTex = SAMPLE_TEXTURE2D(_WaterColorTexture,sampler_WaterColorTexture, i.uv0);
                half3 diffuseColor = _MainTex.rgb;
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _Metallic, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                half NdotV = abs(dot( normalDirection, viewDirection ));
                half NdotH = saturate(dot( normalDirection, halfDirection ));
                //half VdotH = saturate(dot( viewDirection, halfDirection ));
                half visTerm = V_SmithJointGGX( NdotL, NdotV, roughness );
                half normTerm = D_GGX(NdotH, roughness);
                half specularPBL = (visTerm*normTerm) * Pi;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                half3 directSpecular = lightColor *specularPBL*F_Schlick(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                half3 indirectSpecular = giSpecular;
                indirectSpecular *= lerp(specularColor, grazingTerm, Pow4(1 - NdotV));
                indirectSpecular *= surfaceReduction;
                half3 specular = (directSpecular + indirectSpecular);

            	
				//Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                half nlPow5 = Pow5(1-NdotL);
                half nvPow5 = Pow5(1-NdotV);
                half3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * lightColor;
                half3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += giDiffuse;
                half3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;


				//Emissive:
                // float2 NormalMapUV = float2(_Normal_B_Texture_var.r, _Normal_B_Texture_var.g) * _CausticsReflect + (1.0 - _CausticsReflect) * (i.uv0*_CausticsScale);
                // float4 _CausticsTexture_var = SAMPLE_TEXTURE2D(_CausticsTexture,sampler_CausticsTexture, NormalMapUV);
                half WaterAlpha = _MainTex.a;
                half WAWP = (WaterAlpha*_WaterAlphaPower);
                // float CTP = ((_CausticsTexture_var.rgb*(1.0 - WAWP)).r*_CausticsPower);
                // float3 emissive = float3(CTP, CTP, CTP);

				// //Foam
				// float2 _FoamTextureMapUV = float2(_Normal_B_Texture_var.r, _Normal_B_Texture_var.g) * _FoamAlphaReflect + (1.0 - _FoamAlphaReflect) * (i.uv0*_FoamScale);
				// float4 _FoamTexture_var = SAMPLE_TEXTURE2D(_FoamTexture,sampler_FoamTexture, _FoamTextureMapUV);
				// float4 _FoamAlphaTexture_var = SAMPLE_TEXTURE2D(_FoamAlphaTexture,sampler_FoamAlphaTexture, i.uv0);
				// float4 FoamAlpha = _FoamTexture_var * (1 - _FoamAlphaTexture_var) * _FoamAlpha ;

				//Final Color:
				//float3 finalColor = lerp( float3((diffuse + specular * _BaseColor + emissive)) , FoamAlpha , 1 - _FoamAlphaTexture_var.r);
            	half3 finalColor =  half3(diffuse + specular * _BaseColor.rgb);
                //float Alpha = CTP + WAWP;
            	half Alpha = WAWP * _BaseColor.a;
                half4 finalRGBA = half4(finalColor,Alpha);

                return finalRGBA;
            }
            ENDHLSL
        }
    }
    //FallBack "Diffuse"
}
