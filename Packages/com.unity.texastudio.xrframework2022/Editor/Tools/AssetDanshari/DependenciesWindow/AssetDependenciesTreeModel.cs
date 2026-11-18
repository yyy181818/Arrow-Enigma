using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDependenciesTreeModel : AssetTreeModel
    {
        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);

            var resFileList = GetResFileList();
            var guidList = GetGuidFromFileList(resFileList);
            var fileList = GetRefFileList();
            var searchRetList = GetSearchResultList(fileList.Count, guidList.Count);

            ThreadDoFilesTextSearchReplace(fileList, guidList, string.Empty, searchRetList);
            var rootInfo = DirToAssetInfoTree(ResPaths);
            var resInfos = FileListToAssetInfos(resFileList);

            // 根据搜索结果来挂载额外信息
            for (var i = 0; i < fileList.Count; i++)
            {
                for (var j = 0; j < guidList.Count; j++)
                {
                    if (!searchRetList[i][j]) continue;
                    var info = GenAssetInfo(fileList[i]);
                    info.IsExtra = true;
                    resInfos[j].AddChild(info);
                }
            }

            if (AssetDanshariHandler.OnDependenciesLoadDataMore != null)
            {
                AssetDanshariHandler.OnDependenciesLoadDataMore(pathStr, resInfos, this);
            }

            MergeAssetInfo(rootInfo, resInfos);

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