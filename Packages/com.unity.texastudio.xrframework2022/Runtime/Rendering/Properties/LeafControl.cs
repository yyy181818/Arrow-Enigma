using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteAlways]
public class LeafControl : MonoBehaviour
{
    // Start is called before the first frame update
    public float Control = 0.0f;
    [Range(0.0f, 1.0f)]
    public float Saturate = 1.0f;
    //[Range(0.0f, 1.0f)]
    public float GrowthEffect;
    public Vector3 EffectPoint;
    public float TreeGrowth = -2.0f;
    public Material[] mats;
    public Material[] SaturateMats;
    public Material[] Growthmats;
    public Material GtowthTree;
    void Start()
    {
        foreach (var mat in mats)
        {
            mat.SetFloat("_Control", Control);
            mat.SetFloat("_Alpha", Control);
        }
        foreach (var mat in SaturateMats)
        {
            mat.SetFloat("_Saturate", Saturate);
        }
        GtowthTree.SetFloat("_Glow", TreeGrowth);
        
    }

    // Update is called once per frame
    void Update()
    { 
        foreach (var mat in mats)
    {
        mat.SetFloat("_Control", Control);
        mat.SetFloat("_Alpha", Control);
           
    }
        foreach (var mat in SaturateMats)
        {
            mat.SetFloat("_Saturate", Saturate);
        }

        foreach (var mat in Growthmats)
        {
            mat.SetFloat("_GrowEffect",GrowthEffect);
            mat.SetVector("_GrowEffectPoint", EffectPoint);
        }

        GtowthTree.SetFloat("_Glow", TreeGrowth);

    }
}
