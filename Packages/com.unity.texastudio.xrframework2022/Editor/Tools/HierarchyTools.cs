using TexaXRFramework.Core;
using UnityEditor;
using UnityEngine;

public class HierarchyTools
{
    [MenuItem("GameObject/XR开发工具/一键创建Hierarchy目录", false, 1)]
    private static void CreateCustomHierarchy()
    {
        GameObject sceneArtRoot = new GameObject(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        sceneArtRoot.AddComponent<LBEStageDefine>();
        // 创建场景资源节点
        new GameObject("-------- 时间线 --------").transform.SetParent(sceneArtRoot.transform);
        GameObject timelineRoot = new GameObject("TimelineEventRoot");
        timelineRoot.transform.SetParent(sceneArtRoot.transform);

        // 创建场景资源节点
        new GameObject("-------- 环境资源 --------").transform.SetParent(sceneArtRoot.transform);
        GameObject modelsRoot = new GameObject("EnvironmentsRoot");
        modelsRoot.transform.SetParent(sceneArtRoot.transform);

        // 创建交互资源节点
        new GameObject("-------- 音频资源 --------").transform.SetParent(sceneArtRoot.transform);
        GameObject audioRoot = new GameObject("AudioRoot");
        audioRoot.transform.SetParent(sceneArtRoot.transform);

        // 创建场景美术资源节点
        new GameObject("-------- 动画资源 --------").transform.SetParent(sceneArtRoot.transform);
        GameObject animationRoot = new GameObject("AnimationRoot");
        animationRoot.transform.SetParent(sceneArtRoot.transform);

        // 创建交互资源节点
        new GameObject("-------- 交互资源 --------").transform.SetParent(sceneArtRoot.transform);
        GameObject interactiveRoot = new GameObject("InteractiveRoot");
        interactiveRoot.transform.SetParent(sceneArtRoot.transform);


        new GameObject("-------- 区域--------").transform.SetParent(sceneArtRoot.transform); 
        GameObject sceneArea = new GameObject("AreaRoot");
        sceneArea.transform.SetParent(sceneArtRoot.transform);
        new GameObject("Area1(示例命名)").transform.SetParent(sceneArea.transform);
    }
    // 定义固定的名字，你可以根据需要修改这个值
   
    private const string FixedName = "Interactive_";

    [MenuItem("GameObject/XR开发工具/重命名交互物体", false)]
    private static void PerformFixedRename()
    {
        // 获取当前选中的游戏对象
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            // 将选中物体的名字设置为固定名字
            selectedObject.name = FixedName+selectedObject.name;
        }
    }

    [MenuItem("GameObject/XR开发工具/重命名交互物体", true)]
    private static bool ValidateRename()
    {
        // 只有当有物体被选中时，菜单项才可用
        return Selection.activeGameObject != null;
    }

    [MenuItem("GameObject/XR开发工具/删除隐藏子物体", false)]
    private static void DeleteHiddenChildren()
    {
        // 获取当前选中的游戏对象
        GameObject selectedObject = Selection.activeGameObject;
        Transform[] allChildren = selectedObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child != selectedObject.transform && !child.gameObject.activeSelf)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }

    [MenuItem("GameObject/XR开发工具/删除隐藏子物体", true)]
    private static bool ValidateDeleteHiddenChildren()
    {
        // 只有当有物体被选中时，菜单项才可用
        return Selection.activeGameObject != null;
    }

    [MenuItem("GameObject/XR开发工具/删除所有相机子物体", false)]
    private static void DeleteCameras()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("没有选中任何物体，请先选中一个物体。");
            return;
        }

        Camera[] allCameras = selectedObject.GetComponentsInChildren<Camera>(true);
        foreach (Camera camera in allCameras)
        {
            if (camera != null)
            {
                Undo.DestroyObjectImmediate(camera.gameObject);
            }
        }
    }

    [MenuItem("GameObject/XR开发工具/删除所有相机子物体", true)]
    private static bool ValidateDeleteCameras()
    {
        // 只有当有物体被选中时，菜单项才可用
        return Selection.activeGameObject != null;
    }
}