using NS_LBEShareData;
using System;
using System.Collections;
using System.Collections.Generic;
using TexaXRFramework.Core;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// 命令行参数获取 & 解析工具类
    /// </summary>
    public static class LauncherParamHelper
    {
        public const string ContentNameIndex = "contentName";//内容名称
        public const string StartTimeIndex = "startTime";//创建队伍时间
        public const string TeamIDIndex = "teamOriginID";//队伍id
        public const string PlayerIDIndex = "playerOriginID";//玩家id
        public const string NickIndex = "playerNick";//玩家昵称
        public const string CharacterName = "characterName";//玩家角色;
        public const string TeamNumber = "teamNumber";//队内编号
        public const string ServerIP = "serverIP";//内容服务器IP
        public const string ServerPort = "serverPort";//内容服务器端口
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
             {ContentNameIndex, StartTimeIndex,TeamIDIndex, PlayerIDIndex,NickIndex,CharacterName, TeamNumber, ServerIP, ServerPort };
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
            LauncherParamSettings settings = Resources.Load<LauncherParamSettings>("LauncherParam/LauncherParamSettings");
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (settings != null)
            {
                results.Add(ContentNameIndex, settings.contentName);
                results.Add(StartTimeIndex, $"{(long)DateTime.Now.TimeOfDay.TotalSeconds}");
                results.Add(TeamIDIndex, settings.teamOriginID);
                results.Add(PlayerIDIndex, settings.playerOriginID);
                results.Add(NickIndex, $"P-{settings.playerOriginID}");
                results.Add(CharacterName, settings.characterName);
                results.Add(TeamNumber, settings.teamNumber);
                results.Add(ServerIP, settings.serverIP);
                results.Add(ServerPort, settings.serverPort);
            }
            else
            {
                Debug.LogError("[参数] 无法加载 LauncherParamSettings ScriptableObject 文件。请检查路径和文件是否存在。");
            }
            return results;
        }
    }
}