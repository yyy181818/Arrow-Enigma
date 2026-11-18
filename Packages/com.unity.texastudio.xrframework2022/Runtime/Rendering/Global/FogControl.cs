using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteAlways]
public class FogControl : MonoBehaviour
{
    // Start is called before the first frame update
    public float DistanceStart = 0.0f;
    public float DistanceEnd = 700.0f;
    public float HeightStart = 0.0f;
    public float HeightEnd = 0.0f;
    public Color FogColor = new Color(0,0,0,0);
    public Material[] mats;
    void Start()
    {
        foreach (var mat in mats)
        {
            mat.SetFloat("_FogDistanceStart", DistanceStart);
            mat.SetFloat("_FogDistanceEnd", DistanceEnd);
            mat.SetFloat("_FogHeightStart", HeightStart);
            mat.SetFloat("_FogHeightEnd", HeightEnd);
            mat.SetColor("_FogColor",FogColor);
        }
        
    }

    // Update is called once per frame
    // void Update()
    // { foreach (var mat in mats)
    //     {
    //         mat.SetFloat("_FogDistanceStart", DistanceStart);
    //         mat.SetFloat("_FogDistanceEnd", DistanceEnd);
    //         mat.SetFloat("_FogHeightStart", HeightStart);
    //         mat.SetFloat("_FogHeightEnd", HeightEnd);
    //         mat.SetColor("_FogColor",FogColor);
    //     }
    //     
    // }
}
