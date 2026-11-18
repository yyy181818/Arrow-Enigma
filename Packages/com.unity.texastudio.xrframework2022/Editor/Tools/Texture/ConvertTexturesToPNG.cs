using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class ConvertTexturesToPNG : EditorWindow
{
    private const string DUMMY_TEXTURE_PATH = "Assets/Editor/Tools/Texture/convert_dummyy_texturee.png";
    private const bool REMOVE_MATTE_FROM_PSD_BY_DEFAULT = true;

    private readonly GUIContent[] maxTextureSizeStrings =
    {
        new("32"), new("64"), new("128"), new("256"), new("512"),
        new("1024")
    };

    private readonly int[] maxTextureSizeValues = { 32, 64, 128, 256, 512, 1024 };

    private readonly GUIContent rootPathContent =
        new("Root Path:", "Textures inside this folder (recursive) will be converted");

    private readonly GUIContent textureExtensionsContent = new("Textures to Convert:",
        "Only Textures with these extensions will be converted (';' separated)");

    private readonly GUIContent excludedDirectoriesContent = new("Excluded Directories:",
        "Textures inside these directories won't be converted (';' separated)");

    private readonly GUIContent keepOriginalFilesContent = new("Keep Original Files:",
        "If selected, original Texture files won't be deleted after the conversion");

    private readonly GUIContent maxTextureSizeContent = new("Max Texture Size:",
        "Textures larger than this size will be downscaled to this size");

    private readonly GUIContent optiPNGPathContent = new("OptiPNG Path (Optional):",
        "If 'optipng.exe' is selected, it will be used to reduce the image sizes even further (roughly 20%) but the process will take more time");

    private readonly GUIContent optiPNGOptimizationContent = new("OptiPNG Optimization:",
        "Determines how many trials OptiPNG will do to optimize the image sizes. As this value increases, computation time will increase exponentially");

    private readonly GUIContent optiPNGURL = new("...", "http://optipng.sourceforge.net/");
    private readonly GUILayoutOption GL_WIDTH_25 = GUILayout.Width(25f);

    private string rootPath = "";
    private string textureExtensions = ".tga;.psd;.tiff;.tif;.bmp;";
    private string excludedDirectories = "";
    private bool keepOriginalFiles;
    private int maxTextureSize = 512;

    private string optiPNGPath = "";

    private int optiPNGOptimization = 3;

    private Vector2 scrollPos;

    [MenuItem("Tools/纹理相关/Convert Textures to PNG")]
    private static void Init()
    {
        var window = GetWindow<ConvertTexturesToPNG>();
        window.titleContent = new GUIContent("Convert to PNG");
        window.minSize = new Vector2(285f, 160f);
        window.Show();
    }

    private void OnEnable()
    {
        // By default, Root Path points to this project's Assets folder
        if (string.IsNullOrEmpty(rootPath))
            rootPath = Application.dataPath + "/HotBase";

        if (string.IsNullOrEmpty(excludedDirectories))
            excludedDirectories = Application.dataPath.Replace("Assets", "Packages");

        if (string.IsNullOrEmpty(optiPNGPath))
            optiPNGPath = Application.dataPath + "/MetaAR/Editor/Tools/Texture/OptiPNG/optipng.exe";
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        rootPath = PathField(rootPathContent, rootPath, true, "Choose target directory");
        textureExtensions = EditorGUILayout.TextField(textureExtensionsContent, textureExtensions);
        excludedDirectories = EditorGUILayout.TextField(excludedDirectoriesContent, excludedDirectories);
        keepOriginalFiles = EditorGUILayout.Toggle(keepOriginalFilesContent, keepOriginalFiles);
        maxTextureSize = EditorGUILayout.IntPopup(maxTextureSizeContent, maxTextureSize, maxTextureSizeStrings,
            maxTextureSizeValues);
        optiPNGPath = PathField(optiPNGPathContent, optiPNGPath, false, "Choose optipng.exe path", optiPNGURL);
        if (!string.IsNullOrEmpty(optiPNGPath))
        {
            EditorGUI.indentLevel++;
            optiPNGOptimization = EditorGUILayout.IntSlider(optiPNGOptimizationContent, optiPNGOptimization, 2, 7);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Convert Textures to PNG
        if (GUILayout.Button("Convert to PNG!"))
        {
            var startTime = EditorApplication.timeSinceStartup;

            var convertedPaths = new List<string>(128);
            long originalTotalSize = 0L, convertedTotalSize = 0L, convertedTotalSizeOptiPNG = 0L;

            try
            {
                rootPath = rootPath.Trim();
                excludedDirectories = excludedDirectories.Trim();
                textureExtensions = textureExtensions.ToLowerInvariant().Replace(".png", "").Trim();
                optiPNGPath = optiPNGPath.Trim();

                if (rootPath.Length == 0)
                    rootPath = Application.dataPath;

                if (optiPNGPath.Length > 0 && !File.Exists(optiPNGPath))
                    Debug.LogWarning("OptiPNG doesn't exist at path: " + optiPNGPath);

                var paths = FindTexturesToConvert();
                var pathsLengthStr = paths.Length.ToString();
                var progressMultiplier = paths.Length > 0 ? 1f / paths.Length : 1f;

                CreateDummyTexture(); // Dummy Texture is used while reading Textures' pixels

                for (var i = 0; i < paths.Length; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Please wait...",
                            string.Concat("Converting: ", (i + 1).ToString(), "/", pathsLengthStr),
                            (i + 1) * progressMultiplier))
                        throw new Exception("Conversion aborted");

                    var pngFile = Path.ChangeExtension(paths[i], ".png");
                    var pngMetaFile = pngFile + ".meta";
                    var originalMetaFile = paths[i] + ".meta";

                    var isPsdImage = Path.GetExtension(paths[i]).ToLowerInvariant() == ".psd";

                    // Make sure to respect PSD assets' "Remove Matte (PSD)" option
                    if (isPsdImage)
                    {
                        var removeMatte = REMOVE_MATTE_FROM_PSD_BY_DEFAULT;

                        if (File.Exists(originalMetaFile))
                        {
                            const string removeMatteOption = "pSDRemoveMatte: ";

                            var metaContents = File.ReadAllText(originalMetaFile);
                            var removeMatteIndex = metaContents.IndexOf(removeMatteOption);
                            if (removeMatteIndex >= 0)
                                removeMatte = metaContents[removeMatteIndex + removeMatteOption.Length] != '0';
                        }

                        var removeMatteProp =
                            new SerializedObject(AssetImporter.GetAtPath(DUMMY_TEXTURE_PATH)).FindProperty(
                                "m_PSDRemoveMatte");
                        if (removeMatteProp != null && removeMatteProp.boolValue != removeMatte)
                        {
                            removeMatteProp.boolValue = removeMatte;
                            removeMatteProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }

                    // Temporarily copy the image file to Assets folder to create a read-write enabled Texture from it
                    File.Copy(paths[i], DUMMY_TEXTURE_PATH, true);
                    AssetDatabase.ImportAsset(DUMMY_TEXTURE_PATH, ImportAssetOptions.ForceUpdate);

                    // Convert the Texture to PNG and save it
                    var pngBytes = AssetDatabase.LoadAssetAtPath<Texture2D>(DUMMY_TEXTURE_PATH).EncodeToPNG();
                    File.WriteAllBytes(pngFile, pngBytes);
                    originalTotalSize += new FileInfo(paths[i]).Length;
                    convertedTotalSize += new FileInfo(pngFile).Length;

                    // Run OptiPNG to optimize the PNG
                    if (optiPNGPath.Length > 0 && File.Exists(optiPNGPath))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(optiPNGPath)
                            {
                                Arguments = string.Concat("-o ", optiPNGOptimization.ToString(), " \"", pngFile, "\""),
                                CreateNoWindow = true,
                                UseShellExecute = false
                            }).WaitForExit();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }

                        convertedTotalSizeOptiPNG += new FileInfo(pngFile).Length;
                    }

                    // If .meta file exists, copy it to PNG image
                    if (File.Exists(originalMetaFile))
                    {
                        File.Copy(originalMetaFile, pngMetaFile, true);

                        // Try changing original meta file's GUID to avoid collisions with PNG (Credit: https://gist.github.com/ZimM-LostPolygon/7e2f8a3e5a1be183ac19)
                        if (keepOriginalFiles)
                        {
                            var metaContents = File.ReadAllText(originalMetaFile);
                            var guidIndex = metaContents.IndexOf("guid: ");
                            if (guidIndex >= 0)
                            {
                                var guid = metaContents.Substring(guidIndex + 6, 32);
                                var newGuid = Guid.NewGuid().ToString("N");
                                metaContents = metaContents.Replace(guid, newGuid);
                                File.WriteAllText(originalMetaFile, metaContents);
                            }
                        }

                        // Don't show "Remote Matte (PSD)" option for converted Textures
                        if (isPsdImage)
                        {
                            var metaContents = File.ReadAllText(pngMetaFile);
                            var modifiedMeta = false;

                            if (metaContents.Contains("pSDShowRemoveMatteOption: 1"))
                            {
                                metaContents = metaContents.Replace("pSDShowRemoveMatteOption: 1",
                                    "pSDShowRemoveMatteOption: 0");
                                modifiedMeta = true;
                            }

                            if (metaContents.Contains("pSDRemoveMatte: 1"))
                            {
                                metaContents = metaContents.Replace("pSDRemoveMatte: 1", "pSDRemoveMatte: 0");
                                modifiedMeta = true;
                            }

                            if (modifiedMeta)
                                File.WriteAllText(pngMetaFile, metaContents);
                        }
                    }

                    if (!keepOriginalFiles)
                    {
                        File.Delete(paths[i]);

                        if (File.Exists(originalMetaFile))
                            File.Delete(originalMetaFile);
                    }


                    convertedPaths.Add(paths[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (File.Exists(DUMMY_TEXTURE_PATH))
                    AssetDatabase.DeleteAsset(DUMMY_TEXTURE_PATH);

                // Force Unity to import PNG images (otherwise we'd have to minimize Unity and then maximize it)
                AssetDatabase.Refresh();

                // Print information to Console
                var sb = new StringBuilder(100 + convertedPaths.Count * 75);
                sb.Append("Converted ").Append(convertedPaths.Count).Append(" Texture(s) to PNG in ")
                    .Append((EditorApplication.timeSinceStartup - startTime).ToString("F2")).Append(" seconds (")
                    .Append(EditorUtility.FormatBytes(originalTotalSize)).Append(" -> ")
                    .Append(EditorUtility.FormatBytes(convertedTotalSize));

                if (convertedTotalSizeOptiPNG > 0L)
                    sb.Append(" -> ").Append(EditorUtility.FormatBytes(convertedTotalSizeOptiPNG))
                        .Append(" with OptiPNG");

                sb.AppendLine("):");
                foreach (var t in convertedPaths)
                    sb.Append("- ").AppendLine(t);

                Debug.Log(sb.ToString());
            }
        }

        GUILayout.EndScrollView();
    }

    private string PathField(GUIContent label, string path, bool isDirectory, string title,
        GUIContent downloadURL = null)
    {
        GUILayout.BeginHorizontal();
        path = EditorGUILayout.TextField(label, path);
        if (GUILayout.Button("o", GL_WIDTH_25))
        {
            var selectedPath = isDirectory
                ? EditorUtility.OpenFolderPanel(title, "", "")
                : EditorUtility.OpenFilePanel(title, "", "exe");
            if (!string.IsNullOrEmpty(selectedPath))
                path = selectedPath;

            GUIUtility.keyboardControl = 0; // Remove focus from active text field
        }

        if (downloadURL != null && GUILayout.Button(downloadURL, GL_WIDTH_25))
            Application.OpenURL(downloadURL.tooltip);
        GUILayout.EndHorizontal();

        return path;
    }

    private string[] FindTexturesToConvert()
    {
        var texturePaths = new HashSet<string>();
        var targetExtensions = new HashSet<string>(textureExtensions.Split(';'));

        // Get directories to exclude
        var excludedPaths = excludedDirectories.Split(';');
        for (var i = 0; i < excludedPaths.Length; i++)
        {
            excludedPaths[i] = excludedPaths[i].Trim();
            if (excludedPaths[i].Length == 0)
                excludedPaths[i] = "NULL/";
            else
            {
                excludedPaths[i] = Path.GetFullPath(excludedPaths[i]);

                // Make sure excluded directory paths end with directory separator char
                if (Directory.Exists(excludedPaths[i]) &&
                    !excludedPaths[i].EndsWith(Path.DirectorySeparatorChar.ToString()))
                    excludedPaths[i] += Path.DirectorySeparatorChar;
            }
        }

        // Iterate through all files in Root Path
        var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
        foreach (var t in allFiles)
        {
            // Only process filtered image files
            if (!targetExtensions.Contains(Path.GetExtension(t).ToLowerInvariant())) continue;
            var isExcluded = false;
            if (excludedPaths.Length > 0)
            {
                // Make sure the image file isn't part of an excluded directory
                var fileFullPath = Path.GetFullPath(t);
                if (excludedPaths.Any(t1 => fileFullPath.StartsWith(t1)))
                {
                    isExcluded = true;
                }
            }

            if (!isExcluded)
                texturePaths.Add(t);
        }

        var result = new string[texturePaths.Count];
        texturePaths.CopyTo(result);

        return result;
    }

    // Creates dummy Texture asset that will be used to read Textures' pixels
    private void CreateDummyTexture()
    {
        if (!File.Exists(DUMMY_TEXTURE_PATH))
        {
            File.WriteAllBytes(DUMMY_TEXTURE_PATH, new Texture2D(2, 2, TextureFormat.RGBA32, false).EncodeToPNG());
            AssetDatabase.ImportAsset(DUMMY_TEXTURE_PATH, ImportAssetOptions.ForceUpdate);
            Debug.Log("Created dummy Texture at: " + DUMMY_TEXTURE_PATH);
        }

        if (AssetImporter.GetAtPath(DUMMY_TEXTURE_PATH) is not TextureImporter textureImporter) return;
        textureImporter.maxTextureSize = maxTextureSize;
        textureImporter.isReadable = true;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.mipmapEnabled = false;
        textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.SaveAndReimport();
    }
}