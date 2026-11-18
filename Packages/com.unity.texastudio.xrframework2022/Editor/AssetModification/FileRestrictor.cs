using UnityEditor;
using UnityEngine;
using System.IO;
using System;
//[InitializeOnLoad]
// public class FileRestrictor : AssetPostprocessor
// {
//     // 限制的格式列表
//     private static readonly string[] BlockedExtensions = { ".tiff", ".psd",".tif" };
//     // 限制的网格数量
//     private const int MAX_TRIANGLES = 10000;
//     // 限制的贴图大小
//     private const int MAX_TEXTURE_SIZE = 4096;
//     
//     private static DateTime _lastUserInteractionTime;
//     
//     private static bool _isGitPullProcessed = false;
//     [InitializeOnLoad]
//     public class UserInteractionTracker
//     {
//         static UserInteractionTracker()
//         {
//             // 监听鼠标和键盘事件（仅示例）
//             EditorApplication.update += () =>
//             {
//                 if (Event.current != null && 
//                     (Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown))
//                 {
//                     _lastUserInteractionTime = DateTime.Now;
//                 }
//             };
//         }
//     }
//     
//     // 限制贴图
//     private void OnPreprocessAsset()
//     {
//         string extension = Path.GetExtension(assetPath).ToLower();
//         //Debug.Log("111111");
//         if (!assetPath.StartsWith("Assets/"))
//             return;
//         // 跳过 Git 拉取的资源
//         string gitFlagPath = Path.Combine(Application.dataPath, "git_pull.flag");
//         if (File.Exists(gitFlagPath)&& !_isGitPullProcessed)
//         {
//             _isGitPullProcessed = true;
//             File.Delete(gitFlagPath);    // 删除标记文件
//             AssetDatabase.Refresh();
//             //Debug.LogWarning("跳过Git");
//             return;                     
//         }
//         //跳过手动修改的资源
//         string fullAssetPath = Path.GetFullPath(assetImporter.assetPath);
//         bool isExistingAsset = File.Exists(fullAssetPath);
//         string fullMetaPath = fullAssetPath + ".meta";
//         if ((DateTime.Now - _lastUserInteractionTime).TotalSeconds < 2)
//         {
//             if (isExistingAsset)
//             {
//                 //Debug.LogWarning("跳过修改");
//                 return; 
//             }
//         }
//         
//         // --- 跳过已处理的资源 ---
//         if (ProcessedAssetTracker.IsProcessed(assetImporter.assetPath))
//         {
//             //Debug.Log($"[检测跳过] 资源已处理: {assetImporter.assetPath}");
//             return;
//         }
//         
//         if (!File.Exists(fullAssetPath) && File.Exists(fullMetaPath))
//         {
//             return; 
//         }
//         
//
//         // 检查是否属于被禁止的格式
//         if (System.Array.Exists(BlockedExtensions, ext => ext == extension))
//         {
//             // 弹出警告并取消导入
//             int  result = EditorUtility.DisplayDialogComplex(
//                 "非法文件格式",
//                 $"不建议导入 {extension} 文件，请转换为.tga或.png重新到导入\n路径: {assetPath}",
//                 "取消导入",
//                 "关闭",
//                 "强制导入（不推荐）"
//             );
//
//             if  (result == 0 || result == 1) 
//             {
//                 // 删除文件并终止导入
//                 File.Delete(assetPath);
//                 AssetDatabase.Refresh();
//                 throw new System.Exception($"已阻止导入 {extension} 文件！");
//             }
//         }
//         else
//         {
//             Vector2Int originalSize = GetOriginalTextureSize(assetPath);
//             if (originalSize.x <= MAX_TEXTURE_SIZE && originalSize.y <= MAX_TEXTURE_SIZE) return;
//            
//             int result = EditorUtility.DisplayDialogComplex(
//                 "贴图尺寸超限",
//                 $"贴图尺寸 {originalSize.x}x{originalSize.y} 超过限制，非重要资源建议缩小尺寸{MAX_TEXTURE_SIZE}x{MAX_TEXTURE_SIZE}及以下重新导入 \n路径：{assetPath}",
//                 "取消导入",   
//                 "关闭", 
//                 "强制导入（是重要资源）"    
//             );
//             if  (result == 0 || result == 1) 
//             {
//                 // 删除文件并刷新数据库
//                 File.Delete(assetPath);
//                 AssetDatabase.Refresh();
//                 Debug.LogError($"已阻止导入超限贴图：{assetPath}");
//             }
//             else
//             {
//                 Debug.LogWarning($"允许保留超限贴图：{assetPath}");
//             }
//             
//         }
//     }
//     // 限制网格
//     private void OnPostprocessModel(GameObject modelRoot)
//     {
//         // 检查是否为 FBX 文件
//         string extension = Path.GetExtension(assetPath).ToLower();
//         if (extension != ".fbx") return;
//         if (!assetPath.StartsWith("Assets/"))
//             return;
//         
//         // 跳过 Git 拉取的资源
//         string gitFlagPath = Path.Combine(Application.dataPath, "git_pull.flag");
//         if (File.Exists(gitFlagPath)&& !_isGitPullProcessed)
//         {
//             _isGitPullProcessed = true;
//             File.Delete(gitFlagPath);    // 删除标记文件
//             AssetDatabase.Refresh();
//             //Debug.LogWarning("跳过Git");
//             return;                     
//         }
//         
//         //跳过手动修改的资源
//         string fullAssetPath = Path.GetFullPath(assetImporter.assetPath);
//         bool isExistingAsset = File.Exists(fullAssetPath);
//         string fullMetaPath = fullAssetPath + ".meta";
//         if ((DateTime.Now - _lastUserInteractionTime).TotalSeconds < 2)
//         {
//             if (isExistingAsset)
//             {
//                 Debug.LogWarning("跳过修改");
//                 return; 
//             }
//         }
//         // --- 跳过已处理的资源 ---
//         if (ProcessedAssetTracker.IsProcessed(assetImporter.assetPath))
//         {
//             //Debug.Log($"[检测跳过] 资源已处理: {assetImporter.assetPath}");
//             return;
//         }
//         
//         if (!File.Exists(fullAssetPath) && File.Exists(fullMetaPath))
//         {
//             return; 
//         }
//
//         // 计算总三角形数量
//         int totalTriangles = CalculateTotalTriangles(modelRoot);
//         //int triangleCount = GetTriangleCount(assetPath);
//        
//         // 检查是否超限
//         if (totalTriangles > MAX_TRIANGLES)
//         {
//             // 弹出对话框提示用户
//             int  result  = EditorUtility.DisplayDialogComplex(
//                 "模型面数超限",
//                 $"该模型面数 {totalTriangles} 超过限制 {MAX_TRIANGLES},非重要资源不建议导入。请减面或优化之后重新导入\n路径：{assetPath}",
//                 "取消导入",
//                 "关闭",
//                 "强制导入（是重要资源）"
//             );
//
//             if  (result == 0 || result == 1) 
//             {
//                 // 删除文件并刷新数据库
//                 File.Delete(assetPath);
//                 AssetDatabase.Refresh();
//                 Debug.LogError($"已阻止导入面数超限的 FBX 文件：{assetPath}");
//             }
//             else
//             {
//                 Debug.LogWarning($"允许保留超限模型：{assetPath}");
//             }
//         }
//     }
//
//     // 计算模型总三角形数
//     private int CalculateTotalTriangles(GameObject root)
//     {
//         int triangles = 0;
//         MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
//         SkinnedMeshRenderer[] skinnedMeshes = root.GetComponentsInChildren<SkinnedMeshRenderer>();
//
//         // 处理普通网格
//         foreach (MeshFilter filter in meshFilters)
//         {
//             if (filter.sharedMesh != null)
//             {
//                 triangles += filter.sharedMesh.triangles.Length / 3;
//             }
//         }
//
//         // 处理蒙皮网格
//         foreach (SkinnedMeshRenderer renderer in skinnedMeshes)
//         {
//             if (renderer.sharedMesh != null)
//             {
//                 triangles += renderer.sharedMesh.triangles.Length / 3;
//             }
//         }
//
//         return triangles;
//     }
//     
//     // 获取原始贴图尺寸
//     private Vector2Int GetOriginalTextureSize(string path)
//     {
//         try
//         {
//             byte[] fileData = File.ReadAllBytes(path);
//             Texture2D tempTex = new Texture2D(2, 2);
//             tempTex.LoadImage(fileData); 
//             return new Vector2Int(tempTex.width, tempTex.height);
//         }
//         catch
//         {
//             return new Vector2Int(0, 0);
//         }
//     }
//
//}