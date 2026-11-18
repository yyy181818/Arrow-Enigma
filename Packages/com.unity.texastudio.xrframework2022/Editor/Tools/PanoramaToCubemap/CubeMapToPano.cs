using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

enum TextureType
{
    EXR,
    PNG
}

public class CubeMapToPano : ScriptableWizard
{
    [FormerlySerializedAs("Textures")] [SerializeField]
    private List<Texture2D> textures = new();

    private Material _material;

    [FormerlySerializedAs("Resolution")] [SerializeField]
    private Vector2 resolution = new(2048, 1024);

    [SerializeField] private TextureType textureType = TextureType.PNG;
    
    [SerializeField] private bool isRepair;
    
    [FormerlySerializedAs("Path")] [SerializeField]
    private string path = "Assets/Textures";

    [FormerlySerializedAs("Name")] [SerializeField]
    private string outputName = "outputPanorama";

    private RenderTexture _renderTexture;

    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    [MenuItem("Window/Save 6 sided textures to Panorama")]
    private static void MenuEntryCall()
    {
        DisplayWizard("Save 6 sided textures to PNG", typeof(CubeMapToPano), "Save!");
    }

    private void CreateCubeMap()
    {
        if (textures.Count != 6)
        {
            EditorUtility.DisplayDialog("Error", "纹理列表里的贴图数量必须为6个！！！", "Ok");
            return;
        }
        var format = textureType == TextureType.EXR ? TextureFormat.RGBAFloat : TextureFormat.RGB48;
        var cubeMap = new Cubemap((int)resolution.x, format, false);
        foreach (var texture in textures)
        {
            if (texture.name.Contains("nx") is false && texture.name.Contains("ny") is false &&
                texture.name.Contains("nz") is false && texture.name.Contains("px") is false &&
                texture.name.Contains("py") is false && texture.name.Contains("pz") is false)
            {
                EditorUtility.DisplayDialog("Error", "纹理列表里的贴图名称必须包含nx,ny,nz,px,py,pz！！！", "Ok");
                return;
            }

            var textureMaxSize = Mathf.Max(texture.width, texture.height);
            var resolutionMaxSize = Mathf.Max(resolution.x, resolution.y);

            if (textureMaxSize < resolutionMaxSize)
            {
                EditorUtility.DisplayDialog("Error", "纹理列表里的贴图分辨率必须大于等于输出分辨率！！！", "Ok");
                return;
            }

            if (texture.name.Contains("px"))
            {
                var readTexture = IsReadTexture(texture);
                FlipPixels(readTexture, true, true);
                cubeMap.SetPixels(readTexture.GetPixels(), CubemapFace.NegativeX);
            }
            else if (texture.name.Contains("py"))
            {
                var readTexture = IsReadTexture(texture);
                FlipPixels(readTexture, true, true);
                cubeMap.SetPixels(readTexture.GetPixels(), CubemapFace.PositiveY);
            }
            else if (texture.name.Contains("pz"))
            {
                var readTexture = IsReadTexture(texture);
                FlipPixels(readTexture, true, true);
                cubeMap.SetPixels(readTexture.GetPixels(), CubemapFace.PositiveZ);
            }
            else if (texture.name.Contains("nx"))
            {
                var readTexture = IsReadTexture(texture);
                FlipPixels(readTexture, true, true);
                cubeMap.SetPixels(readTexture.GetPixels(), CubemapFace.PositiveX);
            }
            else if (texture.name.Contains("ny"))
            {
                var readTexture = IsReadTexture(texture);
                FlipPixels(readTexture, true, true);
                cubeMap.SetPixels(readTexture.GetPixels(), CubemapFace.NegativeY);
            }
            else if (texture.name.Contains("nz"))
            {
                var readTexture = IsReadTexture(texture);
                FlipPixels(readTexture, true, true);
                cubeMap.SetPixels(readTexture.GetPixels(), CubemapFace.NegativeZ);
            }
        }
        cubeMap.SmoothEdges();
        cubeMap.Apply();
        cubeMap.hideFlags = HideFlags.HideAndDontSave;
        if (_material is not null) return;
        _material = CoreUtils.CreateEngineMaterial("Hidden/CubeMap/CubemapOutput");
        _material.SetTexture(BaseMap, cubeMap);
        if (isRepair)
        {
            _material.EnableKeyword("_REPAIR_ON");
        }
        else
        {
            _material.DisableKeyword("_REPAIR_ON");
        }
    }

    private void CreateRT()
    {
        if (_material is null)
        {
            CreateCubeMap();
        }

        var width = (int)resolution.x;
        var height = (int)resolution.y;
        var format = textureType == TextureType.EXR ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32;
        _renderTexture = new RenderTexture(width, height, 0, format) { enableRandomWrite = true };
        _renderTexture.Create();
        Graphics.Blit(_renderTexture, _renderTexture, _material);
    }

    private void OnDisable()
    {
        if (_material is not null)
        {
            DestroyImmediate(_material);
        }
    }

    private void OnWizardCreate()
    {
        if (_renderTexture is null)
        {
            CreateRT();
        }
        var format = textureType == TextureType.EXR ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
        var texture2D = new Texture2D((int)resolution.x, (int)resolution.y, format, false);
        Graphics.SetRenderTarget(_renderTexture);
        texture2D.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        texture2D.Apply();
        if (Directory.Exists(path) is false)
        {
            Directory.CreateDirectory(path);
        }

        var pathType = textureType == TextureType.EXR ? ".exr" : ".png";
        var outputPath = path + "/" + outputName + pathType;
        var bytes = textureType == TextureType.EXR ? texture2D.EncodeToEXR() : texture2D.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(outputPath);
        DestroyImmediate(texture2D);
        _renderTexture = null;
        var importer = (TextureImporter)AssetImporter.GetAtPath(outputPath);
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private void OnWizardUpdate()
    {
        helpString = "Select 6 sided textures to save to individual .png";
        if (Selection.activeObject is Texture2D && textures is null)
        {
            textures = new List<Texture2D> { Selection.activeObject as Texture2D };
        }

        isValid = textures != null;
    }

    private Texture2D IsReadTexture(Texture texture2D)
    {
        var readWrite = textureType == TextureType.EXR ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Default;
        var rtFormat = textureType == TextureType.EXR ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32;
        var temp = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0, rtFormat, readWrite);
        Graphics.Blit(texture2D, temp);
        var previous = RenderTexture.active;
        RenderTexture.active = temp;
        var format = textureType == TextureType.EXR ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
        var myTexture2D = new Texture2D(texture2D.width, texture2D.height, format, false);
        myTexture2D.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
        myTexture2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);
        return myTexture2D;
    }

    private static void FlipPixels(Texture2D texture, bool flipX, bool flipY)
    {
        var originalPixels = texture.GetPixels32();

        var flippedPixels = Enumerable.Range(0, texture.width * texture.height).Select(index =>
            {
                var x = index % texture.width;
                var y = index / texture.width;
                if (flipX)
                    x = texture.width - 1 - x;

                if (flipY)
                    y = texture.height - 1 - y;

                return originalPixels[y * texture.width + x];
            }
        );

        texture.SetPixels32(flippedPixels.ToArray());
        texture.Apply();
    }
}