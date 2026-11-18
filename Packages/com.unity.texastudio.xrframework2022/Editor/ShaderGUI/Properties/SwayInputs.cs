using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    // 顶点摇摆（风）GUI 参数块
    public struct SwayInputs
    {
        private MaterialProperty _Sway;
        private MaterialProperty _SwayAmount;
        private MaterialProperty _SwaySpeed;
        private MaterialProperty _SwayFrequency;
        private MaterialProperty _SwayHeight;
        private MaterialProperty _SwayByVertexColor;

        public SwayInputs(MaterialProperty[] properties)
        {
            _Sway = BaseShaderGUI.FindProperty("_Sway", properties, false);
            _SwayAmount = BaseShaderGUI.FindProperty("_SwayAmount", properties, false);
            _SwaySpeed = BaseShaderGUI.FindProperty("_SwaySpeed", properties, false);
            _SwayFrequency = BaseShaderGUI.FindProperty("_SwayFrequency", properties, false);
            _SwayHeight = BaseShaderGUI.FindProperty("_SwayHeight", properties, false);
            _SwayByVertexColor = BaseShaderGUI.FindProperty("_SwayByVertexColor", properties, false);
        }

        public void Draw(MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            materialEditor.DrawFloatToggleProperty(new GUIContent("Enable Sway", "开启顶点摇摆（树木 / 植被）"), _Sway);
            bool swayOn = (_Sway != null) && (Mathf.Abs(_Sway.floatValue - 1.0f) < 1.0f);
            CoreUtils.SetKeyword(material, "_SWAY", swayOn);

            if (swayOn)
            {
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(_SwayAmount, new GUIContent("Sway Strength (top)", "顶部最大位移（≈世界单位）"));
                materialEditor.ShaderProperty(_SwaySpeed, new GUIContent("Wind Speed", "摆动快慢（时间频率）"));
                materialEditor.ShaderProperty(_SwayFrequency, new GUIContent("Wind Variation", "风场在世界里变化的疏密 / 邻近树错位程度"));
                materialEditor.ShaderProperty(_SwayHeight, new GUIContent("Model Height", "高度法归一化分母（顶点色关时才用）"));
                materialEditor.DrawFloatToggleProperty(new GUIContent("Use Vertex Color Weight", "用顶点色 Blue 通道当权重（Houdini 批量烘）"), _SwayByVertexColor);
                bool vcolorOn = (_SwayByVertexColor != null) && (Mathf.Abs(_SwayByVertexColor.floatValue - 1.0f) < 1.0f);
                CoreUtils.SetKeyword(material, "_SWAY_VERTEXCOLOR", vcolorOn);
                EditorGUI.indentLevel--;
            }
            else
            {
                // 关闭主开关时，子关键字也强制关掉
                CoreUtils.SetKeyword(material, "_SWAY_VERTEXCOLOR", false);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
