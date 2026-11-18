using System;
using UnityEditor;
using UnityEngine;

public class CullScaleCurves : EditorWindow
{
    [MenuItem("Tools/动画相关/剔除Scale曲线")]
    public static void GetWindow()
    {
        var window = GetWindow<CullScaleCurves>();
        window.titleContent = new GUIContent("剔除Scale曲线");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("请选中要剔除Scale曲线的Object");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("剔除", new GUIStyle(GUI.skin.button) { fontSize = 15 }))
        {
            CullCurves();
        }
    }

    private static void CullCurves()
    {
        var animationGo = Selection.activeObject;
        if (animationGo == null)
        {
            Debug.LogError("Please select a GameObject");
            return;
        }
        if (animationGo.GetType() != typeof(AnimationClip)) return;
        var clip = animationGo as AnimationClip;
        var binds = AnimationUtility.GetCurveBindings(clip);
        foreach (var bind in binds)
        {
            if (bind.propertyName.Contains("Scale"))
            {
                AnimationUtility.SetEditorCurve(clip, bind, null);
            }
        }

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
    }
}