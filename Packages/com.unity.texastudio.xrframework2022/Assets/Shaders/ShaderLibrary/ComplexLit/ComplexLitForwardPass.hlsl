#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#if defined(_NORMALMAP) || defined(_CLEARCOAT_NORMAL) || defined(_DETAIL) || defined(_DOUBLESIDED_ON) || defined(_BENT_NORMAL) || defined(_PIXEL_DISPLACEMENT)
#define REQUIRES_TBN_NORMAL
#endif

#if defined(_MATERIAL_FEATURE_ANISOTROPY) && defined(_TANGENTMAP)
#define REQUIRES_TBN_ANISOTROPY
#elif defined(_MATERIAL_FEATURE_ANISOTROPY)
#define REQUIRES_TBN_ANISOTROPY
#endif

#if defined(REQUIRES_TBN_NORMAL) || defined(REQUIRES_TBN_ANISOTROPY) || defined(_PIXEL_DISPLACEMENT)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

#include "../ShaderLibrary/LitLighting.hlsl"
#include "../ShaderLibrary/LitDisplacement.hlsl"

#if !defined(TESSELLATION_ON)
// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    float2 dynamicLightmapUV : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS : TEXCOORD1;
    #endif

    half3 normalWS : TEXCOORD2;
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
    #endif
    float3 viewDirWS : TEXCOORD4;

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
    #else
    half fogFactor : TEXCOORD5;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
    #endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
    #ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
    #endif

    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};
#endif

void InitializeInputData(Varyings input, SurfaceData surfaceData, out VectorsData vectorsData, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
    #endif

    inputData.positionCS = input.positionCS;
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
    inputData.tangentToWorld = tangentToWorld;
    #endif

    inputData.normalWS = input.normalWS;
    #if defined(_NORMALMAP) || defined(_DETAIL) || defined(_DOUBLESIDED_ON)
    inputData.normalWS = TransformTangentToWorld(surfaceData.normalTS, tangentToWorld);
    #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

    half3 coatNormalWS = inputData.normalWS;
    #if defined(_NORMALMAP) && defined(_CLEARCOAT_NORMAL)
    coatNormalWS = NormalizeNormalPerPixel(TransformTangentToWorld(surfaceData.coatNormalTS, tangentToWorld));
    #endif

    half3 bentNormalWS = inputData.normalWS;
    #if defined(_NORMALMAP) && defined(_BENT_NORMAL)
    bentNormalWS = NormalizeNormalPerPixel(TransformTangentToWorld(surfaceData.bentNormalTS, tangentToWorld));
    #endif

    half4 tangentWS = 0.0;
    #if defined(_MATERIAL_FEATURE_ANISOTROPY) && defined(_TANGENTMAP)
    tangentWS = real4(TransformTangentToWorld(surfaceData.tangentTS, tangentToWorld).xyz, input.tangentWS.w);
    #elif defined(_MATERIAL_FEATURE_ANISOTROPY)
    tangentWS = input.tangentWS;
    #endif

    vectorsData = CreateVectorsData(inputData.normalWS, input.normalWS.xyz, coatNormalWS, bentNormalWS, viewDirWS,
                                    tangentWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    #else
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
    #endif

    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
    #else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    #endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////
#if !defined(TESSELLATION_ON)
// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    #ifdef _VERTEX_DISPLACEMENT
    half3 positionRWS = TransformObjectToWorld(input.positionOS.xyz);
    half3 height = ComputePerVertexDisplacement(_HeightMap, sampler_HeightMap, output.uv, 1);
    positionRWS += normalInput.normalWS * height;
    input.positionOS = mul(unity_WorldToObject, half4(positionRWS, 1.0));
    #endif
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    output.tangentWS = tangentWS;
    #endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    #ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
    #else
    output.fogFactor = fogFactor;
    #endif

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.positionCS = vertexInput.positionCS;

    return output;
}
#else
TessellationControlPoint LitPassVertex(Attributes input)
{
    TessellationControlPoint output = (TessellationControlPoint)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // already normalized from normal transform to WS.
    output.positionOS = input.positionOS;
    output.normalOS = input.normalOS;
    output.tangentOS = input.tangentOS;
    output.texcoord = input.texcoord;

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

    return output;
}
#endif

half3 ApplyHue(float3 aColor, float aHue)
{
    float angle = radians(aHue);
    float3 k = float3(0.57735f, 0.57735f, 0.57735f);
    float sinAngle, cosAngle;
    sincos(angle, sinAngle, cosAngle);

    return aColor * cosAngle + cross(k, aColor) * sinAngle + k * dot(k, aColor) * (1 - cosAngle);
}

half3 ACESToneMapping(half3 color, float adapted_lum)
{
    const float A = 2.51f;
    const float B = 0.03f;
    const float C = 2.43f;
    const float D = 0.59f;
    const float E = 0.14f;

    color *= adapted_lum;
    return saturate(color * (A * color + B) / (color * (C * color + D) + E));
}

void LitPassFragment(Varyings input, half faceSign : VFACE, out float4 outColor : SV_Target
                     #if defined(_PIXEL_DISPLACEMENT) && defined(_DEPTHOFFSET_ON)
    , out float outputDepth : SV_Depth
                     #endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #ifdef _PIXEL_DISPLACEMENT
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
    half depthOffset = ApplyPerPixelDisplacement(viewDirTS, input.uv);
    #if defined(_DEPTHOFFSET_ON)
        input.positionWS += depthOffset * (-viewDirWS);
        outputDepth = ComputeNormalizedDeviceCoordinatesWithZ(input.positionWS, GetWorldToHClipMatrix()).z;
    #endif
    #endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    #ifdef _DOUBLESIDED_ON
    ApplyDoubleSidedFlipOrMirror(faceSign, surfaceData.normalTS);
    #endif

    #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
    GeometricAAFiltering(surfaceData.smoothness, surfaceData.clearCoatSmoothness, input.normalWS, _SpecularAAScreenSpaceVariance, _SpecularAAThreshold);
    #endif

    InputData inputData;
    VectorsData vectorsData;
    InitializeInputData(input, surfaceData, vectorsData, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

    #ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif

    half4 color = ComplexLitFragment(inputData, surfaceData, vectorsData);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    
    #if defined(_ENABLE_COLOR_PROCESSING)
    color.rgb *= _Brightness;
    color.rgb = _EnableHue ? ApplyHue(color.rgb, _Hue) : color.rgb;
    half gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
    half3 grayColor = half3(gray, gray, gray);
    color.rgb = lerp(grayColor, color.rgb, _Saturation);
    half3 avgColor = half3(0.5, 0.5, 0.5);
    color.rgb = lerp(avgColor, color.rgb, _Contrast);
    #endif

    #if defined(_ENABLE_TONEMAP)
    color.rgb = ACESToneMapping(color.rgb,1.0);
    #endif
    color.a = OutputAlpha(color.a, _Surface);

    outColor = color;
}
#endif
