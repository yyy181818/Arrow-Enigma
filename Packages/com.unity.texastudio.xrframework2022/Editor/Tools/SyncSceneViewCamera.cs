using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneGameViewSync
{
    private const string MenuSyncSceneToGame = "Tools/同步 Scene → Game";
    
    private static bool _syncSceneToGame;

    static SceneGameViewSync()
    {
        _syncSceneToGame = EditorPrefs.GetBool(MenuSyncSceneToGame, false);

        EditorApplication.update += Update;
        Apply();
    }
    

    [MenuItem(MenuSyncSceneToGame)]
    private static void ToggleSceneToGame()
    {
        _syncSceneToGame = !_syncSceneToGame;
        EditorPrefs.SetBool(MenuSyncSceneToGame, _syncSceneToGame);
        Apply();
    }

    private static void Apply()
    {
        Menu.SetChecked(MenuSyncSceneToGame, _syncSceneToGame);
    }

    private static void Update()
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (!sceneView) return;

        var cam = Camera.main;
        if (!cam) return;
        
        // ✅ Scene → Game
        if (!_syncSceneToGame) return;
        var svCam = sceneView.camera;
        if (!svCam) return;

        Undo.RecordObject(cam.transform, "Sync Scene To Game");

        cam.transform.position = svCam.transform.position;
        cam.transform.rotation = svCam.transform.rotation;

        // 同步FOV（可选）
        cam.fieldOfView = svCam.fieldOfView;
    }
}