using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

internal enum TextureMaxSize
{
    _64 = 64,
    _128 = 128,
    _256 = 256,
    _512 = 512,
}

internal enum TextureCompression
{
    Uncompressed = 0,
    CompressedHq = 1,
    Compressed = 2,
    CompressedLq = 3,
}

public class TexturesDispose : EditorWindow
{
    [SerializeField] private List<Texture2D> textures = new();
    [SerializeField] private List<Texture2D> fxTextures = new();
    private TextureMaxSize _textureMaxSize = TextureMaxSize._512;
    private int _maxTextureSize = 512;
    private TextureCompression _textureCompression = TextureCompression.CompressedLq;
    private TextureImporterCompression _textureImporterCompression = TextureImporterCompression.CompressedLQ;
    private TextureMaxSize _fxTextureMaxSize = TextureMaxSize._128;
    private int _fxMaxTextureSize = 128;
    private TextureCompression _fxTextureCompression = TextureCompression.Compressed;
    private TextureImporterCompression _fxTextureImporterCompression = TextureImporterCompression.Compressed;
    private SerializedObject _serializedObject;
    private SerializedProperty _textureProperty;
    private SerializedObject _fxSerializedObject;
    private SerializedProperty _fxTextureProperty;
    private Vector2 _scrollPos;
    private Vector2 _scrollPos1;
    private int _textureCount;
    private int _fxTextureCount;

    [MenuItem("Tools/纹理相关/纹理处理工具")]
    private static void Init()
    {
        var window = (TexturesDispose)GetWindow(typeof(TexturesDispose), true, "纹理处理工具 (●'◡'●)");
        window.Show();
    }

    private static void Title(string t)
    {
        GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10) });
        GUILayout.Label(t, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20 });
        GUILayout.EndVertical();
    }

    private void OnGUI()
    {
        Title("纹理处理工具");
        GUILayout.Space(10);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        _serializedObject.Update();
        EditorGUILayout.PropertyField(_textureProperty, true);
        _serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
        GUILayout.Space(10);
        _scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1);
        _fxSerializedObject.Update();
        EditorGUILayout.PropertyField(_fxTextureProperty, true);
        _fxSerializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
        GUILayout.Space(10);
        SelectionAllTextures();
        GUILayout.Space(10);
        GUILayout.Label($"当前路径下所有Textures: {_textureCount}个",
            new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 15 });
        GUILayout.Space(10);
        GUILayout.Label($"当前FX贴图有: {_fxTextureCount}个",
            new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 15 });
        GUILayout.Space(20);
        TextureSettings();
        GUILayout.Space(10);
        FXTextureSettings();
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("处理！", new GUIStyle(GUI.skin.button) { fontSize = 25 }))
        {
            Dispose();
        }
    }

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        _textureProperty = _serializedObject.FindProperty("textures");
        _fxSerializedObject = new SerializedObject(this);
        _fxTextureProperty = _fxSerializedObject.FindProperty("fxTextures");
    }

    private void OnDisable()
    {
        _serializedObject = null;
        _textureProperty = null;
        _fxTextureCount = 0;
        _textureCount = 0;
        textures.Clear();
        fxTextures.Clear();
    }

    private void TextureSettings()
    {
        GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10) });
        GUILayout.Label("纹理设置", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20 });
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.Label("纹理最大尺寸:", new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 15 });
        var index = GUILayout.SelectionGrid(_textureMaxSize.GetHashCode(), new[] { "64", "128", "256", "512" }, 4,
            new GUIStyle(GUI.skin.button) { fontSize = 15 });
        _textureMaxSize = (TextureMaxSize)index;
        _maxTextureSize = (int)_textureMaxSize switch
        {
            0 => (int)TextureMaxSize._64,
            1 => (int)TextureMaxSize._128,
            2 => (int)TextureMaxSize._256,
            3 => (int)TextureMaxSize._512,
            _ => _maxTextureSize
        };
        GUILayout.Space(10);
        GUILayout.Label("纹理压缩格式:", new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 15 });
        var index1 = GUILayout.SelectionGrid(_textureCompression.GetHashCode(),
            new[] { "不压缩", "ASTC4x4", "ASTC6x6", "ASTC8x8" }, 4,
            new GUIStyle(GUI.skin.button) { fontSize = 15 });
        _textureCompression = (TextureCompression)index1;
        _textureImporterCompression = (int)_textureCompression switch
        {
            0 => TextureImporterCompression.Uncompressed,
            1 => TextureImporterCompression.CompressedHQ,
            2 => TextureImporterCompression.Compressed,
            3 => TextureImporterCompression.CompressedLQ,
            _ => _textureImporterCompression
        };
    }
    private void FXTextureSettings()
    {
        GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10) });
        GUILayout.Label("FX贴图设置", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20 });
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.Label("FX贴图最大尺寸:", new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 15 });
        var index = GUILayout.SelectionGrid(_fxTextureMaxSize.GetHashCode(), new[] { "64", "128", "256", "512" }, 4,
            new GUIStyle(GUI.skin.button) { fontSize = 15 });
        _fxTextureMaxSize = (TextureMaxSize)index;
        _fxMaxTextureSize = (int)_fxTextureMaxSize switch
        {
            0 => (int)TextureMaxSize._64,
            1 => (int)TextureMaxSize._128,
            2 => (int)TextureMaxSize._256,
            3 => (int)TextureMaxSize._512,
            _ => _fxMaxTextureSize
        };
        GUILayout.Space(10);
        GUILayout.Label("FX贴图压缩格式:", new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 15 });
        var index1 = GUILayout.SelectionGrid(_fxTextureCompression.GetHashCode(),
            new[] { "不压缩", "ASTC4x4", "ASTC6x6", "ASTC8x8" }, 4,
            new GUIStyle(GUI.skin.button) { fontSize = 15 });
        _fxTextureCompression = (TextureCompression)index1;
        _fxTextureImporterCompression = (int)_fxTextureCompression switch
        {
            0 => TextureImporterCompression.Uncompressed,
            1 => TextureImporterCompression.CompressedHQ,
            2 => TextureImporterCompression.Compressed,
            3 => TextureImporterCompression.CompressedLQ,
            _ => _fxTextureImporterCompression
        };
    }
    private void Dispose()
    {
        foreach (var texture2D in textures)
        {
            EditorUtility.DisplayProgressBar("纹理处理中", $"正在处理{texture2D.name}",
                textures.IndexOf(texture2D) * 1.0f / textures.Count);
            if (textures.Count <= 0) continue;
            var path = AssetDatabase.GetAssetPath(texture2D);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
            var isChanged = false;

            if (importer.maxTextureSize != _maxTextureSize)
            {
                importer.maxTextureSize = SetMaxTextureSize(texture2D, _maxTextureSize);
                isChanged = true;
            }

            if (importer.textureCompression != _textureImporterCompression)
            {
                importer.textureCompression = _textureImporterCompression;
                isChanged = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
            }

            if (importer.isReadable)
            {
                importer.isReadable = false;
                isChanged = true;
            }

            if (!isChanged) continue;

#if UNITY_WEBGL
            SetTexImporterWebGLSettings(importer, importer.maxTextureSize, importer.textureCompression);
#endif
            importer.SaveAndReimport();
        }

        EditorUtility.ClearProgressBar();

        foreach (var fxTexture in fxTextures)
        {
            EditorUtility.DisplayProgressBar("纹理处理中", $"正在处理{fxTexture.name}",
                fxTextures.IndexOf(fxTexture) * 1.0f / fxTextures.Count);
            if (fxTextures.Count <= 0) continue;
            var path = AssetDatabase.GetAssetPath(fxTexture);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
            var isChanged = false;
            if (importer.maxTextureSize != _fxMaxTextureSize)
            {
                importer.maxTextureSize = SetMaxTextureSize(fxTexture, _fxMaxTextureSize);
                isChanged = true;
            }

            if (importer.textureCompression != _fxTextureImporterCompression)
            {
                importer.textureCompression = _fxTextureImporterCompression;
                isChanged = true;
            }

            // if (importer.alphaIsTransparency)
            // {
            //     importer.alphaIsTransparency = false;
            // }

            if (importer.isReadable)
            {
                importer.isReadable = false;
                isChanged = true;
            }

            // if (importer.sRGBTexture is false)
            // {
            //     importer.sRGBTexture = true;
            //     isChanged = true;
            // }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                isChanged = true;
            }

            if (!isChanged) continue;

#if UNITY_WEBGL
            SetTexImporterWebGLSettings(importer, importer.maxTextureSize, importer.textureCompression);
#endif
            importer.SaveAndReimport();
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("提示", "纹理处理结束", "确定");
    }

    private void SelectionAllTextures()
    {
        var paths = Selection.assetGUIDs;
        var path = string.Empty;
        foreach (var p in paths)
        {
            var assetsPath = AssetDatabase.GUIDToAssetPath(p);
            if (!AssetDatabase.IsValidFolder(assetsPath)) continue;
            path = assetsPath;
            break;
        }

        GUILayout.Label("选中的路径: " + path, new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter, fontSize = 12,
            fontStyle = FontStyle.Bold
        });
        var arr = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        if (!GUILayout.Button("查看当前路径下所有Texture", new GUIStyle(GUI.skin.button) { fontSize = 15 })) return;
        textures.Clear();
        fxTextures.Clear();
        if (arr.Length > 0)
        {
            foreach (var o in arr)
            {
                if (o.name.StartsWith("FX") || o.name.StartsWith("fx"))
                {
                    fxTextures.Add((Texture2D)o);
                    _fxTextureCount = fxTextures.Count;
                }
                else
                {
                    textures.Add((Texture2D)o);
                    _textureCount = textures.Count;
                }
            }
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "当前路径下没有Texture！！！", "确定");
            textures.Clear();
        }
    }

    private static int SetMaxTextureSize([NotNull] Texture2D texture2D, int maxTexSize)
    {
        if (texture2D is null) throw new ArgumentNullException(nameof(texture2D));
        var nativeSize = Mathf.NextPowerOfTwo(Mathf.Max(texture2D.width, texture2D.height));
        var multipliedNativeRes = Mathf.RoundToInt(nativeSize * 1.0f);
        var textureSize = Mathf.Min(multipliedNativeRes, maxTexSize);
        return textureSize;
    }

    private static void SetTexImporterWebGLSettings(TextureImporter importer, int textureSize,
        TextureImporterCompression textureCompression)
    {
        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
        {
            overridden = true,
            name = "WebGL",
            maxTextureSize = textureSize,
            textureCompression = textureCompression,
            allowsAlphaSplitting = false
        });
    }
}