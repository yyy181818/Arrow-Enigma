#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 气球模型构建工具（只在编辑器里用）。
///
/// 菜单：Tools/YYYx/气球玩法/3. 生成气球模型预制体
///       Tools/YYYx/气球玩法/4. 把气球预制体填入生成器
///
/// 模型来源：Assets/YYYx/Models/Balloon.obj（球体+绳结）与 BalloonRope.obj（绳子），
/// 由 WorkBuddy 参数化生成。工具负责把它们组装成可直接用的预制体：
///   Balloon（根）  ← Balloon 脚本 + 碰撞体 + 刚体
///     ├─ Body     ← 球体网格，会被 Balloon 脚本自转
///     └─ Rope     ← 绳子网格，不跟随自转
/// </summary>
public static class BalloonModelBuilder
{
    const string BodyModelPath = "Assets/YYYx/Models/Balloon.obj";
    const string RopeModelPath = "Assets/YYYx/Models/BalloonRope.obj";
    const string BodyMatPath = "Assets/YYYx/Mateiral/Balloon.mat";
    const string RopeMatPath = "Assets/YYYx/Mateiral/BalloonRope.mat";
    const string PrefabPath = "Assets/YYYx/Prefabs/Balloon.prefab";

    /// <summary>预制体不存在时，脚本编译完自动构建一次，省得手动点菜单。</summary>
    [InitializeOnLoadMethod]
    static void AutoBuildWhenMissing()
    {
        if (File.Exists(PrefabPath)) return;

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string bodyAbs = Path.Combine(projectRoot, BodyModelPath);
        if (!File.Exists(bodyAbs)) return;

        EditorApplication.update += WaitAndBuild;
    }

    static double _nextAttemptAt;
    static int _attemptCount;

    static void WaitAndBuild()
    {
        if (File.Exists(PrefabPath))
        {
            EditorApplication.update -= WaitAndBuild;
            return;
        }

        // 每 0.5 秒查一次，最多等 30 秒
        if (EditorApplication.timeSinceStartup < _nextAttemptAt) return;
        _nextAttemptAt = EditorApplication.timeSinceStartup + 0.5;

        // OBJ 可能还在导入中
        if (LoadMesh(BodyModelPath, "Balloon") == null)
        {
            if (++_attemptCount > 60)
            {
                EditorApplication.update -= WaitAndBuild;
                Debug.LogWarning("[气球] Balloon.obj 一直没导入成功，稍后手动执行菜单「3. 生成气球模型预制体」即可。");
                return;
            }

            return;
        }

        EditorApplication.update -= WaitAndBuild;
        BuildPrefab();
    }

    [MenuItem("Tools/YYYx/气球玩法/3. 生成气球模型预制体")]
    static void BuildFromMenu()
    {
        if (BuildPrefab())
        {
            EditorUtility.DisplayDialog("气球玩法", "已生成：\n" + PrefabPath, "好");
        }
    }

    [MenuItem("Tools/YYYx/气球玩法/4. 把气球预制体填入生成器")]
    static void AssignToSpawner()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("气球玩法", "还没生成气球预制体，请先执行菜单「3. 生成气球模型预制体」。", "好");
            return;
        }

        BalloonSpawner[] spawners = Object.FindObjectsByType<BalloonSpawner>(FindObjectsInactive.Include);
        if (spawners.Length == 0)
        {
            EditorUtility.DisplayDialog("气球玩法", "当前场景里没有找到 BalloonSpawner。", "好");
            return;
        }

        foreach (BalloonSpawner spawner in spawners)
        {
            Undo.RecordObject(spawner, "填入气球预制体");
            spawner.balloonPrefab = prefab;
            EditorUtility.SetDirty(spawner);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[气球] 已给 {spawners.Length} 个 BalloonSpawner 填入 {PrefabPath}。");
        EditorUtility.DisplayDialog("气球玩法", $"已给 {spawners.Length} 个 BalloonSpawner 填入气球预制体。", "好");
    }

    static bool BuildPrefab()
    {
        AssetDatabase.ImportAsset(BodyModelPath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(RopeModelPath, ImportAssetOptions.ForceSynchronousImport);

        Mesh bodyMesh = LoadMesh(BodyModelPath, "Balloon");
        Mesh ropeMesh = LoadMesh(RopeModelPath, "BalloonRope");
        if (bodyMesh == null)
        {
            Debug.LogError($"[气球] 找不到 {BodyModelPath} 里的网格，请确认 OBJ 已正确导入。");
            return false;
        }

        Material bodyMat = GetOrCreateMaterial(BodyMatPath, new Color(0.85f, 0.12f, 0.12f), 0.55f);
        Material ropeMat = GetOrCreateMaterial(RopeMatPath, new Color(0.85f, 0.83f, 0.78f), 0.15f);

        GameObject root = new GameObject("Balloon");
        try
        {
            root.AddComponent<Balloon>();
            BalloonTint tint = root.AddComponent<BalloonTint>();

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.radius = 0.14f;
            collider.center = new Vector3(0f, 0.015f, 0f);

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.None;

            GameObject body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.AddComponent<MeshFilter>().sharedMesh = bodyMesh;
            MeshRenderer bodyRenderer = body.AddComponent<MeshRenderer>();
            bodyRenderer.sharedMaterial = bodyMat;

            // 只给球体上色，绳子保持米白
            tint.targets = new Renderer[] { bodyRenderer };

            if (ropeMesh != null)
            {
                GameObject rope = new GameObject("Rope");
                rope.transform.SetParent(root.transform, false);
                rope.AddComponent<MeshFilter>().sharedMesh = ropeMesh;
                rope.AddComponent<MeshRenderer>().sharedMaterial = ropeMat;
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            if (saved == null)
            {
                Debug.LogError($"[气球] 预制体保存失败：{PrefabPath}");
                return false;
            }

            Debug.Log($"[气球] 已生成预制体 {PrefabPath}\n" +
                      $"- Body 网格 {bodyMesh.name}（{bodyMesh.vertexCount} 顶点）\n" +
                      $"- 碰撞体半径 {collider.radius}\n" +
                      "执行菜单「4. 把气球预制体填入生成器」即可让场景里的 BalloonSpawner 用上它。");
            return true;
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    static Mesh LoadMesh(string assetPath, string preferredName)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        List<Mesh> meshes = new List<Mesh>();
        foreach (Object obj in all)
        {
            Mesh mesh = obj as Mesh;
            if (mesh != null) meshes.Add(mesh);
        }

        foreach (Mesh mesh in meshes)
        {
            if (mesh.name == preferredName) return mesh;
        }

        return meshes.Count > 0 ? meshes[0] : null;
    }

    static Material GetOrCreateMaterial(string path, Color color, float smoothness)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogError("[气球] 找不到可用的 Lit Shader，材质无法创建。");
                return null;
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);

        EditorUtility.SetDirty(material);
        return material;
    }
}
#endif
