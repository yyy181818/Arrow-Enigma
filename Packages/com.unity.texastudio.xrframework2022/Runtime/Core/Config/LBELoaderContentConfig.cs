using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// LBE内容配置加载器
    /// </summary>
    public class LBELoaderContentConfig
    {
        private static Dictionary<string, LBEStageData> mapData = new Dictionary<string, LBEStageData>();

        public static void OnInit(string path)
        {
            string config = "";
            var jsonStr = UnityEngine.Resources.Load(path) as UnityEngine.TextAsset;
            if (jsonStr == null || string.IsNullOrEmpty(jsonStr.text))
            {
                UnityEngine.Debug.LogError("[配置文件] 找不到地图:" + path);
                return;
            }
            config = jsonStr.text;

            try
            {
                if (!string.IsNullOrEmpty(config))
                {
                    var sData = JsonConvert.DeserializeObject<LBEContentData>(config);
                    if (sData != null)
                    {
                        foreach (var stage in sData.StageList)
                        {
                            if (!mapData.ContainsKey(stage.StageName))
                            {
                                mapData.Add(stage.StageName, stage);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                Debug.LogError("[配置文件] 加载地图出错:" + e);
            }
        }

        public static LBEStageData GetStageData(string mapName)
        {
            if (mapData.TryGetValue(mapName, out var map))
            {
                return map;
            }
            return null;
        }
        /// <summary>
        /// 获取首个区域
        /// </summary>
        /// <param name="stageName">场景名</param>
        /// <returns>区域数据</returns>
        public static LBEAreaData GetFirstArea(string stageName)
        {
            var data = GetStageData(stageName);
            if (data != null && data.AreaList.Count > 0)
            {
                return data.AreaList[0];
            }

            return null;
        }


        /// <summary>
        /// 获取下一个区域
        /// </summary>
        /// <param name="stageName">场景名</param>
        /// <param name="areaName">当前区域名字</param>
        /// <returns>区域数据</returns>
        public static LBEAreaData GetNextArea(string stageName, string areaName)
        {
            var data = GetStageData(stageName);
            int index = 0;
            if (data != null && data.AreaList.Count > 0)
            {
                index = data.AreaList.FindIndex(item => item.AreaName == areaName);
                index++;
                if (index > data.AreaList.Count - 1)
                {
                    index = data.AreaList.Count - 1;
                }
                return data.AreaList[index];
            }
            return null;
        }

        /// <summary>
        /// 获取区域数据
        /// </summary>
        /// <param name="stageName">场景名</param>
        /// <param name="areaName">区域名</param>
        /// <returns>区域数据</returns>
        public static LBEAreaData GetArea(string stageName, string areaName)
        {
            var data = GetStageData(stageName);
            if (data != null)
            {
                return data.AreaList.Find((l) => l.AreaName.Equals(areaName));
            }
            return null;
        }
    }
}
