// Title: Enable Texture Alpha Channel

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Graphics = UnityEngine.Graphics;

public class EnableTextureAlphaChannel : EditorWindow
{
    [SerializeField] private List<Texture2D> textures = new();

    [SerializeField] private List<Texture2D> newAlphaTextures = new();

    private readonly List<Texture2D> _alphaTextures = new();
    private SerializedObject _textureSerializedObject;
    private SerializedProperty _textureSerializedProperty;
    private SerializedObject _newTextureSerializedObject;
    private SerializedProperty _newTextureSerializedProperty;
    private Vector2 _scrollPos;
    private Vector2 _scrollPos2;

    private static readonly int LtyMask = Shader.PropertyToID("_Mask_Tex");
    private static readonly int UniversalMask = Shader.PropertyToID("_MaskTex");
    private static readonly int TaMask = Shader.PropertyToID("_MaskMap");
    private static readonly int LtyRemap = Shader.PropertyToID("_Remap_Tex");
    private static readonly int LtyMainRa = Shader.PropertyToID("_Main_Tex_A_R");
    private static readonly int LtyMaskRa = Shader.PropertyToID("_Mask_Tex_A_R");
    private static readonly int LtyRemapRa = Shader.PropertyToID("_Remap_Tex_A_R");
    private static readonly int UniversalMainRa = Shader.PropertyToID("_RasAlpha_Main");
    private static readonly int UniversalMaskRa = Shader.PropertyToID("_RasAlpha_Mask");
    private static readonly int CommonMainRa = Shader.PropertyToID("_BaseRtoA");
    private static readonly int CommonMaskRa = Shader.PropertyToID("_MaskRtoA");

    //private bool _antiGamma;
    private int _index;
    private bool _hasAlphaChannel;
    private int _allParticleSystemsCount;
    private int _activeParticleSystemsCount;
    private int _disableParticleSystemsCount;
    private bool _alphaIs1;

    [MenuItem("Tools/纹理相关/特效贴图Alpha通道写入工具")]
    private static void Init()
    {
        var window = (EnableTextureAlphaChannel)GetWindow(typeof(EnableTextureAlphaChannel), true, "覆盖特效贴图Alpha通道 (●'◡'●)");
        window.Show();
    }

    private void OnEnable()
    {
        _textureSerializedObject = new SerializedObject(this);
        _textureSerializedProperty = _textureSerializedObject.FindProperty("textures");

        _newTextureSerializedObject = new SerializedObject(this);
        _newTextureSerializedProperty = _newTextureSerializedObject.FindProperty("newAlphaTextures");
    }

    private void OnGUI()
    {
        FindAllObjects();
        GUILayout.Space(15);
        GUILayout.Label("当前场景下有(包括被禁用的和已激活的)粒子特效共有" + _allParticleSystemsCount + "个");
        GUILayout.Space(15);
        GUILayout.Label("当前场景下有(已激活的)粒子特效共有" + _activeParticleSystemsCount + "个");
        GUILayout.Space(15);
        GUILayout.Label("当前场景下有(被禁用的)粒子特效共有" + _disableParticleSystemsCount + "个");
        GUILayout.Space(15);
        AddNewTexList();
        GUILayout.FlexibleSpace();
        WriteAlpha();
    }

    private void OnDisable()
    {
        _textureSerializedObject.Dispose();
        _newTextureSerializedObject.Dispose();
        textures.Clear();
        newAlphaTextures.Clear();
        _alphaTextures.Clear();
    }

    private void FindAllObjects()
    {
        GUILayout.Space(20);
        GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10) });
        GUILayout.Label("查找当前场景下所有粒子特效",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20 });
        GUILayout.EndVertical();
        _index = GUILayout.SelectionGrid(_index, new[] { "包括被禁用的和已激活的", "被禁用的", "已激活的" },
            3, new GUIStyle(GUI.skin.button) { fontSize = 15 });
        GUILayout.Space(20);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        _textureSerializedObject.Update();
        EditorGUILayout.PropertyField(_textureSerializedProperty, true);
        _textureSerializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
        GUILayout.Space(10);
        if (!GUILayout.Button("获取粒子特效使用的MainTex && Mask Tex",
                new GUIStyle(GUI.skin.button) { fontSize = 25 })) return;
        textures.Clear();
        //newAlphaTextures.Clear();
        var allObjects = Resources.FindObjectsOfTypeAll<ParticleSystem>().Where(go =>
            !EditorUtility.IsPersistent(go.transform.root.gameObject) &&
            go.hideFlags is not (HideFlags.NotEditable or HideFlags.HideAndDontSave)).ToList();
        var particleSystems = new List<ParticleSystem>();
        switch (_index)
        {
            case 0:
                particleSystems = allObjects;
                _allParticleSystemsCount = particleSystems.Count;
                break;
            case 1:
                particleSystems = allObjects.Where(h => h.gameObject.activeInHierarchy is false).ToList();
                _disableParticleSystemsCount = particleSystems.Count;
                break;
            case 2:
                particleSystems = allObjects.Where(h => h.gameObject.activeInHierarchy).ToList();
                _activeParticleSystemsCount = particleSystems.Count;
                break;
        }

        if (allObjects.Count is 0)
        {
            EditorUtility.DisplayDialog("提示", "当前场景内没有粒子特效！！！", "确定");
            return;
        }

        foreach (var particleSystem in particleSystems)
        {
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer.gameObject.activeSelf == false) continue;
            var materials = renderer.sharedMaterials;
            if (materials is null) continue;
            foreach (var material in materials)
            {
                if (material is null) continue;
                var shader = material.shader;
                var shaderName = shader.name;
                var mainTexture = material.mainTexture;
                if (mainTexture is not Texture2D texture2D) continue;
                if (textures.Contains(texture2D)) continue;
                if (texture2D.name.Contains("Default-Particle")) continue;
                Debug.Log($"材质为:{material.name}  Shader为:{shaderName}  主贴图为:{mainTexture.name}");
                var isEnableMainTexAChannel = IsEnableMainTextureAChannel(material, shaderName);
                var isEnableMaskTexAChannel = IsEnableMaskTextureAChannel(material, shaderName);
                var isEnableRemapTexAChannel = IsEnableRemapTextureAChannel(material,shaderName);
                if (!isEnableMainTexAChannel) continue;
                textures.Add(texture2D);

                if (!isEnableMaskTexAChannel) continue;
                var maskTexture = shaderName switch
                {
                    "LTY/Shader3.0/ALL3.0_URP" => material.GetTexture(LtyMask),
                    "Effect/UniversalEffect" => material.GetTexture(UniversalMask),
                    "TA Shaders/FX/FX_Common" => material.GetTexture(TaMask),
                    _ => null
                };
                if (isEnableRemapTexAChannel) continue;
                var remapTexture = shaderName switch
                {
                    "LTY/Shader3.0/ALL3.0_URP" => material.GetTexture(LtyRemap),
                    _ => null
                };
                if (maskTexture is not null)
                {
                    Debug.Log("MaskTex:" + maskTexture.name);
                    if (maskTexture is not Texture2D maskTex) continue;
                    if (textures.Contains(maskTex)) continue;
                    if (texture2D.name.Contains("Default-Particle")) continue;
                    textures.Add(maskTex);
                }
                else
                {
                    Debug.Log(particleSystem.name + "  遮罩贴图为空");
                }
                if (remapTexture is not null)
                {
                    Debug.Log("RemapTex:" + remapTexture.name);
                    if (remapTexture is not Texture2D remapTex) continue;
                    if (textures.Contains(remapTex)) continue;
                    if (texture2D.name.Contains("Default-Particle")) continue;
                    textures.Add(remapTex);
                }
                else
                {
                    Debug.Log(particleSystem.name + "  Remap贴图为空");
                }
            }
        }
        Debug.Log("共收集到:" + textures.Count + "个贴图");
        if (newAlphaTextures.Count <= 0) return;
        foreach (var t in newAlphaTextures)
        {
            textures.Remove(t);
        }

        if (_alphaTextures.Count <= 0) return;
        foreach (var t in _alphaTextures)
        {
            textures.Remove(t);
        }

        if (textures.Count is not 0) return;
        EditorUtility.DisplayDialog("提示", "当前场景内的粒子特效没有主贴图或遮罩贴图！！！", "确定");
    }

    private void WriteAlpha()
    {
        if (!GUILayout.Button("写入Alpha", new GUIStyle(GUI.skin.button) { fontSize = 25 })) return;
        if (textures.Count is 0)
        {
            EditorUtility.DisplayDialog("提示", "先获取贴图！！！", "确定");
            return;
        }

        for (var index = textures.Count - 1; index >= 0; index--)
        {
            var tex = textures[index];
            // var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            // if (importer is null) return;
            // var doesHaveAlpha = importer.DoesSourceTextureHaveAlpha();
            // if (!doesHaveAlpha)
            // {
            //     Debug.Log("Texture name:" + tex.name + "没有Alpha通道");
            //     textures.Remove(tex);
            //     continue;
            // }
            var readTexture = IsReadTexture(tex);
            var pixels32 = readTexture.GetPixels32();
            var alphaPixels = new Color32[pixels32.Length];
            if (pixels32.Length is 0)
            {
                Debug.Log("Texture name:" + tex.name + "没有像素");
                return;
            }

            for (var i = 0; i < pixels32.Length; i++)
            {
                if (pixels32[i].a >= 200)
                {
                    _alphaIs1 = true;
                }
                else
                {
                    _alphaIs1 = false;
                    break;
                }

                if (!_alphaIs1) continue;
                Color color = pixels32[i];
                var alpha = Convert.ToByte(color.grayscale * 255);
                alphaPixels[i] = new Color32(pixels32[i].r, pixels32[i].g, pixels32[i].b, alpha);
            }

            if (_alphaIs1)
            {
                EditorUtility.DisplayProgressBar("写入Alpha通道", tex.name,
                    (float)textures.IndexOf(tex) / textures.Count);
                readTexture.SetPixels32(alphaPixels);
                readTexture.Apply();
                var bytes = readTexture.EncodeToPNG();
                var path = AssetDatabase.GetAssetPath(tex);
                var fileName = path.Split('/').Last();
                var filePath = path.Replace(fileName, "");
                var newFileName = fileName.Split('.').First() + "_Alpha." + fileName.Split('.').Last();
                var newPath = filePath + newFileName;
                File.WriteAllBytes(newPath, bytes);
                AssetDatabase.Refresh();
                var oldTexGuid = AssetDatabase.AssetPathToGUID(path);
                var newTexGuid = AssetDatabase.AssetPathToGUID(newPath);
                var newTexMetaPath = newPath + ".meta";
                var content = File.ReadAllText(newTexMetaPath);
                if (Regex.IsMatch(content, newTexGuid))
                {
                    Debug.Log("替换了资源的路径:" + newTexMetaPath,
                        AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(newTexMetaPath)));

                    content = content.Replace(newTexGuid, oldTexGuid);
                    File.WriteAllText(newTexMetaPath, content);
                }
                else
                {
                    Debug.Log("查看了资源的路径:" + newTexMetaPath);
                }

                textures.Remove(tex);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();

                var coverTex = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
                if (coverTex is null) return;
                //textures.Clear();
                newAlphaTextures.Add(coverTex);

                TextureImportSettings(newAlphaTextures);
            }
            else
            {
                Debug.Log("Texture name:" + tex.name + "有Alpha通道并且Alpha通道不为1");
                _alphaTextures.Add(tex);
                textures.Remove(tex);
                _alphaIs1 = false;
            }
            
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("提示", "替换结束", "确定");
    }

    private static bool IsEnableMainTextureAChannel(Material material, string shaderName)
    {
        var isEnable = false;
        switch (shaderName)
        {
            case "LTY/Shader3.0/ALL3.0_URP":
                var mainRa = material.GetFloat(LtyMainRa);
                isEnable = mainRa > 0;
                break;
            case "Effect/UniversalEffect":
                var universalMainRa = material.GetFloat(UniversalMainRa);
                isEnable = universalMainRa < 1;
                break;
            case "TA Shaders/FX/FX_Common":
                var commonMainRa = material.GetFloat(CommonMainRa);
                isEnable = commonMainRa < 1;
                break;
        }

        return isEnable;
    }

    private static bool IsEnableMaskTextureAChannel(Material material, string shaderName)
    {
        var isEnable = false;
        switch (shaderName)
        {
            case "LTY/Shader3.0/ALL3.0_URP":
                var maskRa = material.GetFloat(LtyMaskRa);
                isEnable = maskRa > 0;
                break;
            case "Effect/UniversalEffect":
                var universalMaskRa = material.GetFloat(UniversalMaskRa);
                isEnable = universalMaskRa < 1;
                break;
            case "TA Shaders/FX/FX_Common":
                var commonMaskRa = material.GetFloat(CommonMaskRa);
                isEnable = commonMaskRa < 1;
                break;
        }

        return isEnable;
    }

    private static bool IsEnableRemapTextureAChannel(Material material, string shaderName)
    {
        var isEnable = false;
        switch (shaderName)
        {
            case "LTY/Shader3.0/ALL3.0_URP":
                var remapRa = material.GetFloat(LtyRemapRa);
                isEnable = remapRa > 0;
                break;
        }
        return isEnable;
    }

    private void AddNewTexList()
    {
        _scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2);
        _newTextureSerializedObject.Update();
        EditorGUILayout.PropertyField(_newTextureSerializedProperty, true);
        _newTextureSerializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    private static void TextureImportSettings(List<Texture2D> texture2Ds)
    {
        if (texture2Ds is null) return;
        foreach (var tex in texture2Ds)
        {
            var path = AssetDatabase.GetAssetPath(tex);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Default;
            //importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            var textureSize = SetMaxTextureSize(tex, 128);
            SetTexImporterWebGLSettings(importer, textureSize, TextureImporterCompression.Compressed);
            importer.SaveAndReimport();
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


    private static string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}