#ifndef TEXA_SWAY_INCLUDED
#define TEXA_SWAY_INCLUDED

// ============================================================
//  Texa Vertex Sway (Wind) — 模块化摇摆库
//  依赖：必须在 URP Core.hlsl / Lighting.hlsl 之后 include
//        （用到 unity_ObjectToWorld 与内置 _Time）
//
//  用法（在 vertex shader 中，_SWAY 关键字开启时）：
//      float swayWeight = TexaGetSwayWeight(input.positionOS.xyz, input.color, _SwayHeight);
//      float2 swayOffset = TexaGetSwayOffset(input.positionOS.xyz, swayWeight,
//                                            _SwayAmount, _SwaySpeed, _SwayFrequency);
//      vertexInput.positionWS.xz += swayOffset;
//      vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);
// ============================================================

// 计算摆动权重：0 = 不动，1 = 最大摆幅
//   默认高度法：object Y 归一化到 0~1（y=0 根部不动，y=_SwayHeight 顶部满摆）
//   顶点色法：直接用顶点色 Blue 通道（Houdini 烘的权重，0~1）
float TexaGetSwayWeight(float3 positionOS, float4 vertexColor, float modelHeight)
{
    #ifdef _SWAY_VERTEXCOLOR
        return clamp(vertexColor.b, 0.0, 1.0);
    #else
        float h = max(modelHeight, 1e-4);
        return clamp(positionOS.y / h, 0.0, 1.0);
    #endif
}

// 返回要加到 positionWS.xz 的世界空间偏移量（幅度 = 摇摆量 × 权重）
float2 TexaGetSwayOffset(float3 positionOS, float swayWeight, float swayAmount, float swaySpeed, float swayFrequency)
{
    // 用"静止时"的世界坐标做相位基准 → 每棵树因世界位置不同相位错开
    float3 restWS = TransformObjectToWorld(positionOS);
    float windPhase = restWS.x * 0.5 + restWS.z * 0.5;

    // 主摇摆 + 高频细节层（制造叶片颤动感，幅度固定为 0.3×）
    float sway  = sin(_Time.y * swaySpeed + windPhase * swayFrequency) * swayAmount;
          sway += sin(_Time.y * swaySpeed * 2.3 + windPhase * swayFrequency * 1.7) * swayAmount * 0.3;

    return sway * swayWeight;
}

#endif // TEXA_SWAY_INCLUDED
