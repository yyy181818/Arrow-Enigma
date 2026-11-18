using UnityEngine;
namespace TexaXRFramework.Core
{
    public class LauncherParamSettings : ScriptableObject
    {
        [Header("内容名称")] public string contentName = "PICOLBEContentShow";
        [Header("创建队伍时间")] public string startTime;
        [Header("队伍id")] public string teamOriginID = "11899";
        [Header("玩家id")] public string playerOriginID = "P9999";
        [Header("玩家昵称")] public string playerNick;
        [Header("玩家角色")] public string characterName;
        [Header("队内编号")] public string teamNumber="1";
        [Header("内容服务器IP")] public string serverIP = "127.0.0.1";
        [Header("内容服务器端口")] public string serverPort = "8555";     
    }
}
