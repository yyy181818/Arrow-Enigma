using Newtonsoft.Json;
using SharedModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TexaXRFramework.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class LBEStageGlobalSaveHandler
{
    // 跟踪当前正在处理的场景，防止单次保存重复执行
    private static HashSet<string> _processingScenes = new HashSet<string>();

    static LBEStageGlobalSaveHandler()
    {
        EditorSceneManager.sceneSaving += OnSceneSaving;
        EditorSceneManager.sceneSaved += OnSceneSaved;
    }

    private static void OnSceneSaving(Scene scene, string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        _processingScenes.Remove(path); // 清理状态，确保本次保存能被处理
    }

    private static void OnSceneSaved(Scene scene)
    {
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning($"[LBE] 忽略无效场景: {scene.name}");
            return;
        }

        string scenePath = scene.path;
        if (_processingScenes.Contains(scenePath))
        {
            Debug.Log($"[LBE] 场景「{scene.name}」保存事件已处理，跳过重复执行");
            return;
        }

        _processingScenes.Add(scenePath);
        try
        {
            // 查找并处理场景中第一个LBEStageDefine组件
            ProcessFirstLBEStageDefineInScene(scene);
        }
        finally
        {
            _processingScenes.Remove(scenePath);
        }
    }

    /// <summary>
    /// 查找场景中第一个LBEStageDefine组件并处理
    /// </summary>
    private static void ProcessFirstLBEStageDefineInScene(Scene targetScene)
    {
        LBEStageDefine firstComponent = null;
        var rootObjects = targetScene.GetRootGameObjects();

        // 遍历所有根物体及其子物体，找到第一个有效组件
        foreach (var root in rootObjects)
        {
            // 深度优先查找第一个组件
            firstComponent = root.GetComponentInChildren<LBEStageDefine>(includeInactive: true);

            // 找到第一个后立即终止遍历
            if (firstComponent != null)
            {
                ExportComponentData(firstComponent);
                return; // 只处理第一个，直接返回
            }
        }

        // 如果场景中没有找到任何组件
        if (firstComponent == null)
        {
            Debug.Log($"[LBE] 场景「{targetScene.name}」中未找到LBEStageDefine组件");
        }
    }

    /// <summary>
    /// 导出单个组件的数据
    /// </summary>
    private static void ExportComponentData(LBEStageDefine component)
    {
        // 同步最新数据
        component.Order = component.gameObject.scene.buildIndex;
        component.StageName = component.gameObject.scene.name;
        EditorUtility.SetDirty(component);
        var allAreas = component.gameObject.GetComponentsInChildren<LBEAreaDefine>(true);
        LBEStageData exportData = new LBEStageData
        {
            StageName = component.StageName,
            AreaList = new List<LBEAreaData>(),
            Order = component.Order,
        };

        foreach (var area in allAreas)
        {
            GenStageData(exportData, area);
        }
        exportData.AreaList = exportData.AreaList.OrderBy(item => item.Order).ToList();
        //string parentDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(Application.dataPath));
        var jsonStr = JsonConvert.SerializeObject(exportData);
        var saveToClientPath = $"Assets/Resources/ContentConfig/Stage/{component.StageName}.json";

        var directoryClientPath = Path.GetDirectoryName(saveToClientPath);

        if (!Directory.Exists(directoryClientPath))
        {
            Directory.CreateDirectory(directoryClientPath);
        }
        File.WriteAllText(saveToClientPath, jsonStr);
        AssetDatabase.Refresh();
        Debug.Log($"自动保存关卡数据: {component.StageName} (Order: {component.Order}) 路径:{saveToClientPath})");
    }
    private static void GenStageData(LBEStageData exportData, LBEAreaDefine area)
    {
        var areaData = new LBEAreaData
        {
            AreaName = area.AreaName,
            Order = area.Order,
            MaxStayTime = area.MaxStayTime,
            MinStayTime = area.MinStayTime,
        };
        exportData.AreaList.Add(areaData);
    }
}


