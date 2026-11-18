using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[ExecuteAlways]
public class HolographicController : MonoBehaviour
{
   
    [Header("Controller")]
    [Range(0.0f, 1.0f)]
    public float ChangeAmount;
    public AnimationCurve SpeedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1f, 2.0f);
    [Range(0.0f, 2.0f)] 
    public float HeightFactor = 0.6f;
    [Range(0.0f, 1.0f)]
    public float GlobalAlpha = 1.0f;
    
    [Header("WireFrames")]
    [Range(0.0f, 5.0f)]
    public float RimPower = 1.0f;
    [Range(0.0f, 10.0f)]
    public float RimScale = 5.0f;
    [Range(0.0f, 1.0f)]
    public float WireframeThickness = 1.0f;
    [Range(0.0f, 10.0f)]
    public float WireframeScale = 5.0f;
    [ColorUsage(true,true)]
    public Color FrameColor = Color.white;

    [Header("Wave")] 
    public float WaveAlphaOffset = 0.0f;
    public float WaveAlphaWidth = 5.0f;
    public float WaveOffset1 = 0.44f;
    public float WaveWidth1 = 5.0f;
    public float WaveIntensity1 = 8.0f;
    public float WaveOffset2 = -10.0f;
    public float WaveWidth2 = 15.0f;
    public float WaveIntensity2 = 5.0f;
    
    public Texture2D RampMap;
    public bool toggle;
    
    public Material[] mats;
    
    // void Start()
    // {
    //     InitPosition();
    // }




    // Update is called once per frame
    void Update()
    {
        foreach (var mat in mats)
        {
            mat.SetFloat("_ChangeAmount", (SpeedCurve.Evaluate(ChangeAmount) - 0.03f) * 200f) ;
            mat.SetFloat("_RimPower", RimPower);
            mat.SetFloat("_RimScale", RimScale);
            mat.SetFloat("_WireframeThickness", WireframeThickness);
            mat.SetFloat("_WireframeScale", WireframeScale);
            mat.SetFloat("_HeightFactor", HeightFactor);
            mat.SetFloat("_WaveAlphaOffset", WaveAlphaOffset);
            mat.SetFloat("_WaveAlphaWidth", WaveAlphaWidth);
            mat.SetFloat("_Wave1Offset", WaveOffset1);
            mat.SetFloat("_Wave1Width", WaveWidth1);
            mat.SetFloat("_Wave1Intensity", WaveIntensity1);
            mat.SetFloat("_Wave2Offset", WaveOffset2);
            mat.SetFloat("_Wave2Width", WaveWidth2);
            mat.SetFloat("_Wave2Intensity", WaveIntensity2);
            mat.SetColor("_FrameColor",FrameColor);
            mat.SetVector("_EffectPoint",transform.position);
            mat.SetFloat("_EmissAlpha", GlobalAlpha);
            mat.SetTexture("_Wave1Ramp",RampMap);
           // mat.enabledKeywords(open)
            //mat.SetFloat("_FogEnd", ShaderFogEnd);
            if (toggle)
            {
                    mat.EnableKeyword("_APPEAR");
            }
            else
            {
                mat.DisableKeyword("_APPEAR");
            }
        }
        RenderSettings.fogDensity = Mathf.Lerp(0.08f ,0.03f,SpeedCurve.Evaluate(ChangeAmount));
    }


    // void InitPosition()
    // {
    //
    //     if (GameObject.Find("XRObj"))
    //     {
    //         this.transform.position = GameObject.Find("XRObj").transform.position;
    //     }
    //
    // }
}
