using UnityEngine;

public class RotationMaterialController : MonoBehaviour
{
    [Header("Material Settings")]
    public string materialParameter = "_MyParam";
    [Tooltip("0度时的参数值")]
    public float paramAtZero = 0.3f;
    [Tooltip("触发变化的临界角度（绝对值）")]
    public float thresholdAngle = 60f;
    [Tooltip("达到临界角时的参数值")]
    public float paramAtThreshold = -0.45f;
    [Tooltip("超过临界角时的参数值")]
    public float paramBeyondThreshold = -0.9f;

    public Material _materialInstance;
    
    void Update()
    {
        if (_materialInstance == null) return;
        
        Vector3 localRotation = transform.localEulerAngles;
        float x = NormalizeAngle(localRotation.x);
        float z = NormalizeAngle(localRotation.z);
        
        float maxAbsAngle = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
        
        float param = CalculateMaterialParam(maxAbsAngle);
        _materialInstance.SetFloat(materialParameter, param);
    }
    
    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        return angle > 180f ? angle - 360f : angle;
    }

    float CalculateMaterialParam(float currentAngle)
    {
        if (currentAngle <= thresholdAngle)
        {
            float t = Mathf.Clamp01(currentAngle / thresholdAngle);
            return Mathf.Lerp(paramAtZero, paramAtThreshold, t);
        }
        else
        {
            return paramBeyondThreshold;
        }
    }
    
}