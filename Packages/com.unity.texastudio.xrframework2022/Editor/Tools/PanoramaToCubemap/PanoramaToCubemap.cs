/**
 * 1枚のpanorama画像（2x1の比率のsphere map）をcubemapに展開し6枚のテクスチャを出力.
 * date     : 12/10/2013 - 12/10/2013
 * Author   : Yutaka Yoshisaka
 * Version  : 1.0.0
 */

using UnityEngine;
using UnityEditor;
using System.IO;

public class PanoramaToCubemap : EditorWindow
{
    public const int FACE_FRONT = 0;
    public const int FACE_BACK = 1;
    public const int FACE_LEFT = 2;
    public const int FACE_RIGHT = 3;
    public const int FACE_UP = 4;
    public const int FACE_DOWN = 5;

    public const string outputImageDirectory = "Assets/output_images"; // 生成したテクスチャを出力するディレクトリ.
    public const string outputMaterialDirectory = "Assets/output_materials"; // 生成したマテリアルを出力するディレクトリ.

    private Texture2D m_srcTexture = null;
    private float m_direction = 0.0f;

    private string[] m_textureSize = { "64", "128", "256", "512", "1024" };
    private int m_textureSizeIndex = 3;

    private Texture2D m_dstTextureFront = null;
    private Texture2D m_dstTextureBack = null;
    private Texture2D m_dstTextureLeft = null;
    private Texture2D m_dstTextureRight = null;
    private Texture2D m_dstTextureUp = null;
    private Texture2D m_dstTextureDown = null;

    private Material m_SkyboxMaterial = null;
    private Cubemap m_Cubemap = null;

    [MenuItem("Window/Panorama To Cubemap")]
    private static void Init()
    {
        var window = (PanoramaToCubemap)GetWindow(typeof(PanoramaToCubemap));
        window.minSize = new Vector2(300.0f, 560.0f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        m_srcTexture = EditorGUILayout.ObjectField(m_srcTexture, typeof(Texture2D), true, GUILayout.MinWidth(200),
            GUILayout.MaxWidth(200), GUILayout.MinHeight(100), GUILayout.MaxHeight(100)) as Texture2D;
        m_direction = EditorGUILayout.Slider("Direction", m_direction, -180.0f, 180.0f);
        m_textureSizeIndex = EditorGUILayout.Popup("Texture Size", m_textureSizeIndex, m_textureSize);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        if (GUILayout.Button("Clear"))
        {
            m_Clear();
        }

        if (GUILayout.Button("Convert"))
        {
            m_ConvertPanoramaToCubemap(); //6 side to Cubemap.
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Front (+Z)");
            m_dstTextureFront = EditorGUILayout.ObjectField(m_dstTextureFront, typeof(Texture2D), true,
                GUILayout.MinWidth(64), GUILayout.MaxWidth(64), GUILayout.MinHeight(64),
                GUILayout.MaxHeight(64)) as Texture2D;
            EditorGUILayout.EndVertical();
        }
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Back (-Z)");
            m_dstTextureBack = EditorGUILayout.ObjectField(m_dstTextureBack, typeof(Texture2D), true,
                GUILayout.MinWidth(64), GUILayout.MaxWidth(64), GUILayout.MinHeight(64),
                GUILayout.MaxHeight(64)) as Texture2D;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Left (+X)");
            m_dstTextureLeft = EditorGUILayout.ObjectField(m_dstTextureLeft, typeof(Texture2D), true,
                GUILayout.MinWidth(64), GUILayout.MaxWidth(64), GUILayout.MinHeight(64),
                GUILayout.MaxHeight(64)) as Texture2D;
            EditorGUILayout.EndVertical();
        }
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Right (-X)");
            m_dstTextureRight = EditorGUILayout.ObjectField(m_dstTextureRight, typeof(Texture2D), true,
                GUILayout.MinWidth(64), GUILayout.MaxWidth(64), GUILayout.MinHeight(64),
                GUILayout.MaxHeight(64)) as Texture2D;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Up (+Y)");
            m_dstTextureUp = EditorGUILayout.ObjectField(m_dstTextureUp, typeof(Texture2D), true,
                GUILayout.MinWidth(64), GUILayout.MaxWidth(64), GUILayout.MinHeight(64),
                GUILayout.MaxHeight(64)) as Texture2D;
            EditorGUILayout.EndVertical();
        }
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Down (-Y)");
            m_dstTextureDown = EditorGUILayout.ObjectField(m_dstTextureDown, typeof(Texture2D), true,
                GUILayout.MinWidth(64), GUILayout.MaxWidth(64), GUILayout.MinHeight(64),
                GUILayout.MaxHeight(64)) as Texture2D;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        if (GUILayout.Button("Create Skybox"))
        {
            m_CreateSkybox(); // Create Skybox.
        }

        if (GUILayout.Button("Create Cubemap"))
        {
            m_CreateCubemap(); //Create Cubemap.
        }

        EditorGUILayout.EndVertical();
    }

    /**
     * テクスチャサイズを取得.
     */
    private int m_GetCubemapTextureSize()
    {
        var size = m_textureSizeIndex switch
        {
            0 => 64,
            1 => 128,
            2 => 256,
            3 => 512,
            4 => 1024,
            _ => 512
        };

        return size;
    }

    /**
     * 設定をクリアする.
     */
    private void m_Clear()
    {
        m_srcTexture = null;
        m_direction = 0.0f;
        m_textureSizeIndex = 3;

        m_dstTextureFront = null;
        m_dstTextureBack = null;
        m_dstTextureLeft = null;
        m_dstTextureRight = null;
        m_dstTextureUp = null;
        m_dstTextureDown = null;

        m_SkyboxMaterial = null;
        m_Cubemap = null;
    }

    /**
     * 2x1の縮尺のパノラマ画像をCubemapに展開.
     */
    private void m_ConvertPanoramaToCubemap()
    {
        if (m_srcTexture is null)
        {
            EditorUtility.DisplayDialog("Error", "Please set panorama image!", "OK");
            return;
        }

        // 一時的にテクスチャのGetPixelを有効にする.
        var assetPath = AssetDatabase.GetAssetPath(m_srcTexture);
        var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        var oldIsReadable = ti is not null && ti.isReadable;
        if (ti is not null)
        {
            var oldImporterFormat = ti.textureCompression;
            ti.isReadable = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(assetPath);

            // cubemap画像を出力するディレクトリの作成.
            if (!Directory.Exists(outputImageDirectory))
            {
                Directory.CreateDirectory(outputImageDirectory);
            }

            var filePath = outputImageDirectory + "/" + m_srcTexture.name;
            var texSize = m_GetCubemapTextureSize();

            m_dstTextureFront = m_CreateCubemapTexture(texSize, FACE_FRONT, filePath + "_front.png");
            m_dstTextureBack = m_CreateCubemapTexture(texSize, FACE_BACK, filePath + "_back.png");
            m_dstTextureLeft = m_CreateCubemapTexture(texSize, FACE_LEFT, filePath + "_left.png");
            m_dstTextureRight = m_CreateCubemapTexture(texSize, FACE_RIGHT, filePath + "_right.png");
            m_dstTextureUp = m_CreateCubemapTexture(texSize, FACE_UP, filePath + "_up.png");
            m_dstTextureDown = m_CreateCubemapTexture(texSize, FACE_DOWN, filePath + "_down.png");

            ti.isReadable = oldIsReadable;
            ti.textureCompression = oldImporterFormat;
        }

        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();
    }

    /**
     * panorama to cubemap.
     */
    private Texture2D m_CreateCubemapTexture(int texSize, int faceIndex, string fileName = null)
    {
        var tex = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);

        var vDirA = new Vector3[4];
        switch (faceIndex)
        {
            case FACE_FRONT:
                vDirA[0] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, -1.0f);
                break;
            case FACE_BACK:
                vDirA[0] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(-1.0f, 1.0f, 1.0f);
                break;
            case FACE_LEFT:
                vDirA[0] = new Vector3(1.0f, -1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(1.0f, 1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case FACE_RIGHT:
                vDirA[0] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(-1.0f, 1.0f, -1.0f);
                break;
            case FACE_UP:
                vDirA[0] = new Vector3(-1.0f, 1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, 1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case FACE_DOWN:
                vDirA[0] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, -1.0f, -1.0f);
                break;
        }

        var rotDx1 = (vDirA[1] - vDirA[0]) / texSize;
        var rotDx2 = (vDirA[3] - vDirA[2]) / texSize;

        var dy = 1.0f / texSize;
        var fy = 0.0f;

        var cols = new Color[texSize];
        for (var y = 0; y < texSize; y++)
        {
            var xv1 = vDirA[0];
            var xv2 = vDirA[2];
            for (var x = 0; x < texSize; x++)
            {
                var v = (xv2 - xv1) * fy + xv1;
                v.Normalize();
                cols[x] = m_CalcProjectionSpherical(v);
                xv1 += rotDx1;
                xv2 += rotDx2;
            }

            tex.SetPixels(0, y, texSize, 1, cols);
            fy += dy;
        }

        tex.wrapMode = TextureWrapMode.Clamp; // cubemapの場合は、wrapModeでClampしないと境界が見えてしまう.
        tex.Apply();

        if (fileName != null)
        {
            // pngファイルとして出力.
            var pngData = tex.EncodeToPNG();
            File.WriteAllBytes(fileName, pngData);
        }

        AssetDatabase.Refresh(); // これがないと、AssetDatabase.LoadAssetAtPathでnullが返る場合がある.

        // いったん破棄してから、fileNameのものを読み込んで割り当て.
        // こうしないとメモリリークする.
        DestroyImmediate(tex);
        tex = AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture2D)) as Texture2D;

        {
            if (AssetImporter.GetAtPath(fileName) is not TextureImporter ti) return tex;
            ti.wrapMode = TextureWrapMode.Clamp; // cubemapの場合はwrapModeでClampしないと境界が見えてしまうため、変更.
            AssetDatabase.ImportAsset(fileName);
        }

        return tex;
    }

    /**
     * 指定のTextureでGetPixelを使えるようにする.
     */
    private static void m_EnableTextureGetPixel(Object tex, bool enable)
    {
        var assetPath = AssetDatabase.GetAssetPath(tex);
        if (assetPath is null or "") return;

        var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (enable)
        {
            if (ti is not null)
            {
                ti.isReadable = true;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
        else
        {
            if (ti is not null)
            {
                ti.isReadable = false;
                ti.textureCompression = TextureImporterCompression.Compressed;
            }
        }

        AssetDatabase.ImportAsset(assetPath);
    }

    /**
     * 球投影の場合の指定方向に対応する色を取得.
     */
    private Color m_CalcProjectionSpherical(Vector3 vDir)
    {
        var theta = Mathf.Atan2(vDir.z, vDir.x); // -π ～ +π（水平方向の円周上の回転）.
        var phi = Mathf.Acos(vDir.y); //  0  ～ +π（垂直方向の回転）.

        theta += m_direction * Mathf.PI / 180.0f;
        while (theta < -Mathf.PI) theta += Mathf.PI + Mathf.PI;
        while (theta > Mathf.PI) theta -= Mathf.PI + Mathf.PI;

        var dx = theta / Mathf.PI; // -1.0 ～ +1.0.
        var dy = phi / Mathf.PI; //  0.0 ～ +1.0.

        dx = dx * 0.5f + 0.5f;
        var px = (int)(dx * m_srcTexture.width);
        if (px < 0) px = 0;
        if (px >= m_srcTexture.width) px = m_srcTexture.width - 1;
        var py = (int)(dy * m_srcTexture.height);
        if (py < 0) py = 0;
        if (py >= m_srcTexture.height) py = m_srcTexture.height - 1;

        var col = m_srcTexture.GetPixel(px, m_srcTexture.height - py - 1);
        return col;
    }

    /**
     * SkyBoxのマテリアルを生成.
     */
    private void m_CreateSkybox()
    {
        if (m_srcTexture is null)
        {
            EditorUtility.DisplayDialog("Error", "Please set panorama image!", "OK");
            return;
        }

        // Materialを出力するディレクトリの作成.
        if (!Directory.Exists(outputMaterialDirectory))
        {
            Directory.CreateDirectory(outputMaterialDirectory);
        }

        m_SkyboxMaterial = new Material(Shader.Find("Mobile/Skybox"));
        if (m_SkyboxMaterial is null) return;

        if (m_dstTextureFront is not null) m_SkyboxMaterial.SetTexture("_FrontTex", m_dstTextureFront);
        if (m_dstTextureBack is not null) m_SkyboxMaterial.SetTexture("_BackTex", m_dstTextureBack);
        if (m_dstTextureLeft is not null) m_SkyboxMaterial.SetTexture("_LeftTex", m_dstTextureLeft);
        if (m_dstTextureRight is not null) m_SkyboxMaterial.SetTexture("_RightTex", m_dstTextureRight);
        if (m_dstTextureUp is not null) m_SkyboxMaterial.SetTexture("_UpTex", m_dstTextureUp);
        if (m_dstTextureDown is not null) m_SkyboxMaterial.SetTexture("_DownTex", m_dstTextureDown);

        var fileName = outputMaterialDirectory + "/" + m_srcTexture.name + ".mat";
        AssetDatabase.CreateAsset(m_SkyboxMaterial, fileName);

        Selection.activeObject = m_SkyboxMaterial;
    }

    /**
     * キューブマップの作成.
     */
    private void m_CreateCubemap()
    {
        if (m_srcTexture is null)
        {
            EditorUtility.DisplayDialog("Error", "Please set panorama image!", "OK");
            return;
        }

        // Textureを出力するディレクトリの作成.
        if (!Directory.Exists(outputImageDirectory))
        {
            Directory.CreateDirectory(outputImageDirectory);
        }

        var texSize = m_GetCubemapTextureSize();
        m_Cubemap = new Cubemap(texSize, TextureFormat.RGB24, false);

        if (m_dstTextureFront is not null)
        {
            m_EnableTextureGetPixel(m_dstTextureFront, true);

            var dstCols = new Color[texSize * texSize];

            var iPos = 0;
            for (var y = 0; y < texSize; y++)
            {
                var srcLines = m_dstTextureFront.GetPixels(0, texSize - y - 1, texSize, 1);
                for (var x = 0; x < texSize; x++)
                {
                    dstCols[iPos + x] = srcLines[x];
                }

                iPos += texSize;
            }

            m_Cubemap.SetPixels(dstCols, CubemapFace.PositiveZ);

            m_EnableTextureGetPixel(m_dstTextureFront, false);
        }

        if (m_dstTextureBack is not null)
        {
            m_EnableTextureGetPixel(m_dstTextureBack, true);

            var dstCols = new Color[texSize * texSize];

            var iPos = 0;
            for (var y = 0; y < texSize; y++)
            {
                var srcLines = m_dstTextureBack.GetPixels(0, texSize - y - 1, texSize, 1);
                for (var x = 0; x < texSize; x++)
                {
                    dstCols[iPos + x] = srcLines[x];
                }

                iPos += texSize;
            }

            m_Cubemap.SetPixels(dstCols, CubemapFace.NegativeZ);

            m_EnableTextureGetPixel(m_dstTextureBack, false);
        }

        if (m_dstTextureLeft is not null)
        {
            m_EnableTextureGetPixel(m_dstTextureLeft, true);

            var dstCols = new Color[texSize * texSize];

            var iPos = 0;
            for (var y = 0; y < texSize; y++)
            {
                var srcLines = m_dstTextureLeft.GetPixels(0, texSize - y - 1, texSize, 1);
                for (var x = 0; x < texSize; x++)
                {
                    dstCols[iPos + x] = srcLines[x];
                }

                iPos += texSize;
            }

            m_Cubemap.SetPixels(dstCols, CubemapFace.PositiveX);

            m_EnableTextureGetPixel(m_dstTextureLeft, false);
        }

        if (m_dstTextureRight is not null)
        {
            m_EnableTextureGetPixel(m_dstTextureRight, true);

            var dstCols = new Color[texSize * texSize];

            var iPos = 0;
            for (var y = 0; y < texSize; y++)
            {
                var srcLines = m_dstTextureRight.GetPixels(0, texSize - y - 1, texSize, 1);
                for (var x = 0; x < texSize; x++)
                {
                    dstCols[iPos + x] = srcLines[x];
                }

                iPos += texSize;
            }

            m_Cubemap.SetPixels(dstCols, CubemapFace.NegativeX);

            m_EnableTextureGetPixel(m_dstTextureRight, false);
        }

        if (m_dstTextureUp is not null)
        {
            m_EnableTextureGetPixel(m_dstTextureUp, true);

            var dstCols = new Color[texSize * texSize];

            var iPos = 0;
            for (var y = 0; y < texSize; y++)
            {
                var srcLines = m_dstTextureUp.GetPixels(0, texSize - y - 1, texSize, 1);
                for (var x = 0; x < texSize; x++)
                {
                    dstCols[iPos + x] = srcLines[x];
                }

                iPos += texSize;
            }

            m_Cubemap.SetPixels(dstCols, CubemapFace.PositiveY);

            m_EnableTextureGetPixel(m_dstTextureUp, false);
        }

        if (m_dstTextureDown is not null)
        {
            m_EnableTextureGetPixel(m_dstTextureDown, true);

            var dstCols = new Color[texSize * texSize];

            var iPos = 0;
            for (var y = 0; y < texSize; y++)
            {
                var srcLines = m_dstTextureDown.GetPixels(0, texSize - y - 1, texSize, 1);
                for (var x = 0; x < texSize; x++)
                {
                    dstCols[iPos + x] = srcLines[x];
                }

                iPos += texSize;
            }

            m_Cubemap.SetPixels(dstCols, CubemapFace.NegativeY);

            m_EnableTextureGetPixel(m_dstTextureDown, false);
        }

        m_Cubemap.Apply();

        var fileName = outputImageDirectory + "/" + m_srcTexture.name + ".cubemap";
        AssetDatabase.CreateAsset(m_Cubemap, fileName);

        Selection.activeObject = m_Cubemap;
    }
}