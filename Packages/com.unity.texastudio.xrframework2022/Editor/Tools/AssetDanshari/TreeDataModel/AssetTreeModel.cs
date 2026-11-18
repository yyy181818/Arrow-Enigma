using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetTreeModel
    {
        public class AssetInfo
        {
            public readonly int ID;
            public string FileRelativePath;
            public string DisplayName;
            public bool IsFolder;
            public bool IsExtra;
            public object BindObj;

            public bool Deleted;
            public bool Added;

            public AssetInfo Parent;
            public List<AssetInfo> Children;

            public AssetInfo(int id, string fileRelativePath, string displayName)
            {
                ID = id;
                FileRelativePath = fileRelativePath;
                DisplayName = displayName;
            }

            public bool HasChildren => Children is { Count: > 0 };

            public void AddChild(AssetInfo info)
            {
                Children ??= new List<AssetInfo>();
                Children.Add(info);
                info.Parent = this;
            }
        }

        /// <summary>
        /// 数据根
        /// </summary>
        public AssetInfo Data { get; protected set; }

        /// <summary>
        /// 右下角的路径
        /// </summary>
        public string AssetPaths { get; protected set; }

        protected string[] RefPaths { get; private set; }
        protected string[] ResPaths { get; private set; }
        private string[] CommonPaths { get; set; }

        /// <summary>
        /// 公共目录
        /// </summary>
        public List<AssetInfo> CommonDirs { get; private set; }

        private int _mDataPathLen;
        private int _mId;

        private void ResetAutoId()
        {
            _mId = 0;
        }

        protected int GetAutoId()
        {
            return _mId++;
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return Data != null;
        }

        public virtual void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            Data = null;
            ResetAutoId();
            AssetPaths = pathStr;
            RefPaths = AssetDanshariUtility.PathStrToArray(refPathStr);
            ResPaths = AssetDanshariUtility.PathStrToArray(pathStr);
            CommonPaths = AssetDanshariUtility.PathStrToArray(commonPathStr);
            _mDataPathLen = Application.dataPath.Length - 6;

            CommonDirs = new List<AssetInfo>();
            foreach (var commonPath in CommonPaths)
            {
                if (!Directory.Exists(commonPath))
                {
                    continue;
                }

                var commonName = Path.GetFileNameWithoutExtension(commonPath);
                var commonLen = commonPath.Length - commonName.Length;
                CommonDirs.Add(new AssetInfo(GetAutoId(), commonPath, commonName));

                var allDirs = Directory.GetDirectories(commonPath, "*", SearchOption.AllDirectories);
                foreach (var allDir in allDirs)
                {
                    var dirInfo = GenAssetInfo(PathToStandardized(allDir));
                    dirInfo.DisplayName = dirInfo.FileRelativePath.Substring(commonLen);
                    CommonDirs.Add(dirInfo);
                }
            }
        }

        public AssetInfo GenAssetInfo(string assetPath)
        {
            var info = new AssetInfo(GetAutoId(), PathToStandardized(assetPath), Path.GetFileName(assetPath));
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                info.IsFolder = true;
            }

            return info;
        }

        public static bool SetMoveToCommon(AssetInfo moveInfo, string destDir)
        {
            var style = AssetDanshariStyle.Get();
            var destPath = $"{destDir}/{moveInfo.DisplayName}";
            if (moveInfo.FileRelativePath == destPath)
            {
                return true;
            }

            var errorStr = AssetDatabase.MoveAsset(moveInfo.FileRelativePath, destPath);
            if (string.IsNullOrEmpty(errorStr)) return true;
            EditorUtility.DisplayDialog(style.errorTitle, errorStr, style.sureStr);
            return false;
        }

        public virtual void ExportCsv()
        {
        }

        public static void PingObject(string fileRelativePath)
        {
            if (string.IsNullOrEmpty(fileRelativePath))
            {
                return;
            }

            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileRelativePath);
            if (!obj) return;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        protected string FullPathToRelative(string path)
        {
            return PathToStandardized(path.Substring(_mDataPathLen));
        }

        private static string PathToStandardized(string path)
        {
            return path.Replace('\\', '/');
        }

        #region 继承的方法

        /// <summary>
        /// 引用目录的文件列表
        /// </summary>
        /// <returns></returns>
        protected List<string> GetRefFileList()
        {
            var fileList = new List<string>();

            foreach (var refPath in RefPaths)
            {
                if (!Directory.Exists(refPath))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(refPath, "*", SearchOption.AllDirectories);

                fileList.AddRange(from file in allFiles
                    where AssetDanshariUtility.IsPlainTextExt(file)
                    select PathToStandardized(file));
            }

            return fileList;
        }

        /// <summary>
        /// 得到资源文件列表
        /// </summary>
        /// <returns></returns>
        protected List<string> GetResFileList()
        {
            var fileList = new List<string>();

            foreach (var resPath in ResPaths)
            {
                if (!Directory.Exists(resPath))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, string.Empty, 0f);
                var allFiles = Directory.GetFiles(resPath, "*", SearchOption.AllDirectories);

                fileList.AddRange(from file in allFiles
                    where !AssetDanshariUtility.IsMetaExt(file)
                    select PathToStandardized(file));
            }

            return fileList;
        }

        /// <summary>
        /// 获取文件列表的GUID
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        protected static List<string> GetGuidFromFileList(IEnumerable<string> fileList)
        {
            return fileList.Select(AssetDatabase.AssetPathToGUID).ToList();
        }

        protected static bool[][] GetSearchResultList(int fileCount, int searchCount)
        {
            var ret = new bool[fileCount][];
            for (var i = 0; i < fileCount; i++)
            {
                ret[i] = new bool[searchCount];
            }

            return ret;
        }

        /// <summary>
        /// 首先构建目录树
        /// </summary>
        protected AssetInfo DirToAssetInfoTree(IEnumerable<string> paths)
        {
            var rooInfo = new AssetInfo(GetAutoId(), string.Empty, string.Empty)
            {
                IsFolder = true
            };

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var info = GenAssetInfo(path);
                rooInfo.AddChild(info);
                DirToAssetInfoTreeSub(info, path);
            }

            return rooInfo;
        }

        private void DirToAssetInfoTreeSub(AssetInfo rootInfo, string path)
        {
            var allDirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var drDir in allDirs)
            {
                var info = GenAssetInfo(drDir);
                rootInfo.AddChild(info);

                DirToAssetInfoTreeSub(info, drDir);
            }
        }

        /// <summary>
        /// 再把文件转成信息列表
        /// </summary>
        protected List<AssetInfo> FileListToAssetInfos(IEnumerable<string> fileList)
        {
            return fileList.Select(GenAssetInfo).ToList();
        }

        /// <summary>
        /// 最后合并文件夹和列表
        /// </summary>
        /// <param name="rootInfo"></param>
        /// <param name="infoList"></param>
        protected void MergeAssetInfo(AssetInfo rootInfo, List<AssetInfo> infoList)
        {
            foreach (var info in infoList)
            {
                var parent = FindAssetInfo(rootInfo, PathToStandardized(Path.GetDirectoryName(info.FileRelativePath)));
                parent?.AddChild(info);
            }
        }

        /// <summary>
        /// 查找
        /// </summary>
        private static AssetInfo FindAssetInfo(AssetInfo searchFromThisItem, string assetPath)
        {
            if (searchFromThisItem == null)
            {
                return null;
            }

            if (searchFromThisItem.FileRelativePath == assetPath)
            {
                return searchFromThisItem;
            }

            if (!searchFromThisItem.HasChildren) return null;
            return searchFromThisItem.Children.Select(child => FindAssetInfo(child, assetPath))
                .FirstOrDefault(item => item != null);
        }

        #endregion

        #region 多线程执行

        private class JobFileTextSearchReplace
        {
            private readonly string _mPath;
            private readonly List<string> _mPatterns;
            private readonly string _mReplaceStr;
            private readonly bool[] _mSearchRet;

            public readonly ManualResetEvent DoneEvent;
            public string Exception;

            public JobFileTextSearchReplace(string path, List<string> patterns, string replaceStr, bool[] searchRet)
            {
                _mPath = path;
                _mPatterns = patterns;
                _mReplaceStr = replaceStr;
                _mSearchRet = searchRet;
                DoneEvent = new ManualResetEvent(false);
            }

            public void ThreadPoolCallback(object threadContext)
            {
                try
                {
                    var text = File.ReadAllText(_mPath);

                    // 搜索
                    if (string.IsNullOrEmpty(_mReplaceStr))
                    {
                        for (var i = 0; i < _mPatterns.Count; i++)
                        {
                            if (text.Contains(_mPatterns[i]))
                            {
                                _mSearchRet[i] = true;
                            }
                        }
                    }
                    else
                    {
                        var sb = new StringBuilder(text, text.Length * 2);
                        foreach (var pattern in _mPatterns)
                        {
                            sb.Replace(pattern, _mReplaceStr);
                        }

                        var text2 = sb.ToString();
                        if (!string.Equals(text, text2))
                        {
                            File.WriteAllText(_mPath, text2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exception = _mPath + "\n" + ex.Message;
                }

                DoneEvent.Set();
            }
        }

        protected static void ThreadDoFilesTextSearchReplace(List<string> fileList, List<string> patterns, string replaceStr,
            bool[][] searchRetList)
        {
            var jobList = new List<JobFileTextSearchReplace>();
            var eventList = new List<ManualResetEvent>();

            var numFiles = fileList.Count;
            var numFinished = 0;
            AssetDanshariUtility.DisplayThreadProgressBar(numFiles, numFinished);

            const int timeout = 600000; // 10 分钟超时

            for (var i = 0; i < fileList.Count; i++)
            {
                var job = new JobFileTextSearchReplace(fileList[i], patterns, replaceStr, searchRetList[i]);
                jobList.Add(job);
                eventList.Add(job.DoneEvent);
                ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);

                if (eventList.Count < Environment.ProcessorCount) continue;
                WaitForDoFile(eventList, timeout);
                AssetDanshariUtility.DisplayThreadProgressBar(numFiles, numFinished);
                numFinished++;
            }

            while (eventList.Count > 0)
            {
                WaitForDoFile(eventList, timeout);
                AssetDanshariUtility.DisplayThreadProgressBar(numFiles, numFinished);
                numFinished++;
            }

            foreach (var job in jobList.Where(job => !string.IsNullOrEmpty(job.Exception)))
            {
                Debug.LogError(job.Exception);
            }
        }

        private static void WaitForDoFile(List<ManualResetEvent> events, int timeout)
        {
            var finished = WaitHandle.WaitAny(events.ToArray(), timeout);
            if (finished == WaitHandle.WaitTimeout)
            {
                // 超时
            }

            events.RemoveAt(finished);
        }

        #endregion
    }
}