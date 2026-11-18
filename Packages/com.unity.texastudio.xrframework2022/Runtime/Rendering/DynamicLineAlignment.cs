using UnityEngine;
//[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class DynamicLineAlignment : MonoBehaviour
{
    [Header("Settings")]
    public float tilingBase = 5f; 
    public float angleThreshold = 45f; 
    public Color cornerColor = Color.black;
    public Color defaultColor = Color.white;
    public Material mat;
    private LineRenderer _lineRenderer;
    private float _totalLength;
    public bool RevertUV;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        //处理拐角颜色to shader
        UpdateCornerColors();
        
    }

    void LateUpdate()
    {
        //永远朝向Y
        transform.rotation = Quaternion.Euler(90, 0, 0);
        //纹理自适应to shader
        float length = CalculateLineLength(_lineRenderer);
        float tiling =  length / tilingBase;
        if (RevertUV)
        {
            mat.SetVector("_TillingOffset", new Vector4(1, tiling * 0.5f, 0, 0));
        }
        else
        { 
            mat.SetVector("_TillingOffset", new Vector4(tiling * 0.5f, 1, 0, 0));
        }
    }
    public void UpdateCornerColors()
    {
        Vector3[] positions = new Vector3[_lineRenderer.positionCount];
        _lineRenderer.GetPositions(positions);
        
        Color[] colors = new Color[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = IsCorner(i, positions) ? cornerColor : defaultColor;
        }
        
        _lineRenderer.colorGradient = CreateGradient(colors);
    }

    bool IsCorner(int index, Vector3[] positions)
    {
        if (index <= 0 || index >= positions.Length - 1) return false;

        Vector3 prevDir = (positions[index] - positions[index - 1]).normalized;
        Vector3 nextDir = (positions[index + 1] - positions[index]).normalized;
        float angle = Vector3.Angle(prevDir, nextDir);
        return angle > angleThreshold;
    }

    Gradient CreateGradient(Color[] colors)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colorKeys[i] = new GradientColorKey(colors[i], i / (float)(colors.Length - 1));
        }
        gradient.colorKeys = colorKeys;
        return gradient;
    }
    
    float CalculateLineLength(LineRenderer line)
    {
        
        if (line.positionCount < 2) return 0;
    
        float total = 0;
        for (int i = 0; i < line.positionCount - 1; i++)
        {
            Vector3 start = line.GetPosition(i);
            Vector3 end = line.GetPosition(i + 1);
            total += Vector3.Distance(start, end);
        }
        return total;
    }
}