using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//[ExecuteAlways]
public class DissolveController : MonoBehaviour
{
    public float hitRadius = 1.0f;

    void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector4 hitPoint = new Vector4(currentPosition.x, currentPosition.y, currentPosition.z, Time.time);
        Shader.SetGlobalVector("_HitCenter", hitPoint);
        Shader.SetGlobalFloat("_HitRadius", hitRadius);

    }
}