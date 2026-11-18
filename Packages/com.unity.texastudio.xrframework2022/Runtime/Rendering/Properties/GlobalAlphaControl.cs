using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteAlways]
public class GlobalAlphaControl : MonoBehaviour
{
    public Material[] mats;
    public Material[] mats1;
    public Material[] Revertmats;
    [Range(0.0f, 1.0f)]
    public float GlobalAlpha = 1.0f;
    // Start is called before the first frame update
     void Start()
     {
         foreach (var mat in mats)
         {
             mat.SetFloat("_Alpha", GlobalAlpha);
            
         }
         foreach (var mat in mats1)
         {
             mat.SetFloat("_Alpha", GlobalAlpha);
            
         }
         foreach (var mat in Revertmats)
         {
             mat.SetFloat("_Alpha1", 1.0f-GlobalAlpha);
            
         }
     }

    // Update is called once per frame
    void Update()
    {
        foreach (var mat in mats)
        {
            mat.SetFloat("_Alpha", GlobalAlpha);
            
        }
        foreach (var mat in mats1)
        {
            mat.SetFloat("_Alpha", GlobalAlpha-0.5f);
            
        }
        foreach (var mat in Revertmats)
        {
            mat.SetFloat("_Alpha1", 1.0f-GlobalAlpha);
            
        }
    }
}
