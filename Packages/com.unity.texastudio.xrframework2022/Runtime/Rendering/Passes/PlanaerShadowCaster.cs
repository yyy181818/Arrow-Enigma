using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class PlanaerShadowCaster : MonoBehaviour
{
    public Transform receiver;
    private bool usePLANARSHADOW = true;
    public float invokeTime = 0.5f;
    private Renderer _receiverRender;
    private static readonly int World2Ground = Shader.PropertyToID("_World2Ground");
    private static readonly int Ground2World = Shader.PropertyToID("_Ground2World");

    private void Start()
    {
        _receiverRender = GetComponent<Renderer>();
        InvokeRepeating(nameof(SetReceiverMatrix), 0, invokeTime);
    }

    void SetReceiverMatrix()
    {
        if (_receiverRender == null)
        {
            for (int i = 0; i < _receiverRender.materials.Length; i++)
            {
                _receiverRender.materials[i].DisableKeyword("_PLANARSHADOW_ENABLE");
                usePLANARSHADOW = false;
            }
        }

        if (_receiverRender != null && usePLANARSHADOW)
        {
            for (int i = 0; i < _receiverRender.materials.Length; i++)
            {
                _receiverRender.materials[i].SetMatrix(World2Ground, receiver.worldToLocalMatrix);
                _receiverRender.materials[i].SetMatrix(Ground2World, receiver.localToWorldMatrix);
            }
        }
    }
}