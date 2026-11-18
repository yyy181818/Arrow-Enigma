using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class ScenceChange : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float Change = 0f;
    public AnimationCurve SpeedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1f, 1.0f);
    public Transform DissolveCenter;
    public Texture2D NoiseMap;
    [Range(0.0f, 1.0f)]
    public float NoiseSpeed = 0.1f;
    [Range(0.0f, 1.0f)]
    public float NoiseOffset = 0.1f;
    [Range(0.0f, 10.0f)]
    public float NoiseTiiling = 0.3f;
    [Range(0.0f, 1.0f)]
    public float EdgeWidth = 0.3f;
    [ColorUsageAttribute(true, true)] 
    public Color EdgeColor = new Color(0,0,0,0);
    
    // Start is called before the first frame update
    void Start()
    {
        Shader.SetGlobalFloat("_scenceContrl",(SpeedCurve.Evaluate(Change+ 0.3f ) * 0.1f ) );
        Shader.SetGlobalVector("_DissolveCenter",DissolveCenter.localPosition);
        Shader.SetGlobalFloat("_NoiseSpeed",NoiseSpeed);
        Shader.SetGlobalFloat("_NoiseOffset",NoiseOffset * 0.1f);
        Shader.SetGlobalFloat("_NoiseTilling",NoiseTiiling );
        Shader.SetGlobalFloat("_EdgeWidth",EdgeWidth * 0.01f);
        Shader.SetGlobalColor("_EdgeColor",EdgeColor);
        Shader.SetGlobalTexture("_DissolveNoiseMap",NoiseMap);
        
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalFloat("_scenceContrl",(SpeedCurve.Evaluate(Change+0.3f)* 0.1f ) );
        Shader.SetGlobalVector("_DissolveCenter",DissolveCenter.localPosition);
        Shader.SetGlobalFloat("_NoiseSpeed",NoiseSpeed);
        Shader.SetGlobalFloat("_NoiseOffset",NoiseOffset * 0.1f);
        Shader.SetGlobalFloat("_NoiseTilling",NoiseTiiling );
        Shader.SetGlobalFloat("_EdgeWidth",EdgeWidth * 0.01f);
        Shader.SetGlobalColor("_EdgeColor",EdgeColor);
        Shader.SetGlobalTexture("_DissolveNoiseMap",NoiseMap);
    }
}
