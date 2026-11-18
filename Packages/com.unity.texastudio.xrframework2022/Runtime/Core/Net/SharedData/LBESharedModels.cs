using Newtonsoft.Json;
using System;
using System.Collections.Generic;


#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_ANDROID
using UnityEngine;
#endif
namespace SharedModels
{

    #region 向量结构定义（基础数据类型）


    /// <summary>
    /// 三维浮点向量（LBE专用）
    /// </summary>
    public struct LBEFVector3
    {
        public float X;
        public float Y;
        public float Z;

        /// <summary>
        /// 零向量（X=0,Y=0,Z=0）
        /// </summary>
        public static LBEFVector3 Zero => new LBEFVector3(0, 0, 0);

        /// <summary>
        /// 初始化三维向量
        /// </summary>
        /// <param name="x">X轴分量</param>
        /// <param name="y">Y轴分量</param>
        /// <param name="z">Z轴分量</param>
        public LBEFVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_ANDROID
        /// <summary>
        /// 从Unity向量转换为LBE向量
        /// </summary>
        /// <param name="vector">Unity三维向量</param>
        /// <returns>LBE三维向量</returns>
        public static LBEFVector3 FromUnityVector3(UnityEngine.Vector3 vector)
        {
            return new LBEFVector3(vector.x, vector.y, vector.z);
        }
#endif
    }

    /// <summary>
    /// 四维浮点向量
    /// </summary>
    public struct LBEFVector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        /// <summary>
        /// 零向量（X=0,Y=0,Z=0,W=0）
        /// </summary>
        public static LBEFVector4 Zero => new LBEFVector4(0, 0, 0, 0);

        /// <summary>
        /// 初始化四维向量
        /// </summary>
        /// <param name="x">X轴分量</param>
        /// <param name="y">Y轴分量</param>
        /// <param name="z">Z轴分量</param>
        /// <param name="w">W分量</param>
        public LBEFVector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    #endregion

    #region 协议ID定义（网络通信）
    /// <summary>
    /// 协议类型ID（C2S=客户端到服务端，S2C=服务端到客户端）
    /// </summary>
    public enum ProtoType : ushort
    {
        /// <summary>客户端登录请求</summary>
        C2S_Login = 400,

        /// <summary>客户端进入场景请求</summary>
        C2S_EntryStage = 401,

        /// <summary>客户端进入区域请求</summary>
        C2S_EntryArea = 402,

        /// <summary>客户端同步Transform数据</summary>
        C2S_SyncTransform = 403,

        /// <summary>客户端同步交互数据</summary>
        C2S_SynInteractionData=404,
    }
    #endregion

    #region 队伍与玩家数据（业务数据）

    /// <summary>
    /// 队伍数据（包含队伍当前状态与进度）
    /// </summary>
    [Serializable]
    public class LBETeamData
    {
        /// <summary>队伍唯一标识（GUID）</summary>
        public string TeamID;

        /// <summary>当前所属幕（阶段）名称</summary>
        public string CurStageName;

        /// <summary>当前所属区域名称</summary>
        public string CurAreaName;

        /// <summary>进入当前区域的起始帧（用于计时）</summary>
        public uint AreaStartTime = 0;

        /// <summary>当前队伍的帧计数（用于断线重连同步）</summary>
        public uint CurFrame = 0;

        /// <summary>在当前区域的耗时（毫秒，用于断线重连恢复）</summary>
        public long SpanTimeInArea;

        /// <summary>当前区域运行时间（秒，进度计算）</summary>
        public int CurAreaRunningTime;

        /// <summary>当前场景总运行时间（毫秒，全局进度）</summary>
        public long CurStageRunningTime;

        /// <summary>客户端是否需要恢复进度（断线重连标记）</summary>
        public bool IsNeedToResume;

        /// <summary>转换为JSON字符串</summary>
        /// <returns>序列化后的JSON</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// 玩家数据（包含玩家基本信息与状态）
    /// </summary>
    [Serializable]
    public class LBEPlayerData
    {
        /// <summary>玩家客户端唯一标识（网络层用）</summary>
        public string ClientID;

        /// <summary>玩家游玩的内容名称</summary>
        public string ContentName;

        /// <summary>玩家全局唯一标识</summary>
        public string PlayerID;

        /// <summary>所属队伍ID（关联LBETeamData.TeamID）</summary>
        public string TeamID;

        /// <summary>玩家昵称</summary>
        public string Nick;

        /// <summary>角色名称</summary>
        public string CharacterName;

        /// <summary>开始时间（ISO格式字符串）</summary>
        public string StartTime;

        /// <summary>扩展数据（预留JSON字符串）</summary>
        public string ExtendJsonData;

        /// <summary>上次断线时的帧计数（用于重连校验）</summary>
        public uint DeadFrame = 0;

        /// <summary>是否需要断线重连</summary>
        public bool IsNeedReconnect = false;

        /// <summary>断线重连所需的队伍数据</summary>
        public LBETeamData ReconnectData;

        /// <summary>转换为JSON字符串</summary>
        /// <returns>序列化后的JSON</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
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
        /// 最大停留时间
        /// </summary>
        public int MaxStayTime;

        /// <summary>
        /// 最小停留时间
        /// </summary>
        public int MinStayTime;
    }
    #endregion
}
