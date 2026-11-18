//Created by Wans Wu
#ifndef LIGHTING_COMMON_INCLUDE
	#define LIGHTING_COMMON_INCLUDE
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
void GetSSAO_float(float2 ScreenUV,out float SSAO)
{
    SSAO = 1.0f;
    #ifndef SHADERGRAPH_PREVIEW
    #if defined(_SCREEN_SPACE_OCCLUSION)
	    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(ScreenUV);
	    SSAO = aoFactor.indirectAmbientOcclusion;
    #endif
    #endif
}
// void ParallaxDecal(Texture2D<> HeightMap,float Height,float Steps,float3 ViewDir,SamplerState HeightMapSamplerState,float2 UV,out float2 ParralaxUV,out float Depth)
// {
	// float3 p = float3(UV, 0);
	// float3 v = normalize( -ViewDir );
	// v.z = abs(v.z);
 //             
	// v.xy *= Height;
	// v /= (v.z);
	// const int binarySearchSteps = Steps;
	// for (int idx = 0; idx < Steps; idx++)
	// {
	// 	v *= 0.5;
	// 	float tex = SAMPLE_TEXTURE2D(HeightMap, HeightMapSamplerState, p.xy).r;
	// 	if (p.z < tex)		p += v;	else	p -= v;
	// }
	// ParralaxUV = p.xy;
	// Depth = p.z;
	// return ParralaxUV;
  
// }
float3 NormalReconstructZ(float2 xy,float scale = 1.0)
{
	xy = xy * 2.0  - 1.0;
	xy *= scale;
	//real z =  sqrt(1.0 - saturate(xy.x * xy.x + xy.y * xy.y));
	float z =  sqrt(1.0 - saturate(dot(xy,xy)));
	
	//z = z * 2 - 1;
	float3 normal =normalize( float3(xy,z));
	return normal;
	
}
inline half Pow2(half x)
{
    return x*x ;
}
// inline half Pow4(half x)
// {
//     return x*x * x*x ;
// }
    
inline half Pow5(half x)
{
    return x*x * x*x *x;
}
inline half Pow6(half x)
{
	return x*x * x*x * x*x;
}
float3 Diffuse_Lambert( float3 DiffuseColor )
{
    return DiffuseColor * (1 / PI);
}

float D_GGX_UE4( float a2, float NoH )
{
    float d = ( NoH * a2 - NoH ) * NoH + 1;	// 2 mad
    return a2 / ( PI*d*d );					// 4 mul, 1 rcp
}
float Vis_Implicit()
{
    return 0.25;
}
float3 F_None( float3 SpecularColor )
    {
        return SpecularColor;
    }
    
    
    float Vis_SmithJointApprox( float a2, float NoV, float NoL )
    {
	    float a = sqrt(a2);
	    float Vis_SmithV = NoL * ( NoV * ( 1 - a ) + a );
	    float Vis_SmithL = NoV * ( NoL * ( 1 - a ) + a );
	    return 0.5 * rcp( Vis_SmithV + Vis_SmithL );
    }
    float3 F_Schlick_UE4( float3 SpecularColor, float VoH )
    {
	    float Fc = Pow5( 1 - VoH );					// 1 sub, 3 mul
	    //return Fc + (1 - Fc) * SpecularColor;		// 1 add, 3 mad
	
	    // Anything less than 2% is physically impossible and is instead considered to be shadowing
	    return saturate( 50.0 * SpecularColor.g ) * Fc + (1 - Fc) * SpecularColor;
	
    }
    // half DisneyDiffuse(half NdotV, half NdotL, half LdotH, half perceptualRoughness)
    // {
    //     half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
    //     // Two schlick fresnel term
    //     half lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    //     half viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));
    //
    //     return lightScatter * viewScatter;
    // }
    inline float GGXTerm_Unity (float NdotH, float Alpha)
    {
        float a2 = Alpha * Alpha;
        float d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
        return INV_PI * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
                                                // therefore epsilon is smaller than what can be represented by half
    }
    half3 EnvBRDFApprox( half3 SpecularColor, half Roughness, half NoV )
    {
	    // [ Lazarov 2013, "Getting More Physical in Call of Duty: Black Ops II" ]
	    // Adaptation to fit our G term.
	    const half4 c0 = { -1, -0.0275, -0.572, 0.022 };
	    const half4 c1 = { 1, 0.0425, 1.04, -0.04 };
	    half4 r = Roughness * c0 + c1;
	    half a004 = min( r.x * r.x, exp2( -9.28 * NoV ) ) * r.x + r.y;
	    half2 AB = half2( -1.04, 1.04 ) * a004 + r.zw;

	    // Anything less than 2% is physically impossible and is instead considered to be shadowing
	    // Note: this is needed for the 'specular' show flag to work, since it uses a SpecularColor of 0
	    AB.y *= saturate( 50.0 * SpecularColor.g );

	    return SpecularColor * AB.x + AB.y;
    }

// half3 EnvironmentBRDFSpecular(half3 SpecularColor, half Roughness, half fresnelTerm)
// {
// 	float surfaceReduction = 1.0 / (Roughness + 1.0);
// 	return surfaceReduction * lerp(SpecularColor, brdfData.grazingTerm, fresnelTerm);
// }
//
// half3 EnvironmentBRDF( half3 indirectDiffuse, half3 indirectSpecular,half Roughness, half fresnelTerm)
// {
// 	half3 c = indirectDiffuse ;
// 	c += indirectSpecular * EnvironmentBRDFSpecular(indirectSpecular,Roughness, fresnelTerm);
// 	return c;
// }

    inline half3 RotateDirection(half3 R, half degrees)
    {
	    float3 reflUVW = R;
	    half theta = degrees * PI / 180.0f;
	    half costha = cos(theta);
	    half sintha = sin(theta);
	    reflUVW = half3(reflUVW.x * costha - reflUVW.z * sintha, reflUVW.y, reflUVW.x * sintha + reflUVW.z * costha);
	    return reflUVW;
    }
    float GetSpecularOcclusion(float NoV, float RoughnessSq, float AO)
    {
	    return saturate( pow( saturate(NoV + AO), RoughnessSq ) - 1 + AO );
    }

    float3 AOMultiBounce( float3 BaseColor, float AO )
    {
	    float3 a =  2.0404 * BaseColor - 0.3324;
	    float3 b = -4.7951 * BaseColor + 0.6417;
	    float3 c =  2.7552 * BaseColor + 0.6903;
	    return max( AO, ( ( AO * a + b ) * AO + c ) * AO );
    }
	void GetCurvature_float(float SSSRange,float SSSPower,float3 WorldNormal,float3 WorldPos,out float Curvature)
	{
		Curvature = 1.0;
	
		float deltaWorldNormal = length(fwidth(WorldNormal));
		float deltaWorldPosition = length(fwidth(WorldPos));
		Curvature = saturate(SSSRange + deltaWorldNormal/deltaWorldPosition * SSSPower);
	}


half3 GlossyEnvironmentReflection_revert(half3 reflectVector, half perceptualRoughness, half occlusion)
{
	#if defined(_ENVIRONMENTREFLECTIONS_OFF)
	half3 irradiance;
	half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
	half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));

	irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);

	return irradiance * occlusion;
	#else

	return _GlossyEnvironmentColor.rgb * occlusion;
#endif // _ENVIRONMENTREFLECTIONS_OFF
}
half3 GlossyEnvironmentReflection_custom(half3 reflectVector, half perceptualRoughness, half occlusion,half4 encodedIrradiance ,half4 ReflectionCubeMap_HDR )
{
	#if defined(_CUSTOMREFLECTION)
	//half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
	//half4 encodedIrradiance = SAMPLE_TEXTURECUBE(ReflectionCubeMap, sampler_ReflectionCubeMap, reflectVector);

	 #if defined(UNITY_USE_NATIVE_HDR)
	half3 irradiance = encodedIrradiance.rgb;
	 #else
	 half3 irradiance = DecodeHDREnvironment(encodedIrradiance, ReflectionCubeMap_HDR);
	 #endif

	return irradiance * occlusion;
	#endif // GLOSSY_REFLECTIONS

	return _GlossyEnvironmentColor.rgb * occlusion;
}
float HeightLerp(float Height,float TransitionPhase,float BlendContrast)
{
	float result =clamp(lerp(0 - BlendContrast,BlendContrast + 1, clamp((Height - 1) + (TransitionPhase * 2) ,0 ,1 )),0,1);
	return result;
}

float LinearFog(float Distance,float Start,float End)
{
	return 1 - (clamp((End - Distance) / (End - Start),0,1));
	//return 1 - clamp(exp2(-Distance),0,1);
}
half3 MixFog_custom(float3 positionWS,float3 positionCamera,float FogDistanceStart,float FogDistanceEnd,float FogHeightStart,float FogHeightEnd,float3 vDir,float3 lDir,float SunFogRange, float SunFogIntensity,float FogIntensity,float3 FogColor,float3 SunFogColor, float3 color)
{
	float distanceCam = distance(positionWS,positionCamera);
	float FogLinear = LinearFog(distanceCam,FogDistanceStart,FogDistanceEnd);
	//float FogHeight = 1 - LinearFog(distanceCam,FogHeightStart,FogHeightEnd);
	float3 orgPos =mul( GetObjectToWorldMatrix(), float4( float3( 0,0,0 ), 1 ) ).xyz;
	float FogHeight = 1 - LinearFog(dot(positionWS ,half3(0,1,0)),FogHeightStart,FogHeightEnd);
	float FogSun = clamp(pow((dot(-vDir,lDir) * 0.5 + 0.5 ),SunFogRange),0,1) * SunFogIntensity;
	float finalFog = clamp(FogLinear * FogHeight * FogIntensity,0,1);
	float3 finalColor = lerp(color,lerp(FogColor,SunFogColor,FogSun),finalFog);
	return finalColor;
}
half Fresnel(float3 NormalWS,float3 Vdir,half Bias,half Scale,half Power)
{
	half result = saturate(pow(1 - saturate(dot(NormalWS,Vdir)),Power)) * Scale + Bias;
	return result;
}
// float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
// {
// 	original -= center;
// 	float C = cos( angle );
// 	float S = sin( angle );
// 	float t = 1 - C;
// 	float m00 = t * u.x * u.x + C;
// 	float m01 = t * u.x * u.y - S * u.z;
// 	float m02 = t * u.x * u.z + S * u.y;
// 	float m10 = t * u.x * u.y + S * u.z;
// 	float m11 = t * u.y * u.y + C;
// 	float m12 = t * u.y * u.z - S * u.x;
// 	float m20 = t * u.x * u.z - S * u.y;
// 	float m21 = t * u.y * u.z + S * u.x;
// 	float m22 = t * u.z * u.z + C;
// 	float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
// 	return mul( finalMatrix, original ) + center;
// }


float3 RotateVertex(float3 Vertex,float3 Pivot,float3 Axis ,float Angle)
{
	float3 centerlength = Vertex - Pivot;
	float radiansAngle = radians(Angle) * 0.5;
	float3 Lhs = normalize(Axis) * sin(radiansAngle);
	float3 Rhs = centerlength * cos(radiansAngle) + cross(Lhs,centerlength);
	return  centerlength + Pivot + cross(Lhs,Rhs) * 2;
}

half3 SpecularIBL (float3 R ,float3 WorldPos,float Roughness,float3 SpecularColor,float NoV )
{
	//#ifndef SHADERGRAPH_PREVIEW
	float3 SpecularG = GlossyEnvironmentReflection_revert(R,Roughness,1.0f);
	float3 SpecularDFG = EnvBRDFApprox(SpecularColor,Roughness,NoV);
	return  SpecularG * SpecularDFG;
	//#endif
	return  0;
	
}
float3 Projection(float3 A,float3 B)
{
	return dot(A,B) / dot(B,B) * B;
}
float3 Rejection(float3 A,float3 B)
{
	return  A - Projection(A,B);
}
float4 mod( float4 x )
{
	return x - floor( x / 289.0 ) * 289.0;
}
float4 permute( float4 x )
{
	return mod ( ( x * 34.0 + 1.0 ) * x );
}
float4 taylorInvSqrt( float4 r )
{
	return 1.79284291400159 - r * 0.85373472095314;
}
float simple3Dnoise (float3 uv,half scale)
{
		float3 v = uv * scale;
		const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
		float3 i = floor( v + dot( v, C.yyy ) );
		float3 x0 = v - i + dot( i, C.xxx );
		float3 g = step( x0.yzx, x0.xyz );
		float3 l = 1.0 - g;
		float3 i1 = min( g.xyz, l.zxy );
		float3 i2 = max( g.xyz, l.zxy );
		float3 x1 = x0 - i1 + C.xxx;
		float3 x2 = x0 - i2 + C.yyy;
		float3 x3 = x0 - 0.5;
		i = i - floor( i / 289.0 ) * 289.0;
		float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
		float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
		float4 x_ = floor( j / 7.0 );
		float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
		float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
		float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
		float4 h = 1.0 - abs( x ) - abs( y );
		float4 b0 = float4( x.xy, y.xy );
		float4 b1 = float4( x.zw, y.zw );
		float4 s0 = floor( b0 ) * 2.0 + 1.0;
		float4 s1 = floor( b1 ) * 2.0 + 1.0;
		float4 sh = -step( h, 0.0 );
		float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
		float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
		float3 g0 = float3( a0.xy, h.x );
		float3 g1 = float3( a0.zw, h.y );
		float3 g2 = float3( a1.xy, h.z );
		float3 g3 = float3( a1.zw, h.w );
		float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
		g0 *= norm.x;
		g1 *= norm.y;
		g2 *= norm.z;
		g3 *= norm.w;
		float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
		m = m* m;
		m = m* m;
		float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
		return 42.0 * dot( m, px);
	
}
float3 lerp3(float3 input1,float3 input2,float3 input3,float inputA)
{
	float3 lerp1 = lerp(input1,input2,clamp(inputA * 2,0,1));
	float3 lerp2 = lerp(lerp1,input3,clamp(inputA * 2 - 1,0,1));
	return lerp2;
}
float LinearStep(float Min,float Max,float Value)
{
	return clamp((Value - Min) / (Max  - Min),0,1);
}
float remap(float oldMin ,float olaMax,float newMin,float newMax,float t)
{
	return (newMin + (t - oldMin) * (newMax - newMin) / (olaMax - oldMin));
}

#endif