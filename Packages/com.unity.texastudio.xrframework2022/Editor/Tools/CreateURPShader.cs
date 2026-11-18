using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;

public class CreateURPShader
{
    private static readonly string ShaderTemplatePath =
        $"{Path.GetFullPath("Packages/com.unity.texastudio.xrframework2022")}/Assets/Shaders/Template/URPShader.shader";

    [MenuItem("Assets/Create/Shader/URP Shader")]
    public static void CreatUrpShaderTemplate()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<UrpShaderAsset>(),
            GetSelectedPathOrFallback() + "/URPShader.shader",
            null,
            ShaderTemplatePath);
    }


    //获取选择的路径
    private static string GetSelectedPathOrFallback()
    {
        var path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
            path = Path.GetDirectoryName(path);
            break;
        }

        return path;
    }
}


internal class UrpShaderAsset : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        var o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    private static Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
    {
        var fullPath = Path.GetFullPath(pathName);
        var streamReader = new StreamReader(resourceFile);
        var text = streamReader.ReadToEnd(); //读取模板内容
        streamReader.Close();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
        text = Regex.Replace(text, "URPShader", fileNameWithoutExtension); //将模板的名字 替换成文件名
        if (text.Contains("Hidden/Template"))
        {
            text = text.Replace("Hidden/Template", "TA Shaders");
        }

        var encoderShouldEmitUTF8Identifier = true;
        var throwOnInvalidBytes = false;
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        var append = false;
        var streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }
}