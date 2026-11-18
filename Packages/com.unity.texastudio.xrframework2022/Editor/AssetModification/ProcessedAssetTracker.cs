#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ProcessedAssetTracker
{
//     private const string SAVE_FILE = "Assets/Editor/ProcessedAssets.txt";
//     private static HashSet<string> _processedGuids = new HashSet<string>();
//
//     static ProcessedAssetTracker()
//     {
//         // 加载已处理资源的 GUID
//         if (File.Exists(SAVE_FILE))
//         {
//             string[] guids = File.ReadAllLines(SAVE_FILE);
//             foreach (string guid in guids)
//             {
//                 _processedGuids.Add(guid);
//             }
//         }
//     }
//
//     // 判断资源是否已处理
//     public static bool IsProcessed(string assetPath)
//     {
//         string guid = AssetDatabase.AssetPathToGUID(assetPath);
//         return _processedGuids.Contains(guid);
//     }
//
//     // 标记资源为已处理
//     public static void MarkAsProcessed(string assetPath)
//     {
//         string guid = AssetDatabase.AssetPathToGUID(assetPath);
//         if (!_processedGuids.Contains(guid))
//         {
//             _processedGuids.Add(guid);
//             File.AppendAllText(SAVE_FILE, guid + "\n");
//         }
//     }
}
#endif