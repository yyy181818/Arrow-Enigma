using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ScenceChangeZhou : MonoBehaviour
{
    [Label("切换总控制")]
    [Range(0.0f, 1.0f)]
    public float Change = 0f;
    [Label("溶解最小值")]
    public float ChangeMin = -20.0f;
    [Label("溶解最大值")]
    public float ChangeMax = 120.0f;
    [Label("溶解中心")]
    public Transform DissolveCenter;
    
    [Header("噪声")]
    [Label("溶解贴图")]
    public Texture2D NoiseMap;
    [Label("溶解速度")]
    [Range(0.0f, 1.0f)]
    public float NoiseSpeed = 0.1f;
    [Label("溶解Tilling")]
    [Range(0.0f, 1.0f)]
    public float NoiseTiiling = 0.05f;
    
    [Header("溶解波")]
    [Label("溶解宽度")]
    public float Wave0Width = 1.0f;
    [Label("溶解偏移")]
    public float Wave0Offset = 0.0f;
    
    [Header("边缘")]
    [Label("边缘颜色")]
    [ColorUsageAttribute(true, true)] 
    public Color EdgeColor = new Color(0,0,0,0);
    [Label("边缘宽度")]
    public float Wave1Width = 0.2f;
    [Label("边缘偏移")]
    public float Wave1Offset = -1.0f;
    
    [Header("消失压暗")]
    [Label("压暗颜色")]
    [ColorUsageAttribute(true, false)] 
    public Color Wave2Color = new Color(0,0,0,0);
    [Label("压暗宽度")]
    public float Wave2Width = 7.0f;
    [Label("压暗偏移")]
    public float Wave2Offset = -13.0f;
    
    [Header("透明&粒子材质控制")]
    [Label("第一场景材质")]
    public Material[] FirstAlphaMats;
    [Label("第二场景材质")]
    public Material[] SecondAlphaMats;
    [Label("透明度切换")]
    //[Range(0.0f, 1.0f)]
    //public float GlobalAlpha = 1.0f;
    public AnimationCurve GlobalAlpha = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(0.5f, 0),
        new Keyframe(1, 1)
    );
    
    [Header("天空盒控制")]
    public Material[] SkyMats;
    [Label("天空颜色")]
    [ColorUsageAttribute(true, false)] 
    public Color SkyColor = new Color(0,0,0,0);
    [Label("天空盒变色")]
    public AnimationCurve SkyIntensity = new AnimationCurve(
        new Keyframe(0, 1),
        new Keyframe(0.5f, 0),
        new Keyframe(1, 1)
    );
    
    private int _fogColorId;
    private int _particleAdvanceAlphaId;
    
    // // Start is called before the first frame update
    void Start()
    {
        _fogColorId = Shader.PropertyToID("_FogColor");
        _particleAdvanceAlphaId = Shader.PropertyToID("_PartcleAdvanceAlpha");
        
        
        if (DissolveCenter == null)
        {
            CreateDefaultDissolveCenter();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Shader.SetGlobalFloat("_scenceContrl",(SpeedCurve.Evaluate(Change+0.3f)* 0.1f ) );
        Shader.SetGlobalFloat("_scenceContrl",Mathf.Lerp(ChangeMin,ChangeMax,Change));
        //Shader.SetGlobalVector("_DissolveCenter",DissolveCenter.localPosition);
        Vector3 dissolveCenterPos = DissolveCenter != null ? DissolveCenter.position : Vector3.zero;
        Shader.SetGlobalVector("_DissolveCenter", dissolveCenterPos);
        Shader.SetGlobalFloat("_NoiseSpeed",NoiseSpeed);
        Shader.SetGlobalFloat("_Wave0Offset",Wave0Offset);
        Shader.SetGlobalFloat("_Wave0Width",Wave0Width);
        Shader.SetGlobalFloat("_Wave1ffset",Wave1Offset);
        Shader.SetGlobalFloat("_Wave1Width",Wave1Width);
        Shader.SetGlobalFloat("_Wave2ffset",Wave2Offset);
        Shader.SetGlobalFloat("_Wave2Width",Wave2Width);
        //Shader.SetGlobalColor("_Wave2Width",Wave2Color);
        Shader.SetGlobalFloat("_NoiseTilling",NoiseTiiling );
        Shader.SetGlobalColor("_EdgeColor",EdgeColor);
        Shader.SetGlobalTexture("_DissolveNoiseMap",NoiseMap);
        
        //Alpha
        foreach (var mat in FirstAlphaMats)
        {
            if (mat == null) continue;
            if (mat.HasProperty(_particleAdvanceAlphaId))
            {
                mat.SetFloat("_PartcleAdvanceAlpha", GlobalAlpha.Evaluate(Change));
            }
        }
        foreach (var mat in SecondAlphaMats)
        {
            if (mat == null) continue;
            if (mat.HasProperty(_fogColorId))
            {
                Color baseColor = mat.GetColor("_FogColor");
                baseColor.a = GlobalAlpha.Evaluate(Change);
                mat.SetColor("_FogColor", baseColor);
            }

            if (mat.HasProperty(_particleAdvanceAlphaId))
            {
                mat.SetFloat("_PartcleAdvanceAlpha", 1-GlobalAlpha.Evaluate(Change));
                Debug.Log("111");

            }
        }
        
        //sky
        foreach (var mat in SkyMats)
        {
            Color targetColor = Color.Lerp(SkyColor, Color.gray, SkyIntensity.Evaluate(Change));
            mat.SetColor("_Tint",  targetColor);
            
        }
    }
    
    private void CreateDefaultDissolveCenter()
    {
       
        if (DissolveCenter == null)
        {
            
            GameObject tempObject = new GameObject("TempDissolveCenter");
            tempObject.transform.position = Vector3.zero;
            DissolveCenter = tempObject.transform;
        
            #if UNITY_EDITOR
            Debug.LogWarning("DissolveCenter 未设置，使用世界原点 (0,0,0) 作为默认溶解中心");
            #endif
        }
    }

}
