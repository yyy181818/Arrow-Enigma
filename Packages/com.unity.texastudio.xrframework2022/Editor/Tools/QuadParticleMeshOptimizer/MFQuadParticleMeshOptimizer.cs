using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public class MFQuadParticleMeshOptimizer : EditorWindow
{
    public Texture2D tex;
    public bool sequenceMode;
    public Vector2Int gridNum = Vector2Int.one;
    public int cuttingMode;
    private static readonly string[] cuttingModeText = { "裁黑边", "裁透明", "容差裁透明" };
    public float tolerance;
    public int previewIndex;
    public bool showOutline;
    public int edgeMode;
    private static readonly string[] edgeModeText = { "灰度", "明度" };
    private Vector2Int wh;
    private Vector2Int singleCellSize;
    private HashSet<Vector2Int> _edgepoint;
    private Mesh _mesh;
    public int blendMode;
    private static readonly string[] blendModeText = { "Alpha", "Add" };
    private GameObject _previewObject;
    private MeshFilter _previewMeshFilter;
    private MeshRenderer _previewMeshRenderer;
    private Material _previewMat;

    private Delaunator delaunator;


    private static string PREVIEW_SHADER_NAME = "Hidden/QuadParticlePreview";
    
    private static string PREVIEW_SHADER_TEX_ST_PROP_NAME = "_MainTex_ST";
    private static readonly string PREVIEW_BLEND_SRC = "_SrcBlend";
    private static readonly string PREVIEW_BLEND_DST = "_DstBlend";

    [MenuItem("Tools/生成特效Mesh工具(采样贴图)")]
    private static void ShowWindow()
    {
        var window = GetWindow(typeof(MFQuadParticleMeshOptimizer), false, "特效纹理生成Mesh (●'◡'●)");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        tex = EditorGUILayout.ObjectField("贴图", tex, typeof(Texture2D), false) as Texture2D;
        sequenceMode = EditorGUILayout.ToggleLeft("序列帧模式", sequenceMode);
        if (sequenceMode)
        {
            EditorGUI.indentLevel++;
            gridNum = EditorGUILayout.Vector2IntField("序列帧行列", gridNum);
            previewIndex = EditorGUILayout.IntSlider("预览序号", previewIndex, 0, gridNum.x * gridNum.y - 1);
            EditorGUI.indentLevel--;
        }
        else
        {
            gridNum = Vector2Int.one;
        }

        GUILayout.Space(10);
        tolerance = EditorGUILayout.Slider("容差", tolerance, 0f, 0.999f);
        GUILayout.Space(10);
        edgeMode = EditorGUILayout.Popup("边缘检测模式", edgeMode, edgeModeText);
        GUILayout.Space(10);
        blendMode = EditorGUILayout.Popup("材质预览透明度混合模式", blendMode, blendModeText);
        GUILayout.Space(10);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("生成"))
            {
                _mesh = Tex2Mesh();
            }

            if (GUILayout.Button("预览"))
            {
                // if (_mesh == null)
                Clear();
                _mesh = Tex2Mesh();
                // if(_mesh!=null)
                CreatePreview(_mesh);
            }

            if (GUILayout.Button("清除"))
            {
                Clear();
            }
        }

        if (GUILayout.Button("切换轮廓显示"))
        {
            showOutline = !showOutline;
            var asm = Assembly.GetAssembly(typeof(Editor));
            var type = asm.GetType("UnityEditor.AnnotationUtility");

            if (type == null)
            {
                return;
            }

            var setSelectionWire =
                type.GetProperty("showSelectionOutline", BindingFlags.Static | BindingFlags.NonPublic);
            if (setSelectionWire == null)
                return;
            setSelectionWire.SetValue(null, showOutline, null);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("同名保存到贴图路径"))
            {
                var path = AssetDatabase.GetAssetPath(tex);
                path = path.Replace(Path.GetExtension(path), ".asset");
                AssetDatabase.CreateAsset(_mesh, path);
                var meshPath = AssetDatabase.GetAssetPath(_mesh);
                var content = File.ReadAllText(meshPath);
                if (Regex.IsMatch(content, "m_IsReadable: 1"))
                {
                    content = content.Replace("m_IsReadable: 1", "m_IsReadable: 0");
                    File.WriteAllText(meshPath, content);
                }
                else
                {
                    Debug.Log(meshPath + " isReadable is already false");
                }

                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("保存到指定路径"))
            {
                if (tex is not null)
                {
                    var path = EditorUtility.SaveFilePanel("保存到", Application.dataPath, tex.name, "asset");
                    AssetDatabase.CreateAsset(_mesh, "Assets" + path[Application.dataPath.Length..]);
                    var meshPath = AssetDatabase.GetAssetPath(_mesh);
                    var content = File.ReadAllText(meshPath);
                    if (Regex.IsMatch(content, "m_IsReadable: 1"))
                    {
                        content = content.Replace("m_IsReadable: 1", "m_IsReadable: 0");
                        File.WriteAllText(meshPath, content);
                    }
                    else
                    {
                        Debug.Log(meshPath + " isReadable is already false");
                    }

                    AssetDatabase.Refresh();
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            if (_previewMat is not null)
            {
                _previewMat.SetVector(PREVIEW_SHADER_TEX_ST_PROP_NAME, GetIndexOffset(previewIndex));
            }
        }
    }

    private Vector4 GetIndexOffset(int index)
    {
        // return new Vector4(1, 1, 0, 0);
        //序号起始左上，xy起始左下，y从1向0排
        var st = new Vector4(1.0f / gridNum.x, 1.0f / gridNum.y, 0, 0);
        var t = index / gridNum.x * gridNum.x;
        st.z = index - t;
        st.w = (index - st.z) / gridNum.x + 1;
        st.z /= gridNum.x;
        st.w = 1.0f - st.w / gridNum.y;
        return st;
    }

    private void OnDestroy()
    {
        Clear();
    }

    private Mesh Tex2Mesh()
    {
        if (tex is null) return null;
        // var originalReadable = tex.isReadable;
        // if (!originalReadable)
        // {
        //     var pathToTex = AssetDatabase.GetAssetPath(tex);
        //     if (AssetImporter.GetAtPath(pathToTex) is TextureImporter importer)
        //     {
        //         importer.isReadable = true;
        //         EditorUtility.SetDirty(importer);
        //         importer.SaveAndReimport();
        //     }
        // }
        var readTex = IsReadTexture(tex);
        wh.x = readTex.width;
        wh.y = readTex.height;
        _edgepoint = new HashSet<Vector2Int>();
        AddPoints(readTex);
        var hullPoints = Andrew_monotone_chain(_edgepoint);
        // if (originalReadable) return MakeMeshByDelaunator(hullPoints);
        // {
        //     var pathToTex = AssetDatabase.GetAssetPath(tex);
        //     if (AssetImporter.GetAtPath(pathToTex) is not TextureImporter importer)
        //         return MakeMeshByDelaunator(hullPoints);
        //     importer.isReadable = false;
        //     EditorUtility.SetDirty(importer);
        //     importer.SaveAndReimport();
        // }
        readTex = null;
        EditorApplication.delayCall += () => { DestroyImmediate(readTex); };
        return MakeMeshByDelaunator(hullPoints);
    }

    private static Texture2D IsReadTexture(Texture texture2D)
    {
        var temp = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);
        Graphics.Blit(texture2D, temp);
        var previous = RenderTexture.active;
        RenderTexture.active = temp;
        var myTexture2D = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, false);
        myTexture2D.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
        myTexture2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);
        return myTexture2D;
    }

    private void CreatePreview(Mesh mesh)
    {
        if (_previewObject is null)
        {
            _previewObject = new GameObject();
            _previewObject.name = "Preview Object";
            _previewMeshFilter = _previewObject.AddComponent<MeshFilter>();
            _previewMeshRenderer = _previewObject.AddComponent<MeshRenderer>();
            if (_previewMat is null)
            {
                var shader = Shader.Find(PREVIEW_SHADER_NAME);
                _previewMat = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            if (blendMode == 0)
            {
                _previewMat.SetFloat(PREVIEW_BLEND_SRC, 5);
                _previewMat.SetFloat(PREVIEW_BLEND_DST, 10);
            }
            else
            {
                _previewMat.SetFloat(PREVIEW_BLEND_SRC, 5);
                _previewMat.SetFloat(PREVIEW_BLEND_DST, 1);
            }

            _previewMeshRenderer.material = _previewMat;
        }

        _previewMeshFilter.sharedMesh = mesh;
        //_previewMat.SetTexture(PREVIEW_SHADER_TEX_PROP_NAME, tex);
        _previewMat.mainTexture = tex;
        Selection.activeGameObject = _previewObject;
    }

    private void Clear()
    {
        DestroyImmediate(_previewObject);
        DestroyImmediate(_previewMat);
        DestroyImmediate(_mesh);
        _previewObject = null;
        _previewMat = null;
        _mesh = null;
    }

    private void AddPoints(Texture2D texture2D)
    {
        singleCellSize = new Vector2Int(texture2D.width / gridNum.x, texture2D.height / gridNum.y);
        for (var i = 0; i < texture2D.width; i++)
        {
            for (var j = 0; j < texture2D.height; j++)
            {
                var pixel = texture2D.GetPixel(i, j);
                var threshold = edgeMode == 0 ? pixel.grayscale : ColorUtils.Luminance(pixel);
                if (!(pixel.a > tolerance) || !(threshold > tolerance)) continue;
                if (sequenceMode)
                {
                    _edgepoint.Add(new Vector2Int(FracByGrid(i, singleCellSize.x),
                        FracByGrid(j, singleCellSize.y)));
                }
                else
                    _edgepoint.Add(new Vector2Int(i, j));
            }
        }
    }

    private static int FracByGrid(int input, int grid)
    {
        return input - input / grid * grid;
    }

    private static double CrossVector(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    private static int CompareVector2Int(Vector2Int a, Vector2Int b)
    {
        if (a.x < b.x) return -1;
        if (a.x != b.x) return 1;
        if (a.y == b.y) return 0;
        if (a.y < b.y) return -1;
        return 1;
    }

    private static Vector2Int[] SortPoint(IEnumerable<Vector2Int> edgePoints)
    {
        return SortPoint(edgePoints.ToArray());
    }

    private static Vector2Int[] SortPoint(Vector2Int[] originalPoints)
    {
        //Vector2Int[] sortedPoint = new Vector2Int[originalPoints.Length];
        //sortedPoint = originalPoints.ToArray();
        //sortedPoint.Sort(CompareVector2Int);
        var sortedPoints = (Vector2Int[])originalPoints.Clone();
        Array.Sort(sortedPoints, CompareVector2Int);
        return sortedPoints;
    }

    private static IPoint[] Andrew_monotone_chain(IEnumerable<Vector2Int> point)
    {
        //https://web.ntnu.edu.tw/~algo/ConvexHull.html
        // 將所有點依照座標大小排序
        var P = SortPoint(point);
        var L = new Vector2Int[P.Length];
        var U = new Vector2Int[P.Length];
        int l = 0, u = 0; // 下凸包的點數、上凸包的點數
        foreach (var t in P)
        {
            while (l >= 2 && CrossVector(L[l - 2], L[l - 1], t) <= 0) l--;
            while (u >= 2 && CrossVector(U[u - 2], U[u - 1], t) >= 0) u--;
            L[l++] = t;
            U[u++] = t;
        }

        //Clean 0,0
        var maxSize = new Vector2Int(-1, -1);
        for (var i = 1; i < P.Length; i++)
        {
            if (L[i].x == 0 && L[i].y == 0 && maxSize.x == -1) maxSize.x = i;
            if (U[i].x == 0 && U[i].y == 0 && maxSize.y == -1) maxSize.y = i;
        }

        var hullPoints = new IPoint[maxSize.x + maxSize.y];
        for (var i = 0; i < hullPoints.Length; i++)
        {
            var src = i < maxSize.x ? L[i] : U[i - maxSize.x];
            hullPoints[i] = new Point(src.x, src.y);
        }

        return hullPoints;
        // outL = new Vector2Int[maxSize.x];
        // outU = new Vector2Int[maxSize.y];
        // Array.ConstrainedCopy(L,0,outL,0,maxSize.x);
        // Array.ConstrainedCopy(U,0,outU,0,maxSize.y);
        // outL = SortPoint(outL);
        // outU = SortPoint(outU);
    }

    // private IPoint[] TransULLine2Points(Vector2Int[] u, Vector2Int[] l)
    // {
    //     IPoint[] points = new IPoint[u.Length + l.Length];
    //     for (int i = 0; i < u.Length; i++)
    //     {
    //         points[i] = new Point(u[i].x, u[i].y);
    //     }
    //
    //     for (int i = u.Length; i < points.Length; i++)
    //     {
    //         points[i] = new Point(l[i-u.Length].x, l[i-u.Length].y);
    //     }
    //     return points;
    // }

    private Mesh MakeMeshByDelaunator(IPoint[] points)
    {
        delaunator = new Delaunator(points);
        var editedUV = delaunator.Points.ToVectors2();
        var normalizedPos = delaunator.Points.ToVectors3();
        var realSize = sequenceMode ? singleCellSize : wh;
        for (var i = 0; i < editedUV.Length; i++)
        {
            editedUV[i].x /= realSize.x;
            editedUV[i].y /= realSize.y;
            normalizedPos[i].x = editedUV[i].x;
            normalizedPos[i].y = editedUV[i].y;
            normalizedPos[i].z = normalizedPos[i].z;
        }

        var mesh = new Mesh
        {
            vertices = normalizedPos,
            triangles = delaunator.Triangles,
            uv = editedUV
        };
        mesh.RecalculateBounds();
        return mesh;
    }
}