// using UnityEditor;
// using UnityEngine;
//
// [InitializeOnLoad]
// public class DefaultMaterialShaderSetter : AssetModificationProcessor
// {
//     private static void OnWillCreateAsset(string assetPath)
//     {
//         if (!assetPath.EndsWith(".mat")) return;
//
//         EditorApplication.delayCall += () =>
//         {
//             // 确保材质已加载完成
//             var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
//             if (!mat) return;
//             // 获取材质的初始Shader
//             var initialShader = mat.shader;
//             if (!IsDefaultShader(initialShader)) return;
//             var customShader = Shader.Find($"TA Shaders/Default/Lit");
//             if (!customShader || Equals(mat.shader, customShader)) return;
//             // 替换Shader但不立即保存
//             mat.shader = customShader;
//             EditorUtility.SetDirty(mat);
//             // 延迟保存避免触发GUI刷新
//             EditorApplication.delayCall += AssetDatabase.SaveAssets;
//         };
//     }
//
//     private static bool IsDefaultShader(Shader shader)
//     {
//         if (!shader) return false;
//
//         var name = shader.name;
//         
//         var replaceList = new[]
//         {
//             "Standard",
//             "Universal Render Pipeline/Lit",
//             "Universal Render Pipeline/Simple Lit",
//             "Universal Render Pipeline/Unlit",
//             "Legacy Shaders/",
//         };
//
//         foreach (var prefix in replaceList)
//         {
//             if (name.StartsWith(prefix)) return true;
//         }
//
//         return false;
//     }
// }