using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationClipDispose : EditorWindow
{
    [SerializeField] private List<AnimationClip> animationClipList = new();

    private SerializedObject _serializedObject;
    private SerializedProperty _serializedProperty;

    private Vector2 _vector2;

    private bool _useF2;

    private bool _isCullScale;

    [MenuItem("Tools/动画相关/动画压缩工具")]
    private static void Init()
    {
        var window = GetWindow<AnimationClipDispose>();
        window.Show();
    }

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        _serializedProperty = _serializedObject.FindProperty("animationClipList");
    }

    private void OnGUI()
    {
        _vector2 = EditorGUILayout.BeginScrollView(_vector2);
        _serializedObject.Update();
        EditorGUILayout.PropertyField(_serializedProperty);
        _serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
        GUILayout.Space(10);
        if (GUILayout.Button("获取工程内所有Animation Clip", new GUIStyle(GUI.skin.button) { fontSize = 15 }))
        {
            GetAllAnimationClips();
        }
        GUILayout.Space(20);
        GUILayout.Label("压缩选项(使用后可能会导致动画变形！慎用！！))", new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold});
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(5);
        _useF2 = EditorGUILayout.Toggle("使用f2精度", _useF2, new GUIStyle(GUI.skin.toggle) { fontSize = 20 });
        GUILayout.Space(5);
        _isCullScale =
            EditorGUILayout.Toggle("剔除Scale曲线", _isCullScale, new GUIStyle(GUI.skin.toggle) { fontSize = 20 });
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("压缩！", new GUIStyle(GUI.skin.button) { fontSize = 20 }))
        {
            CompressedAnimationClip();
        }
    }

    private void GetAllAnimationClips()
    {
        animationClipList.Clear();
        var allClipsGuid = AssetDatabase.FindAssets("t:AnimationClip");
        if (allClipsGuid.Length <= 0)
        {
            EditorUtility.DisplayDialog("提示", "当前工程内没有AnimationClip！！！", "确定");
        }
        else
        {
            foreach (var guid in allClipsGuid)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                animationClipList.Add(animationClip);
            }

            Debug.Log("共获取到:" + animationClipList.Count + "个AnimationClip");
        }
    }

    private void CompressedAnimationClip()
    {
        if (animationClipList.Count <= 0 || animationClipList is null)
        {
            Debug.Log("先获取AnimationClip！！！");
            return;
        }

        foreach (var theAnimation in animationClipList)
        {
            EditorUtility.DisplayProgressBar("剔除异常量和无限接近0的曲线", theAnimation.name,
                (float)animationClipList.IndexOf(theAnimation) / animationClipList.Count);
            foreach (var theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
            {
                var curve = AnimationUtility.GetEditorCurve(theAnimation, theCurveBinding);
                var keys = curve.keys;
                var removeIdxs = new List<int>();
                for (var i = 0; i < keys.Length; i++)
                {
                    var keyframe = keys[i];
                    // 是否是异常量关键帧或者无限接近0
                    if ((float.IsInfinity(keyframe.inTangent) && float.IsInfinity(keyframe.outTangent))
                        || (Mathf.Abs(keyframe.inTangent) <= float.Epsilon &&
                            Mathf.Abs(keyframe.outTangent) <= float.Epsilon))
                    {
                        removeIdxs.Add(i);
                    }
                }

                // 移除
                for (var r = removeIdxs.Count - 1; r >= 0; --r)
                {
                    curve.RemoveKey(removeIdxs[r]);
                }

                // 如果curve的关键帧已经没有了，直接移除空的curve，重新指定
                AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, curve.length == 0 ? null : curve);
            }

            EditorUtility.SetDirty(theAnimation);
            AssetDatabase.SaveAssets();
        }

        EditorUtility.ClearProgressBar();
        // 剔除Scale曲线
        if (_isCullScale)
        {
            foreach (var theAnimationClip in animationClipList)
            {
                EditorUtility.DisplayProgressBar("剔除Scale曲线", theAnimationClip.name,
                    (float)animationClipList.IndexOf(theAnimationClip) / animationClipList.Count);
                var binds = AnimationUtility.GetCurveBindings(theAnimationClip);
                foreach (var bind in binds)
                {
                    if (bind.propertyName.Contains("Scale"))
                    {
                        AnimationUtility.SetEditorCurve(theAnimationClip, bind, null);
                    }
                }

                EditorUtility.SetDirty(theAnimationClip);
                AssetDatabase.SaveAssets();
            }

            EditorUtility.ClearProgressBar();
            Debug.Log("剔除Scale曲线完成");
        }

        foreach (var theAnimationClip in animationClipList)
        {
            EditorUtility.DisplayProgressBar("压缩AnimationClip", theAnimationClip.name,
                (float)animationClipList.IndexOf(theAnimationClip) / animationClipList.Count);
            var assetPath = AssetDatabase.GetAssetPath(theAnimationClip);
            try
            {
                var binds = AnimationUtility.GetCurveBindings(theAnimationClip);
                foreach (var bind in binds)
                {
                    
                    var curve = AnimationUtility.GetEditorCurve(theAnimationClip, bind);
                    var keys = curve.keys;
                    for (var i = 0; i < keys.Length; i++)
                    {
                        var keyframe = keys[i];
                        var format = _useF2 ? "f2" : "f3";
                        keyframe.value = float.Parse(keyframe.value.ToString(format));
                        keyframe.inTangent = float.Parse(keyframe.inTangent.ToString(format));
                        keyframe.outTangent = float.Parse(keyframe.outTangent.ToString(format));
                        keys[i] = keyframe;
                    }

                    curve.keys = keys;
                    AnimationUtility.SetEditorCurve(theAnimationClip, bind, curve);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"CompressAnimationClip Failed !!! animationPath : {assetPath} error: {e}");
            }

            EditorUtility.SetDirty(theAnimationClip);
            AssetDatabase.SaveAssets();
        }
        EditorUtility.ClearProgressBar();
        Debug.Log("动画优化完成");
    }
}