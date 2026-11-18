using UnityEditor;
using UnityEngine;

public class CustomMaterialPostprocessor : AssetPostprocessor
{
    private void OnPostprocessMaterial(Material material)
    {
        var customShader = Shader.Find($"TA Shaders/Default/Lit");
        if (!customShader || Equals(material.shader, customShader)) return;
        material.shader = customShader;
        EditorUtility.SetDirty(material);
    }
}