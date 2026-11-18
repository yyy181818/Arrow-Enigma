#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 把《六国剑》里的手上巨弩（NuJianCrossbow）挂到右手上。
/// 菜单：Tools/YYYx/弓弩/...
///
/// 巨弩物体常驻 Laucher 场景（跟玩家 Rig 一起，不会随 GameScene 重载消失），
/// 靠 NuJianCrossbow 的场景门控只在 GameScene 里显示。
/// </summary>
public static class CrossbowMountMenu
{
    const string PrefabPath = "Assets/YYYx/Prefabs/InteractiveRoot.prefab";
    const string ArrowPath = "Assets/YYYx/Prefabs/Arrow.prefab";
    const string RootName = "HandCrossbow";

    [MenuItem("Tools/YYYx/弓弩/1. 在 Laucher 场景把巨弩挂到右手")]
    static void MountToRightHand()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("弓弩", "请先打开场景再执行。", "好");
            return;
        }

        if (scene.name != "Laucher" &&
            !EditorUtility.DisplayDialog("弓弩",
                $"当前打开的是 {scene.name}，不是 Laucher。\n巨弩挂到手上后建议放在 Laucher 场景（常驻、不随 GameScene 重载消失）。\n仍然继续吗？",
                "继续", "取消"))
        {
            return;
        }

        Transform hand = FindRightHand();
        if (hand == null)
        {
            EditorUtility.DisplayDialog("弓弩",
                "没找到右手节点。\n请先在 Hierarchy 里选中右手（名字里带 Right 的 Hand/Controller），再执行一次。", "好");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("弓弩", $"找不到预制体：{PrefabPath}\n请确认资源已导入。", "好");
            return;
        }

        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("弓弩", "场景里已经有 HandCrossbow 了，要重新创建吗？", "重新创建", "取消"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Undo.RegisterCreatedObjectUndo(go, "挂载巨弩");
        go.name = RootName;

        go.transform.SetParent(hand, false);
        go.transform.localPosition = new Vector3(0f, -0.06f, 0.12f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.25f;

        // 巨弩预制体里带着靶场的“刷新点”，这里用不到，删掉
        Transform refresh = go.transform.Find("刷新点");
        if (refresh != null) Undo.DestroyObjectImmediate(refresh.gameObject);

        NuJianCrossbow crossbow = go.GetComponent<NuJianCrossbow>();
        if (crossbow == null) crossbow = Undo.AddComponent<NuJianCrossbow>(go);

        crossbow.onlyInRequiredScene = true;
        crossbow.requiredScene = "GameScene";     // 只在玩法场景出现
        crossbow.hideControllerModelInScene = false; // 默认不藏手/手柄模型，勾上会看不到手
        crossbow.aimReference = null;             // 用自己朝向
        crossbow.arrowPrefab = AssetDatabase.LoadAssetAtPath<Arrow>(ArrowPath);
        crossbow.autoFire = true;
        crossbow.groundY = 0f;                    // 地面高度，按房间地面改
        crossbow.speed = 30f;
        crossbow.ignoreRoot = hand;               // 别被自己的手挡住

        EditorUtility.SetDirty(crossbow);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        Debug.Log($"[巨弩] 已挂到 {GetPath(hand)} 下，scale=0.25。\n" +
                  "· 枪口朝向不对就在 Inspector 里改 HandCrossbow 的 Rotation（或改 NuJianCrossbow.barrelAxisLocal）；\n" +
                  "· 太大/太小改 localScale；\n" +
                  "· 只在 GameScene 显示：requiredScene 已设成 GameScene。");
    }

    [MenuItem("Tools/YYYx/弓弩/2. 移除手上的巨弩")]
    static void RemoveCrossbow()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing == null)
        {
            EditorUtility.DisplayDialog("弓弩", "场景里没有 HandCrossbow。", "好");
            return;
        }

        Undo.DestroyObjectImmediate(existing);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[巨弩] 已移除 HandCrossbow");
    }

    static Transform FindRightHand()
    {
        // 1) 优先用当前选中的物体
        if (Selection.activeTransform != null)
        {
            string sn = Selection.activeTransform.name.ToLowerInvariant();
            if (sn.Contains("right") && (sn.Contains("hand") || sn.Contains("controller")))
                return Selection.activeTransform;
        }

        // 2) 全场景按名字找右手
        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Transform fallback = null;

        foreach (Transform t in all)
        {
            string n = t.name.ToLowerInvariant();
            if (!n.Contains("right")) continue;

            if (n.Contains("hand") || n.Contains("controller")) return t;
            if (fallback == null) fallback = t;
        }

        return fallback;
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        Transform parent = t.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
#endif
