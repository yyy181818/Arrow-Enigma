// HEUEnvironmentDiagnostics.cs
// 放到 Editor/（必须在 Editor 文件夹里）。
//
// 用途：当对方机器上"HDA 一拖就报错 / HEU 起不来"时，一键排查环境问题。
// 检测项：
//   1) 插件期望的 Houdini 版本；
//   2) HEU Settings 里手动保存的 Houdini 路径；
//   3) HAPI_PATH 环境变量（Machine/User/Process）；
//   4) HEU 实际解析到的 Houdini 安装路径；
//   5) Windows 注册表里所有已安装的 Houdini / Houdini Engine 版本；
//   6) 关键文件是否齐全（libHAPIL.dll / libHARC.dll / HARS.exe / hython.exe）；
//   7) 给出"匹配 / 不匹配 / 缺失"结论与建议。
//
// 只做只读检测，不修改任何东西。

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class HEUEnvironmentDiagnostics
{
    private const string MENU = "Tools/Houdini Engine/环境诊断（HEU 无法启动排查）";

    [MenuItem(MENU, false, 2)]
    private static void Diagnose()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========== HEU 环境诊断 ==========\n");

        // 1) 插件期望版本
        string want = "?";
        try
        {
            want = HoudiniEngineUnity.HEU_HoudiniVersion.HOUDINI_VERSION_STRING;
            sb.AppendLine($"[插件] 期望 Houdini 版本: {want}");
            sb.AppendLine($"[插件] Houdini Engine API: " +
                HoudiniEngineUnity.HEU_HoudiniVersion.HOUDINI_ENGINE_MAJOR + "." +
                HoudiniEngineUnity.HEU_HoudiniVersion.HOUDINI_ENGINE_MINOR + "." +
                HoudiniEngineUnity.HEU_HoudiniVersion.HOUDINI_ENGINE_API);
        }
        catch (Exception e)
        {
            sb.AppendLine($"[X] 读取插件版本失败: {e.Message}");
        }
        sb.AppendLine();

        // 2) Settings 里手动保存的路径
        try
        {
            string saved = HoudiniEngineUnity.HEU_PluginSettings.HoudiniInstallPath;
            sb.AppendLine("[Settings] 手动指定的 Houdini 路径: " + (string.IsNullOrEmpty(saved) ? "(未设置)" : saved));
        }
        catch (Exception e)
        {
            sb.AppendLine($"[X] 读取 Settings 路径失败: {e.Message}");
        }
        sb.AppendLine();

#if UNITY_EDITOR_WIN
        // 3) HAPI_PATH 环境变量
        DumpEnvVar(sb, "HAPI_PATH");

        // 4) 注册表里所有已安装的 Houdini 版本
        DumpRegistry(sb);

        // 5) HEU 实际解析到的路径（HAPI_PATH → 注册表，不含 Settings）
        string resolved = null;
        try
        {
            resolved = HoudiniEngineUnity.HEU_Platform.GetHoudiniEngineDefaultPath();
        }
        catch (Exception e)
        {
            sb.AppendLine($"[X] 调用 HEU 路径解析异常: {e.Message}");
        }

        if (string.IsNullOrEmpty(resolved))
        {
            sb.AppendLine("[X] HEU 解析到的 Houdini 路径: 空（检测失败）\n");
        }
        else
        {
            sb.AppendLine($"[OK] HEU 解析到的 Houdini 路径: {resolved}");
            CheckCriticalFiles(sb, resolved);
            sb.AppendLine();
        }
#else
        sb.AppendLine("[i] 本诊断工具当前只支持 Windows（注册表检测）。macOS 请检查 /Applications/Houdini/Houdini" + want + " 是否存在。\n");
#endif

        // 6) 结论
        sb.AppendLine("---------- 结论与建议 ----------");
        AppendConclusion(sb, want);

        Debug.Log(sb.ToString());
    }

    // ------------------------------------------------------------------
#if UNITY_EDITOR_WIN
    [System.Runtime.InteropServices.DllImport("Advapi32.dll")]
    private static extern uint RegOpenKeyEx(
        UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);

    [System.Runtime.InteropServices.DllImport("Advapi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern uint RegEnumValue(
        int hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcchValueName,
        IntPtr lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

    [System.Runtime.InteropServices.DllImport("Advapi32.dll")]
    private static extern uint RegCloseKey(int hKey);

    private static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);

    private const int KEY_QUERY_VALUE = 0x0001;
    private const int KEY_WOW64_64KEY = 0x0100;

    /// <summary>
    /// 枚举某个注册表键下的所有"值"（值名 -> 值数据）。
    /// Houdini 把版本号作为"值名"（如 20.5.0.278）、安装路径作为值数据，
    /// 而不是用"子键"。所以必须用 RegEnumValue 枚举值，不能用 RegEnumKeyEx 枚举子键。
    /// </summary>
    private static List<KeyValuePair<string, string>> EnumRegistryValues(string keyName)
    {
        var result = new List<KeyValuePair<string, string>>();
        int hKey = 0;
        uint r = RegOpenKeyEx(HKEY_LOCAL_MACHINE, keyName, 0, KEY_QUERY_VALUE | KEY_WOW64_64KEY, out hKey);
        if (r != 0 || hKey == 0) return result;

        uint idx = 0;
        while (true)
        {
            var name = new StringBuilder(256);
            uint nameLen = (uint)name.Capacity;
            var data = new StringBuilder(1024);
            uint dataLen = (uint)data.Capacity;
            uint type = 0;
            r = RegEnumValue(hKey, idx, name, ref nameLen, IntPtr.Zero, out type, data, ref dataLen);
            if (r != 0) break; // ERROR_NO_MORE_ITEMS 或其他
            result.Add(new KeyValuePair<string, string>(name.ToString(), data.ToString()));
            idx++;
        }
        RegCloseKey(hKey);
        return result;
    }

    private static void DumpEnvVar(StringBuilder sb, string name)
    {
        sb.AppendLine($"[环境变量] {name}:");
        string m = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
        string u = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
        string p = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        sb.AppendLine("  Machine: " + (string.IsNullOrEmpty(m) ? "(未设置)" : m));
        sb.AppendLine("  User   : " + (string.IsNullOrEmpty(u) ? "(未设置)" : u));
        sb.AppendLine("  Process: " + (string.IsNullOrEmpty(p) ? "(未设置)" : p));
        sb.AppendLine();
    }

    private static void DumpRegistry(StringBuilder sb)
    {
        string[] roots =
        {
            "SOFTWARE\\Side Effects Software\\Houdini",
            "SOFTWARE\\Side Effects Software\\Houdini Engine",
        };
        foreach (string root in roots)
        {
            sb.AppendLine($"[注册表] HKLM\\{root}");
            var values = EnumRegistryValues(root);
            if (values.Count == 0)
            {
                sb.AppendLine("  (无此键，或没有任何版本记录)");
            }
            else
            {
                foreach (var kv in values)
                {
                    string name = string.IsNullOrEmpty(kv.Key) ? "(默认值)" : kv.Key;
                    sb.AppendLine("  - " + name + "  =>  " + (string.IsNullOrEmpty(kv.Value) ? "(空值)" : kv.Value));
                }
            }
            sb.AppendLine();
        }
    }

    private static void CheckCriticalFiles(StringBuilder sb, string houdiniPath)
    {
        string bin = Path.Combine(houdiniPath, "bin");
        string[] files =
        {
            "libHAPIL.dll",   // HAPI 主库
            "libHARC.dll",    // HARC（启动 HARS 进程用）
            "HARS.exe",       // Houdini Engine 服务器
            "hython.exe",     // 用来看版本
            "hkey.exe",       // License Administrator（用来看 license 类型）
        };
        sb.AppendLine("  关键文件检查:");
        bool allOk = true;
        foreach (string f in files)
        {
            string full = Path.Combine(bin, f);
            bool ok = File.Exists(full);
            allOk &= ok;
            sb.AppendLine("    " + (ok ? "[OK]" : "[缺失]") + "  " + full);
        }
        if (!allOk)
            sb.AppendLine("    [!] 关键文件缺失，HEU 无法启动 HARS。");
    }
#endif

    private static void AppendConclusion(StringBuilder sb, string want)
    {
        // 把结论文字放在这里，输出引导用户对照
        sb.AppendLine("  对照上面结果：");
        sb.AppendLine("  1) 若[注册表]里没有 " + want + " 对应的版本键 → HEU 检测不到，这是报错 'Unable to detect' 的根因；");
        sb.AppendLine("  2) 若有 " + want + " 但[关键文件]缺失 → Houdini 安装不完整，重装 Houdini；");
        sb.AppendLine("  3) 若版本/文件都对，但仍 'Timed out waiting on server ready' → 大概率是 license 问题。");
        sb.AppendLine();
        sb.AppendLine("  [License 自查] 请打开 Houdini 的 License Administrator（或命令行运行 hkey）确认授权类型：");
        sb.AppendLine("    - Apprentice（学习版/免费）  → 不支持 Houdini Engine，必报错，需换 Indie/Core/FX/Commercial 并含 Engine 授权；");
        sb.AppendLine("    - Indie                        → 含 Houdini Engine Indie，可用（有面数等限制）；");
        sb.AppendLine("    - Core / FX / Commercial       → 需另外购买 Houdini Engine 授权；");
        sb.AppendLine("    - 未登录 / 未激活             → 登录 SideFX 账号并激活 license。");
        sb.AppendLine("  （把本日志 + license 类型截图发回，即可精准判断下一步。）");
    }
}
#endif
