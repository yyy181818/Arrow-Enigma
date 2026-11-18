using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTex : MonoBehaviour
{
    public Material mat;

    public Texture2D SwitchTexture;
    
    public string TexName;
    

    // Update is called once per frame
    void Start()
    {
        mat.SetTexture(TexName,SwitchTexture);
    }
}
