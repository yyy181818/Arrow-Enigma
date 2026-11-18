using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using TexaXRFramework.Core;
using SharedModels;

[CustomEditor(typeof(LBEContentDefine))]
public class LBEContentDefineEditor : Editor
{
    private LBEContentDefine self;
    private void OnEnable()
    {
        self = target as LBEContentDefine;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (self.ContentName==""|| self.ContentName==string.Empty)
        {
            self.ContentName= PlayerSettings.productName;
        }
        GUILayout.Space(20);
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.green; // 设置字体颜色为红色
        if (GUILayout.Button("导出全部内容数据",buttonStyle))
        {
            ExportContentData();
            AssetDatabase.Refresh();
        }
    }

    private void ExportContentData()
    {
        // 遍历指定文件夹下的所有子文件夹
        string folderPath = "Assets\\Resources\\ContentConfig\\Stage";

        string parentDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(Application.dataPath));
        var saveToClientPath1 = $"Assets\\Resources\\ContentConfig\\AllStageConfigs.json";
        var saveToClientPath2 = $"Assets\\Resources\\ContentConfig\\{self.ContentName}.json";
        var saveToServerPath = $"\\LBEGameServer\\Config\\Content\\AllStageConfigs.json";

        saveToServerPath = parentDirectory + saveToServerPath;
        string directoryClientPath = Path.GetDirectoryName(saveToServerPath);
        if (!Directory.Exists(directoryClientPath))
        {
            Directory.CreateDirectory(directoryClientPath);
        }

        LBEContentData exportData = new LBEContentData()
        {
            ContentName = self.ContentName,
            StageList = new List<LBEStageData>()
        };

        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath);
            List<LBEStageData> tempStageList = new List<LBEStageData>();
            foreach (string file in files)
            {
                // 获取文件的名称
                string fileName = Path.GetFileName(file);
                // 检查文件是否为 .json 格式
                if (Path.GetExtension(fileName).ToLower() == ".json")
                {
                    // 读取文件内容
                    string jsonContent = File.ReadAllText(file);
                    // 反序列化 JSON 数据
                    var stageData = JsonConvert.DeserializeObject<LBEStageData>(jsonContent);

                    var eData = tempStageList.Find(x => x.StageName == stageData.StageName);
                    if (eData == null)
                    {
         
                        tempStageList.Add(stageData);
                        //File.Copy(Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), file), Path.Combine(directoryClientPath, fileName), true);
                        //Debug.Log("文件复制成功: " + file + " -> " + directoryClientPath);
    
                    }
                    else
                    {
                        Debug.LogError("关卡(场景名字重复):" + stageData.StageName);
                    }          
                }
            }

            exportData.StageList = tempStageList.OrderBy(x => x.Order).ToList();
            var jsonStr = JsonConvert.SerializeObject(exportData);
            File.WriteAllText(saveToClientPath1, jsonStr);
            File.WriteAllText(saveToClientPath2, jsonStr);
            File.WriteAllText(saveToServerPath, jsonStr);
            Debug.Log($"已导出内容数据");
        }
    }    
}
