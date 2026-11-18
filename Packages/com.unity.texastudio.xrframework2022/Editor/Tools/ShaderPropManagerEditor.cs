#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShaderPropManager))]
public class ShaderPropManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var script = (ShaderPropManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));

        var currentMode = (TargetMode)serializedObject.FindProperty("mode").enumValueIndex;

        if (currentMode == TargetMode.RendererParams)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetRenderers"), true);
            if (GUILayout.Button("Auto Find Children Renderers"))
            {
                script.targetRenderers.Clear();
                script.GetComponentsInChildren(true, script.targetRenderers);
            }
        }
        else if (currentMode == TargetMode.SpecificMaterial)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("specificMaterial"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Properties List", EditorStyles.boldLabel);

        // 绘制列表
        var list = serializedObject.FindProperty("properties");
        for (var i = 0; i < list.arraySize; i++)
        {
            var item = list.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Prop {i + 1}", EditorStyles.miniBoldLabel, GUILayout.Width(50));
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                list.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(item.FindPropertyRelative("propertyName"));
            EditorGUILayout.PropertyField(item.FindPropertyRelative("isColor"));

            var sourceProp = item.FindPropertyRelative("source");
            EditorGUILayout.PropertyField(sourceProp);

            // 根据选择显示不同的输入框
            var source = (ValueSource)sourceProp.enumValueIndex;
            if (source == ValueSource.Constant)
            {
                EditorGUILayout.PropertyField(item.FindPropertyRelative("isColor").boolValue
                    ? item.FindPropertyRelative("constantColor")
                    : item.FindPropertyRelative("constantValue"));
            }
            else
            {
                EditorGUILayout.HelpBox($"Controlled by {source}", MessageType.Info);
                if (item.FindPropertyRelative("isColor").boolValue)
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("constantColor"),
                        new GUIContent("Base Color Tint"));
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Add New Property"))
        {
            list.InsertArrayElementAtIndex(list.arraySize);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug / Timeline Channels", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channelA"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channelB"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channelC"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channelD"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif