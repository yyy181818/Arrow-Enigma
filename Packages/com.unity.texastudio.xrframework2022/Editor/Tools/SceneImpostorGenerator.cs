using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class SceneImpostorGenerator : EditorWindow
{
    // --- 核心模式选择 ---
    private enum OutputMode
    {
        [InspectorName("单图模式 (旧版策略 - 画质最高)")] Individual_Texture,

        [InspectorName("图集模式 (新版策略 - 性能优先)")] Atlas_Cluster
    }

    private enum CaptureResolution
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }

    private enum AtlasSize
    {
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    // --- 参数设置 ---
    private OutputMode outputMode = OutputMode.Individual_Texture;

    private Transform observationPoint;
    private CaptureResolution captureResolution = CaptureResolution._2048;
    private bool forceFixedOutput = true;

    // 图集模式专用参数
    private AtlasSize maxAtlasSize = AtlasSize._4096;
    private int atlasPadding = 4;
    private bool mergeMeshesInAtlas = true;

    // 单图模式专用参数
    private int singleTexturePadding = 16;

    // 通用参数
    private bool treatRootAsWhole = true;
    private bool onlyActiveObjects = true;
    private bool autoDisableFog = true;
    private bool fixAlphaClip = true;
    private float alphaThreshold = 0.5f;
    private string baseSavePath = "Assets/GeneratedImpostors";

    private const int TempLayer = 31;
    private const string TargetShaderName = "TA Shaders/Impostor/Unlit";

    // 查找 Impostor Shader，找不到时给出明确报错而不是抛 ArgumentException
    private static Shader FindImpostorShader()
    {
        Shader s = Shader.Find(TargetShaderName);
        if (s == null)
            Debug.LogError($"[SceneImpostorGenerator] 找不到 Shader: \"{TargetShaderName}\"，请确认 Assets/Shaders/Impostor 下的 Shader 已正确导入且能编译。");
        return s;
    }

    // --- 内部数据结构 ---
    private class CaptureData
    {
        public GameObject Source;
        public Texture2D FinalTexture;
        public string Name;
        public Vector3 BoundsCenter;
        public float CamDist;
        public float CamFOV;

        public Rect CropRectPixels;
        public int OriginalCaptureSize;
        public bool IsResized;
    }

    [MenuItem("Tools/Impostor 生成器 (融合版)")]
    public static void ShowWindow()
    {
        GetWindow<SceneImpostorGenerator>("Impostor");
    }

    void OnGUI()
    {
        GUILayout.Label("核心模式", EditorStyles.boldLabel);
        outputMode = (OutputMode)EditorGUILayout.EnumPopup("生成模式", outputMode);

        if (outputMode == OutputMode.Atlas_Cluster)
        {
            EditorGUILayout.HelpBox("【图集模式】\n已启用自动优化：生成的图集会自动裁剪掉多余空白，最大化空间利用率。", MessageType.Info);
        }

        GUILayout.Space(10);
        GUILayout.Label("观察点设置", EditorStyles.boldLabel);
        observationPoint = (Transform)EditorGUILayout.ObjectField("中心观察点", observationPoint, typeof(Transform), true);

        GUILayout.Space(5);
        GUILayout.Label("通用设置", EditorStyles.boldLabel);
        captureResolution = (CaptureResolution)EditorGUILayout.EnumPopup("采集分辨率", captureResolution);

        if (outputMode == OutputMode.Individual_Texture)
        {
            forceFixedOutput = EditorGUILayout.Toggle("强制固定输出尺寸", forceFixedOutput);
        }

        treatRootAsWhole = EditorGUILayout.Toggle("将父物体视为整体", treatRootAsWhole);

        if (outputMode == OutputMode.Individual_Texture)
        {
            GUILayout.Label("单图模式参数", EditorStyles.boldLabel);
            singleTexturePadding = EditorGUILayout.IntField("边缘留白(px)", singleTexturePadding);
        }
        else
        {
            GUILayout.Label("图集模式参数", EditorStyles.boldLabel);
            maxAtlasSize = (AtlasSize)EditorGUILayout.EnumPopup("图集最大尺寸", maxAtlasSize);
            atlasPadding = EditorGUILayout.IntField("图集间隙", atlasPadding);
            mergeMeshesInAtlas = EditorGUILayout.Toggle("合并 Mesh", mergeMeshesInAtlas);
            if (!mergeMeshesInAtlas)
            {
                EditorGUILayout.HelpBox("关闭合并后，将生成多个独立 GameObject，共享同一个材质和图集。", MessageType.None);
            }
        }

        GUILayout.Space(5);
        GUILayout.Label("渲染优化", EditorStyles.boldLabel);
        autoDisableFog = EditorGUILayout.Toggle("自动关闭 Fog", autoDisableFog);
        fixAlphaClip = EditorGUILayout.Toggle("Alpha 修正", fixAlphaClip);
        if (fixAlphaClip) alphaThreshold = EditorGUILayout.Slider("Alpha 阈值", alphaThreshold, 0.1f, 0.9f);

        GUILayout.Space(5);
        baseSavePath = EditorGUILayout.TextField("保存路径", baseSavePath);

        GUILayout.Space(15);
        if (GUILayout.Button("开始生成", GUILayout.Height(40)))
        {
            Generate();
        }
    }

    private void Generate()
    {
        if (!observationPoint)
        {
            EditorUtility.DisplayDialog("错误", "请指定中心观察点！", "OK");
            return;
        }

        var selectedRoots = Selection.gameObjects;
        if (selectedRoots.Length == 0) return;

        // 1. 收集
        List<GameObject> targets = new List<GameObject>();
        if (treatRootAsWhole)
        {
            foreach (var r in selectedRoots)
            {
                if (onlyActiveObjects && !r.activeInHierarchy) continue;
                if (r.GetComponentsInChildren<Renderer>().Length > 0) targets.Add(r);
            }
        }
        else
        {
            foreach (var r in selectedRoots)
            {
                foreach (var ren in r.GetComponentsInChildren<Renderer>())
                    if (!targets.Contains(ren.gameObject) && (!onlyActiveObjects || ren.gameObject.activeInHierarchy))
                        targets.Add(ren.gameObject);
            }
        }

        if (targets.Count == 0) return;

        // 2. 目录
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName)) sceneName = "Untitled";
        string rootPath = Path.Combine(baseSavePath, sceneName);
        Directory.CreateDirectory(Path.Combine(rootPath, "Textures"));
        Directory.CreateDirectory(Path.Combine(rootPath, "Materials"));
        Directory.CreateDirectory(Path.Combine(rootPath, "Meshes"));

        GameObject rootParent = GameObject.Find("Impostor_Root");
        if (!rootParent) rootParent = new GameObject("Impostor_Root");

        // 3. 渲染准备
        GameObject tempCamObj = new GameObject("Temp_Cam");
        Camera cam = tempCamObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.cullingMask = 1 << TempLayer;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 5000f;

        int capSize = (int)captureResolution;
        RenderTexture rt = RenderTexture.GetTemporary(capSize, capSize, 32, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8;
        cam.targetTexture = rt;

        Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
        bool originalFog = RenderSettings.fog;
        if (autoDisableFog) RenderSettings.fog = false;

        List<CaptureData> capturedData = new List<CaptureData>();

        try
        {
            // 4. 循环采集
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                EditorUtility.DisplayProgressBar("渲染中", t.name, (float)i / targets.Count);

                Bounds b;
                if (treatRootAsWhole) b = GetRecursiveBounds(t);
                else b = t.GetComponent<Renderer>().bounds;
                if (b.extents == Vector3.zero) continue;

                cam.transform.position = observationPoint.position;
                cam.transform.LookAt(b.center);
                float dist = Vector3.Distance(cam.transform.position, b.center);
                float radius = b.extents.magnitude;
                float angle = 2.0f * Mathf.Atan(radius / dist) * Mathf.Rad2Deg;
                cam.fieldOfView = Mathf.Clamp(angle * 1.05f, 1f, 179f);

                SetLayerRecursively(t, TempLayer, originalLayers);
                cam.Render();
                RestoreLayers(originalLayers);

                RenderTexture.active = rt;
                Texture2D raw = new Texture2D(capSize, capSize, TextureFormat.RGBA32, false);
                raw.ReadPixels(new Rect(0, 0, capSize, capSize), 0, 0);
                raw.Apply();
                RenderTexture.active = null;

                Rect cropRect = CalculateVisualBounds(raw);
                if (cropRect.width <= 0)
                {
                    DestroyImmediate(raw);
                    continue;
                }

                int pad = (outputMode == OutputMode.Individual_Texture) ? singleTexturePadding : 0;
                int x = Mathf.Max(0, (int)cropRect.x - pad);
                int y = Mathf.Max(0, (int)cropRect.y - pad);
                int w = Mathf.Min(capSize - x, (int)cropRect.width + pad * 2);
                int h = Mathf.Min(capSize - y, (int)cropRect.height + pad * 2);

                Color[] pixels = raw.GetPixels(x, y, w, h);
                DestroyImmediate(raw);

                if (fixAlphaClip)
                {
                    for (int p = 0; p < pixels.Length; p++)
                        if (pixels[p].a < alphaThreshold)
                            pixels[p] = Color.clear;
                }

                Texture2D croppedTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                croppedTex.SetPixels(pixels);
                croppedTex.Apply();

                Texture2D finalTex = croppedTex;
                bool isResized = false;

                if (outputMode == OutputMode.Individual_Texture && forceFixedOutput)
                {
                    finalTex = ResizeTexture(croppedTex, capSize, capSize);
                    DestroyImmediate(croppedTex);
                    isResized = true;
                }

                capturedData.Add(new CaptureData
                {
                    Source = t,
                    FinalTexture = finalTex,
                    Name = t.name,
                    BoundsCenter = b.center,
                    CamDist = dist,
                    CamFOV = cam.fieldOfView,
                    CropRectPixels = new Rect(x, y, w, h),
                    OriginalCaptureSize = capSize,
                    IsResized = isResized
                });
            }

            if (capturedData.Count == 0) return;
            string uid = System.Guid.NewGuid().ToString().Substring(0, 4);

            // ================== 分支处理 ==================

            if (outputMode == OutputMode.Individual_Texture)
            {
                // [单图模式]
                for (int i = 0; i < capturedData.Count; i++)
                {
                    var d = capturedData[i];
                    EditorUtility.DisplayProgressBar("保存中", d.Name, (float)i / capturedData.Count);

                    string texName = $"{d.Name}_{uid}.png";
                    string texPath = Path.Combine(rootPath, "Textures", texName);
                    File.WriteAllBytes(texPath, d.FinalTexture.EncodeToPNG());
                    AssetDatabase.Refresh();

                    SetupIndividualImporter(texPath);

                    Shader impostorShader = FindImpostorShader();
                    if (impostorShader == null) break;
                    Material mat = new Material(impostorShader);
                    Texture2D savedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    mat.mainTexture = savedTex;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", savedTex);
                    mat.SetFloat("_Surface", 0);
                    mat.SetFloat("_AlphaClip", 1);
                    mat.SetFloat("_Cutoff", 0.5f);
                    AssetDatabase.CreateAsset(mat, Path.Combine(rootPath, "Materials", $"{d.Name}_{uid}.mat"));

                    CreateImpObject(d, null, mat, rootPath, uid, rootParent.transform, cam, observationPoint.position);

                    DestroyImmediate(d.FinalTexture);
                }
            }
            else
            {
                // ================== [图集模式优化版] ==================
                EditorUtility.DisplayProgressBar("打包", "正在优化图集布局...", 1f);

                // 1. 打包
                Texture2D rawAtlas = new Texture2D((int)maxAtlasSize, (int)maxAtlasSize, TextureFormat.RGBA32, false);
                Rect[] rawUvs = rawAtlas.PackTextures(capturedData.Select(x => x.FinalTexture).ToArray(), atlasPadding,
                    (int)maxAtlasSize);

                // 2. 裁剪空白
                Rect visualBounds = CalculateVisualBounds(rawAtlas);
                if (visualBounds.width == 0) visualBounds = new Rect(0, 0, 1, 1);

                int trimX = (int)visualBounds.x;
                int trimY = (int)visualBounds.y;
                int trimW = (int)visualBounds.width;
                int trimH = (int)visualBounds.height;

                if (trimW % 4 != 0) trimW += 4 - (trimW % 4);
                if (trimH % 4 != 0) trimH += 4 - (trimH % 4);
                trimW = Mathf.Min(trimW, rawAtlas.width - trimX);
                trimH = Mathf.Min(trimH, rawAtlas.height - trimY);

                Color[] trimPixels = rawAtlas.GetPixels(trimX, trimY, trimW, trimH);
                Texture2D finalAtlas = new Texture2D(trimW, trimH, TextureFormat.RGBA32, false);
                finalAtlas.SetPixels(trimPixels);
                finalAtlas.Apply();

                // 3. 重算 UV
                Rect[] finalUvs = new Rect[rawUvs.Length];
                for (int i = 0; i < rawUvs.Length; i++)
                {
                    Rect old = rawUvs[i];
                    float oldPixelX = old.x * rawAtlas.width;
                    float oldPixelY = old.y * rawAtlas.height;
                    float oldPixelW = old.width * rawAtlas.width;
                    float oldPixelH = old.height * rawAtlas.height;

                    float newPixelX = oldPixelX - trimX;
                    float newPixelY = oldPixelY - trimY;

                    finalUvs[i] = new Rect(
                        newPixelX / trimW,
                        newPixelY / trimH,
                        oldPixelW / trimW,
                        oldPixelH / trimH
                    );
                }

                DestroyImmediate(rawAtlas);

                // 4. 保存资源
                string atlasPath = Path.Combine(rootPath, "Textures", $"{sceneName}_Atlas_{uid}.png");
                File.WriteAllBytes(atlasPath, finalAtlas.EncodeToPNG());
                AssetDatabase.Refresh();

                SetupAtlasImporter(atlasPath, trimW, trimH);

                Shader impostorShader = FindImpostorShader();
                if (impostorShader == null) throw new System.Exception($"Shader \"{TargetShaderName}\" 未找到，已中止生成。");
                Material mat = new Material(impostorShader);
                Texture2D savedAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                mat.mainTexture = savedAtlas;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", savedAtlas);
                mat.SetFloat("_Surface", 0);
                mat.SetFloat("_AlphaClip", 1);
                mat.SetFloat("_Cutoff", 0.5f);
                AssetDatabase.CreateAsset(mat, Path.Combine(rootPath, "Materials", $"{sceneName}_Mat_{uid}.mat"));

                // 5. 生成 Mesh (分支处理)
                if (mergeMeshesInAtlas)
                {
                    // === 合并模式 ===
                    List<CombineInstance> combines = new List<CombineInstance>();
                    for (int i = 0; i < capturedData.Count; i++)
                    {
                        var d = capturedData[i];
                        Mesh quad = CreateQuadMeshWithUV(
                            CalculateWorldSize(d, true),
                            CalculateWorldSize(d, false),
                            finalUvs[i]);

                        // 计算世界位置
                        Vector3 worldPos;
                        Quaternion worldRot;
                        CalculateWorldTransform(d, cam, observationPoint.position, out worldPos, out worldRot);

                        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, worldRot, Vector3.one);
                        combines.Add(new CombineInstance { mesh = quad, transform = matrix });
                    }

                    Mesh clusterMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
                    clusterMesh.CombineMeshes(combines.ToArray(), true, true);
                    AssetDatabase.CreateAsset(clusterMesh,
                        Path.Combine(rootPath, "Meshes", $"{sceneName}_Cluster_{uid}.asset"));

                    GameObject go = new GameObject($"{sceneName}_Cluster");
                    go.transform.SetParent(rootParent.transform);
                    go.AddComponent<MeshFilter>().sharedMesh = clusterMesh;
                    go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                }
                else
                {
                    // === 【修复点】不合并模式：生成独立物体 ===
                    for (int i = 0; i < capturedData.Count; i++)
                    {
                        var d = capturedData[i];

                        // 创建独立 Mesh (带Atlas UV)
                        Mesh m = CreateQuadMeshWithUV(
                            CalculateWorldSize(d, true),
                            CalculateWorldSize(d, false),
                            finalUvs[i]);

                        AssetDatabase.CreateAsset(m, Path.Combine(rootPath, "Meshes", $"{d.Name}_{uid}.asset"));

                        // 计算位置
                        Vector3 worldPos;
                        Quaternion worldRot;
                        CalculateWorldTransform(d, cam, observationPoint.position, out worldPos, out worldRot);

                        GameObject go = new GameObject(d.Name + "_Imp");
                        go.transform.SetParent(rootParent.transform);
                        go.transform.position = worldPos;
                        go.transform.rotation = worldRot;
                        go.AddComponent<MeshFilter>().sharedMesh = m;
                        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    }
                }

                foreach (var d in capturedData) DestroyImmediate(d.FinalTexture);
            }

            foreach (var t in targets)
            {
                Undo.RecordObject(t, "Hide Source");
                t.SetActive(false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (autoDisableFog) RenderSettings.fog = originalFog;
            RenderTexture.ReleaseTemporary(rt);
            DestroyImmediate(tempCamObj);
            EditorUtility.ClearProgressBar();
            RestoreLayers(originalLayers);
            AssetDatabase.Refresh();
        }
    }

    // --- 重构的辅助方法，减少重复代码 ---

    float CalculateWorldSize(CaptureData d, bool isWidth)
    {
        float frustumHeight = 2.0f * d.CamDist * Mathf.Tan(d.CamFOV * 0.5f * Mathf.Deg2Rad);
        if (isWidth)
            return frustumHeight * (d.CropRectPixels.width / (float)d.OriginalCaptureSize); // Aspect=1 for capture
        else
            return frustumHeight * (d.CropRectPixels.height / (float)d.OriginalCaptureSize);
    }

    void CalculateWorldTransform(CaptureData d, Camera cam, Vector3 obsPoint, out Vector3 pos, out Quaternion rot)
    {
        float cx = d.CropRectPixels.x + d.CropRectPixels.width * 0.5f;
        float cy = d.CropRectPixels.y + d.CropRectPixels.height * 0.5f;
        float u = cx / (float)d.OriginalCaptureSize;
        float v = cy / (float)d.OriginalCaptureSize;

        cam.transform.position = obsPoint;
        cam.transform.LookAt(d.BoundsCenter);
        pos = cam.ViewportToWorldPoint(new Vector3(u, v, d.CamDist));
        rot = cam.transform.rotation;
    }

    // 单图模式专用生成器
    void CreateImpObject(CaptureData d, Rect? uv, Material mat, string path, string uid, Transform parent, Camera cam,
        Vector3 obsPoint)
    {
        float w = CalculateWorldSize(d, true);
        float h = CalculateWorldSize(d, false);

        Mesh m;
        if (uv.HasValue) m = CreateQuadMeshWithUV(w, h, uv.Value);
        else m = CreateQuadMesh(w, h);

        AssetDatabase.CreateAsset(m, Path.Combine(path, "Meshes", $"{d.Name}_{uid}.asset"));

        Vector3 pos;
        Quaternion rot;
        CalculateWorldTransform(d, cam, obsPoint, out pos, out rot);

        GameObject go = new GameObject(d.Name + "_Imp");
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = rot;
        go.AddComponent<MeshFilter>().sharedMesh = m;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // --- 基础辅助方法 ---

    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    Mesh CreateQuadMesh(float width, float height)
    {
        Mesh m = new Mesh();
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        m.vertices = new Vector3[]
        {
            new Vector3(-hw, -hh, 0), new Vector3(hw, -hh, 0),
            new Vector3(-hw, hh, 0), new Vector3(hw, hh, 0)
        };
        m.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        m.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    Mesh CreateQuadMeshWithUV(float width, float height, Rect uv)
    {
        Mesh m = new Mesh();
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        m.vertices = new Vector3[]
        {
            new Vector3(-hw, -hh, 0), new Vector3(hw, -hh, 0),
            new Vector3(-hw, hh, 0), new Vector3(hw, hh, 0)
        };
        m.uv = new Vector2[]
        {
            new Vector2(uv.x, uv.y),
            new Vector2(uv.x + uv.width, uv.y),
            new Vector2(uv.x, uv.y + uv.height),
            new Vector2(uv.x + uv.width, uv.y + uv.height)
        };
        m.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    void SetupIndividualImporter(string path)
    {
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp)
        {
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = true;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.anisoLevel = 4;
            imp.filterMode = FilterMode.Bilinear;
            imp.mipMapBias = -0.5f;
            var set = imp.GetDefaultPlatformTextureSettings();
            set.textureCompression = TextureImporterCompression.CompressedHQ;
            set.maxTextureSize = forceFixedOutput ? (int)captureResolution : 4096;
            imp.SetPlatformTextureSettings(set);
            imp.SaveAndReimport();
        }
    }

    void SetupAtlasImporter(string path, int w, int h)
    {
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp)
        {
            imp.alphaIsTransparency = true;
            imp.sRGBTexture = true;
            imp.mipmapEnabled = true;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.filterMode = FilterMode.Bilinear;
            imp.anisoLevel = 4;
            var s = imp.GetDefaultPlatformTextureSettings();
            s.maxTextureSize = 8192;
            s.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.SetPlatformTextureSettings(s);
            imp.SaveAndReimport();
        }
    }

    Rect CalculateVisualBounds(Texture2D tex)
    {
        Color32[] p = tex.GetPixels32();
        int w = tex.width, h = tex.height;
        int minX = w, maxX = 0, minY = h, maxY = 0;
        bool found = false;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (p[y * w + x].a > 10)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    found = true;
                }
            }
        }

        return found ? new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1) : new Rect(0, 0, 0, 0);
    }

    Bounds GetRecursiveBounds(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }

    void SetLayerRecursively(GameObject obj, int layer, Dictionary<GameObject, int> dict)
    {
        if (!dict.ContainsKey(obj)) dict[obj] = obj.layer;
        obj.layer = layer;
        foreach (Transform c in obj.transform) SetLayerRecursively(c.gameObject, layer, dict);
    }

    void RestoreLayers(Dictionary<GameObject, int> dict)
    {
        foreach (var kv in dict)
            if (kv.Key)
                kv.Key.layer = kv.Value;
        dict.Clear();
    }
}