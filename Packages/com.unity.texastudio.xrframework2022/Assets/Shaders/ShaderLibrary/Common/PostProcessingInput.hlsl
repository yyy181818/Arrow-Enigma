#ifndef POST_PROCESSING_INPUT_HLSL
#define POST_PROCESSING_INPUT_HLSL

TEXTURE2D_X(_BlitTexture);
float4 _BlitTexture_TexelSize;
// Common inline samplers.
// Separated into its own file for robust including from any other file.
// Helps with sharing samplers between intermediate and/or procedural textures (D3D11 has a active sampler limit of 16).
SamplerState sampler_PointClamp;
SamplerState sampler_LinearClamp;
SamplerState sampler_PointRepeat;
SamplerState sampler_LinearRepeat;

#if SHADER_API_GLES
struct PostProcessingInput
{
    float4 positionOS       : POSITION;
    float2 uv               : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
#else
struct PostProcessingInput
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
#endif
struct PostProcessingOutput
{
    float4 positionCS : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

PostProcessingOutput Vert(PostProcessingInput input)
{
    PostProcessingOutput output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    #if SHADER_API_GLES
    float4 pos = input.positionOS;
    float2 uv  = input.uv;
    #else
    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
    #endif
    output.positionCS = pos;
    output.texcoord = uv;
    return output;
}
#endif
