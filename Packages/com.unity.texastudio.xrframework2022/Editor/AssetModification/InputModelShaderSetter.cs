using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public class InputModelShaderSetter : AssetPostprocessor
{
    private void OnPostprocessModel(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials;
            foreach (var mat in materials)
            {
                if (!mat) continue;

                var customShader = Shader.Find($"TA Shaders/Default/Lit");
                if (!customShader || Equals(mat.shader, customShader)) continue;

                // 记录材质属性
                var mainTex = mat.mainTexture;
                var color = mat.color;
                var metallic = mat.GetFloat("_Metallic");
                var smoothness = mat.GetFloat("_Glossiness");

                // 替换Shader
                mat.shader = customShader;
                mat.SetTexture("_BaseMap", mainTex);
                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);

                EditorUtility.SetDirty(mat);
            }
        }

        // 延迟保存避免在导入过程中触发GUI
        EditorApplication.delayCall += AssetDatabase.SaveAssets;
    }
}