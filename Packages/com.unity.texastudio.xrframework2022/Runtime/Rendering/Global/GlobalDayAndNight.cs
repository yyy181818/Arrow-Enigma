using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class GlobalDayAndNight : MonoBehaviour
{
    public bool DayAndNight;
    public Material[] mats;
    public Material[] Alphamats;
    public Material[] FXmats;
    public Material[] DynamicMats;
    public Material Skyboxmat;
    [Range(0.0f, 1.0f)] 
    public float Control = 1.0f;
    
    private float Intensiy = 0.7f;
    private float Pow = 1.5f;
    private float ClampValve = 0.45f;
    private float SkyLight = 0.3f;
    private float RemapValve = 0.8f;
   

    // Update is called once per frame
    void Update()
    {
        Vector4 LightCotrl = new Vector4(Mathf.SmoothStep(Pow, 1f, Control),
            Mathf.SmoothStep(ClampValve, 5.0f, Control), Mathf.SmoothStep(RemapValve,5.0f,Control), Control);
        foreach (var mat in mats)
        {
           if (DayAndNight)
           {
               mat.EnableKeyword("_DAYNIGHT");
               mat.SetFloat("_LightIntensitiy", Mathf.SmoothStep(Intensiy,1.0f,Control));
               mat.SetVector("_LightVector",LightCotrl);
           }
           else
           {
               mat.DisableKeyword("_DAYNIGHT");
           }
        }
        foreach (var mat in FXmats)
        {
            if (DayAndNight)
            {
            mat.SetFloat("_Alpha", 1.0f - Control);
            }
            
        }
        foreach (var mat in Alphamats)
        {
            if (DayAndNight)
            {
                mat.SetFloat("_Alpha",  Control);
            }
            
        }
        
        foreach (var mat in DynamicMats)
        {
            if (DayAndNight)
            {
                mat.SetFloat("_LightIntensitiy",  Mathf.SmoothStep(0.3f,1.0f,Control));
            }
            
        }
        if (DayAndNight)
        {
        Skyboxmat.SetFloat("_Exposure",Mathf.SmoothStep(SkyLight,1.0f,Control));
        }
        
    }
}
