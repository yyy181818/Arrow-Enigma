using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum TargetMode
{
    SpecificMaterial, // 指定具体的材质球
    RendererParams, // 使用 MaterialPropertyBlock (推荐，性能好，不产生材质实例)
    Global // Shader.SetGlobal...
}

public enum ValueSource
{
    Constant, // 使用固定值
    ChannelA, // 绑定到时间轴通道 A
    ChannelB, // 绑定到时间轴通道 B
    ChannelC, // 绑定到时间轴通道 C
    ChannelD // 绑定到时间轴通道 D
}

[Serializable]
public class ShaderPropItem
{
    public string propertyName = "_Color";
    public ValueSource source = ValueSource.Constant;
    public float constantValue;
    public Color constantColor = Color.white;

    // 你可以扩展更多类型，比如 Vector, Color 等
    public bool isColor;
}

[ExecuteInEditMode]
public class ShaderPropManager : MonoBehaviour
{
    public TargetMode mode = TargetMode.RendererParams;

    [Tooltip("如果是 SpecificMaterial 模式，请手动拖拽；如果是 RendererParams，留空则自动获取自身和子物体")]
    public List<Renderer> targetRenderers = new();

    public Material specificMaterial;

    [Header("Dynamic Properties")] public List<ShaderPropItem> properties = new();
    
    [FormerlySerializedAs("Channel_A")] [Header("Timeline Control Channels")] [Range(0, 1)]
    public float channelA = 0f;

    [FormerlySerializedAs("Channel_B")] [Range(0, 1)] public float channelB = 0f;
    [FormerlySerializedAs("Channel_C")] [Range(0, 1)] public float channelC = 0f;
    [FormerlySerializedAs("Channel_D")] [Range(0, 1)] public float channelD = 0f;

    private MaterialPropertyBlock _propBlock;

    private void OnEnable()
    {
        _propBlock = new MaterialPropertyBlock();
        if (targetRenderers.Count == 0 && mode == TargetMode.RendererParams)
        {
            // 自动获取自身和子物体的 Renderer
            GetComponentsInChildren(true, targetRenderers);
        }
    }
    private void OnDisable()
    {
        // 脚本禁用或销毁时，清理掉 PropertyBlock 的残留，方便在编辑器里单独调材质
        if (mode != TargetMode.RendererParams || targetRenderers == null) return;
        foreach (var r in targetRenderers.Where(r => r))
        {
            r.SetPropertyBlock(null);
        }
    }
    void Update()
    {
        UpdateShaderProperties();
    }

    // 供 Timeline 或者外部调用刷新
    private void UpdateShaderProperties()
    {
        if (properties == null) return;
        if (mode == TargetMode.RendererParams)
        {
            _propBlock ??= new MaterialPropertyBlock();

            foreach (var r in targetRenderers.Where(r => r))
            {
                // 如果确信只有这个脚本控制，直接 Clear 是最安全的做法。
                r.GetPropertyBlock(_propBlock);
                _propBlock.Clear(); 

                foreach (var item in properties)
                {
                    var val = GetCurrentValue(item);
                    var propID = Shader.PropertyToID(item.propertyName);

                    if (item.isColor)
                        _propBlock.SetColor(propID, item.constantColor * val);
                    else
                        _propBlock.SetFloat(propID, val);
                }

                r.SetPropertyBlock(_propBlock);
            }
        }
        else
        {
            foreach (var item in properties)
            {
                var val = GetCurrentValue(item);
                var propID = Shader.PropertyToID(item.propertyName);

                if (mode == TargetMode.Global)
                {
                    if (item.isColor)
                        Shader.SetGlobalColor(propID, item.constantColor * val);
                    else
                        Shader.SetGlobalFloat(propID, val);
                }
                else if (mode == TargetMode.SpecificMaterial && specificMaterial != null)
                {
                    if (item.isColor)
                        specificMaterial.SetColor(propID, item.constantColor * val);
                    else
                        specificMaterial.SetFloat(propID, val);
                }
            }
        }
        
    }
    private float GetCurrentValue(ShaderPropItem item)
    {
        return item.source switch
        {
            ValueSource.ChannelA => channelA,
            ValueSource.ChannelB => channelB,
            ValueSource.ChannelC => channelC,
            ValueSource.ChannelD => channelD,
            _ => item.constantValue
        };
    }
}