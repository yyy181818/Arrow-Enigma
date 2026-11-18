// HEUTerrainResolutionFixer.cs
// 放到 Assets/Editor/（必须在 Editor 文件夹里）。
//
// 用途：Houdini Engine 导入 Terrain 时若报
//   "length is out of range" / "Unable to set height values"，
// 基本都是 HEU 插件自身的 off-by-one BUG（靠调 Unity 分辨率无解，
// 必须改 HEU 源码两处，见下方 Check 输出的补丁说明）。
//
// 本工具只做两件事：
//   1) 检查 HEU 源码补丁是否已打上；
//   2) 选中一个 Terrain，直接打印 HDA 需要的参数填写清单。

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public static class HEUTerrainResolutionFixer
{
    private const string MENU = "Tools/Houdini Engine/地形→顶点色网格 参数检查";
    // HEU 随本包一起分发，放在包根的 Plugins/ 下（不在 Assets/）。
    // 优先用 AssetDatabase 定位（embedded / file: / git 引用都能命中），
    // 兜底再试 Packages/<包名> 物理路径与 Assets/Plugins 手装两种情况。
    private const string HEU_PKG = "com.unity.texastudio.xrframework2022";
    private const string HEU_REL =
        "Plugins/HoudiniEngineUnity/Scripts/Utility/HEU_InputInterfaceTerrain.cs";

    [MenuItem(MENU, false, 1)]
    private static void Check()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== HEU 地形参数检查 ===\n");

        // 1) HEU 补丁状态（这是 length out of range 的真正根因）
        switch (CheckPatchState(out string heuPath))
        {
            case PatchState.Patched:
                sb.AppendLine("[OK] HEU 补丁已打上，分辨率随便配都不报错，可直接 Recook。");
                break;
            case PatchState.NotPatched:
                sb.AppendLine("[X] HEU 补丁未打！Recook 必报 length is out of range。");
                sb.AppendLine("    改 " + heuPath + " 两处：");
                sb.AppendLine("    1) UploadAlphaMaps()  : 建 volume 容器用 heightmapResolution 代替 alphamapResolution");
                sb.AppendLine("    2) SetMaskLayer()     : 生成 mask 数组用 heightmapResolution 代替 alphamapResolution");
                break;
            default:
                sb.AppendLine("[?] 找不到 HEU 源码，无法判断补丁（插件可能装在别的路径）。");
                break;
        }

        // 2) 选中 Terrain，打印 HDA 填写清单
        Terrain t = GetSelectedTerrain();
        if (t == null) { Debug.Log(sb.ToString()); return; }

        TerrainData td = t.terrainData;
        int H = td.heightmapResolution;
        int A = td.alphamapResolution;
        bool square = Mathf.Approximately(td.size.x, td.size.z);

        sb.AppendLine($"\n地形: {t.name}");
        sb.AppendLine($"Heightmap={H}  Control={A}  正方形={square}");
        if (!square)
            sb.AppendLine("    [!] 非正方形，HEU 会报 'Houdini heightmap must be square!'，先把 Width/Length 设成一样。");

        var layers = td.terrainLayers;
        if (layers == null || layers.Length == 0) { Debug.Log(sb.ToString()); return; }

        if (layers.Length > 3)
            sb.AppendLine($"[!] 共 {layers.Length} 层，但顶点色只有 RGB 三通道，超出层会丢失，请先归并到 3 类。");

        sb.AppendLine("\n--- HDA 参数填写清单 ---");
        sb.AppendLine("Layer(R)=第1个  Layer(G)=第2个  Layer(B)=第3个");
        for (int i = 0; i < Mathf.Min(layers.Length, 3); i++)
        {
            if (layers[i] == null) continue;
            char slot = i == 0 ? 'R' : i == 1 ? 'G' : 'B';
            sb.AppendLine($"  {slot}  <-  {ToHoudiniLayerName(layers[i].name)}");
        }

        sb.AppendLine("\nMesh Density 建议：");
        sb.AppendLine($"  省面/预览 0.1  -> ~{(long)(H * 0.1) * (long)(H * 0.1):N0} 顶点");
        sb.AppendLine($"  精细     0.25 -> ~{(long)(H * 0.25) * (long)(H * 0.25):N0} 顶点");
        sb.AppendLine("Smoothness A = 1 （默认；顶点色A 作全局光滑度遮罩，1=不调制）");

        Debug.Log(sb.ToString(), t);
    }

    // ------------------------------------------------------------------
    private enum PatchState { Patched, NotPatched, SourceMissing }

    private static PatchState CheckPatchState(out string found)
    {
        found = null;

        // 方式1（最健壮）：直接问 Unity 资产数据库，embedded / file: / git 引用都能命中
        foreach (string g in AssetDatabase.FindAssets("HEU_InputInterfaceTerrain t:MonoScript"))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (Path.GetFileNameWithoutExtension(p) != "HEU_InputInterfaceTerrain") continue;
            string disk = Path.GetFullPath(p);
            if (File.Exists(disk)) { found = disk; break; }
        }

        // 方式2：兜底——本机 embedded 包（Packages/<包名> 物理目录）或 Assets/Plugins 手装
        if (found == null)
        {
            string[] candidates = new string[]
            {
                Path.GetFullPath("Packages/" + HEU_PKG + "/" + HEU_REL),
                Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                    "Assets/Plugins/HoudiniEngineUnity/Scripts/Utility/HEU_InputInterfaceTerrain.cs"),
            };
            foreach (string full in candidates)
            {
                if (File.Exists(full)) { found = full; break; }
            }
        }

        if (found == null) return PatchState.SourceMissing;

        string src = File.ReadAllText(found);
        bool p1 = src.Contains("hfWorldSize, hfWorldSize");
        bool p2 = src.Contains("int sizeX = idt._terrainData.heightmapResolution;");
        return (p1 && p2) ? PatchState.Patched : PatchState.NotPatched;
    }

    // 复刻 HEU 的层名处理：空格 -> 下划线，去掉 .terrainlayer 后缀
    private static string ToHoudiniLayerName(string raw)
    {
        string n = raw.Replace(" ", "_");
        int ext = n.LastIndexOf(".terrainlayer");
        if (ext > 0) n = n.Remove(ext);
        return n;
    }

    private static Terrain GetSelectedTerrain()
    {
        GameObject go = Selection.activeGameObject;
        Terrain t = go != null ? go.GetComponentInChildren<Terrain>(true) : null;
        if (t == null || t.terrainData == null)
        {
            EditorUtility.DisplayDialog("没选中 Terrain",
                "请先在 Hierarchy 选中带 Terrain 组件的 GameObject，再执行本菜单。", "好");
            return null;
        }
        return t;
    }
}
#endif
