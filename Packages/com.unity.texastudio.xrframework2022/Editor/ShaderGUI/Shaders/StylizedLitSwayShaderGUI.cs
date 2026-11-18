using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    // Stylized Lit + 顶点摇摆（风）专用 GUI
    // 继承原 StylizedLitShader，保留全部原有面板，并在 Advanced 区追加摇摆控制
    internal class StylizedLitSwayShaderGUI : StylizedLitShader
    {
        private SwayInputs _swayInputs;
        private AlphaToMaskInputs _alphaToMaskInputs;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _swayInputs = new SwayInputs(properties);
            _alphaToMaskInputs = new AlphaToMaskInputs(properties);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            base.DrawAdvancedOptions(material);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Vertex Sway (Wind)", MessageType.None);

            EditorGUI.BeginChangeCheck();
            _swayInputs.Draw(materialEditor, material);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateMaterial(material);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Alpha To Coverage (Cutout AA)", MessageType.None);

            EditorGUI.BeginChangeCheck();
            _alphaToMaskInputs.Draw(materialEditor, material);
            if (EditorGUI.EndChangeCheck())
            {
                ValidateMaterial(material);
            }
        }
    }
}
