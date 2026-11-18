using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace UnityGuidRegenerator
{
    public class UnityGuidRegeneratorMenu
    {
        [MenuItem("Tools/资源相关/重新生成资源GUID")]
        public static void RegenerateGuids()
        {
            if (EditorUtility.DisplayDialog("GUID重新生成工具",
                $"即将保存并重新生成所选文件夹内的GUID引用，文件夹内的互相引用关系不会丢失，同时会保持对外部的引用  \n\n所选文件夹：{UnityGuidRegenerator.GetCurrentAssetDirectory()} \n\n*这将破坏外部对选中资源的引用， 过程不可逆，请自行备份！",
                "执行", "取消"))
            {
                try
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.StartAssetEditing();
                    string path = Path.GetFullPath(".") + Path.DirectorySeparatorChar + UnityGuidRegenerator.GetCurrentAssetDirectory();//要更新guid的文件夹
                    UnityGuidRegenerator regenerator = new UnityGuidRegenerator(path);
                    regenerator.RegenerateGuids();
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }
            }
        }
    }


    internal class UnityGuidRegenerator
    {
        private static readonly string[] kDefaultFileExtensions = {
            // "*.meta",
            // "*.mat",
            // "*.anim",
            // "*.prefab",
            // "*.unity",
            // "*.asset"
            "*.*"
        };

        private readonly string _assetsPath;

        public static string GetCurrentAssetDirectory()
        {
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath((UnityEngine.Object)obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (System.IO.Directory.Exists(path))
                    return path;
                else if (System.IO.File.Exists(path))
                    return System.IO.Path.GetDirectoryName(path);
            }
            return "Assets/未选择文件夹";
        }
        public UnityGuidRegenerator(string assetsPath)
        {
            _assetsPath = assetsPath;
        }

        public void RegenerateGuids(string[] regeneratedExtensions = null)
        {
            if (regeneratedExtensions == null)
            {
                regeneratedExtensions = kDefaultFileExtensions;
            }

            // 获取文件列表
            List<string> filesPaths = new List<string>();
            foreach (string extension in regeneratedExtensions)
            {
                filesPaths.AddRange(
                    Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                    );
            }

            // 存储旧-新对应关系
            Dictionary<string, string> guidOldToNewMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> guidsInFileMap = new Dictionary<string, List<string>>();

            // 只能替换资产中存在的资源的 GUID
            // 否则，内置资源（着色器、网格等）将被覆盖
            HashSet<string> ownGuids = new HashSet<string>();

            // 遍历所有文件，记住哪些 GUID 位于哪些文件中，并生成新的 GUID
            int counter = 0;
            foreach (string filePath in filesPaths)
            {
                EditorUtility.DisplayProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath), counter / (float)filesPaths.Count);
                string contents = File.ReadAllText(filePath);

                IEnumerable<string> guids = GetGuids(contents);
                bool isFirstGuid = true;
                foreach (string oldGuid in guids)
                {
                    // .meta 文件中的第一个 GUID 始终是资产本身的 GUID
                    if (isFirstGuid && Path.GetExtension(filePath) == ".meta")
                    {
                        ownGuids.Add(oldGuid);
                        isFirstGuid = false;
                    }
                    // 生成并保存新的 GUID（如果之前未添加）
                    if (!guidOldToNewMap.ContainsKey(oldGuid))
                    {
                        string newGuid = Guid.NewGuid().ToString("N");
                        guidOldToNewMap.Add(oldGuid, newGuid);
                    }

                    if (!guidsInFileMap.ContainsKey(filePath))
                        guidsInFileMap[filePath] = new List<string>();

                    if (!guidsInFileMap[filePath].Contains(oldGuid))
                    {
                        guidsInFileMap[filePath].Add(oldGuid);
                    }
                }

                counter++;
            }

            // 再次遍历文件并替换旧的 GUID
            counter = -1;
            int guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
            foreach (string filePath in guidsInFileMap.Keys)
            {
                EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath), counter / (float)guidsInFileMapKeysCount);
                counter++;

                string contents = File.ReadAllText(filePath);
                foreach (string oldGuid in guidsInFileMap[filePath])
                {
                    if (!ownGuids.Contains(oldGuid))
                        continue;

                    string newGuid = guidOldToNewMap[oldGuid];
                    if (string.IsNullOrEmpty(newGuid))
                        throw new NullReferenceException("newGuid == null");

                    contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
                }
                File.WriteAllText(filePath, contents);
            }

            EditorUtility.ClearProgressBar();
        }

        private static IEnumerable<string> GetGuids(string text)
        {
            const string guidStart = "guid: ";
            const int guidLength = 32;
            int textLength = text.Length;
            int guidStartLength = guidStart.Length;
            List<string> guids = new List<string>();

            int index = 0;
            while (index + guidStartLength + guidLength < textLength)
            {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                string guid = text.Substring(index, guidLength);
                index += guidLength;

                if (IsGuid(guid))
                {
                    guids.Add(guid);
                }
            }

            return guids;
        }

        private static bool IsGuid(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (
                    !((c >= '0' && c <= '9') ||
                      (c >= 'a' && c <= 'z'))
                    )
                    return false;
            }

            return true;
        }

        private static string MakeRelativePath(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }
    }
}