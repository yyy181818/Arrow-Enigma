using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetReferenceTreeModel : AssetTreeModel
    {
        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            AssetPaths = refPathStr;

            var resFileList = GetResFileList();
            var guidList = GetGuidFromFileList(resFileList);
            var fileList = GetRefFileList();
            var searchRetList = GetSearchResultList(fileList.Count, guidList.Count);

            ThreadDoFilesTextSearchReplace(fileList, guidList, string.Empty, searchRetList);
            var rootInfo = DirToAssetInfoTree(RefPaths);
            var refInfos = FileListToAssetInfos(fileList);

            // 隶属信息
            var spritePackingDict = new Dictionary<string, string>();
            if (spritePackingDict == null) throw new ArgumentNullException(nameof(spritePackingDict));

            // 根据搜索结果来挂载额外信息
            for (var i = 0; i < fileList.Count; i++)
            {
                for (var j = 0; j < guidList.Count; j++)
                {
                    if (!searchRetList[i][j]) continue;
                    var info = GenAssetInfo(resFileList[j]);
                    info.IsExtra = true;
                    refInfos[i].AddChild(info);

                    // 隶属信息
                    if (!spritePackingDict.TryGetValue(info.FileRelativePath, out var val))
                    {
                        var assetImporter = AssetImporter.GetAtPath(info.FileRelativePath);
                        var textureImporter = assetImporter as TextureImporter;
                        if (textureImporter)
                        {
                            val = textureImporter.spritePackingTag;
                        }
                    }

                    info.BindObj = val;
                }
            }
            MergeAssetInfo(rootInfo, refInfos);

            if (rootInfo.HasChildren)
            {
                Data = rootInfo;
            }
            EditorUtility.ClearProgressBar();
        }

        public override void ExportCsv()
        {
            var path = AssetDanshariUtility.GetSaveFilePath(nameof(AssetDependenciesWindow));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}\",", style.nameHeaderContent.text);
            sb.AppendFormat("\"{0}\"\n", style.dependenciesHeaderContent2.text);

            foreach (var info in Data.Children)
            {
                ExportCsvDataDir(info, sb, "├");
            }

            AssetDanshariUtility.SaveFileText(path, sb.ToString());
            GUIUtility.ExitGUI();
        }

        private static void ExportCsvDataDir(AssetInfo assetInfo, StringBuilder sb, string pre)
        {
            if (assetInfo.IsExtra)
            {
                sb.AppendFormat(",\"{0}\"\n", assetInfo.DisplayName);
            }
            else if (assetInfo.IsFolder)
            {
                sb.AppendLine(pre + assetInfo.DisplayName);
            }
            else
            {
                sb.AppendFormat("\"{0}\"", pre + assetInfo.DisplayName);
                if (assetInfo.HasChildren && assetInfo.Children.Count > 0)
                {
                    sb.AppendFormat(",\"{0}\"", assetInfo.Children.Count.ToString());
                }
                sb.AppendLine();
            }

            if (!assetInfo.HasChildren) return;
            foreach (var childInfo in assetInfo.Children)
            {
                ExportCsvDataDir(childInfo, sb, pre + pre);
            }
        }
    }
}