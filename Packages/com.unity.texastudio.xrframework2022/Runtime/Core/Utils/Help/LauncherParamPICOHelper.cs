using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NS_LBEShareData;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// 命令行参数获取 & 解析工具类
    /// </summary>
    public static class LauncherParamPICOHelper
    {
        public const string VenueIDIndex = "venueID";//场馆ID（Saas下发）
        public const string RoomNameIndex = "roomName";//场馆中创建的房间名
        public const string ContentIDIndex = "contentID";//内容id
        public const string ContentNameIndex = "contentName";//内容名称
        public const string ContentVersionIndex = "contentVersion";//内容版本 Eg:1.0.0
        public const string SecretKeyIndex = "secretKey";//内容密钥
        public const string CheckFrequencyIndex = "checkFrequency";//校验频次
        public const string StartTimeIndex = "startTime";//创建队伍时间
        public const string TeamIDIndex = "teamOriginID";//队伍id
        public const string TeamNameIndex = "teamName";//队伍名
        public const string TeamCount = "teamCount";//队伍人数
        public const string PlayerIDIndex = "playerOriginID";//玩家id
        public const string NickIndex = "playerNick";//玩家昵称
        public const string JsonData = "jsonData";

        public const int ServerPort = 8555;
        public const int BridgeServerPort = 7799;

        /// <summary>
        /// 通过启动传参获取用户nick
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetLauncherParam(string[] keys)
        {
            if (keys == null)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> results = new Dictionary<string, string>(keys.Length);
            Debug.Log($"[参数] 开始获取启动参数:");
#if UNITY_ANDROID && !UNITY_EDITOR
         AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if (unityPlayer == null)
        {
            return results;
        }
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (currentActivity == null)
        {
            return results;
        }

       
        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        if (intent != null)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (intent.Call<bool>("hasExtra", keys[i]))
                {
                    string extraData = intent.Call<string>("getStringExtra",  keys[i]);
                    Debug.Log($"[参数] key:{ keys[i]}, extraData: {extraData}");
                    results.Add(keys[i], extraData);
                }
            }
        }

#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
        string[] param = Environment.GetCommandLineArgs();
        if (param != null && param.Length > 1)
        {
            for (int i = 1; i < param.Length - 1; i += 2)
            {
                var key = param[i];
                var value = param[i + 1];
                Debug.Log($"[参数] key:{key} - value:{value}");
                if (!results.ContainsKey(key))
                {
                    results.Add(key, value);
                }
                else
                {
                    Debug.LogError($"key:{key} repeated");
                }
            }
        }  

#endif
            return results;
        }

        public static Dictionary<string, string> GetLauncherParam()
        {
            string[] paramKey = new[]
            {
            VenueIDIndex,TeamNameIndex, TeamIDIndex, NickIndex,
            TeamCount, PlayerIDIndex, ContentNameIndex,
            ContentIDIndex,CheckFrequencyIndex,SecretKeyIndex, StartTimeIndex,
            ContentVersionIndex, JsonData
        };
            Dictionary<string, string> paramDic = GetLauncherParam(paramKey);
            if (paramDic.Count < 1)
            {
                paramDic = CreateDefaultParam();
                Debug.Log("[参数] 启播参数为空，使用默认参数启动");
            }
            Debug.Log("---------- 启播参数 ----------");
            foreach (var key in paramDic.Keys)
            {
                Debug.Log($"[参数] {key} : {paramDic[key]}");
            }
            Debug.Log("------------------------------");
            return paramDic;
        }
        public static Dictionary<string, string> CreateDefaultParam()
        {
            LauncherParamPICOSettings settings = Resources.Load<LauncherParamPICOSettings>("LauncherParam/LauncherParamPICOSettings");
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (settings != null)
            {
                results.Add(VenueIDIndex, settings.venueID);
                results.Add(RoomNameIndex, settings.roomName);
                results.Add(ContentIDIndex, settings.contentID);
                results.Add(ContentNameIndex, settings.contentName);
                results.Add(ContentVersionIndex, settings.contentVersion);
                results.Add(SecretKeyIndex, settings.secretKey);
                results.Add(CheckFrequencyIndex, settings.checkFrequency);
                results.Add(StartTimeIndex, $"{(long)DateTime.Now.TimeOfDay.TotalSeconds}");
                results.Add(TeamIDIndex, settings.teamOriginID);
                results.Add(TeamNameIndex, "Team" + settings.teamOriginID);
                results.Add(TeamCount, settings.teamCount);
                results.Add(PlayerIDIndex, settings.playerOriginID);
                results.Add(NickIndex, $"P-{settings.playerOriginID}");
                var jd = new ContentArgsDataJsonData(settings.serverIP, settings.serverPort, settings.bridgeServerIP, settings.bridgeServerPort, settings.fileUpdateURL, settings.telephone, settings.iconLink,
                    settings.maxServerIP, settings.maxServerPort, settings.maxConfigUpdateURL, settings.avatarResourceName, settings.avatarSkinResourceName);
                results.Add(JsonData, JsonConvert.SerializeObject(jd));
            }
            else
            {
                Debug.LogError("[参数] 无法加载 LauncherParamSettings ScriptableObject 文件。请检查路径和文件是否存在。");
            }
            return results;
        }
    }
}