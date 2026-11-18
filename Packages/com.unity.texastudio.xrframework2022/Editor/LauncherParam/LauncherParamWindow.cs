using LBEServer.ShareNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace TexaXRFramework.Core.Editor
{
    public class LauncherParamWindow : EditorWindow
    {
        [MenuItem("XR开发工具/创建测试启动配置/默认配置")]
        public static void CreateJson()
        {
            // 创建 ScriptableObject 实例
            LauncherParamSettings scriptableObject =CreateInstance<LauncherParamSettings>();
            // 将对象转换为JSON字符串
            var savelauncherParamJsonDataPath = $"Assets/Resources/LauncherParam/LauncherParamSettings.asset";
            string directoryPath = Path.GetDirectoryName(savelauncherParamJsonDataPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            try
            {
                if (!File.Exists(savelauncherParamJsonDataPath))
                {
                    AssetDatabase.CreateAsset(scriptableObject, savelauncherParamJsonDataPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"成功创建启播文件: {savelauncherParamJsonDataPath}");
                }
                else
                {
                    Debug.Log($"启播文件已存在: {savelauncherParamJsonDataPath}");
                }              
            }
            catch (System.Exception e)
            {
                Debug.LogError("创建启播配置失败: " + e.Message);
            }
        }
    }
}