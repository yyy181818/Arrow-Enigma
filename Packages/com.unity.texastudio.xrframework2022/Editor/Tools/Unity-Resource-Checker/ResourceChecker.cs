// Resource Checker
// (c) 2012 Simon Oliver / HandCircus / hello@handcircus.com
// (c) 2015 Brice Clocher / Mangatome / hello@mangatome.net
// Public domain, do with whatever you like, commercial or not
// This comes with no warranty, use at your own risk!
// https://github.com/handcircus/Unity-Resource-Checker

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine.SceneManagement;
using Alias = UnityEngine.GameObject;
using Object = UnityEngine.Object;

public class TextureDetails : IEquatable<TextureDetails>
{
    public readonly bool IsCubeMap;
    public float MemSizeKb;
    public readonly Texture Texture;
    public TextureFormat Format;
    public int MipMapCount;
    public readonly List<Object> FoundInMaterials = new();
    public readonly List<Object> FoundInRenderers = new();
    public readonly List<Object> FoundInAnimators = new();
    public readonly List<Object> FoundInScripts = new();
    public readonly List<Object> FoundInGraphics = new();
    public readonly List<Object> FoundInButtons = new();
    public bool IsSky;
    public bool Instance;
    public bool IsGui;

    public TextureDetails()
    {
    }

    public TextureDetails(Texture texture, bool isCubeMap)
    {
        Texture = texture;
        IsCubeMap = isCubeMap;
    }

    public bool Equals(TextureDetails other)
    {
        return other != null && Texture != null && other.Texture != null &&
               Texture.GetNativeTexturePtr() == other.Texture.GetNativeTexturePtr();
    }

    public override int GetHashCode()
    {
        return (int)Texture.GetNativeTexturePtr();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TextureDetails);
    }
};

public class MaterialDetails
{
    public Material Material;

    public readonly List<Renderer> FoundInRenderers = new();
    public readonly List<Graphic> FoundInGraphics = new();
    public bool Instance;
    public bool IsGui;
    public bool IsSky;
};

public class MeshDetails
{
    public Mesh Mesh;

    public readonly List<MeshFilter> FoundInMeshFilters = new();
    public readonly List<SkinnedMeshRenderer> FoundInSkinnedMeshRenderer = new();
    public readonly List<Alias> StaticBatchingEnabled = new();
    public bool Instance;
};

public class MissingGraphic
{
    public Transform Object;
    public string Type;
    public string Name;
}

public class ResourceChecker : EditorWindow
{
    private readonly string[] _inspectToolbarStrings = { "Textures", "Materials", "Meshes" };
    private readonly string[] _inspectToolbarStrings2 = { "Textures", "Materials", "Meshes", "Missing" };

    private enum InspectType
    {
        Textures,
        Materials,
        Meshes,
        Missing
    };

    private bool _includeDisabledObjects = true;
    private bool _includeSpriteAnimations = true;
    private bool _includeScriptReferences = true;
    private bool _includeGuiElements = true;
    private bool _includeLightmapTextures = true;
    private bool _thingsMissing;

    private InspectType _activeInspectType = InspectType.Textures;

    private const float ThumbnailWidth = 40;
    private const float ThumbnailHeight = 40;

    private List<TextureDetails> _activeTextures = new();
    private readonly List<MaterialDetails> _activeMaterials = new();
    private readonly List<MeshDetails> _activeMeshDetails = new();
    private readonly List<MissingGraphic> _missingObjects = new();

    private Vector2 _textureListScrollPos = new(0, 0);
    private Vector2 _materialListScrollPos = new(0, 0);
    private Vector2 _meshListScrollPos = new(0, 0);
    private Vector2 _missingListScrollPos = new(0, 0);

    private float _totalTextureMemory;
    private int _totalMeshVertices;

    private bool _ctrlPressed;

    private const int MinWidth = 475;
    private Color _defColor;

    private bool _collectedInPlayingMode;

    [MenuItem("Tools/资源相关/Resource Checker")]
    private static void Init()
    {
        var window = (ResourceChecker)GetWindow(typeof(ResourceChecker));
        window.CheckResources();
        window.minSize = new Vector2(MinWidth, 475);
    }

    private void OnGUI()
    {
        _defColor = GUI.color;
        _includeDisabledObjects =
            GUILayout.Toggle(_includeDisabledObjects, "Include disabled objects", GUILayout.Width(300));
        _includeSpriteAnimations =
            GUILayout.Toggle(_includeSpriteAnimations, "Look in sprite animations", GUILayout.Width(300));
        GUI.color = new Color(0.8f, 0.8f, 1.0f, 1.0f);
        _includeScriptReferences =
            GUILayout.Toggle(_includeScriptReferences, "Look in behavior fields", GUILayout.Width(300));
        GUI.color = new Color(1.0f, 0.95f, 0.8f, 1.0f);
        _includeGuiElements = GUILayout.Toggle(_includeGuiElements, "Look in GUI elements", GUILayout.Width(300));
        _includeLightmapTextures =
            GUILayout.Toggle(_includeLightmapTextures, "Look in Lightmap textures", GUILayout.Width(300));
        GUI.color = _defColor;
        GUILayout.BeginArea(new Rect(position.width - 85, 5, 100, 65));
        if (GUILayout.Button("Calculate", GUILayout.Width(80), GUILayout.Height(40)))
            CheckResources();
        if (GUILayout.Button("CleanUp", GUILayout.Width(80), GUILayout.Height(20)))
            Resources.UnloadUnusedAssets();
        GUILayout.EndArea();
        RemoveDestroyedResources();

        GUILayout.Space(30);
        if (_thingsMissing)
        {
            EditorGUI.HelpBox(new Rect(8, 93, 300, 25), "Some GameObjects are missing graphical elements.",
                MessageType.Error);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Textures " + _activeTextures.Count + " - " + FormatSizeString(_totalTextureMemory));
        GUILayout.Label("Materials " + _activeMaterials.Count);
        GUILayout.Label("Meshes " + _activeMeshDetails.Count + " - " + _totalMeshVertices + " verts");
        GUILayout.EndHorizontal();
        if (_thingsMissing)
        {
            _activeInspectType = (InspectType)GUILayout.Toolbar((int)_activeInspectType, _inspectToolbarStrings2);
        }
        else
        {
            _activeInspectType = (InspectType)GUILayout.Toolbar((int)_activeInspectType, _inspectToolbarStrings);
        }

        _ctrlPressed = Event.current.control || Event.current.command;

        switch (_activeInspectType)
        {
            case InspectType.Textures:
                ListTextures();
                break;
            case InspectType.Materials:
                ListMaterials();
                break;
            case InspectType.Meshes:
                ListMeshes();
                break;
            case InspectType.Missing:
                ListMissing();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RemoveDestroyedResources()
    {
        if (_collectedInPlayingMode != Application.isPlaying)
        {
            _activeTextures.Clear();
            _activeMaterials.Clear();
            _activeMeshDetails.Clear();
            _missingObjects.Clear();
            _thingsMissing = false;
            _collectedInPlayingMode = Application.isPlaying;
        }

        _activeTextures.RemoveAll(x => !x.Texture);
        _activeTextures.ForEach(delegate(TextureDetails obj)
        {
            obj.FoundInAnimators.RemoveAll(x => !x);
            obj.FoundInMaterials.RemoveAll(x => !x);
            obj.FoundInRenderers.RemoveAll(x => !x);
            obj.FoundInScripts.RemoveAll(x => !x);
            obj.FoundInGraphics.RemoveAll(x => !x);
        });

        _activeMaterials.RemoveAll(x => !x.Material);
        _activeMaterials.ForEach(delegate(MaterialDetails obj)
        {
            obj.FoundInRenderers.RemoveAll(x => !x);
            obj.FoundInGraphics.RemoveAll(x => !x);
        });

        _activeMeshDetails.RemoveAll(x => !x.Mesh);
        _activeMeshDetails.ForEach(delegate(MeshDetails obj)
        {
            obj.FoundInMeshFilters.RemoveAll(x => !x);
            obj.FoundInSkinnedMeshRenderer.RemoveAll(x => !x);
            obj.StaticBatchingEnabled.RemoveAll(x => !x);
        });

        _totalTextureMemory = 0;
        foreach (var tTextureDetails in _activeTextures) _totalTextureMemory += tTextureDetails.MemSizeKb;

        _totalMeshVertices = 0;
        foreach (var tMeshDetails in _activeMeshDetails) _totalMeshVertices += tMeshDetails.Mesh.vertexCount;
    }

    private static float GetBitsPerPixel(TextureFormat format)
    {
        return format switch
        {
            TextureFormat.Alpha8 => 8, //	 Alpha-only texture format.

            TextureFormat.ARGB4444 => 16, //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.

            TextureFormat.RGBA4444 => 16, //	 A 16 bits/pixel texture format.

            TextureFormat.RGB24 => 24, // A color texture format.

            TextureFormat.RGBA32 => 32, //Color with an alpha channel texture format.

            TextureFormat.ARGB32 => 32, //Color with an alpha channel texture format.

            TextureFormat.RGB565 => 16, //	 A 16 bit color texture format.

            TextureFormat.DXT1 => 4, // Compressed color texture format.

            TextureFormat.DXT5 => 8, // Compressed color with alpha channel texture format.
            
            TextureFormat.PVRTC_RGB2 => 2, //	 PowerVR (iOS) 2 bits/pixel compressed color texture format.

            TextureFormat.PVRTC_RGBA2 => 2, //	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format

            TextureFormat.PVRTC_RGB4 => 4, //	 PowerVR (iOS) 4 bits/pixel compressed color texture format.

            TextureFormat.PVRTC_RGBA4 => 4, //	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format

            TextureFormat.ETC_RGB4 => 4, //	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.

            TextureFormat.ETC2_RGBA8 => 8,
            
            TextureFormat.ASTC_4x4 => 8,
            
            TextureFormat.ASTC_6x6 => 3.625f,
            
            TextureFormat.ASTC_8x8 => 2,
            
            TextureFormat.EAC_R => 4,
            
            TextureFormat.BGRA32 => 32, //	 Format returned by iPhone camera
            
            TextureFormat.RGBA64 => 64, //	 HDR RGB texture format, 16 bit floating point per channel
            
            TextureFormat.R8 => 8, //	 One channel texture format, 8 bit integer per channel
            
            TextureFormat.R16 => 16, //	 One channel HDR texture format, 16 bit float per channel
            
            TextureFormat.BC5 => 8, //	 BC5 compressed format for 3D textures and cubemaps
            
            TextureFormat.BC7 => 8, //	 BC7 compressed format for 3D textures and cubemaps
            _ => 0
        };
    }

    private static float CalculateTextureSizeBytes(Texture tTexture)
    {
        var tWidth = tTexture.width;
        var tHeight = tTexture.height;
        switch (tTexture)
        {
            case Texture2D tTex2D:
            {
                var bitsPerPixel = GetBitsPerPixel(tTex2D.format);
                var mipMapCount = tTex2D.mipmapCount;
                var mipLevel = 1;
                float tSize = 0;
                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth /= 2;
                    tHeight /= 2;
                    mipLevel++;
                }

                return tSize;
            }
            case Texture2DArray array:
            {
                var bitsPerPixel = GetBitsPerPixel(array.format);
                const int mipMapCount = 10;
                var mipLevel = 1;
                float tSize = 0;
                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth /= 2;
                    tHeight /= 2;
                    mipLevel++;
                }

                return tSize * array.depth;
            }
        }

        if (tTexture is not Cubemap cubeMap) return 0;
        {
            var bitsPerPixel = GetBitsPerPixel(cubeMap.format);
            return tWidth * tHeight * 6 * bitsPerPixel / 8;
        }
    }


    private static void SelectObject(Object selectedObject, bool append)
    {
        if (append)
        {
            var currentSelection = new List<Object>(Selection.objects);
            // Allow toggle selection
            if (currentSelection.Contains(selectedObject)) currentSelection.Remove(selectedObject);
            else currentSelection.Add(selectedObject);

            Selection.objects = currentSelection.ToArray();
        }
        else Selection.activeObject = selectedObject;
    }

    private static void SelectObjects(List<Object> selectedObjects, bool append)
    {
        if (append)
        {
            var currentSelection = new List<Object>(Selection.objects);
            currentSelection.AddRange(selectedObjects);
            Selection.objects = currentSelection.ToArray();
        }
        else Selection.objects = selectedObjects.ToArray();
    }

    private void ListTextures()
    {
        _textureListScrollPos = EditorGUILayout.BeginScrollView(_textureListScrollPos);

        foreach (var tDetails in _activeTextures)
        {
            GUILayout.BeginHorizontal();
            var tex = tDetails.Texture;
            if (tDetails.Texture is Texture2DArray or Cubemap)
            {
                tex = AssetPreview.GetMiniThumbnail(tDetails.Texture);
            }

            GUILayout.Box(tex, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));

            if (tDetails.Instance)
                GUI.color = new Color(0.8f, 0.8f, _defColor.b, 1.0f);
            if (tDetails.IsGui)
                GUI.color = new Color(_defColor.r, 0.95f, 0.8f, 1.0f);
            if (tDetails.IsSky)
                GUI.color = new Color(0.9f, _defColor.g, _defColor.b, 1.0f);
            if (GUILayout.Button(tDetails.Texture.name, GUILayout.Width(150)))
            {
                SelectObject(tDetails.Texture, _ctrlPressed);
            }

            GUI.color = _defColor;

            var sizeLabel = "" + tDetails.Texture.width + "x" + tDetails.Texture.height;
            if (tDetails.IsCubeMap) sizeLabel += "x6";
            if (tDetails.Texture is Texture2DArray array)
                sizeLabel += "[]\n" + array.depth + "depths";
            sizeLabel += " - " + tDetails.MipMapCount + "mip\n" + FormatSizeString(tDetails.MemSizeKb) + " - " +
                         tDetails.Format;

            GUILayout.Label(sizeLabel, GUILayout.Width(120));

            if (GUILayout.Button(tDetails.FoundInMaterials.Count + " Mat", GUILayout.Width(50)))
            {
                SelectObjects(tDetails.FoundInMaterials, _ctrlPressed);
            }

            var foundObjects = new HashSet<Object>();
            foreach (var renderer in tDetails.FoundInRenderers.Cast<Renderer>())
            {
                foundObjects.Add(renderer.gameObject);
            }

            foreach (var animator in tDetails.FoundInAnimators.Cast<Animator>())
            {
                foundObjects.Add(animator.gameObject);
            }

            foreach (var graphic in tDetails.FoundInGraphics.Cast<Graphic>())
            {
                foundObjects.Add(graphic.gameObject);
            }

            foreach (var button in tDetails.FoundInButtons.Cast<Button>())
            {
                foundObjects.Add(button.gameObject);
            }

            foreach (var script in tDetails.FoundInScripts.Cast<MonoBehaviour>())
            {
                foundObjects.Add(script.gameObject);
            }

            if (GUILayout.Button(foundObjects.Count + " GO", GUILayout.Width(50)))
            {
                SelectObjects(new List<Object>(foundObjects), _ctrlPressed);
            }

            GUILayout.EndHorizontal();
        }

        if (_activeTextures.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            //GUILayout.Box(" ",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
            if (GUILayout.Button("Select \n All", GUILayout.Width(ThumbnailWidth * 2)))
            {
                var allTextures = _activeTextures.Select(tDetails => tDetails.Texture).Cast<Object>().ToList();
                SelectObjects(allTextures, _ctrlPressed);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ListMaterials()
    {
        _materialListScrollPos = EditorGUILayout.BeginScrollView(_materialListScrollPos);

        foreach (var tDetails in _activeMaterials.Where(tDetails => tDetails.Material is not null))
        {
            GUILayout.BeginHorizontal();

            GUILayout.Box(AssetPreview.GetAssetPreview(tDetails.Material), GUILayout.Width(ThumbnailWidth),
                GUILayout.Height(ThumbnailHeight));

            if (tDetails.Instance)
                GUI.color = new Color(0.8f, 0.8f, _defColor.b, 1.0f);
            if (tDetails.IsGui)
                GUI.color = new Color(_defColor.r, 0.95f, 0.8f, 1.0f);
            if (tDetails.IsSky)
                GUI.color = new Color(0.9f, _defColor.g, _defColor.b, 1.0f);
            if (GUILayout.Button(tDetails.Material.name, GUILayout.Width(150)))
            {
                SelectObject(tDetails.Material, _ctrlPressed);
            }

            GUI.color = _defColor;

            var shaderLabel = tDetails.Material.shader is not null ? tDetails.Material.shader.name : "no shader";
            GUILayout.Label(shaderLabel, GUILayout.Width(200));

            if (GUILayout.Button((tDetails.FoundInRenderers.Count + tDetails.FoundInGraphics.Count) + " GO",
                    GUILayout.Width(50)))
            {
                var foundObjects = tDetails.FoundInRenderers.Select(renderer => renderer.gameObject).Cast<Object>()
                    .ToList();
                foundObjects.AddRange(tDetails.FoundInGraphics.Select(graphic => graphic.gameObject));
                SelectObjects(foundObjects, _ctrlPressed);
            }

            var queue = tDetails.Material.renderQueue;
            EditorGUI.BeginChangeCheck();
            queue = EditorGUILayout.DelayedIntField(queue, GUILayout.Width(35));
            if (EditorGUI.EndChangeCheck())
            {
                tDetails.Material.renderQueue = queue;
                _activeMaterials.Sort(MaterialSorter);
                GUIUtility.ExitGUI();
                break;
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Sort by RenderQueue
    /// </summary>
    private static int MaterialSorter(MaterialDetails first, MaterialDetails second)
    {
        var firstIsNull = first.Material is null;
        var secondIsNull = second.Material is null;

        switch (firstIsNull)
        {
            case true when secondIsNull:
                return 0;
            case true:
                return int.MaxValue;
        }

        if (secondIsNull) return int.MinValue;

        return first.Material.renderQueue - second.Material.renderQueue;
    }

    private void ListMeshes()
    {
        _meshListScrollPos = EditorGUILayout.BeginScrollView(_meshListScrollPos);

        foreach (var tDetails in _activeMeshDetails.Where(tDetails => tDetails.Mesh is not null))
        {
            GUILayout.BeginHorizontal();
            var meshName = tDetails.Mesh.name;
            if (!meshName.Any())
                meshName = tDetails.FoundInMeshFilters[0].gameObject.name;
            if (tDetails.Instance)
                GUI.color = new Color(0.8f, 0.8f, _defColor.b, 1.0f);
            if (GUILayout.Button(meshName, GUILayout.Width(150)))
            {
                SelectObject(tDetails.Mesh, _ctrlPressed);
            }

            GUI.color = _defColor;
            var sizeLabel = "" + tDetails.Mesh.vertexCount + " vert";

            GUILayout.Label(sizeLabel, GUILayout.Width(100));


            if (GUILayout.Button(tDetails.FoundInMeshFilters.Count + " GO", GUILayout.Width(50)))
            {
                var foundObjects = tDetails.FoundInMeshFilters.Select(meshFilter => meshFilter.gameObject)
                    .Cast<Object>().ToList();
                SelectObjects(foundObjects, _ctrlPressed);
            }

            if (tDetails.FoundInSkinnedMeshRenderer.Count > 0)
            {
                if (GUILayout.Button(tDetails.FoundInSkinnedMeshRenderer.Count + " skinned mesh GO",
                        GUILayout.Width(140)))
                {
                    var foundObjects = tDetails.FoundInSkinnedMeshRenderer
                        .Select(skinnedMeshRenderer => skinnedMeshRenderer.gameObject).Cast<Object>().ToList();
                    SelectObjects(foundObjects, _ctrlPressed);
                }
            }
            else
            {
                GUI.color = new Color(_defColor.r, _defColor.g, _defColor.b, 0.5f);
                GUILayout.Label("   0 skinned mesh");
                GUI.color = _defColor;
            }

            if (tDetails.StaticBatchingEnabled.Count > 0)
            {
                if (GUILayout.Button(tDetails.StaticBatchingEnabled.Count + " Static Batching",
                        GUILayout.Width(140)))
                {
                    var foundObjects = tDetails.StaticBatchingEnabled.Cast<Object>().ToList();
                    SelectObjects(foundObjects, _ctrlPressed);
                }
            }
            else
            {
                GUI.color = new Color(_defColor.r, _defColor.g, _defColor.b, 0.5f);
                GUILayout.Label("   0 static batching");
                GUI.color = _defColor;
            }


            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ListMissing()
    {
        _missingListScrollPos = EditorGUILayout.BeginScrollView(_missingListScrollPos);
        foreach (var dMissing in _missingObjects)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(dMissing.Name, GUILayout.Width(150)))
                SelectObject(dMissing.Object, _ctrlPressed);
            GUILayout.Label("missing ", GUILayout.Width(48));
            GUI.color = dMissing.Type switch
            {
                "mesh" => new Color(0.8f, 0.8f, _defColor.b, 1.0f),
                "sprite" => new Color(_defColor.r, 0.8f, 0.8f, 1.0f),
                "material" => new Color(0.8f, _defColor.g, 0.8f, 1.0f),
                _ => GUI.color
            };

            GUILayout.Label(dMissing.Type);
            GUI.color = _defColor;
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private static string FormatSizeString(float memSizeKb)
    {
        if (memSizeKb < 1024) return "" + memSizeKb.ToString("f1") + "k";
        var memSizeMb = memSizeKb / 1024.0f;
        return memSizeMb.ToString("0.00") + "Mb";
    }


    private TextureDetails FindTextureDetails(Object tTexture)
    {
        return _activeTextures.FirstOrDefault(tTextureDetails => tTextureDetails.Texture == tTexture);
    }

    private MaterialDetails FindMaterialDetails(Object tMaterial)
    {
        return _activeMaterials.FirstOrDefault(tMaterialDetails => tMaterialDetails.Material == tMaterial);
    }

    private MeshDetails FindMeshDetails(Object tMesh)
    {
        return _activeMeshDetails.FirstOrDefault(tMeshDetails => tMeshDetails.Mesh == tMesh);
    }


    private void CheckResources()
    {
        _activeTextures.Clear();
        _activeMaterials.Clear();
        _activeMeshDetails.Clear();
        _missingObjects.Clear();
        _thingsMissing = false;

        var renderers = FindObjects<Renderer>();

        var skyMat = new MaterialDetails
        {
            Material = RenderSettings.skybox,
            IsSky = true
        };
        _activeMaterials.Add(skyMat);

        //Debug.Log("Total renderers "+renderers.Length);
        foreach (var renderer in renderers)
        {
            //Debug.Log("Renderer is "+renderer.name);
            foreach (var material in renderer.sharedMaterials)
            {
                var tMaterialDetails = FindMaterialDetails(material);
                if (tMaterialDetails == null)
                {
                    tMaterialDetails = new MaterialDetails
                    {
                        Material = material
                    };
                    _activeMaterials.Add(tMaterialDetails);
                }

                tMaterialDetails.FoundInRenderers.Add(renderer);
            }

            if (renderer is not SpriteRenderer tSpriteRenderer) continue;
            if (tSpriteRenderer.sprite is not null)
            {
                var tSpriteTextureDetail = GetTextureDetail(tSpriteRenderer.sprite.texture, tSpriteRenderer);
                if (!_activeTextures.Contains(tSpriteTextureDetail))
                {
                    _activeTextures.Add(tSpriteTextureDetail);
                }
            }
            else if (tSpriteRenderer.sprite is null)
            {
                var tMissing = new MissingGraphic();
                var transform = tSpriteRenderer.transform;
                tMissing.Object = transform;
                tMissing.Type = "sprite";
                tMissing.Name = transform.name;
                _missingObjects.Add(tMissing);
                _thingsMissing = true;
            }
        }

        if (_includeLightmapTextures)
        {
            var lightmapTextures = LightmapSettings.lightmaps;

            // Unity lightMaps
            foreach (var lightmapData in lightmapTextures)
            {
                if (lightmapData.lightmapColor is not null)
                {
                    var textureDetail = GetTextureDetail(lightmapData.lightmapColor);

                    if (!_activeTextures.Contains(textureDetail))
                        _activeTextures.Add(textureDetail);
                }

                if (lightmapData.lightmapDir is not null)
                {
                    var textureDetail = GetTextureDetail(lightmapData.lightmapColor);

                    if (!_activeTextures.Contains(textureDetail))
                        _activeTextures.Add(textureDetail);
                }

                if (lightmapData.shadowMask is null) continue;
                {
                    var textureDetail = GetTextureDetail(lightmapData.shadowMask);

                    if (!_activeTextures.Contains(textureDetail))
                        _activeTextures.Add(textureDetail);
                }
            }
        }

        if (_includeGuiElements)
        {
            var graphics = FindObjects<Graphic>();

            foreach (var graphic in graphics)
            {
                if (graphic.mainTexture)
                {
                    var tSpriteTextureDetail = GetTextureDetail(graphic.mainTexture, graphic);
                    if (!_activeTextures.Contains(tSpriteTextureDetail))
                    {
                        _activeTextures.Add(tSpriteTextureDetail);
                    }
                }

                if (!graphic.materialForRendering) continue;
                var tMaterialDetails = FindMaterialDetails(graphic.materialForRendering);
                if (tMaterialDetails == null)
                {
                    tMaterialDetails = new MaterialDetails
                    {
                        Material = graphic.materialForRendering,
                        IsGui = true
                    };
                    _activeMaterials.Add(tMaterialDetails);
                }

                tMaterialDetails.FoundInGraphics.Add(graphic);
            }

            var buttons = FindObjects<Button>();
            foreach (var button in buttons)
            {
                CheckButtonSpriteState(button, button.spriteState.disabledSprite);
                CheckButtonSpriteState(button, button.spriteState.highlightedSprite);
                CheckButtonSpriteState(button, button.spriteState.pressedSprite);
            }
        }

        foreach (var tMaterialDetails in _activeMaterials)
        {
            var tMaterial = tMaterialDetails.Material;
            if (tMaterial is null) continue;
            var dependencies = EditorUtility.CollectDependencies(new Object[] { tMaterial });
            foreach (var obj in dependencies)
            {
                if (obj is not Texture tTexture) continue;
                var tTextureDetail = GetTextureDetail(tTexture, tMaterial, tMaterialDetails);
                tTextureDetail.IsSky = tMaterialDetails.IsSky;
                tTextureDetail.Instance = tMaterialDetails.Instance;
                tTextureDetail.IsGui = tMaterialDetails.IsGui;
                _activeTextures.Add(tTextureDetail);
            }

            //if the texture was downloaded, it won't be included in the editor dependencies
            if (!tMaterial.HasProperty("_MainTex")) continue;
            {
                if (tMaterial.mainTexture is null || dependencies.Contains(tMaterial.mainTexture)) continue;
                var tTextureDetail = GetTextureDetail(tMaterial.mainTexture, tMaterial, tMaterialDetails);
                _activeTextures.Add(tTextureDetail);
            }
        }


        var meshFilters = FindObjects<MeshFilter>();

        foreach (var tMeshFilter in meshFilters)
        {
            var tMesh = tMeshFilter.sharedMesh;
            if (tMesh is not null)
            {
                var tMeshDetails = FindMeshDetails(tMesh);
                if (tMeshDetails == null)
                {
                    tMeshDetails = new MeshDetails
                    {
                        Mesh = tMesh
                    };
                    _activeMeshDetails.Add(tMeshDetails);
                }

                tMeshDetails.FoundInMeshFilters.Add(tMeshFilter);

                if (GameObjectUtility.AreStaticEditorFlagsSet(tMeshFilter.gameObject, StaticEditorFlags.BatchingStatic))
                {
                    tMeshDetails.StaticBatchingEnabled.Add(tMeshFilter.gameObject);
                }
            }
            else if (tMesh is null && tMeshFilter.transform.GetComponent<TextContainer>() is null)
            {
                var tMissing = new MissingGraphic();
                var transform = tMeshFilter.transform;
                tMissing.Object = transform;
                tMissing.Type = "mesh";
                tMissing.Name = transform.name;
                _missingObjects.Add(tMissing);
                _thingsMissing = true;
            }

            var meshRenderer = tMeshFilter.transform.GetComponent<MeshRenderer>();

            if (meshRenderer is not null && meshRenderer.sharedMaterial is not null) continue;
            {
                var transform = tMeshFilter.transform;
                var tMissing = new MissingGraphic
                {
                    Object = transform,
                    Type = "material",
                    Name = transform.name
                };
                _missingObjects.Add(tMissing);
                _thingsMissing = true;
            }
        }

        var skinnedMeshRenderers = FindObjects<SkinnedMeshRenderer>();

        foreach (var tSkinnedMeshRenderer in skinnedMeshRenderers)
        {
            var tMesh = tSkinnedMeshRenderer.sharedMesh;
            if (tMesh is not null)
            {
                var tMeshDetails = FindMeshDetails(tMesh);
                if (tMeshDetails == null)
                {
                    tMeshDetails = new MeshDetails
                    {
                        Mesh = tMesh
                    };
                    _activeMeshDetails.Add(tMeshDetails);
                }

                tMeshDetails.FoundInSkinnedMeshRenderer.Add(tSkinnedMeshRenderer);
            }
            else if (tMesh is null)
            {
                var tMissing = new MissingGraphic();
                var transform = tSkinnedMeshRenderer.transform;
                tMissing.Object = transform;
                tMissing.Type = "mesh";
                tMissing.Name = transform.name;
                _missingObjects.Add(tMissing);
                _thingsMissing = true;
            }

            if (tSkinnedMeshRenderer.sharedMaterial is not null) continue;
            {
                var tMissing = new MissingGraphic();
                var transform = tSkinnedMeshRenderer.transform;
                tMissing.Object = transform;
                tMissing.Type = "material";
                tMissing.Name = transform.name;
                _missingObjects.Add(tMissing);
                _thingsMissing = true;
            }
        }

        if (_includeSpriteAnimations)
        {
            var animators = FindObjects<Animator>();
            foreach (var anim in animators)
            {
#if UNITY_4_6 || UNITY_4_5 || UNITY_4_4 || UNITY_4_3
				UnityEditorInternal.AnimatorController ac =
 anim.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
#elif UNITY_5 || UNITY_5_3_OR_NEWER
                var ac = anim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
#endif

                //Skip animators without layers, this can happen if they don't have an animator controller.
                if (!ac || ac.layers == null || ac.layers.Length == 0)
                    continue;

                for (var x = 0; x < anim.layerCount; x++)
                {
                    var sm = ac.layers[x].stateMachine;
                    var cnt = sm.states.Length;

                    for (var i = 0; i < cnt; i++)
                    {
                        var state = sm.states[i].state;
                        var m = state.motion;
                        switch (m)
                        {
                            case null:
                                continue;
                            case AnimationClip clip:
                            {
                                var ebbs = AnimationUtility.GetObjectReferenceCurveBindings(clip);

                                foreach (var ecb in ebbs)
                                {
                                    if (ecb.propertyName != "m_Sprite") continue;
                                    foreach (var keyframe in AnimationUtility
                                                 .GetObjectReferenceCurve(clip, ecb))
                                    {
                                        var tSprite = keyframe.value as Sprite;
                                        if (tSprite is null) continue;
                                        var tTextureDetail = GetTextureDetail(tSprite.texture, anim);
                                        if (!_activeTextures.Contains(tTextureDetail))
                                        {
                                            _activeTextures.Add(tTextureDetail);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (_includeScriptReferences)
        {
            var scripts = FindObjects<MonoBehaviour>();
            foreach (var script in scripts)
            {
                const BindingFlags
                    flags = BindingFlags.Public |
                            BindingFlags.Instance; // only public non-static fields are bound to by Unity.
                var fields = script.GetType().GetFields(flags);

                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    if (fieldType == typeof(Sprite))
                    {
                        if (field.GetValue(script) is Sprite tSprite)
                        {
                            var tSpriteTextureDetail = GetTextureDetail(tSprite.texture, script);
                            if (!_activeTextures.Contains(tSpriteTextureDetail))
                            {
                                _activeTextures.Add(tSpriteTextureDetail);
                            }
                        }
                    }

                    if (fieldType == typeof(Mesh))
                    {
                        if (field.GetValue(script) is Mesh tMesh)
                        {
                            var tMeshDetails = FindMeshDetails(tMesh);
                            if (tMeshDetails == null)
                            {
                                tMeshDetails = new MeshDetails
                                {
                                    Mesh = tMesh,
                                    Instance = true
                                };
                                _activeMeshDetails.Add(tMeshDetails);
                            }
                        }
                    }

                    if (fieldType != typeof(Material) || field.GetValue(script) is not Material tMaterial) continue;
                    {
                        var tMatDetails = FindMaterialDetails(tMaterial);
                        if (tMatDetails == null)
                        {
                            tMatDetails = new MaterialDetails
                            {
                                Instance = true,
                                Material = tMaterial
                            };
                            if (!_activeMaterials.Contains(tMatDetails))
                                _activeMaterials.Add(tMatDetails);
                        }

                        if (tMaterial.mainTexture)
                        {
                            var tSpriteTextureDetail = GetTextureDetail(tMaterial.mainTexture);
                            if (!_activeTextures.Contains(tSpriteTextureDetail))
                            {
                                _activeTextures.Add(tSpriteTextureDetail);
                            }
                        }

                        var dependencies =
                            EditorUtility.CollectDependencies(new Object[] { tMaterial });
                        foreach (var obj in dependencies)
                        {
                            if (obj is not Texture tTexture) continue;
                            var tTextureDetail = GetTextureDetail(tTexture, tMaterial, tMatDetails);
                            if (!_activeTextures.Contains(tTextureDetail))
                                _activeTextures.Add(tTextureDetail);
                        }
                    }
                }
            }
        }

        _totalTextureMemory = 0;
        foreach (var tTextureDetails in _activeTextures) _totalTextureMemory += tTextureDetails.MemSizeKb;

        _totalMeshVertices = 0;
        foreach (var tMeshDetails in _activeMeshDetails) _totalMeshVertices += tMeshDetails.Mesh.vertexCount;

        // Sort by size, descending
        _activeTextures.Sort((details1, details2) => Convert.ToInt32(details2.MemSizeKb - details1.MemSizeKb));
        _activeTextures = _activeTextures.Distinct().ToList();
        _activeMeshDetails.Sort((details1, details2) => details2.Mesh.vertexCount - details1.Mesh.vertexCount);

        _collectedInPlayingMode = Application.isPlaying;

        // Sort by render queue
        _activeMaterials.Sort(MaterialSorter);
    }

    private void CheckButtonSpriteState(Button button, Sprite sprite)
    {
        if (sprite == null) return;

        var texture = sprite.texture;
        var tButtonTextureDetail = GetTextureDetail(texture, button);
        if (!_activeTextures.Contains(tButtonTextureDetail))
        {
            _activeTextures.Add(tButtonTextureDetail);
        }
    }

    private static IEnumerable<Alias> GetAllRootGameObjects()
    {
#if !UNITY_5 && !UNITY_5_3_OR_NEWER
        return SceneManager.GetActiveScene().GetRootGameObjects().ToArray();
#else
        List<Alias> allGo = new List<Alias>();
        // void OnSceneManagerOnsceneLoaded(Scene sc, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        // {
        //     allGo.AddRange(sc.GetRootGameObjects().ToArray());
        // }
        for (var sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
        {
            allGo.AddRange(SceneManager.GetSceneAt(sceneIdx).GetRootGameObjects()
                .ToArray());
            //SceneManager.sceneLoaded += OnSceneManagerOnsceneLoaded;
        }

        allGo.AddRange(GetDontDestroyOnLoadRoots());
        return allGo.ToArray();
#endif
    }

    private static IEnumerable<Alias> GetDontDestroyOnLoadRoots()
    {
        List<Alias> objs = new List<Alias>();
        if (Application.isPlaying)
        {
            Alias temp = null;
            try
            {
                temp = new Alias();
                DontDestroyOnLoad(temp);
                var dontDestryScene = temp.scene;
                DestroyImmediate(temp);
                temp = null;

                if (dontDestryScene.IsValid())
                {
                    objs = dontDestryScene.GetRootGameObjects().ToList();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            finally
            {
                if (temp is not null)
                    DestroyImmediate(temp);
            }
        }

        return objs;
    }

    private T[] FindObjects<T>() where T : Object
    {
        if (_includeDisabledObjects)
        {
            List<T> meshfilters = new List<T>();
            IEnumerable<Alias> allGo = GetAllRootGameObjects();
            foreach (Alias go in allGo)
            {
                Transform[] tgo = go.GetComponentsInChildren<Transform>(true).ToArray();
                meshfilters.AddRange(from tr in tgo where tr.GetComponent<T>() select tr.GetComponent<T>());
            }

            return meshfilters.ToArray();
        }

        return (T[])FindObjectsOfType(typeof(T));
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Material tMaterial, MaterialDetails tMaterialDetails)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInMaterials.Add(tMaterial);
        foreach (Renderer renderer in tMaterialDetails.FoundInRenderers)
        {
            if (!tTextureDetails.FoundInRenderers.Contains(renderer)) tTextureDetails.FoundInRenderers.Add(renderer);
        }

        foreach (Graphic graphic in tMaterialDetails.FoundInGraphics)
        {
            if (!tTextureDetails.FoundInGraphics.Contains(graphic)) tTextureDetails.FoundInGraphics.Add(graphic);
        }

        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Renderer renderer)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInRenderers.Add(renderer);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Animator animator)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInAnimators.Add(animator);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Graphic graphic)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInGraphics.Add(graphic);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, MonoBehaviour script)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInScripts.Add(script);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Button button)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        if (!tTextureDetails.FoundInButtons.Contains(button))
        {
            tTextureDetails.FoundInButtons.Add(button);
        }

        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture)
    {
        var tTextureDetails = FindTextureDetails(tTexture);
        if (tTextureDetails != null) return tTextureDetails;
        tTextureDetails = new TextureDetails(texture: tTexture, isCubeMap: tTexture is Cubemap);
        var memSize = CalculateTextureSizeBytes(tTexture);

        var tFormat = TextureFormat.RGBA32;
        var tMipMapCount = 1;
        switch (tTexture)
        {
            case Texture2D texture2D:
                tFormat = texture2D.format;
                tMipMapCount = texture2D.mipmapCount;
                break;
            case Cubemap cubeMap:
                tFormat = cubeMap.format;
                memSize = 8 * cubeMap.height * cubeMap.width;
                break;
            case Texture2DArray array:
                tFormat = array.format;
                tMipMapCount = 10;
                break;
        }

        tTextureDetails.MemSizeKb = memSize / 1024;
        tTextureDetails.Format = tFormat;
        tTextureDetails.MipMapCount = tMipMapCount;

        return tTextureDetails;
    }
}