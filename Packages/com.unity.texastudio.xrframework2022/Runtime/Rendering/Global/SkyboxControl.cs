using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
//[ExecuteInEditMode]
public class SkyboxControl : MonoBehaviour
{
    public Material SkyboxMaterial = null;
    [Range(0.0f, 1.0f)]
    public float Intensity = 1.0f;
    // Start is called before the first frame update
    private void Awake()
    {
        if ( SkyboxMaterial == null)
            return;
        
    }

    // Update is called once per frame
    void Update()
    {
       // SkyboxMaterial.SetFloat("_Exposure", Intensity);
        RenderSettings.skybox.SetFloat("_Exposure", Intensity);
    }
}
