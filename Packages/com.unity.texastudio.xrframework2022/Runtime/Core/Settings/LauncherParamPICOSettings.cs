using UnityEngine;
namespace TexaXRFramework.Core
{
    public class LauncherParamPICOSettings : ScriptableObject
    {
        [Header("场馆ID")] public string venueID = "1";
        [Header("场馆中创建的房间ID")] public string roomName = "房间1";
        [Header("内容id")] public string contentID = "16615612511944704";
        [Header("内容名称")] public string contentName = "PICOLBEContentShow";
        [Header("内容版本")] public string contentVersion = "1.0.1";
        [Header("内容密钥")] public string secretKey = "MFIEDI6xZ8i9BPuIGSxDogQwel9negHWCdgm0eLcUsT2kG+G33A1sKcsJJgcdPxjvhn9cUVU7UjauDCRZ/87+WfGBBD6jYfk1vXkEandyUAzKKYe";
        [Header("校验频次（单位天）")] public string checkFrequency = "1";
        [Header("创建队伍时间")] public string startTime;
        [Header("队伍id")] public string teamOriginID = "11899";
        //[Header("队伍名")] public string teamName = teamOriginID + teamOriginID;
        [Header("队伍人数")] public string teamCount = "1";
        [Header("玩家id")] public string playerOriginID = "P9999";
        [Header("玩家昵称")] public string playerNick;

        [Header("内容服务器IP")] public string serverIP = "127.0.0.1";
        [Header("内容服务器端口")] public int serverPort = 8555;
        [Header("桥接服务器IP")] public string bridgeServerIP = "127.0.0.1";
        [Header("桥接服务器端口")] public int bridgeServerPort = 7799;
        [Header("文件更新根地址")] public string fileUpdateURL= "http://";
        [Header("玩家电话号码")] public string telephone= "13234567890";
        [Header("自拍头像下载链接")] public string iconLink;
        [Header("高坪效服务器IP")] public string maxServerIP= "127.0.0.1";
        [Header("高坪效服务器端口")] public int maxServerPort =8000;
        [Header("高坪效配置更新路径")] public string maxConfigUpdateURL= "http://";
        [Header("avatar形象对应的资源名称")] public string avatarResourceName= "1";
        [Header("avatar皮肤对应的资源名称")] public string avatarSkinResourceName= "2";
    }
}
