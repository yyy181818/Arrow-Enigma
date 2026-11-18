using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class SearchWindows : EditorWindow
{
    [MenuItem ("Tools/资源相关/Search Reference")]
    public static void OpenWindow ()
    { 
        ((SearchWindows)GetWindow(typeof(SearchWindows))).search = Selection.activeObject;
    }
 
    [FormerlySerializedAs("Search")] public Object search;
    private readonly List<Object> _searchOut= new();
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label ("Search:", EditorStyles.boldLabel);
        search = EditorGUILayout.ObjectField(search, typeof(Object), true);
        EditorGUILayout.EndHorizontal();
 
        if (GUILayout.Button("Search!"))
        {
            if (search != null)
            {
                _searchOut.Clear();
                var @out = new List<string>();
                var path = AssetDatabase.GetAssetPath(search);
                if (!string.IsNullOrEmpty(path))
                {
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    var meta = AssetDatabase.GetTextMetaFilePathFromAssetPath(path);
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.WorkingDirectory = Application.dataPath;
                    p.StartInfo.FileName = $"{Application.dataPath}/TexaXRFramework/Editor/Tools/Search/ripgrep/rg.exe";
                    p.StartInfo.Arguments = $"-l {guid}";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    while (!p.StandardOutput.EndOfStream)
                    {
                        string line = $"Assets/{p.StandardOutput.ReadLine().Replace("\\","/")}";
                        if (line != meta)
                        {
                            var item = AssetDatabase.LoadAssetAtPath(line, typeof(Object));
                            if(item != null)
                                _searchOut.Add(item);
                        }
                    }
                }
            }
        }
        if (_searchOut.Count > 0)
        {
            GUILayout.Label("Out:", EditorStyles.boldLabel);
            foreach (var o in _searchOut)
            {
                EditorGUILayout.ObjectField(o, typeof(Object), true);
            }
        }
    }
}