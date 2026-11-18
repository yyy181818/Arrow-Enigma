using Cysharp.Threading.Tasks;
using LBEServer.ShareNet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 网络同步管理器
    /// </summary>
    [DisallowMultipleComponent]
    public class LBENetworkSyncManager : MonoSingleton<LBENetworkSyncManager>
    {
 
        [EnumAttirbute("客户端启动模式")][Space(15)] public HeadlessStartOptions headlessStartMode = HeadlessStartOptions.DoNothing;

        [EnumAttirbute("客户端服务逻辑")][Space(15)] public ServerControlType serverControlType = ServerControlType.PicoServer;

        [Header("准备场景名")] public string EntranceStageName = "EntranceStage";

        [Header("首个游戏场景名")] public string FirstGameStageName = "ArtStage1";

        [Header("角色预制件资源")]
        public List<GameObject> PlayerPrefabs = new List<GameObject>();

        [Header("非同组角色预制资源")]
        public List<GameObject> NeighborPrefabs = new List<GameObject>();

        public bool IsNetworkConnected { get; private set;} = false;

        public TCPClientNetwork TcpNetClient { get { return _tcpNetClient; } }

        private TCPClientNetwork _tcpNetClient;

        private  INetworkLogic _networkLogic;

        private NetworkInteractiveSyncService _interactiveSyncService;

        // 响应适配器工厂
        private ResponseAdapterFactory _responseAdapterFactory;

        private IOperationAdapter _operationAdapter;

        public NetworkOperationService _operationService;


        private CancellationTokenSource _sendLoopCts;

        /// <summary>
        /// 每一帧发送的最大操作数量，避免异常操作，导致消息堆积
        /// </summary>
        private int FRAME_MAX_MSG_COUNT = 500;

        /// <summary>
        /// 每一帧发送的最大操作数量
        /// </summary>
        private int OPERTATION_MAX_COUNT_ONE_MSG = 50;

        /// <summary>
        /// 网络逻辑接口
        /// </summary>
        public INetworkLogic NetworkLogic { get=> _networkLogic;}

        /// <summary>
        /// 网络操作服务
        /// </summary>
        public NetworkOperationService OperationService { get => _operationService; }

        /// <summary>
        ///  网络交互同步服务
        /// </summary>
        public NetworkInteractiveSyncService InteractiveSyncService { get => _interactiveSyncService; }

        protected override void Awake()
        {
            base.Awake();
            GameVersion gameVersion = Util.LitJson.ToObject<GameVersion>(Resources.Load<TextAsset>(nameof(GameVersion)).ToString());
            GameConfig.LocalGameVersion = gameVersion;

            Logger();

            _networkLogic = serverControlType switch
            {
                ServerControlType.PicoServer => new PicoServerNetworkLogic(this),
                ServerControlType.SelfHostedServer => new SelfHostedServerNetworkLogic(this),
                _ => throw new ArgumentOutOfRangeException()
            };

            _responseAdapterFactory = new ResponseAdapterFactory(serverControlType);

            _operationService = new NetworkOperationService(this);

            _interactiveSyncService = new NetworkInteractiveSyncService(this);
        }

        private void Start()
        {
            _networkLogic.OnInit();
            switch (headlessStartMode)
            {
                case HeadlessStartOptions.AutoStartClient:
                    StartServer();
                    break;
                case HeadlessStartOptions.OfflineStartClient:
                    _networkLogic.HandleOfflineLogin(); 
                    break;
            }
        }

        private void Logger()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (GameConfig.LocalGameVersion.LocallySavedLog)
                {
                    LBELogger.Initialize();
                }
            }
        }

        #region 外部接口

        /// <summary>
        /// 玩家队伍内编号（从1开始）
        /// </summary>
        public int TeamNumber
        {
            get
            {
                return _networkLogic.TeamNumber;
            }
        }

        #endregion

        #region 网络服务
        public void StartServer()
        {
            var ip = _networkLogic.GetServerIP;
            var port = _networkLogic.GetServerPort;
            if (string.IsNullOrEmpty(ip) || port <= 0)
            {
                Debug.LogError("服务器IP或端口无效");
                return;
            }
            _tcpNetClient = new TCPClientNetwork(ip, port, new U3DNetLogger());
            _tcpNetClient.OnDisconnected += OnServerDisConnected;
            _tcpNetClient.OnMessageReceived += OnMessageHandler;
            _tcpNetClient.OnConnected += OnServerConnected;

            _ = _tcpNetClient.StartAsync();

            _operationAdapter= OperationAdapterFactory.CreateAdapter(serverControlType,_tcpNetClient, _networkLogic, FRAME_MAX_MSG_COUNT, OPERTATION_MAX_COUNT_ONE_MSG);

            Debug.Log($"[网络连接] 开始连接服务器 :{ip}:{port}");
        }


        /// <summary>
        /// 断开连接
        /// </summary>
        void OnServerDisConnected()
        {
            Debug.Log($"[网络连接] 服务器已断开连接...");
            IsNetworkConnected = false;
            _networkLogic.OnDisconnected();
            StopSendLoop();
            _operationAdapter.ClearQueue();
        }

        /// <summary>
        ///  创建连接回调
        /// </summary>
        void OnServerConnected()
        {
            Debug.Log($"[网络连接] 服务器已连接...");
            IsNetworkConnected = true;
            _networkLogic.OnConnected();
            StartSendLoop();
            _ = _networkLogic.HandleOnlineLogin();
        }

        /// <summary>
        /// 消息接受
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="data"></param>
        /// <param name="exCode"></param>
        private void OnMessageHandler(int msgID, byte[] data, int exCode)
        {
            _ = AsyncMainThreadMsgHandler(msgID, data, exCode);
        }

        private void StopServer()
        {
            if (_tcpNetClient != null)
            {
                _tcpNetClient.IsFinished = true;
                _tcpNetClient.OnMessageReceived -= OnMessageHandler;
                _tcpNetClient.OnConnected -= OnServerConnected;
                _tcpNetClient.OnDisconnected -= OnServerDisConnected;
                _tcpNetClient.StopAsync();
                _tcpNetClient = null;
                Debug.Log($"[网络连接] 停止网络服务断开连接...");
            }
        }
        #endregion

        #region 操作指令

        private void StartSendLoop()
        {
            // 停止现有循环
            StopSendLoop();

            _sendLoopCts = new CancellationTokenSource();
            _ = RunSendLoop(_sendLoopCts.Token);
        }

        private void StopSendLoop()
        {
            if (_sendLoopCts != null)
            {
                _sendLoopCts.Cancel();
                _sendLoopCts.Dispose();
                _sendLoopCts = null;
            }
        }

        // 供业务服务调用
        public void AddOperation<T>(T operation) where T : class
        {
            if (!IsNetworkConnected || operation == null) return;
            _operationAdapter.EnqueueOperation(operation);
        }

        /// <summary>
        /// 发送循环（保留33ms延迟）
        /// </summary>
        private async Task RunSendLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(16, cancellationToken).ConfigureAwait(false);

                    // 处理并发送操作
                    await _operationAdapter.ProcessOperationsAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不记录日志
            }
            catch (Exception ex)
            {
                Debug.LogError($"发送循环异常: {ex.Message}");
            }
        }
        #endregion
      
        #region 服务器消息分发
        public readonly object LockObject = new object();

        async UniTask AsyncMainThreadMsgHandler(int msgID, byte[] data, int exCode)
        {
            await UniTask.SwitchToMainThread();
            lock (LockObject)
            {
               _networkLogic.HandleMessage(msgID, data, exCode);
            }
        }

        /// <summary>
        /// 处理服务器响应
        /// </summary>
        public void HandleRPCResponse(int rpcId, byte[] data)
        {
            // 根据当前协议反序列化
            object originalResponse = DeserializeResponse(rpcId, data);
            if (originalResponse == null)
                return;

            // 创建响应适配器
            IResponseAdapter response = _responseAdapterFactory.CreateFromOriginal(originalResponse);

            if (_requestUtcs.TryGetValue(rpcId, out var tcs))
            {
                tcs.TrySetResult(response);
                _requestUtcs.Remove(rpcId);
            }
        }
        /// <summary>
        /// 反序列化响应数据
        /// </summary>
        private object DeserializeResponse(int msgID, byte[] data)
        {
            try
            {
                switch (serverControlType)
                {
                    case ServerControlType.PicoServer :
                        return ProtoTransfer.DeserializeProtoBuf<PBMessage.S2CResponse>(data);
                    case  ServerControlType.SelfHostedServer:
                        return ProtoTransfer.DeserializeProtoBuf<ProtoMessagesNet.S2CResponse>(data);
                    default:
                        Debug.LogError($"Unsupported protocol type: {serverControlType}");
                        return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"响应反序列化失败: {e.Message}");
                return null;
            }
        }
        #endregion

        #region 发送消息请求

        private readonly Dictionary<int, UniTaskCompletionSource<IResponse>> _requestUtcs = new Dictionary<int, UniTaskCompletionSource<IResponse>>();
        public async UniTask<IResponse> SendRequest<TProto>(TProto msgID, string data) where TProto : struct, Enum
        {
            if (!IsNetworkConnected ||!_networkLogic.ISCurPlayerDataValid)
            {
                return default;
            }
            var rpcId = IDGenerate.NextIntId();
            var uniTaskCompletionSource = new UniTaskCompletionSource<IResponse>();
   
                _requestUtcs.Add(rpcId, uniTaskCompletionSource);
            
            try
            {
                SendRequestInternal(msgID, data, rpcId);
                Debug.Log($"[服务器请求] 发送请求 : {msgID}, rpc id:{rpcId} ,到服务器..");
                SetTimeOut(rpcId).Forget();

                return await uniTaskCompletionSource.Task;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                _requestUtcs.Remove(rpcId);
                return default;
            }
        }

        /// <summary>
        /// 内部发送逻辑（按协议类型处理）
        /// </summary>
        private void SendRequestInternal<TProto>(TProto msgID, string data, int rpcId) where TProto : struct, Enum
        {
            // 实际项目中这里应该使用请求对象池
            // 简化实现，直接创建请求对象
            int msgIdInt = Convert.ToInt32(msgID);
            SendRequest(msgIdInt, data, rpcId);
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        private void SendRequest(int msgID, string exData = "{}", int extra = 0)
        {
            if (!_networkLogic.ISCurPlayerDataValid)
                return;

            switch (serverControlType)
            {
                case ServerControlType.PicoServer:
                    // 根据当前协议类型创建对应的请求对象
                    var picoRequest = CreatePicoServerRequest(msgID, exData, extra);
                    if (picoRequest == null)
                        return;

                    // 序列化请求对象
                    byte[] picoData = ProtoTransfer.SerializeProtoBuf(picoRequest);

                    // 发送消息
                    SendMsg((int)PBMessage.MessageID.GM_C2SRequest, picoData, extra);
                    break;
                case ServerControlType.SelfHostedServer:
                    // 根据当前协议类型创建对应的请求对象
                    var selfHostedRequest = CreateSelfHostedServerRequest(msgID, exData, extra);
                    if (selfHostedRequest == null)
                        return;

                    // 序列化请求对象
                    byte[] selfHostedData = ProtoTransfer.SerializeProtoBuf(selfHostedRequest);

                    // 发送消息
                    SendMsg((int)ProtoMessagesNet.MessageID.GM_C2S_Request, selfHostedData, extra);
                    break;
            }

        }

        /// <summary>
        /// 根据协议类型创建请求对象
        /// </summary>
        private PBMessage.C2SRequest CreatePicoServerRequest(int msgID, string exData, int extra)
        {
            return new PBMessage.C2SRequest
            {
                msgID = msgID,
                playerID = _networkLogic.GetCurPlayerID,
                teamID = _networkLogic.GetCurPlayerTeamID,
                data = exData,
                clientID = _networkLogic.GetCurPlayerClientID,
            };
        }

        private ProtoMessagesNet.C2SRequest CreateSelfHostedServerRequest(int msgID, string exData, int extra)
        {
            return new ProtoMessagesNet.C2SRequest
            {
                msgID = msgID,
                playerID = _networkLogic.GetCurPlayerID,
                teamID = _networkLogic.GetCurPlayerTeamID,
                data = exData,
                clientID = _networkLogic.GetCurPlayerClientID,
            };
        }

        private void SendMsg(int msgID, byte[] data, int extra)
        {
            if (_tcpNetClient != null)
            {
                _ = _tcpNetClient.SendMessageAsync(msgID, data, extra);
            }
        }


        /// <summary>
        ///  超时处理 
        /// </summary>
        /// <param name="uuid">client uid</param>
        private async UniTask SetTimeOut(int uuid)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(10));

            if (_requestUtcs.TryGetValue(uuid, out var utcs))
            {
                Debug.LogError($"[服务器请求] 请求超时 :{uuid}, 返回为空");
                utcs.TrySetResult(default);
                _requestUtcs.Remove(uuid);
            }
        }
        #endregion

        #region 登录&退出&重连
        void OnDestroy()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (GameConfig.LocalGameVersion.LocallySavedLog)
                {
                    LBELogger.Shutdown();
                }
            }
            StopServer();
        }

        private void OnApplicationQuit()
        {

            if (_tcpNetClient != null)
            {
                Debug.Log("OnApplicationQuit 处理网络");
                StopServer();
            }
            StopSendLoop();
            if (_operationAdapter!=null)
            {
                _operationAdapter.ClearQueue();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if ( _networkLogic!=null)
            {
                _networkLogic.OnApplicationPause(pauseStatus);
            }
        }

        #endregion
    }
}

