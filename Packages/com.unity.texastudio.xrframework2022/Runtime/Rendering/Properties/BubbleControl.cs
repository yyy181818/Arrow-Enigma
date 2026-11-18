//using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class BubbleControl : MonoBehaviour {
    public float currentFrame=181;
    public Material _fluidMat;
    public Material _bubbleMat;
    //readonly static int DisplayFrame = Shader.PropertyToID( "_DisplayFrame" );
    void Update()
    {
        _fluidMat.SetFloat("_DisplayFrame", currentFrame);
        _bubbleMat.SetFloat("_DisplayFrame", currentFrame);
    }
}
