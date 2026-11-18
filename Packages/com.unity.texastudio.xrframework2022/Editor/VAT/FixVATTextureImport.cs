// 一键修复 VAT 贴图导入设置
// 用法：Project 里选中 RBD_pos.exr / RBD_rot.exr / RBD_col.exr（可多选）
//       -> 菜单 Tools / VAT / Fix Texture Import Settings
//
// 做什么：
//   Compression = None      —— 关键！Compressed 会把 EXR 浮点压成 8bit UNorm，
//                              负值截断为 0、>1 截断为 1，VAT 数据直接报废
//   sRGB = Off              —— 数据贴图不能走 gamma
//   Filter = Point          —— 数据贴图不插值
//   Mipmaps = Off
// 修完 Inspector 里 Format 应显示 RGBA Half / RGBA Float（不再是 RGBA32）
using UnityEditor;
using UnityEngine;

public static class FixVATTextureImport
{
    [MenuItem("Tools/VAT/Fix Texture Import Settings")]
    static void Fix()
    {
        int n = 0;
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;
            if (!(AssetImporter.GetAtPath(path) is TextureImporter imp)) continue;

            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.sRGBTexture = false;
            imp.filterMode = FilterMode.Point;
            imp.mipmapEnabled = false;
            imp.npotScale = TextureImporterNPOTScale.None;

            // 关键：关闭 RGBM 编码（HDR 贴图导入选项）。
            // RGBM 把 alpha 通道占用为乘数 M，且只能表示正值 —— VAT 的负偏移直接报废。
            // 注意：部分 Unity 版本已删除 TextureImporterRGBMEncoding 类型，这里用反射兜底。
            var rgbmProp = typeof(TextureImporterSettings).GetProperty("rgbm");
            Debug.Log($"[VAT] rgbm 属性反射: {(rgbmProp == null ? "未找到(跳过)" : "已找到, 置 Off")}");
            if (rgbmProp != null)
            {
                var settings = new TextureImporterSettings();
                imp.ReadTextureSettings(settings);
                object boxed = settings;
                rgbmProp.SetValue(boxed, System.Enum.Parse(rgbmProp.PropertyType, "Off"));
                imp.SetTextureSettings((TextureImporterSettings)boxed);
            }

            // 决定性手段：直接强制平台覆盖格式为 RGBAFloat，绕开 RGBM/压缩的自动决策
            foreach (var platform in new[] { "Standalone", "Android", "iOS" })
            {
                var ps = imp.GetPlatformTextureSettings(platform);
                ps.overridden = true;
                ps.format = TextureImporterFormat.RGBAFloat;
                ps.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SetPlatformTextureSettings(ps);
            }
            // 默认平台也强制（编辑器预览走这里）。
            // 注意：2022.3 没有 SetDefaultPlatformTextureSettings，
            // 默认平台设置同样走 SetPlatformTextureSettings 写回
            var def = imp.GetDefaultPlatformTextureSettings();
            def.format = TextureImporterFormat.RGBAFloat;
            def.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SetPlatformTextureSettings(def);

            imp.SaveAndReimport();

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Debug.Log($"[VAT] 已修复 {path}\n      -> format={tex.graphicsFormat}  " +
                      $"{tex.width}x{tex.height}  (应看到 Half/Float 字样，不再是 UNorm)");
            n++;
        }
        if (n == 0)
            Debug.LogWarning("[VAT] 没有选中任何贴图。请在 Project 里选中 RBD_pos/rot/col 的 .exr 再执行。");
        else
            Debug.Log($"[VAT] 共修复 {n} 张贴图。请重新跑 Tools/VAT/Dump Texture Data 验证。");
    }
}
