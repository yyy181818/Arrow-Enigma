using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateTreeModel : AssetTreeModel
    {
        public class FileMd5Info
        {
            public string FilePath;
            public string FileLength;
            public string FileTime;
            public long FileSize;
            public string MD5;
        }

        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            var style = AssetDanshariStyle.Get();

            var resFileList = GetResFileList();
            var fileList = GetFileMd5Infos(resFileList);
            if (fileList == null || fileList.Count == 0)
            {
                return;
            }

            var rootInfo = new AssetInfo(GetAutoId(), string.Empty, string.Empty);
            var groups = fileList.GroupBy(info => info.MD5).Where(g => g.Count() > 1);
            foreach (var group in groups)
            {
                var dirInfo = new AssetInfo(GetAutoId(), string.Empty,
                    string.Format(style.duplicateGroup, group.Count()))
                {
                    IsExtra = true
                };
                rootInfo.AddChild(dirInfo);

                foreach (var member in group)
                {
                    dirInfo.AddChild(GetAssetInfoByFileMd5Info(member));
                }
            }

            if (rootInfo.HasChildren)
            {
                Data = rootInfo;
            }

            EditorUtility.ClearProgressBar();
        }

        private List<FileMd5Info> GetFileMd5Infos(List<string> fileArray)
        {
            var style = AssetDanshariStyle.Get();
            var fileList = new List<FileMd5Info>();

            for (var i = 0; i < fileArray.Count;)
            {
                var file = fileArray[i];
                if (string.IsNullOrEmpty(file))
                {
                    i++;
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, file, i * 1f / fileArray.Count);
                try
                {
                    using (var md5 = MD5.Create())
                    {
                        var fileInfo = new FileInfo(file);
                        using (var stream = File.OpenRead(fileInfo.FullName))
                        {
                            FileMd5Info info = new FileMd5Info();
                            info.FilePath = fileInfo.FullName;
                            info.FileSize = fileInfo.Length;
                            info.FileTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                            info.MD5 = BitConverter.ToString(md5.ComputeHash(stream)).ToLower();
                            fileList.Add(info);
                        }
                    }

                    i++;
                }
                catch (Exception e)
                {
                    if (EditorUtility.DisplayDialog(style.errorTitle, file + "\n" + e.Message,
                            style.continueStr, style.cancelStr)) continue;
                    EditorUtility.ClearProgressBar();
                    return null;
                }
            }

            return fileList;
        }

        private AssetInfo GetAssetInfoByFileMd5Info(FileMd5Info fileInfo)
        {
            var info = GenAssetInfo(FullPathToRelative(fileInfo.FilePath));
            info.BindObj = fileInfo;

            fileInfo.FileLength = fileInfo.FileSize switch
            {
                >= 1 << 20 => $"{fileInfo.FileSize / 1024f / 1024f:F} MB",
                >= 1 << 10 => $"{fileInfo.FileSize / 1024f:F} KB",
                _ => $"{fileInfo.FileSize:F} B"
            };

            return info;
        }

        /// <summary>
        /// 去引用到的目录查找所有用到的guid，批量更改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="useInfo"></param>
        public void SetUseThis(AssetInfo group, AssetInfo useInfo)
        {
            var style = AssetDanshariStyle.Get();
            if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.duplicateContextOnlyUseThis.text,
                    style.sureStr, style.cancelStr))
            {
                return;
            }

            var patterns = (from info in @group.Children
                where info != useInfo
                select AssetDatabase.AssetPathToGUID(info.FileRelativePath)).ToList();

            var replaceStr = AssetDatabase.AssetPathToGUID(useInfo.FileRelativePath);
            var fileList = GetRefFileList();

            ThreadDoFilesTextSearchReplace(fileList, patterns, replaceStr, GetSearchResultList(fileList.Count, 0));
            EditorUtility.DisplayProgressBar(style.progressTitle, style.deleteFile, 0.98f);
            SetRemoveAllOther(group, useInfo);
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(string.Empty, style.progressFinish, style.sureStr);
        }

        private void SetRemoveAllOther(AssetInfo group, AssetInfo selectInfo)
        {
            foreach (var info in group.Children.Where(info => info != selectInfo && !info.Deleted)
                         .Where(info => AssetDatabase.DeleteAsset(info.FileRelativePath)))
            {
                info.Deleted = true;
            }
        }

        /// <summary>
        /// 手动添加数据
        /// </summary>
        /// <param name="filePaths"></param>
        public int AddManualData(IEnumerable<string> filePaths)
        {
            var fileList = GetFileMd5Infos(filePaths.ToList());
            if (fileList == null || fileList.Count < 2 || !HasData())
            {
                EditorUtility.ClearProgressBar();
                return 0;
            }

            var style = AssetDanshariStyle.Get();
            var dirInfo = new AssetInfo(GetAutoId(), string.Empty, string.Format(style.duplicateGroup, fileList.Count))
            {
                IsExtra = true
            };
            Data.AddChild(dirInfo);

            foreach (var member in fileList)
            {
                dirInfo.AddChild(GetAssetInfoByFileMd5Info(member));
            }

            EditorUtility.ClearProgressBar();
            return dirInfo.Children[^1].ID;
        }

        public override void ExportCsv()
        {
            var path = AssetDanshariUtility.GetSaveFilePath(nameof(AssetDuplicateWindow));
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}\",", style.duplicateHeaderContent.text);
            sb.AppendFormat("\"{0}\",", style.duplicateHeaderContent2.text);
            sb.AppendFormat("\"{0}\",", style.duplicateHeaderContent3.text);
            sb.AppendFormat("\"{0}\"\n", style.duplicateHeaderContent4.text);

            foreach (var group in Data.Children)
            {
                sb.AppendLine(String.Format(style.duplicateGroup, group.DisplayName));

                foreach (var info in group.Children)
                {
                    sb.AppendFormat("\"{0}\",", info.DisplayName);
                    sb.AppendFormat("\"{0}\",", info.FileRelativePath);

                    if (info.BindObj is not FileMd5Info md5Info) continue;
                    sb.AppendFormat("\"{0}\",", md5Info.FileLength);
                    sb.AppendFormat("\"{0}\"\n", md5Info.FileTime);
                }
            }

            AssetDanshariUtility.SaveFileText(path, sb.ToString());
            GUIUtility.ExitGUI();
        }
    }
}