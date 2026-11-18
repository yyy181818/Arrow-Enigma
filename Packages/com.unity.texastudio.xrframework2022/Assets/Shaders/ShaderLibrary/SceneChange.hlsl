//Created by Wans Wu

void ScenceDisappear(float3 positionWS, float4 center, float scenceContrl, float noise, float noiseOffset,
                     float edgeWidth,half4 edgeColor, out float dissoloveAlpha, out float4 outColor)
{
    float Radius = scenceContrl;
    float circleMask = 1 - saturate(distance(positionWS, center.xyz) / (Radius * 100));
    float noiseCircleMask = circleMask * noise;
    float dissoloveEdge = step((noiseCircleMask - edgeWidth), noiseOffset);
    float dissoloveOffset = step(noiseOffset, noiseCircleMask);
    dissoloveAlpha = dissoloveEdge;
    outColor = dissoloveEdge * dissoloveOffset * edgeColor;
}

void ScenceAppear(float3 positionWS, float4 center, float scenceContrl, float noise, float noiseOffset, float edgeWidth,
                  half4 edgeColor, out float dissoloveAlpha, out float4 outColor)
{
    float Radius = scenceContrl;
    float circleMask = 1 - saturate(distance(positionWS, center) / (Radius * 100));
    float noiseCircleMask = circleMask - noise;
    float dissoloveEdge = step((noiseCircleMask - edgeWidth), noiseOffset);
    float dissoloveOffset = step(noiseOffset, noiseCircleMask);
    dissoloveAlpha = saturate(1 - noiseCircleMask);
    //outColor = dissoloveEdge * dissoloveOffset * edgeColor;
    outColor = dissoloveEdge;
}

void SphereMask_Height(TEXTURE2D_PARAM(Dissolve, sampler_Dissolve), float3 worldPos, float3 center, float noiseTilling,
                       float noiseSpeed, float3 n, float3 v,half waveOffset0,half waveWidth0,half waveOffset1,
                       half waveWidth1,half waveOffset2,half waveWidth2,half4 edgeColor,half4 waveColor2,half control,
                       half baseAlpha,
                       out half3 EdgeColor, out half3 DarkColor, out half Alpha)
{
    #if defined(_SPHERE_MASK_INVERT)
    float2 noiseUV = float2((worldPos.xz * noiseTilling) + _Time.y * noiseSpeed);
    float noise = SAMPLE_TEXTURE2D(Dissolve, sampler_Dissolve, noiseUV).r;
    //Distance
    float DistanceField = distance(center, worldPos) + Fresnel(n, v, 0, 2, 1) - control + 3;
    //WAVE0
    half waveAlpha = 1 - saturate((DistanceField - noise - waveOffset0) / waveWidth0);
    Alpha = saturate(waveAlpha * baseAlpha);
    clip(Alpha - 0.01);
    //WAVE1
    EdgeColor = (1 - saturate((DistanceField - noise + waveOffset1) / (-max(0, waveWidth1)))) * edgeColor;
    DarkColor = 1;
    #else
    //Dissolve
    float2 noiseUV = float2((worldPos.xz * noiseTilling) + _Time.y * noiseSpeed);
    float noise = SAMPLE_TEXTURE2D(Dissolve, sampler_Dissolve, noiseUV).r;

    //Distance
    float DistanceField = -(distance(center, worldPos) + Fresnel(n, v, 0, 2, 1) - control);
    //WAVE0
    half waveAlpha = 1 - saturate((DistanceField - noise - waveOffset0) / waveWidth0);
    Alpha = saturate(waveAlpha * baseAlpha);
    clip(Alpha - 0.01);
    //WAVE2
    float Edge = 1 - saturate((DistanceField - noise + waveOffset1) / -max(0, waveWidth1));

    EdgeColor = Edge * edgeColor.rgb;
    //WAVE3
    DarkColor = (lerp(waveColor2.rgb, 1, 1 - saturate((DistanceField - waveOffset2 - 1) / waveWidth2)) + Edge) *
        half3(1, 1, 1);
    #endif
}

void SphereMask_Height_AlphaClip(TEXTURE2D_PARAM(Dissolve, sampler_Dissolve),
                       float3 worldPos, float3 center, float noiseTilling,
                       float noiseSpeed, float3 n, float3 v,
                       float waveOffset0, float waveWidth0, // 改为 float
                       float waveOffset1, float waveWidth1, // 改为 float
                       float waveOffset2, float waveWidth2, // 改为 float
                       half4 edgeColor, half4 waveColor2, float control,half baseAlpha,
                       out half3 EdgeColor, out half3 DarkColor, out half Alpha)
{
    // 采样噪声 (UV计算可以用 float)
    float2 noiseUV = float2((worldPos.xz * noiseTilling) + _Time.y * noiseSpeed);
    half noise = SAMPLE_TEXTURE2D(Dissolve, sampler_Dissolve, noiseUV).r;

    // 核心距离计算 (必须 float)
    float dist = distance(center, worldPos);

    // Fresnel 这种视觉效果可以用 half 没关系，为了保险先 float
    float fresnelVal = Fresnel(n, v, 0, 2, 1);

    #if defined(_SPHERE_MASK_INVERT)
    // --- 场景 2 (显示) 逻辑 ---
    // 只有在 Sphere 内部才显示，外部 Clip 掉

    // 计算距离场：(dist - control)
    // 如果 dist > control (圈外)，值 > 0
    // 如果 dist < control (圈内)，值 < 0
    float DistanceField = dist + fresnelVal - control + 3.0; // +3 是你的原始偏移

    // WAVE0 (Alpha)
    // 如果 DistanceField 是大的正数 (圈外)，saturate(...) = 1, Alpha = 0 -> Clip
    // 如果 DistanceField 是负数 (圈内)，saturate(...) = 0, Alpha = 1 -> 显示
    float waveVal = (DistanceField - noise - waveOffset0) / max(0.001, waveWidth0);
    half waveAlpha = 1.0 - saturate(waveVal);

    Alpha = saturate(waveAlpha * baseAlpha);
    clip(Alpha - 0.01);

    // WAVE1 (Edge)
    // 边缘发光
    float edgeVal = (DistanceField - noise + waveOffset1) / (-max(0.001, waveWidth1));
    EdgeColor = (1.0 - saturate(edgeVal)) * edgeColor.rgb;
    DarkColor = half3(1, 1, 1);

    #else
    // --- 场景 1 (消失) 逻辑 ---
    // 只有在 Sphere 外部才显示，内部 Clip 掉

    // 原始逻辑： -(dist ... - control)
    // 简化逻辑： control - dist
    // 如果 control > dist (圈内)，DistanceField > 0
    float DistanceField = -(dist + fresnelVal - control);

    // WAVE0
    float waveVal = (DistanceField - noise - waveOffset0) / max(0.001, waveWidth0);
    half waveAlpha = 1.0 - saturate(waveVal);
    Alpha = saturate(waveAlpha * baseAlpha);
    clip(Alpha - 0.01);

    // WAVE2 (Edge)
    float edgeVal = (DistanceField - noise + waveOffset1) / (-max(0.001, waveWidth1));
    half EdgeFactor = 1.0 - saturate(edgeVal);
    EdgeColor = EdgeFactor * edgeColor.rgb;

    // WAVE3 (Darkening)
    float darkVal = (DistanceField - waveOffset2 - 1.0) / max(0.001, waveWidth2);
    half3 darkFactor = lerp(waveColor2.rgb, half3(1, 1, 1), 1.0 - saturate(darkVal));
    DarkColor = darkFactor + EdgeColor; // 叠加边缘
    #endif
}

void ReadTriInfo(float4 uv3, float4 uv2, float thickness, float smoothness, float diameter, out float Wireframe,
                 out float3 TriPos, out float TriSize, out float TriRandom)
{
    float3 fw = abs(ddx(uv3.xyz)) + abs(ddy(uv3.xyz));
    float3 s = fw * (1 + smoothness * 10) + 1e-6;
    float3 l = uv3.xyz - thickness.xxx;
    float3 a1 = smoothstep(0, s, l);
    float w1 = min(min(a1.x, a1.y), a1.z);
    float3 a2 = l / s;
    float w2 = smoothstep(0.0, 1.0, min(a2.x, min(a2.y, a2.z)) + 0.5);
    Wireframe = 1 - saturate(lerp(w2, w1, diameter));
    TriRandom = uv3.w;
    TriPos = uv2.xyz;
    TriSize = uv2.w;
}

float SphereMask(float3 worldPos, float3 center, float radius, float hardness)
{
    float dis = worldPos - center;
    float edge = dis / radius;
    float dt = saturate(dot(edge, edge));
    return pow(dt, hardness);
}
