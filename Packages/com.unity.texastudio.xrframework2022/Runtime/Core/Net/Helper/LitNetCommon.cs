using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using PBMessage;
using NS_LBEShareData;

namespace LBEServer.ShareNet
{
    #region Launcher Param
    [Serializable]
    /// <summary>
    /// 启动参数
    /// </summary>
    public class LauncherParam
    {
        public string venueID;//场馆ID
        public string contentID;// 内容id
        public string contentName;// 内容名
        public string contentVersion;// 内容版本
        public string secretKey;// 内容秘钥
        public string checkFrequency;// 校验频次 说明：从1开始计数，单位是：天；
        public string startTime;// 创建队伍时间
        public string teamOriginID;// 队伍ID
        public string teamName;// 队伍名
        public string teamCount;// 队伍人数
        public string playerOriginID;// 玩家ID
        public string playerNick;// 玩家昵称
        public string languageType;// 语言类型
        public string jsonData;// 扩展信息
    }

    [Serializable]
    /// <summary>
    /// JsonData:扩展数据
    /// </summary>
    public class ExLauncherParam
    {
        public string serverIP;
        public int serverPort;
        public long telephone;
        public string iconLink;
        public string maxServerIP;
        public int maxServerPort;
        public string maxConfigUpdateURL;
        public string avatarResourceName;
        public string avatarSkinResourceName;
    }
    #endregion

    #region Common
    public delegate void OnDisConnected(string id);
    public delegate void OnConnected(string id);
    public delegate void OnUpdateEvent();

    public class LitNetDefine
    {
        #region Common
        public static string CLIENT_ID = "clientId";
        public static string REQUEST_KEY = "request";
        public static string RESPONSE_KEY = "response";
        public static string CMD_MSG = "CMD_MSG_EVENT";
        #endregion

        /// <summary>
        /// request 消息最大code
        /// </summary>
        public static int RequstMsgMAXID = 300;

        /// <summary>
        /// response 消息最大code
        /// </summary>
        public static int ResponseMsgMAXID = 500;

        /// <summary>
        /// 桥接服务器IP
        /// </summary>
        public static string BRIDGE_SERVER_IP = @"10.67.148.143";
        
        /// <summary>
        /// 桥接服务器端口
        /// </summary>
        public static readonly int BRIDGE_SERVER_PORT = 7799;

        /// <summary>
        /// 逻辑服务器端口
        /// </summary>
        public static readonly ushort LOGIC_SERVER_PORT = 9528;


        /// <summary>
        /// 桥接服务器IP配置Key
        /// </summary>
        public static string BRIDGE_SERVER_IP_KEY = "bridgeserverip";

        /// <summary>
        /// 桥接服务器IP配置Key
        /// </summary>
        public static string BRIDGE_SERVER_PORT_KEY = "bridgeserverport";

        /// <summary>
        /// 文件服务器IP配置key
        /// </summary>
        public static string FILE_SERVER_IP_KEY = "fileserverip";

        /// <summary>
        /// 文件服务器Port配置key
        /// </summary>
        public static string FILE_SERVER_PORT_KEY = "fileserverport";

        /// <summary>
        /// 逻辑服务器端口配置key
        /// </summary>
        public static string LOGIC_SERVER_PORT_KEY = "logicserverport";

        /// <summary>
        /// 逻辑服务器IP配置key
        /// </summary>
        public static string LOGIC_SERVER_IP_KEY = "logicserverip";

        /// <summary>
        /// 本地配置加载路径key
        /// </summary>
        public static string CONIFG_PATH_KEY = "configpath";

        /// <summary>
        /// 配置jsondata
        /// </summary>
        public static string CONFIG_JSON_DATA_KEY = "jsondata";

    }

    public interface ILitLogger
    {
        void Log(string info);
        void LogError(string info);
        void LogWarning(string info);
    }

    /// <summary>
    /// send request 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LitRequest<T>
    {
        public LitMsgData<T> request;
    }

    /// <summary>
    /// response
    /// </summary>
    /// <typeparam name="T">param type</typeparam>
    public class LitResponse<T>
    {
        public LitMsgData<T> response;
        public LitResponse()
        {

        }
        public LitResponse(LitMsgData<T> responseData)
        {
            response = responseData;
        }
    }

    /// <summary>
    /// response data 
    /// </summary>
    /// <typeparam name="T">param type</typeparam>
    public class LitMsgData<T>
    {
        public string uuid;
        public string msg;
        public string function;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T data;
    }
    
    /// <summary>
    /// 客户端实例
    /// </summary>
    public class LitSocketClient
    {
        public int id;
        public Socket socket;

        public CancellationTokenSource mCancelTk;
        // 示例
        public LitSocketClient(int id, Socket socket)
        {
            this.id = id;
            this.socket = socket;
            mCancelTk = new CancellationTokenSource();
        }
    }

    public class PingMonitor
    {
        private CancellationTokenSource mCancelTk;

        private bool mIsReceivePing = false;

        public void ReceivePing()
        {
            mIsReceivePing = true;
        }

        public async UniTask WaitForTimeout()
        {
            while (true)
            {
                if (!mCancelTk.IsCancellationRequested && !mIsReceivePing)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    if (!mIsReceivePing)
                    {
                        break;
                    }
                    else
                    {
                        mIsReceivePing = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 心跳检测
    /// </summary>
    public class HeartbeatMonitor
    {
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isHeartbeatReceived;
        private float timeout = 10000;

        //示例
        public HeartbeatMonitor(CancellationTokenSource tk = null)
        {
            _cancellationTokenSource = null == tk ? new CancellationTokenSource() :tk;
        }

        // 启动心跳检测
        public async UniTask StartMonitoring(float time = 10000)
        {
            timeout = time;

            // 等待心跳检测任务完成
            await CheckHeartbeat(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// 心跳丢失
        /// </summary>
        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// 接收心跳
        /// </summary>
        public void ReceiveHeartbeat()
        {
            _isHeartbeatReceived = true;
        }

        /// <summary>
        /// 检测心跳
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async Task CheckHeartbeat(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 创建一个延迟任务，等待心跳超时时间
                Task delayTask = Task.Delay(10000, cancellationToken);

                // 等待心跳超时时间或接收到心跳
                await Task.WhenAny(delayTask, WaitForHeartbeat(cancellationToken));

                if (delayTask.IsCompleted && !_isHeartbeatReceived)
                {
                    // 心跳超时逻辑
                    // 可以在这里触发相应的操作，比如重新连接等
                    break;
                }

                _isHeartbeatReceived = false; // 重置心跳接收状态
            }
        }

        /// <summary>
        /// 等待接收到下一个心跳
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async Task WaitForHeartbeat(CancellationToken cancellationToken)
        {
            while (!_isHeartbeatReceived)
            {
                // 等待一小段时间，然后再次检查是否接收到心跳
                await Task.Delay(1000);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    #endregion
    
    #region Content Sync
    [Serializable]
    public class TeamCacheData
    {
        public string TeamID;
        public string TeamName;
        public FVector3 XRPosOffset;
        public FVector4 XRRotOffset;
        public uint TeamFrame;
        public List<LBEPlayerData> PlayerDataDic;
        public LBETeamData CurTeamData;

        public bool IsFinished = false;
    }
    #endregion

    #region ID
    /// <summary>
    /// 生成不重复id
    /// </summary>
    public class IDGenerate
    {
        public static Snowflake Snowflake = new Snowflake(GetWorkId(), 1);

        private static int _autoMsgID = 0;

        /// <summary>
        /// 获取自增ID
        /// </summary>
        public static int NewRpcID
        {
            get 
            {
                return ++_autoMsgID;
            }
        }

        private static long GetWorkId()
        {
            // 获取本地主机名
            string hostName = Dns.GetHostName();
            // 获取本地主机的IP地址
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            long workId = 0;
            // 遍历IP地址，获取IPv4地址
            foreach (var address in addresses)
            {
                // 如果IP地址是IPv4地址
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    // 获取IP地址的每一个字节
                    var ipBytes = address.GetAddressBytes();
                    // 将IP地址的第三个字节左移8位，并加上第四个字节
                    var a = (ipBytes[2] << 8);
                    workId = a + ipBytes[3];
                    // 将workId的值限制在0-1023之间
                    workId &= 1023;
                }
            }
            return workId;
        }

        public static long NextID()
        {
            return Snowflake.NextId();
        }

        public static string NextStringId()
        {
            return Snowflake.NextId().ToString("x2");
        }

        public static int NextIntId()
        {
            var now = DateTime.Now;
            // 获取今天的开始时间
            var today = now.Date;
            // 计算今天过去的时间间隔
            var elapsed = now - today;
            // 将间隔的Ticks除以10得到100微秒数
            var hundredMicroseconds = elapsed.Ticks / 1000;
            return (int)hundredMicroseconds;
        }
    }

    #endregion
    
    #region Json Ex
    /// <summary>
    /// 反序列化json.添加try catch
    /// </summary>
    public static class JsonTool
    {
        public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value, converters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default;
            }
        }
    }

    public static class ExtensionClass
    {
        /// <summary>
        /// 获取Request的uuid
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string GetRequestUuid(this JObject jObject)
        {
            return jObject["request"]?["uuid"]?.ToString();
        }

        /// <summary>
        /// 获取Response的uuid
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string GetResponseUuid(this JObject jObject)
        {
            return jObject["response"]?["uuid"]?.ToString();
        }

        /// <summary>
        /// 获取Response的uuid
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string GetCmdCode(this JObject jObject)
        {
            return jObject["function"]?.ToString();
        }

        /// <summary>
        /// 获取Response的uuid
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        public static string GetRequestClientID(this JObject jObject)
        {
            return jObject[LitNetDefine.CLIENT_ID]?.ToString();
        }

        public static int GetClientID(this JObject jObject)
        {
            string info = jObject.GetRequestClientID();

            if (!string.IsNullOrEmpty(info) &&
            int.TryParse(info, out var clinetID))
            {
                return clinetID;
            }
            return -1;
        }


    }
    #endregion

    #region Bytes
    public static class ByteHelper
    {
        public static string ToHex(this byte b)
        {
            return b.ToString("X2");
        }

        public static string ToHex(this byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in bytes)
            {
                stringBuilder.Append(b.ToString("X2"));
            }
            return stringBuilder.ToString();
        }

        public static string ToHex(this byte[] bytes, string format)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in bytes)
            {
                stringBuilder.Append(b.ToString(format));
            }
            return stringBuilder.ToString();
        }

        public static string ToHex(this byte[] bytes, int offset, int count)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = offset; i < offset + count; ++i)
            {
                stringBuilder.Append(bytes[i].ToString("X2"));
            }
            return stringBuilder.ToString();
        }

        public static string ToStr(this byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        public static string ToStr(this byte[] bytes, int index, int count)
        {
            return Encoding.Default.GetString(bytes, index, count);
        }

        public static string Utf8ToStr(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Utf8ToStr(this byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        public static void WriteTo(this byte[] bytes, int offset, uint num)
        {
            bytes[offset] = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
            bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
            bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
        }

        public static void WriteTo(this byte[] bytes, int offset, int num)
        {
            bytes[offset] = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
            bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
            bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
        }

        public static void WriteTo(this byte[] bytes, int offset, byte num)
        {
            bytes[offset] = num;
        }

        public static void WriteTo(this byte[] bytes, int offset, short num)
        {
            bytes[offset] = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
        }

        public static void WriteTo(this byte[] bytes, int offset, ushort num)
        {
            bytes[offset] = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
        }
    }
    #endregion

    #region Timer
    public class Time
    {
        /// <summary>
        /// 计时统计:秒
        /// </summary>
        public static float time;
        public const float deltTime = 0.033f;
    }
    #endregion
}
