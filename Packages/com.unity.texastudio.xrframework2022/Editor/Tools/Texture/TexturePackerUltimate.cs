using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TexturePackerUltimate : EditorWindow
{
    enum PackMode
    {
        MRO,
        NMR
    }

    enum Channel
    {
        R,
        G,
        B,
        A
    }

    [System.Serializable]
    class InputSet
    {
        public Texture2D tex;
        public Channel channel;
    }

    class TexSet
    {
        public Texture2D metallic;
        public Texture2D roughness;
        public Texture2D ao;
        public Texture2D normal;
        public Texture2D basemap;
        public Channel aoChannel = Channel.R;
    }

    PackMode mode;

    InputSet inR = new InputSet();
    InputSet inG = new InputSet();
    InputSet inB = new InputSet();
    InputSet inA = new InputSet();

    // NMR 专用
    InputSet inBasemap = new InputSet();
    InputSet inAO = new InputSet();

    // MRO AO channel
    Channel aoChannel = Channel.R;

    List<Texture2D> inputTextures = new List<Texture2D>();

    Vector2Int size = new Vector2Int(1024, 1024);
    bool invertRoughness = true;

    string fileNameFormat = "{name}_{mode}";
    bool useSourcePath = true;
    string customSavePath = "Assets/Packed";

    [MenuItem("Tools/贴图工具/贴图打包工具")]
    static void Open()
    {
        GetWindow<TexturePackerUltimate>("贴图打包工具");
    }

    void OnGUI()
    {
        mode = (PackMode)EditorGUILayout.EnumPopup("模式", mode);
        size = EditorGUILayout.Vector2IntField("尺寸", size);
        invertRoughness = EditorGUILayout.Toggle("反转 Roughness", invertRoughness);

        GUILayout.Space(10);
        GUILayout.Label("输出设置", EditorStyles.boldLabel);

        fileNameFormat = EditorGUILayout.TextField("命名格式", fileNameFormat);
        useSourcePath = EditorGUILayout.Toggle("使用源贴图路径", useSourcePath);

        if (!useSourcePath)
        {
            EditorGUILayout.BeginHorizontal();
            customSavePath = EditorGUILayout.TextField("保存路径", customSavePath);

            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string p = EditorUtility.OpenFolderPanel("选择路径", "Assets", "");
                if (!string.IsNullOrEmpty(p))
                {
                    if (p.StartsWith(Application.dataPath))
                        customSavePath = "Assets" + p.Substring(Application.dataPath.Length);
                    else
                        Debug.LogError("❌ 必须选择 Assets 目录内路径");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("单张打包", EditorStyles.boldLabel);

        if (mode == PackMode.MRO)
        {
            DrawSeparator("— MRO 贴图 —");
            DrawInput("Metallic → R", inR);
            DrawInput("Roughness → G", inG);
            DrawInputWithChannel("AO → B", inB, ref aoChannel);
        }
        else
        {
            DrawSeparator("— NMR 贴图 —");
            DrawInputNormalLocked("Normal（自动RG）", inR);
            DrawInput("Metallic → B", inB);
            DrawInput("Roughness → A", inA);

            GUILayout.Space(5);
            DrawSeparator("— Basemap（AO → Alpha）—");

            // Basemap 锁定 RGB
            DrawInputBasemapLocked("Basemap（RGB）", inBasemap);

            // AO 可选 channel
            DrawInputWithChannel("AO → Alpha", inAO, ref inAO.channel);

            EditorGUILayout.HelpBox(
                "AO 写入 Basemap 的 Alpha 通道，仅限不透明物体使用",
                MessageType.Info);
        }

        if (GUILayout.Button("生成单张", GUILayout.Height(30)))
            GenerateManual();

        GUILayout.Space(20);
        GUILayout.Label("批处理（自动识别）", EditorStyles.boldLabel);

        for (int i = 0; i < inputTextures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            inputTextures[i] = (Texture2D)EditorGUILayout.ObjectField(
                inputTextures[i], typeof(Texture2D), false);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                inputTextures.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        Rect drop = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(drop, "拖拽贴图到这里");
        HandleDrag(drop);

        if (GUILayout.Button("批量打包", GUILayout.Height(30)))
            ProcessBatch();
    }

    // ================= GUI 工具 =================

    void DrawSeparator(string label)
    {
        GUILayout.Space(4);
        EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(2);
    }

    // 普通输入（可选 channel）
    void DrawInput(string label, InputSet set)
    {
        EditorGUILayout.BeginHorizontal();
        set.tex = (Texture2D)EditorGUILayout.ObjectField(label, set.tex, typeof(Texture2D), false);
        set.channel = (Channel)EditorGUILayout.EnumPopup(set.channel, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
    }

    // Normal 锁定 RG（不显示 channel 下拉）
    void DrawInputNormalLocked(string label, InputSet set)
    {
        EditorGUILayout.BeginHorizontal();
        set.tex = (Texture2D)EditorGUILayout.ObjectField(label, set.tex, typeof(Texture2D), false);
        GUILayout.Label("RG", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
    }

    // Basemap 锁定 RGB（不显示 channel 下拉）
    void DrawInputBasemapLocked(string label, InputSet set)
    {
        EditorGUILayout.BeginHorizontal();
        set.tex = (Texture2D)EditorGUILayout.ObjectField(label, set.tex, typeof(Texture2D), false);
        GUILayout.Label("RGB", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
    }

    // 带独立 channel ref 的输入（AO 专用）
    void DrawInputWithChannel(string label, InputSet set, ref Channel ch)
    {
        EditorGUILayout.BeginHorizontal();
        set.tex = (Texture2D)EditorGUILayout.ObjectField(label, set.tex, typeof(Texture2D), false);
        ch = (Channel)EditorGUILayout.EnumPopup(ch, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
    }

    void HandleDrag(Rect area)
    {
        Event evt = Event.current;

        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            && area.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                    if (obj is Texture2D tex)
                        inputTextures.Add(tex);
            }

            evt.Use();
        }
    }

    // ================= 单张 =================

    void GenerateManual()
    {
        int w = size.x;
        int h = size.y;
        Color[] pixels = new Color[w * h];

        if (mode == PackMode.MRO)
        {
            var rTex = Read(inR.tex, w, h);
            var gTex = Read(inG.tex, w, h);
            var bTex = Read(inB.tex, w, h);

            for (int i = 0; i < pixels.Length; i++)
            {
                float r = Sample(rTex, i, inR.channel);
                float g = Sample(gTex, i, inG.channel);
                float b = Sample(bTex, i, aoChannel); // AO channel

                if (invertRoughness) g = 1 - g;

                pixels[i] = new Color(r, g, b, 1);
            }

            string p = SaveTexture(pixels, w, h, "Manual", inR.tex);
            SetSRGB(p, false);
        }
        else
        {
            // NMR 主贴图
            var normal = Read(inR.tex, w, h, true);
            var mTex = Read(inB.tex, w, h);
            var rTex = Read(inA.tex, w, h);

            for (int i = 0; i < pixels.Length; i++)
            {
                float nx = normal != null ? normal[i].r : 0.5f;
                float ny = normal != null ? normal[i].g : 0.5f;
                float metallic = Sample(mTex, i, inB.channel);
                float rough = Sample(rTex, i, inA.channel);

                if (invertRoughness) rough = 1 - rough;

                pixels[i] = new Color(nx, ny, metallic, rough);
            }

            string p = SaveTexture(pixels, w, h, "Manual", inR.tex);
            SetSRGB(p, false);

            // Basemap + AO → Alpha
            if (inBasemap.tex != null)
            {
                var basePx = GenerateBaseWithAO(
                    inBasemap.tex, inAO.tex, inAO.channel, w, h);
                // Basemap 保持 sRGB = true
                SaveTexture(basePx, w, h, "Manual_Basemap", inBasemap.tex);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("单张打包完成");
    }

    // ================= 批处理 =================

    void ProcessBatch()
    {
        Dictionary<string, TexSet> groups = new Dictionary<string, TexSet>();

        foreach (var tex in inputTextures)
        {
            if (!tex) continue;

            string key = tex.name.Split('_')[0];

            if (!groups.ContainsKey(key))
                groups[key] = new TexSet();

            if (tex.name.Contains("_Metallic")) groups[key].metallic = tex;
            else if (tex.name.Contains("_Roughness")) groups[key].roughness = tex;
            else if (tex.name.Contains("_AO")) groups[key].ao = tex;
            else if (tex.name.Contains("_Normal")) groups[key].normal = tex;
            else if (tex.name.Contains("_BaseColor") ||
                     tex.name.Contains("_Basemap") ||
                     tex.name.Contains("_Albedo")) groups[key].basemap = tex;
        }

        foreach (var kv in groups)
        {
            string baseName = kv.Key;
            TexSet set = kv.Value;

            var packed = Pack(set);

            if (mode == PackMode.MRO)
            {
                string p = SaveTexture(
                    packed.GetPixels(), packed.width, packed.height,
                    baseName, set.metallic);
                SetSRGB(p, false);
            }
            else
            {
                string p = SaveTexture(
                    packed.GetPixels(), packed.width, packed.height,
                    baseName, set.normal);
                SetSRGB(p, false);

                if (set.basemap)
                {
                    var basePx = GenerateBaseWithAO(
                        set.basemap, set.ao, aoChannel, size.x, size.y);
                    SaveTexture(basePx, size.x, size.y,
                        baseName + "_Basemap", set.basemap);
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("批处理完成");
    }

    // ================= 打包逻辑 =================

    Texture2D Pack(TexSet set)
    {
        int w = size.x;
        int h = size.y;

        var m = Read(set.metallic, w, h);
        var r = Read(set.roughness, w, h);
        var ao = Read(set.ao, w, h, true);
        var n = Read(set.normal, w, h, true);

        Color[] pixels = new Color[w * h];

        for (int i = 0; i < pixels.Length; i++)
        {
            float metallic = Get(m, i);
            float rough = Get(r, i);
            float occlusion = Sample(ao, i, aoChannel); // AO channel

            if (invertRoughness) rough = 1 - rough;

            if (mode == PackMode.MRO)
            {
                pixels[i] = new Color(metallic, rough, occlusion, 1);
            }
            else
            {
                float nx = n != null ? n[i].r : 0.5f;
                float ny = n != null ? n[i].g : 0.5f;
                pixels[i] = new Color(nx, ny, metallic, rough);
            }
        }

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // Basemap RGB + AO → Alpha（支持选 channel）
    Color[] GenerateBaseWithAO(Texture2D basemap, Texture2D ao, Channel aoCh, int w, int h)
    {
        var basePx = Read(basemap, w, h, false, true);
        var aoPx = Read(ao, w, h, true);

        Color[] result = new Color[w * h];

        for (int i = 0; i < result.Length; i++)
        {
            float r = basePx != null ? basePx[i].r : 0;
            float g = basePx != null ? basePx[i].g : 0;
            float b = basePx != null ? basePx[i].b : 0;
            float a = aoPx != null ? Sample(aoPx, i, aoCh) : 1f; // 按选定 channel 采样

            result[i] = new Color(r, g, b, a);
        }

        return result;
    }

    // ================= 读取 =================

    Color[] Read(Texture2D src, int w, int h, bool forceLinear = false, bool srgb = false)
    {
        if (!src) return null;

        string path = AssetDatabase.GetAssetPath(src);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);

        TextureImporterType originalType = TextureImporterType.Default;
        bool originalSRGB = false;
        bool originalReadable = false;
        TextureImporterCompression originalCompression = TextureImporterCompression.Compressed;
        bool needRestore = false;

        if (importer && forceLinear)
        {
            originalType = importer.textureType;
            originalSRGB = importer.sRGBTexture;
            originalReadable = importer.isReadable;
            originalCompression = importer.textureCompression;
            needRestore = true;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        var rt = new RenderTexture(
            w, h, 0,
            RenderTextureFormat.ARGB32,
            srgb ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);

        Graphics.Blit(src, rt);
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, !srgb);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        rt.Release();

        var result = tex.GetPixels();

        if (needRestore && importer)
        {
            importer.textureType = originalType;
            importer.sRGBTexture = originalSRGB;
            importer.isReadable = originalReadable;
            importer.textureCompression = originalCompression;
            importer.SaveAndReimport();

            Debug.Log($"已还原法线贴图: {src.name} → {originalType}");
        }

        return result;
    }
    
    // ================= 工具 =================

    float GetChannel(Color c, Channel ch)
    {
        switch (ch)
        {
            case Channel.R: return c.r;
            case Channel.G: return c.g;
            case Channel.B: return c.b;
            case Channel.A: return c.a;
        }

        return 0;
    }

    float Sample(Color[] arr, int i, Channel ch)
    {
        if (arr == null) return 0;
        return GetChannel(arr[i], ch);
    }

    float Get(Color[] arr, int i)
    {
        if (arr == null) return 0;
        return arr[i].r;
    }

    // ================= 保存 =================

    string SaveTexture(Color[] pixels, int w, int h, string baseName, Texture2D refTex)
    {
        string finalName = fileNameFormat
            .Replace("{name}", baseName)
            .Replace("{mode}", mode.ToString());

        string folder;

        if (useSourcePath && refTex != null)
        {
            string refPath = AssetDatabase.GetAssetPath(refTex);
            folder = Path.GetDirectoryName(refPath)?.Replace("\\", "/");
        }
        else
        {
            folder = customSavePath;
        }

        if (!Directory.Exists(folder))
            if (folder != null)
                Directory.CreateDirectory(folder);

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        tex.SetPixels(pixels);
        tex.Apply();

        string fullPath = folder + "/" + finalName + ".png";
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());

        // 返回 Asset 相对路径
        string assetPath = fullPath.StartsWith(Application.dataPath)
            ? "Assets" + fullPath.Substring(Application.dataPath.Length)
            : fullPath;

        Debug.Log("保存: " + assetPath);
        return assetPath;
    }

    void SetSRGB(string assetPath, bool sRGB)
    {
        AssetDatabase.ImportAsset(assetPath);

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogWarning("⚠️ 找不到 Importer: " + assetPath);
            return;
        }

        importer.sRGBTexture = sRGB;
        importer.SaveAndReimport();

        Debug.Log($"sRGB = {sRGB} : {assetPath}");
    }
}