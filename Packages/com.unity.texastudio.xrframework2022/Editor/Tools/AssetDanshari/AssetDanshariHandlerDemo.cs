using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDanshariHandlerDemo
    {
        public AssetDanshariHandlerDemo()
        {
            AssetDanshariHandler.OnDependenciesLoadDataMore += OnDependenciesLoadDataMore;
            AssetDanshariHandler.OnDependenciesContextDraw += OnDependenciesContextDraw;
        }

        private static void OnDependenciesLoadDataMore(string resPath, List<AssetTreeModel.AssetInfo> resInfos, AssetTreeModel treeModel)
        {
            // 去代码定义文件去查找
            if (resPath != "\"Assets/Simple UI/PNG\"")
            {
                return;
            }

            var preLen = resPath.Length - "PNG".Length - 2;
            const string codePath = "Assets/Demo/UISpriteDefine.cs";
            try
            {
                var text = File.ReadAllText(codePath);

                foreach (var assetInfo in resInfos)
                {
                    var searchText = assetInfo.FileRelativePath[preLen..];
                    searchText = searchText.Remove(searchText.Length - 4);

                    if (!text.Contains(searchText)) continue;
                    var info = treeModel.GenAssetInfo(codePath);
                    info.IsExtra = true;
                    assetInfo.AddChild(info);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private static void OnDependenciesContextDraw(GenericMenu menu)
        {
            menu.AddSeparator(String.Empty);
            menu.AddItem(new GUIContent("生成代码定义/C# 定义"), false, OnDependenciesContextGenCodeCSharp);
            menu.AddItem(new GUIContent("生成代码定义/Lua 定义"), false, OnDependenciesContextGenCodeLua);
        }

        private static void OnDependenciesContextGenCodeCSharp()
        {

        }

        private static void OnDependenciesContextGenCodeLua()
        {

        }
    }
}