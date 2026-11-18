using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    // Alpha-To-Coverage (A2C) GUI 参数块
    // A2C 硬件开关走 shader 内 AlphaToMask On（Pass 级固定状态，无法按关键字/marker 切换）；
    // 这里只设置 _ALPHATOMASK 关键字，由 shader 在 fragment 决定是否输出真实 alpha 来开启/关闭 A2C 效果。
    public struct AlphaToMaskInputs
    {
        private MaterialProperty _AlphaToMask;

        public AlphaToMaskInputs(MaterialProperty[] properties)
        {
            _AlphaToMask = BaseShaderGUI.FindProperty("_AlphaToMask", properties, false);
        }

        public void Draw(MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            materialEditor.DrawFloatToggleProperty(
                new GUIContent("Alpha To Mask", "Cutout 边缘抗锯齿（需 MSAA 开启）。开启后由 ForwardLit 的 AlphaToMask 驱动 MSAA 覆盖采样，平滑剪边、缓解锯齿抖动。"),
                _AlphaToMask);

            bool on = (_AlphaToMask != null) && (Mathf.Abs(_AlphaToMask.floatValue - 1.0f) < 1.0f);
            // 着色器侧：绕过 _Cutoff 硬剪，让 alpha 全范围驱动覆盖采样
            CoreUtils.SetKeyword(material, "_ALPHATOMASK", on);
            // 硬件侧开关走 shader 内 AlphaToMask On（Pass 级固定状态，无法按关键字切换）；
            // 是否生效完全由上面的 _ALPHATOMASK 关键字控制 fragment 输出真实 alpha 决定。

            EditorGUILayout.EndVertical();
        }
    }
}
