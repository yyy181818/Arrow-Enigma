using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class AlphaControlZhou : MonoBehaviour
{
    [Header("透明&粒子材质控制")]
    public Material[] AlphaMats;
    
    [Label("透明度")]
    [Range(0.0f, 1.0f)]
    public float Alpha = 1f;
    private int _particleAdvanceAlphaId;
    
    // // Start is called before the first frame update
    void Start()
    {
        _particleAdvanceAlphaId = Shader.PropertyToID("_PartcleAdvanceAlpha");
        
    }

    // Update is called once per frame
    void Update()
    {
        //Alpha
        foreach (var mat in AlphaMats)
        {
            if (mat == null) continue;
            if (mat.HasProperty(_particleAdvanceAlphaId))
            {
                mat.SetFloat("_PartcleAdvanceAlpha", 1-Alpha);
            }
        }
        
        
    }
    
}
