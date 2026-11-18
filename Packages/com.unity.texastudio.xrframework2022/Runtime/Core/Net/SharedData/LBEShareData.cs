using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TexaXRFramework.Core;

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_ANDROID
using UnityEngine;
#endif

namespace NS_LBEShareData
{
    #region 动线定义

    public struct LBEFVector2
    {
        public float x;
        public float y;
    }

    public struct LBEFVector3
    {
        public float x;
        public float y;
        public float z;
        
        public static LBEFVector3 Zero
        {
            get
            { 
                return new LBEFVector3() { x = 0, y = 0, z = 0 };
            }
        }

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_ANDROID
        public static LBEFVector3 Get(UnityEngine.Vector3 pos)
        {
            return new LBEFVector3() { x = pos.x, y = pos.y, z = pos.z };
        }
#endif
    }

    public struct LBEFVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
        
        public static LBEFVector4 Zero
        {
            get
            {
                return new LBEFVector4() { x = 0, y = 0, z = 0, w = 0 };
            }
        }

    }

    /// <summary>
    /// 内容区域类型：
    /// Hide：隐藏；
    /// EnterHide：进入隐藏;
    /// Show：显示;
    /// </summary>
    public enum LBEContentAreaType : byte
    {
        Hide = 1,
        EnterHide=2,
        Show=3
    }

    /// <summary>
    /// 内容边界类型
    /// Hide：隐藏；
    /// Show：显示;
    /// ShowWhenNearby：靠近显示;
    /// </summary>
    public enum LBEContentBoundaryType : byte
    {
        Hide = 1,
        Show = 2,
        ShowWhenNearby = 3
    }

    /// <summary>
    /// 区域类型：
    /// Normal：普通6dof区域；
    /// Device：必经区域;
    /// </summary>
    public enum LBEAreaType : byte
    {
        Normal = 1,
        MustPass
    }
    
    
    /// <summary>
    /// 区域触发器类型
    /// </summary>
    public enum LBEAreaTriggerType :byte
    {
        /// <summary>
        /// 抢占式
        /// </summary>
        [EnumAttirbute("抢占式")]
        Normal = 1,

        /// <summary>
        /// 队伍触发式：队伍中的所有玩家都需要在触发器区域内
        /// </summary>
        [EnumAttirbute("队伍触发式")]
        TeamTrigger,

        /// <summary>
        /// 定时触发式
        /// </summary>
        [EnumAttirbute("定时触发式")]
        TimeTrigger,
    }

    /// <summary>
    /// 区域切换Timeline触发类型
    /// </summary>
    public enum LEBAreaSwitcTimelineTriggerType : byte
    {
        /// <summary>
        /// 时间触发
        /// </summary>
        [EnumAttirbute("时间触发")]
        TimeTrigger = 1,

        /// <summary>
        /// 结束触发
        /// </summary>
        [EnumAttirbute("结束触发")]
        StoppedTrigger,
    }
    #endregion

    #region 内容动线配置数据
    [Serializable]
    public class LBEContentData
    {
        public string ContentName;
        public List<LBEStageData> StageList = new List<LBEStageData>();
    }

    [Serializable]
    public class LBEStageData
    {
        public string StageName;

        /// <summary>
        /// 次序，从1开始
        /// </summary>
        public int Order;

        /// <summary>
        /// 设置场景中心位置：后续旋转都以这个位置为中心
        /// </summary>
        public LBEFVector3 CenterPos;

        /// <summary>
        /// 场景根节点偏移
        /// </summary>
        public LBEFVector3 PositionOffset;

        /// <summary>
        /// 场景根节点旋转偏移
        /// </summary>
        public LBEFVector4 RotationOffset;

        /// <summary>
        /// 区域列表
        /// </summary>
        public List<LBEAreaData> AreaList = new List<LBEAreaData>();
        
        /// <summary>
        /// 读取地图数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LBEStageData ReadData(string path)
        {
            string config = "";
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_ANDROID
            var jsonStr = Resources.Load(path) as TextAsset;
            if (jsonStr == null || string.IsNullOrEmpty(jsonStr.text))
            {
                Debug.LogError("cant find map:" + path);
                return null;
            }
            config = jsonStr.text;

#else
            if (File.Exists(path))
            {
                config = File.ReadAllText(path);
            }
#endif

            try
            {
                if (!string.IsNullOrEmpty(config))
                {
                    var sData = JsonConvert.DeserializeObject<LBEStageData>(config);
                    return sData;
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
           
        }
    }

    [Serializable]
    public class LBEAreaData
    {
        /// <summary>
        /// 区域名：场景唯一
        /// </summary>
        public string AreaName;

        /// <summary>
        /// 次序：从1开始
        /// </summary>
        public byte Order;

        /// <summary>
        /// 区域类型：
        /// 1-普通区域;
        /// 2-必经区域;
        /// </summary>
        public byte AreaType;

        /// <summary>
        /// 最大停留时间
        /// </summary>
        public int MaxStayTime;

        /// <summary>
        /// 最小停留时间
        /// </summary>
        public int MinStayTime;

        /// <summary>
        /// 边界数据
        /// </summary>
        public List<LBEFVector3> OrangeBoundary = new List<LBEFVector3>();

        /// <summary>
        /// 蓝色区域数据：最佳体验区域
        /// </summary>
        public List<LBEFVector3> BlueBoundary = new List<LBEFVector3>();
        
    }
    #endregion

    #region 协议ID定义

    public enum ProtoType : ushort
    {
        /// <summary>
        /// 登录
        /// </summary>
        C2S_Login = 400,
       
        /// <summary>
        /// 进去场景
        /// </summary>
        C2S_EntryStage,
        
        /// <summary>
        /// 进入区域
        /// </summary>
        C2S_EntryArea,
        
        /// <summary>
        /// 同步Transform属性
        /// </summary>
        C2S_SyncTransform,
        
        /// <summary>
        /// 同步节点数据
        /// </summary>
        C2S_SyncNodeRoot,

        /// <summary>
        /// 同步交互状态
        /// </summary>
        C2S_SynInteractionState,

        /// <summary>
        /// 触发器进入
        /// </summary>
        C2S_EntryTrigger,
        
        /// <summary>
        /// 触发器退出
        /// </summary>
        C2S_ExitTrigger,

        /// <summary>
        /// 授权验证
        /// </summary>
        C2S_AuthVerify,

        /// <summary>
        /// 其他玩家退出
        /// </summary>
        S2C_PlayerExit,
        
        /// <summary>
        /// 其他玩家加入
        /// </summary>
        S2C_PlayerJoin,
        
        /// <summary>
        /// 非同组玩家靠近
        /// </summary>
        S2C_CollisionPlayers,
        
        /// <summary>
        /// 队伍触发器触发
        /// </summary>
        S2C_TeamTriggered,

        /// <summary>
        /// 播控指令
        /// </summary>
        S2C_ContentCtrCmd,
    }

    #endregion

    #region 队伍 & 玩家数据
    public enum LBEStageType
    {
        EntryStage,
        Stage1,
        Stage2
    }

    /// <summary>
    /// 队伍数据
    /// </summary>
    [Serializable]
    public class LBETeamData
    {
        /// <summary>
        /// 队伍唯一标识
        /// </summary>
        public string TeamID = string.Empty;

        /// <summary>
        /// 玩家所在队伍
        /// </summary>
        public string TeamName;

        /// <summary>
        /// 当前所属幕
        /// </summary>
        public string CurStageName = string.Empty;

        /// <summary>
        /// 当前所属区域
        /// </summary>
        public string CurAreaName = string.Empty;

        /// <summary>
        /// 进入当前区域时间(帧)
        /// </summary>
        public uint AreaStartTime = 0;

        /// <summary>
        /// 重连进来时刻队伍帧数：用于断线重连；
        /// </summary>
        public uint CurFreme = 0;

        /// <summary>
        /// 进入区域以后耗时ms：用于断线重连；
        /// </summary>
        public long SpanTimeInArea;

        /// <summary>
        /// 当前区域进度
        /// </summary>
        public int CurAreaRunningTime;

        /// <summary>
        /// 当前场景进度
        /// </summary>
        public long CurStageRunningTime;

        /// <summary>
        /// 客户端是否需要恢复进度
        /// </summary>
        public bool IsNeedToResume;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        ~LBETeamData()
        {
        }
    }

    /// <summary>
    /// 玩家数据
    /// </summary>
    [Serializable]
    public class LBEPlayerData
    {
        /// <summary>
        /// 玩家ClientID：网络层唯一
        /// </summary>
        public string ClientID=string.Empty;

        /// <summary>
        /// 标记唯一ID
        /// </summary>
        public string PlayerID = string.Empty;
        /// <summary>
        /// 外漏所属队伍ID，方便直接使用
        /// </summary>
        public string TeamID = string.Empty;

        /// <summary>
        /// 玩家昵称
        /// </summary>
        public string Nick = string.Empty;

        public string ContentName = string.Empty;

        public string ContentID = string.Empty;

        public string ContentVersion = string.Empty;

        public string SecretKey = string.Empty;

        public int TeamCount;

        public string TeamName = string.Empty;

        public string StartTime = string.Empty;

        public string ExtendData = string.Empty;

        public ContentArgsDataJsonData ExLaunchParam;

        /// <summary>
        /// 玩家扩展数据
        /// </summary>
        public LBEPlayerDataEx ExData=new LBEPlayerDataEx();

        /// <summary>
        /// 上次断线时间：队伍计时帧：用于断线重连；
        /// </summary>
        public uint DeadFrame = 0;

        /// <summary>
        /// 是否需要断线重连
        /// </summary>
        public bool IsNeedReconnect = false;

        /// <summary>
        /// 断线重连所需数据
        /// </summary>
        public LBETeamData ReconnectData=new LBETeamData();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class LBEPlayerDataEx
    {
        /// <summary>
        /// 玩家出生点索引
        /// </summary>
        public int SpawnIndex;

        /// <summary>
        /// 当前血量
        /// </summary>
        public int HP = 100;

        /// <summary>
        /// 最大血量
        /// </summary>
        public int HPMAX = 100;

        /// <summary>
        /// 扩展信息
        /// </summary>
        public string ExJson=string.Empty;
    }

    [Serializable]
    public class LBEAuthVerifyData
    {
        /// <summary>
        /// 鉴权结果 成功0，失败-1，成功可以不传
        /// </summary>
        public int AuthStatus = -1;

        /// <summary>
        /// 具体错误信息，直接填充鉴权SDK返回信息
        /// </summary>
        public string AuthMsg;
    }

    // 交互状态变化事件数据
    [Serializable]
    public class InteractionStateChange
    {
        public string Id;// 交互唯一标识
        public int State;// 交互状态
        public string SourceId;// 交互发起者ID（如玩家ID）
    }
    #endregion

    #region 起播参数
    //启动参数
    //传给PC程序调用ToExeArgs方法
    //传给一体机Agent调用ToApkArgs方法
    public class ContentArgsData
    {
        //场馆ID
        public string venueID;
        //内容id
        public string contentID;
        //内容名
        public string contentName;
        //:内容版本
        public string contentVersion;
        //内容秘钥
        public string secretKey;
        //校验频次
        public string checkFrequency;
        //创建队伍时间 int64 时间戳
        public string startTime;
        //队伍ID int64
        public string teamOriginID;
        //队伍名
        public string teamName;
        //队伍人数 
        public string teamCount;
        //玩家ID int64
        public string playerOriginID;
        //玩家昵称 
        public string playerNick;
        //语言类型 zh
        public string languageType;
        //扩展信息
        [JsonIgnore]
        public ContentArgsDataJsonData jsonData;

        public ContentArgsData(string venueID, string contentID, string contentName, string contentVersion, string secretKey, string checkFrequency, string startTime, string teamOriginID, string teamName, string teamCount, string playerOriginID, string playerNick, string languageType, ContentArgsDataJsonData jsonData)
        {
            this.venueID = venueID;
            this.contentID = contentID;
            this.contentName = contentName;
            this.contentVersion = contentVersion;
            this.secretKey = secretKey;
            this.checkFrequency = checkFrequency;
            this.startTime = startTime;
            this.teamOriginID = teamOriginID;
            this.teamName = teamName;
            this.teamCount = teamCount;
            this.playerOriginID = playerOriginID;
            this.playerNick = playerNick;
            this.languageType = languageType;
            this.jsonData = jsonData;
        }

        //jsonData要转换为json字符串带转义字符
        public string ToExeArgs()
        {
            return $"venueID \"{venueID}\" contentID \"{contentID}\" contentName \"{contentName}\" contentVersion \"{contentVersion}\"  secretKey \"{secretKey}\"  checkFrequency \"{checkFrequency}\"  startTime \"{startTime}\" teamOriginID \"{teamOriginID}\" teamName \"{teamName}\"  teamCount \"{teamCount}\" playerOriginID \"{playerOriginID}\" playerNick \"{playerNick}\"  languageType \"{languageType}\" jsonData {JsonConvert.SerializeObject(jsonData).Replace("\"", "\\\"")}";
        }

        public string ToApkArgs()
        {
            string jsonString = JsonConvert.SerializeObject(this);

            // 将 JSON 字符串解析为 JObject
            JObject jObject = JObject.Parse(jsonString);
            jObject.Add("jsonData", JsonConvert.SerializeObject(jsonData));
            return jObject.ToString(Formatting.None);
        }
    }

    //拓展信息
    public class ContentArgsDataJsonData
    {
        // 内容服务器IP
        public string serverIP;
        // 内容服务器端口
        public int serverPort;
        // 桥接服务器IP
        public string bridgeServerIP;
        // 桥接服务器端口
        public int bridgeServerPort;
        // 文件更新根地址
        public string fileUpdateURL;
        // 玩家电话号码
        public string telephone;
        // 自拍头像下载链接
        public string iconLink;
        // 高坪效服务器IP
        public string maxServerIP;
        //高坪效服务器端口
        public int maxServerPort;
        //高坪效配置更新路径
        public string maxConfigUpdateURL;
        // avatar形象对应的资源名称
        public string avatarResourceName;
        // avatar皮肤对应的资源名称
        public string avatarSkinResourceName;

        public ContentArgsDataJsonData(string serverIP, int serverPort, string bridgeServerIP, int bridgeServerPort, string fileUpdateURL, string telephone, string iconLink, string maxServerIP, int maxServerPort, string maxConfigUpdateURL, string avatarResourceName, string avatarSkinResourceName)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
            this.bridgeServerIP = bridgeServerIP;
            this.bridgeServerPort = bridgeServerPort;
            this.fileUpdateURL = fileUpdateURL;
            this.telephone = telephone;
            this.iconLink = iconLink;
            this.maxServerIP = maxServerIP;
            this.maxServerPort = maxServerPort;
            this.maxConfigUpdateURL = maxConfigUpdateURL;
            this.avatarResourceName = avatarResourceName;
            this.avatarSkinResourceName = avatarSkinResourceName;
        }
    }
    #endregion

    #region 播控数据
    public class SeekData
    {
        public string stageName;
        public string areaName;
        public long areaSeekTime = 0;
        public long areaStartTime = 0;
        public long stageRunningTime = 0;
        public bool isReconnect = false;

        public string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    #endregion
    
    #region 本地消息
    /// <summary>
    /// 扩展事件定义
    /// </summary>
    public partial class LocalEventCommad
    {
        public static string AreaEvent_Trigger = "AreaEvent_Trigger";

        public static string AreaEvent_Enter = "AreaEvent_Enter";

        public static string AreaEvent_TimelineTrigger = "AreaEvent_TimelineTrigger";

        /// <summary>
        /// 触发区域激活/隐藏
        /// </summary>
        public static string AreaEvent_RefreshTrigger = "AreaEvent_RefreshTrigger";
    }
    #endregion
}
