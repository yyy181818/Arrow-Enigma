// ShaderLibrary/VAT/VATDecode.hlsl
// Shared VAT (Vertex Animation Texture) decode helpers.
// Used by all VAT passes (Forward / ShadowCaster / DepthOnly / DepthNormals)
// so the decode math stays identical across the pipeline.
#ifndef TA_SHADERLIB_VAT_DECODE
#define TA_SHADERLIB_VAT_DECODE

// ---------------------------------------------------------------------------
// Piece pivot decode (Houdini "rigid body / piece" VAT convention)
//   uvA = mesh.uv3, uvB = mesh.uv4
// ---------------------------------------------------------------------------
float3 VATDecodePiecePivot(float2 uvA, float2 uvB)
{
    return float3(-uvA.x, uvB.x, 1.0 - uvB.y);
}

// ---------------------------------------------------------------------------
// Rotation decode. The rotation texture stores an encoded quaternion; the
// position texture's .w channel stores which component was packed.
// ---------------------------------------------------------------------------
float4 VATDecodeQuat(float4 rotTex, float4 posTex)
{
    float w  = sqrt(max(1.0 - dot(rotTex.xyz, rotTex.xyz), 0.0));
    float mc = floor(posTex.w * 4.0);
    float4 q;
    if      (mc < 0.5) q = float4(rotTex.xyz,  w);
    else if (mc < 1.5) q = float4(w, rotTex.yzx);
    else if (mc < 2.5) q = float4(rotTex.x, -w, rotTex.z, -rotTex.y);
    else                q = float4(rotTex.xy, -w, -rotTex.z);
    return normalize(q);
}

// ---------------------------------------------------------------------------
// Rotate a vector by a quaternion.
// ---------------------------------------------------------------------------
float3 VATRotateVec(float3 v, float4 q)
{
    return v + 2.0 * cross(q.xyz, cross(q.xyz, v) + q.w * v);
}

// ---------------------------------------------------------------------------
// Compute current frame index, next frame index and interpolation factor.
//   _Time.y   : URP built-in time (seconds)
//   _AutoPlay : > 0.5 -> play loop, otherwise use _ManualFrame
// ---------------------------------------------------------------------------
void VATComputeFrame(float autoPlay, float manualFrame, float playSpeed,
                     float houdiniFPS, float numFrames, float interpolate,
                     out float frameF, out float frameN, out float interp)
{
    if (autoPlay > 0.5)
    {
        float t  = houdiniFPS / max(numFrames - 0.01, 1.0) * _Time.y * playSpeed;
        float ft = frac(t) * numFrames;
        frameF  = floor(ft) + 1.0;
        interp  = frac(ft);
    }
    else
    {
        frameF = floor(manualFrame);
        interp = 0.0;
    }
    interp = interpolate > 0.5 ? interp : 0.0;
    frameF = clamp(frameF, 1.0, numFrames);
    frameN = frameF + 1.0;
    if (frameN > numFrames) frameN = 1.0;
    frameN = clamp(frameN, 1.0, numFrames);
}

#endif
