using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteAlways]
public class SetPlayerPosToShader : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform playerPos;
    public Material gtoundMaterial;
    void Start()
    {
        gtoundMaterial.SetVector("_playPos", playerPos.position);
    }

    // Update is called once per frame
    void Update()
    {
        gtoundMaterial.SetVector("_playPos", playerPos.position);
    }
}
