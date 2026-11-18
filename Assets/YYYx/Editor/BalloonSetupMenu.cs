#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 气球玩法的场景辅助工具（只在编辑器里用）。
/// 菜单：Tools/YYYx/气球玩法/...
/// </summary>
public static class BalloonSetupMenu
{
    [MenuItem("Tools/YYYx/气球玩法/1. 在场景中创建气球生成器")]
    static void CreateBalloonRig()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("气球玩法", "请先打开 GameScene 再执行。", "好");
            return;
        }

        Vector3 position = Vector3.zero;
        bool positioned = false;

        GameSceneManager gameScene = Object.FindAnyObjectByType<GameSceneManager>(FindObjectsInactive.Include);
        if (gameScene != null && gameScene.Spawn != null)
        {
            position = gameScene.Spawn.transform.position + gameScene.Spawn.transform.forward * 2.5f;
            positioned = true;
        }
        else if (Camera.main != null)
        {
            position = Camera.main.transform.position + Camera.main.transform.forward * 3f;
            positioned = true;
        }

        if (!positioned)
        {
            EditorUtility.DisplayDialog("气球玩法",
                "场景里没有找到 GameSceneManager.Spawn，也没有 Main Camera。\n" +
                "请手动把生成器挪到玩家面前。", "好");
        }

        GameObject root = new GameObject("BalloonGame");
        Undo.RegisterCreatedObjectUndo(root, "创建气球生成器");
        root.transform.position = position;

        BalloonSpawner spawner = Undo.AddComponent<BalloonSpawner>(root);
        BalloonGameManager manager = Undo.AddComponent<BalloonGameManager>(root);

        manager.spawner = spawner;
        manager.clueManager = Object.FindAnyObjectByType<ClueManager>(FindObjectsInactive.Include);
        manager.winBioManager = Object.FindAnyObjectByType<WinBioManager>(FindObjectsInactive.Include);

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);

        Debug.Log($"[气球] 已在 {root.transform.position} 创建 BalloonGame。\n" +
                  $"- ClueManager: {(manager.clueManager != null ? manager.clueManager.name : "未找到")}\n" +
                  $"- WinBioManager: {(manager.winBioManager != null ? manager.winBioManager.name : "未找到")}\n" +
                  "请按房间大小调整 BalloonSpawner 的 areaSize / flightHeight，并把生成器摆到玩家面前。");
    }

    [MenuItem("Tools/YYYx/气球玩法/2. 停用旧的轮盘赌物体")]
    static void DisableLegacyRouletteObjects()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("气球玩法", "请先打开场景再执行。", "好");
            return;
        }

        // 注意：这里只停用，不删除，方便随时回退。
        string[] legacyNames = { "Roulette", "wheel", "gold", "default", "Button 1" };

        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int disabled = 0;

        foreach (Transform t in all)
        {
            if (t == null) continue;
            if (System.Array.IndexOf(legacyNames, t.name) < 0) continue;
            if (t.gameObject.activeSelf == false) continue;

            Undo.RecordObject(t.gameObject, "停用旧轮盘物体");
            t.gameObject.SetActive(false);
            disabled++;
            Debug.Log($"[气球] 已停用：{GetPath(t)}");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("气球玩法",
            disabled > 0
                ? $"已停用 {disabled} 个旧轮盘物体（未删除，可在 Hierarchy 里恢复或自行删除）。"
                : "没有找到需要停用的物体。",
            "好");
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
