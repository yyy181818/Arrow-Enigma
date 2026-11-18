using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class VibeLabExport
{
    public static void Precheck()
    {
        Debug.Log("VibeLabPrecheck: project=" + System.IO.Directory.GetCurrentDirectory());
        Debug.Log("VibeLabPrecheck: unity=" + Application.unityVersion);
        Debug.Log("VibeLabPrecheck: activeTarget=" + EditorUserBuildSettings.activeBuildTarget);
        Debug.Log("VibeLabPrecheck: applicationId=" + Application.identifier);
        Debug.Log("VibeLabPrecheck: enabledScenes=" + EditorBuildSettings.scenes.Count(s => s.enabled));
        Debug.Log("VibeLabPrecheck: androidSupported=" + BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android));
        EditorApplication.Exit(0);
    }

    public static void Export()
    {
        string outDir = System.Environment.GetEnvironmentVariable("VIBELAB_EXPORT_DIR");
        if (string.IsNullOrEmpty(outDir)) { Debug.LogError("VIBELAB_EXPORT_DIR not set"); EditorApplication.Exit(2); return; }
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) { Debug.LogError("no enabled scenes"); EditorApplication.Exit(2); return; }

        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outDir,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("VibeLabExport result=" + report.summary.result + " errors=" + report.summary.totalErrors);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
