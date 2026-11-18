using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Newtonsoft.Json;
using NS_LBEShareData;
using PBMessage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// Pico自建服网络逻辑
    /// </summary>
    public class PicoServerNetworkLogic : INetworkLogic
    {
        private LBENetworkSyncManager _network;
        /// <summary>
        /// 玩家数据
        /// </summary>
        private LBEPlayerData CurPlayerData = new LBEPlayerData();
        /// <summary>
        /// 当前阶段逻辑
        /// </summary>
        private LBEStageLogicBase _curStage;

        /// <summary>
        /// 断线重连恢复数据
        /// </summary>
        private RecoveryData _reconnectRecoveryData = new RecoveryData();

        private string _venueID;
        private string _secretKey;
        private string _checkFrequency;

        private bool _isLogined = false;
        private Dictionary<string, GameObject> _neighborPrefabDic;

        private Dictionary<string, GameObject> _playerPrefabDic;

        private Dictionary<string, LBERemotePlayersPositionSyncComponent> _syncTransformsDic = new Dictionary<string, LBERemotePlayersPositionSyncComponent>();

        public string GetServerIP => CurPlayerData.ExLaunchParam.serverIP;
        public int GetServerPort => CurPlayerData.ExLaunchParam.serverPort;

        public bool ISCurPlayerDataValid => CurPlayerData != null;
        public string GetCurPlayerID => CurPlayerData.PlayerID;

        public string GetCurPlayerTeamID => CurPlayerData.TeamID;

        public string GetCurPlayerClientID => CurPlayerData.ClientID;

        public string GetCurPlayerCharacterName => CurPlayerData.ExLaunchParam.avatarResourceName;

        public string GetCurPlayerNick => CurPlayerData.Nick;

        public bool LoginStatus => _isLogined;

        public LBEStageLogicBase GetCurStage => _curStage;

        public RecoveryData ReconnectRecoveryData => _reconnectRecoveryData;

        internal PicoServerNetworkLogic(LBENetworkSyncManager network)
        {
            _network = network;
        }

        public void OnInit()
        {
            ReceiveLauncherParam();
            LBELoaderContentConfig.OnInit($"ContentConfig/AllStageConfigs");
            InitPrefabDictionaries();
            CheckAuthorizationStatus();
        }

        public void HandleMessage(int msgID, byte[] data, int exCode)
        {
            switch ((MessageID)msgID)
            {
                //处理response返回
                case MessageID.GM_S2CResponse:
                    _network.HandleRPCResponse(exCode, data);
                    break;

                //处理服务器消息
                case MessageID.GM_S2CMsg:
                    S2CMsgHandler(ProtoTransfer.DeserializeProtoBuf<S2CMsg>(data));
                    break;

                // TODO:处理播控指令
                case MessageID.RPC_ContentControl:
                    var controlData = ProtoTransfer.DeserializeProtoBuf<SyncContentControlData>(data);

                    if (controlData != null)
                    {
                        ContentCtrCmdHandler((byte)controlData.Cmd, controlData.JsonData);
                    }

                    break;

                // 处理服务器广播过来的客户端状态的客户端操作指令
                case MessageID.GM_Operation:
                    var operations = ProtoTransfer.DeserializeProtoBuf<SyncOperations>(data);
                    if (operations == null || string.IsNullOrEmpty(operations.teamID) ||
                        string.IsNullOrEmpty(operations.playerID))
                    {
                        Debug.LogError("[消息处理] 服务器广播的玩家数据为空....");
                        return;
                    }

                    SyncOperationsHandler(operations);
                    break;
            }
        }

        private void SyncOperationsHandler(SyncOperations opts)
        {
            foreach (var op in opts.operations)
            {
                switch ((ProtoType)op.cmd)
                {
                    // 客态transform同步
                    case ProtoType.C2S_SyncTransform:
                        // 不需要处理主态和同组玩家预制体为空的情况
                        if (op.playerID.Equals(CurPlayerData.PlayerID) || _network.PlayerPrefabs.Count == 0)
                        {
                            continue;
                        }
                        // 新到对象，创建
                        if (!_syncTransformsDic.ContainsKey(op.identity))
                        {
                            DoCreate(op.name2, op.identity, op.name3, op.pos, op.rotation);
                        }
                        else
                        {
                            var st = _syncTransformsDic[op.identity];
                            if (st != null)
                            {
                                VectorConvertTool.ToUnityVector3(op.pos, ref _reusableNetPosition);

                                VectorConvertTool.ToUnityQuaternion(op.rotation, ref _reusableNetRotation);
                                st.DoSyncTransform(_reusableNetPosition, _reusableNetRotation);
                            }
                        }

                        break;
                    case ProtoType.C2S_SynInteractionState:
                        // 不需要处理主态
                        if (op.playerID.Equals(CurPlayerData.PlayerID))
                        {
                            continue;
                        }
                        VectorConvertTool.ToUnityVector3(op.pos, ref _reusableNetPosition);
                        VectorConvertTool.ToUnityQuaternion(op.rotation, ref _reusableNetRotation);
                        _network.InteractiveSyncService.HandleSyncData(op.name1, int.Parse(op.name2), _reusableNetPosition, _reusableNetRotation, op.name3);
                        break;
                    // 客态团队触发器
                    case ProtoType.S2C_TeamTriggered:
                        var areaName = op.name1;
                        var triggerName = op.name2;
                        Debug.Log($"team trigger:{triggerName} in area:{areaName} be triggered");
                        _curStage?.OnTeamTriggered(areaName, triggerName);
                        break;

                    // 播控指令处理
                    case ProtoType.S2C_ContentCtrCmd:
                        ContentCtrCmdHandler((byte)op.index1, op.name1);
                        break;
                    // 其他客态操作
                    default:
                        _curStage?.OnOperation();
                        break;
                }
            }
        }

        private void S2CMsgHandler(S2CMsg msg)
        {
            try
            {
                var id = (ProtoType)msg.cmd;
                switch (id)
                {
                    case ProtoType.S2C_PlayerJoin://其他玩家加入
                        var otherPlayer = JsonConvert.DeserializeObject<LBEPlayerData>(msg.data);
                        Debug.Log($"[服务器推送] 其他玩家加入:{otherPlayer.PlayerID} 加入团队:{msg.teamID}");
                        break;

                    case ProtoType.S2C_PlayerExit://其他玩家退出
                        otherPlayer = JsonConvert.DeserializeObject<LBEPlayerData>(msg.data);
                        Debug.Log($"[服务器推送] 其他玩家退出:{otherPlayer.PlayerID} 退出团队:{msg.teamID}");
                        break;

                    case ProtoType.S2C_CollisionPlayers://非同组玩家
                        var collisionPlayers = JsonConvert.DeserializeObject<B2L_PlayerData>(msg.data);
                        if (collisionPlayers != null && collisionPlayers.ID.Equals(CurPlayerData.PlayerID))
                        {
                            Msg502Handler(collisionPlayers.CollisionPlayers);
                        }
                        break;

                    default:
                        //CurStage?.S2CMsgHandler(msg);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[服务器推送] 响应失败:{e}");
            }
        }

        public void OnConnected()
        {

        }

        public void OnDisconnected()
        {

        }

        public async UniTaskVoid HandleOnlineLogin()
        {
            if (CurPlayerData != null)
            {
                var playerInfo = JsonConvert.SerializeObject(CurPlayerData);
                Debug.Log($"[登录] 开始登录 :{playerInfo}");
                using (var response = await _network.SendRequest(ProtoType.C2S_Login, playerInfo) as IResponseAdapter)
                {
                    await UniTask.SwitchToMainThread();
                    if (response != null && 0 == response.Result)
                    {
                        try
                        {
                            HandleLogin(response.Data);

                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[登录] 玩家登录错误:{e}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[登录] 玩家登录失败");
                    }
                }
            }
        }

        public void HandleOfflineLogin()
        {
            HandleLogin(JsonConvert.SerializeObject(CurPlayerData));
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _isLogined)
            {
                _ = HandleOnlineLogin();
            }
        }

        /// <summary>
        /// 注册客态对象同步组件
        /// </summary>
        /// <param name="st">客态对象</param>
        public void OnSyncTransformRegister(LBERemotePlayersPositionSyncComponent st)
        {
            if (st != null && st.gameObject.activeSelf && !_syncTransformsDic.ContainsKey(st.ObjectId))
            {
                _syncTransformsDic.Add(st.ObjectId, st);
            }
        }

        public void OnSyncTransformUnRegister(LBERemotePlayersPositionSyncComponent st)
        {
            if (st != null && _syncTransformsDic.ContainsKey(st.ObjectId))
            {
                _syncTransformsDic.Remove(st.ObjectId);
            }
        }

        #region 登录处理
        private void HandleLogin(string response)
        {
            CurPlayerData = JsonConvert.DeserializeObject<LBEPlayerData>(response);
            if (!_isLogined)
            {
                Debug.Log("[登录] 登录成功");

                _isLogined = true;
                SyncAuthResultToServer();
                if (CurPlayerData.IsNeedReconnect)
                {
                    var teamData = CurPlayerData.ReconnectData;
                    if (teamData != null)
                    {
                        Debug.Log($"[登录] 重新连接成功，恢复到:{teamData.ToJson()}");
                        ReconnectHandler(teamData);
                    }
                    else
                    {
                        Debug.LogError($"[登录] 获取恢复数据错误：团队数据为空");
                    }
                }
                else
                {
                    // 首次登录，一定是从入口场景进入的
                    if (_curStage != null)
                    {
                        _curStage.OnDispose();
                        _curStage = null;
                    }

                    // 进入入口场景
                    OnEntryStage(_network.EntranceStageName);
                }
            }
        }

        private void ReconnectHandler(LBETeamData teamData)
        {
            Debug.Log($"[断线重连] 重新连接处理, 创建定位数据, 执行定位操作...");
            if (!string.IsNullOrEmpty(teamData.CurStageName) &&
                   !string.IsNullOrEmpty(teamData.CurAreaName))
            {
                _reconnectRecoveryData.IsReconnect = true;
                _reconnectRecoveryData.StageName = teamData.CurStageName;
                _reconnectRecoveryData.AreaName = teamData.CurAreaName;
                _reconnectRecoveryData.AreaSeekTime = teamData.SpanTimeInArea;
                _reconnectRecoveryData.AreaStartTime = teamData.AreaStartTime;
                _reconnectRecoveryData.StageRunningTime = teamData.CurStageRunningTime;
                OnEntryStage(_reconnectRecoveryData.StageName);
            }
            else
            {
                Debug.LogError($"[播控指令处理]  定位数据错误:{teamData.ToString()}");
            }
        }
        #endregion

        #region 阶段
        public void OnEntryStage(string nextStage)
        {
            if (_curStage != null && _curStage.StageName.Equals(nextStage))
            {
                if (_reconnectRecoveryData.IsReconnect)//当前是断线重连
                {
                    var oldAreaName = _curStage.CurArea != null ? _curStage.CurArea.AreaName : "";
                    _curStage?.OnEntryArea(oldAreaName, _reconnectRecoveryData.AreaName);
                    _curStage?.RestoreProgress(_reconnectRecoveryData.StageRunningTime);
                }
                return;
            }
            _curStage?.OnDispose();
            LBESceneHelper.OnSyncLoad(nextStage, null, (result) =>
            {
                if (result)
                {
                    Debug.Log($"[切换场景] 加载场景:{nextStage} 完成");
                }
            });
        }

        /// <summary>
        /// 切换Stage完成，设置当前Stage
        /// </summary>
        /// <param name="stage">当前Stage</param>
        public void OnSetCurStage(LBEStageLogicBase stage)
        {
            _curStage = stage;
        }

        public void OnSwitchNextArea(string nextAreaName)
        {
            _curStage ?.OnSwitchNextArea(nextAreaName);
        }

        public void OnSwitchNextArea()
        {
            _curStage?.OnSwitchNextArea();
        }
        #endregion

        #region 播控指令处理

        /// <summary>
        /// 处理播控指令：暂停-1，恢复-2，Seek-3，退出-4，开始放映-5，设置身高-6
        /// </summary>
        /// <param name="cmd">播控指令</param>
        /// <param name="extra">额外数据</param>
        private void ContentCtrCmdHandler(byte cmd, string exData)
        {
            Debug.Log($"[播控指令处理] 接收内容控制命令: {cmd} , 额外数据: {exData} ");

            switch (cmd)
            {
                case 4: // 队伍退出
                    TeamExitHandler();
                    break;
                case 5: // 开始放映
                    PlayGameCmdHandler();
                    break;
            }
        }

        private void PlayGameCmdHandler()
        {
            Debug.Log($"[播控指令处理] 接收内容控制命令: 开始游戏 ");
            OnEntryStage(LBENetworkSyncManager.Instance.FirstGameStageName);
        }

        private void TeamExitHandler()
        {
            _ = DelayQuit();
        }

        IEnumerator DelayQuit()
        {
            Debug.Log($"[播控指令处理]  接收退出命令，延迟 3 秒....");
            yield return new WaitForSeconds(3);
            Application.Quit(999);
        }

        #endregion

        #region 接收起播参数
        /// <summary>
        /// 接收起播参数
        /// </summary>
        void ReceiveLauncherParam()
        {

            FillPlayerData(LauncherParamPICOHelper.GetLauncherParam());
        }

        private void FillPlayerData(Dictionary<string, string> paramDic)
        {
            if (paramDic != null)
            {
                paramDic.TryGetValue(LauncherParamPICOHelper.VenueIDIndex, out _venueID);
                paramDic.TryGetValue(LauncherParamPICOHelper.SecretKeyIndex, out _secretKey);
                paramDic.TryGetValue(LauncherParamPICOHelper.CheckFrequencyIndex, out _checkFrequency);
                paramDic.TryGetValue(LauncherParamPICOHelper.ContentNameIndex, out CurPlayerData.ContentName);
                paramDic.TryGetValue(LauncherParamPICOHelper.ContentIDIndex, out CurPlayerData.ContentID);
                paramDic.TryGetValue(LauncherParamPICOHelper.ContentVersionIndex, out CurPlayerData.ContentVersion);
                paramDic.TryGetValue(LauncherParamPICOHelper.TeamIDIndex, out CurPlayerData.TeamID);
                paramDic.TryGetValue(LauncherParamPICOHelper.TeamNameIndex, out CurPlayerData.TeamName);
                paramDic.TryGetValue(LauncherParamPICOHelper.PlayerIDIndex, out CurPlayerData.PlayerID);
                paramDic.TryGetValue(LauncherParamPICOHelper.NickIndex, out CurPlayerData.Nick);
                paramDic.TryGetValue(LauncherParamPICOHelper.SecretKeyIndex, out CurPlayerData.SecretKey);
                paramDic.TryGetValue(LauncherParamPICOHelper.StartTimeIndex, out CurPlayerData.StartTime);
                paramDic.TryGetValue(LauncherParamPICOHelper.TeamCount, out var teamCount);
                if (!string.IsNullOrEmpty(teamCount))
                {
                    int.TryParse(teamCount, out CurPlayerData.TeamCount);
                }

                if (paramDic.ContainsKey(LauncherParamPICOHelper.JsonData))
                {
                    string jsonData = paramDic[LauncherParamPICOHelper.JsonData];
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        try
                        {
                            CurPlayerData.ExLaunchParam =
                                JsonConvert.DeserializeObject<ContentArgsDataJsonData>(jsonData);

                            CurPlayerData.ExtendData = jsonData;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
            }
        }

        #endregion

        #region 鉴权操作
        private bool _isVerifyResultToServer = false;
        private void CheckAuthorizationStatus()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                LBEAuthorizationWrapper.CheckAuthorizationStatus(_venueID, CurPlayerData.ContentID, _secretKey, _checkFrequency, "PA9410MGJ9270000G", _network.headlessStartMode);
            }
        }

        public void SyncAuthResultToServer()
        {
            if (GameConfig.LocalGameVersion.VersionEnv == BuildVersion.Release)
            {
                if (_isVerifyResultToServer) return;
                if (_isLogined && !LBEAuthorizationWrapper.AuthStatus && LBEAuthorizationWrapper.Authorization_Detection_Completed)
                {
                    Debug.Log($"[鉴权] 发送数据到服务器");
                    _isVerifyResultToServer = true;
                    _network.OperationService.SyncAuthResultToServer();

                }
            }
        }
        #endregion

        #region 客态玩家操作同步
        private Vector3 _reusableNetPosition = Vector3.zero;
        private Quaternion _reusableNetRotation = Quaternion.identity;
        private void Msg502Handler(List<SyncPlayerData> collisionPlayer)
        {
            // 提前缓存同组ID和预制体检查结果，避免循环内重复计算
            var curTeamId = CurPlayerData.TeamID;
            var hasNeighborPrefabs = _network.NeighborPrefabs.Count > 0;
            foreach (var item in collisionPlayer)
            {
                // 跳过同组玩家或无预制体的情况
                if (item.TeamID == curTeamId || !hasNeighborPrefabs)
                {
                    continue;
                }

                if (_syncTransformsDic.TryGetValue(item.ID, out var st))
                {
                    VectorConvertTool.ToUnityVector3(item.Pos, ref _reusableNetPosition);

                    VectorConvertTool.ToUnityQuaternion(item.Rotation, ref _reusableNetRotation);

                    st.DoSyncTransform(_reusableNetPosition, _reusableNetRotation, false);
                }
                else
                {
                    DoCreate(item.PrefabName, item.ID, string.Empty, item.Pos, item.Rotation, false);
                }
            }
        }
        private Vector3 _reusablePos = Vector3.zero;
        private Quaternion _reusableRot = Quaternion.identity;
        private void DoCreate(string prefabsName, string identity, string nick, FVector3 netPos, FVector4 netRot, bool isCollisionPlayer = true)
        {
            VectorConvertTool.ToUnityVector3(netPos ?? new FVector3(), ref _reusablePos);
            VectorConvertTool.ToUnityQuaternion(netRot ?? new FVector4(), ref _reusableRot);

            var prefab = GetPrefabByType(prefabsName, isCollisionPlayer);
            if (prefab == null) return;

            var goIns = CreateGameObject(prefabsName, _reusablePos, _reusableRot, prefab);
            if (goIns == null) return;

            goIns.SetActive(true);
            goIns.name = identity;

            var syncTransform = goIns.GetComponent<LBERemotePlayersPositionSyncComponent>();
            syncTransform?.OnInitNetObject(identity, nick);
        }

        // 根据类型从字典获取预制体
        private GameObject GetPrefabByType(string prefabsName, bool isCollisionPlayer)
        {
            if (isCollisionPlayer)
            {
                if (!string.IsNullOrEmpty(prefabsName) && _playerPrefabDic.TryGetValue(prefabsName, out var prefab))
                {
                    return prefab;
                }

                if (_network.PlayerPrefabs != null && _network.PlayerPrefabs.Count > 0)
                {
                    Debug.LogError($"同组玩家未找到预制体 {prefabsName}，使用默认：{_network.PlayerPrefabs[0].name}");
                    return _network.PlayerPrefabs[0];
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(prefabsName) && _neighborPrefabDic.TryGetValue(prefabsName, out var prefab))
                {
                    return prefab;
                }

                if (_network.NeighborPrefabs != null && _network.NeighborPrefabs.Count > 0)
                {
                    Debug.LogError($"非同组玩家未找到预制体 {prefabsName}，使用默认：{_network.NeighborPrefabs[0].name}");
                    return _network.NeighborPrefabs[0];
                }
            }
            Debug.LogError($"{(isCollisionPlayer ? "NeighborPrefabs" : "PlayerPrefabs")}为空，无法创建对象");
            return null;
        }

        private GameObject CreateGameObject(string prefabsName, Vector3 pos, Quaternion rot, GameObject gameObject)
        {
            var goIns = UnityEngine.Object.Instantiate(gameObject, _network.transform, true);
            goIns.name = prefabsName;
            goIns.transform.localPosition = pos;
            goIns.transform.localRotation = rot;
            return goIns;
        }

        private void InitPrefabDictionaries()
        {
            if (_neighborPrefabDic == null && _network.NeighborPrefabs != null)
            {
                _neighborPrefabDic = _network.NeighborPrefabs
                    .ToDictionary(p => p.name, p => p); // 键：预制体名称，值：预制体实例
            }

            if (_playerPrefabDic == null && _network.PlayerPrefabs != null)
            {
                _playerPrefabDic = _network.PlayerPrefabs
                    .ToDictionary(p => p.name, p => p);
            }
        }


        #endregion

        #region 业务辅助
        public int TeamNumber
        {
            get
            {
                return CurPlayerData.ExData.SpawnIndex != 0 ? CurPlayerData.ExData.SpawnIndex : 1;
            }
        }
        #endregion

        public void ResetReconnectRecoveryData()
        {
            _reconnectRecoveryData.StageName = string.Empty;

            _reconnectRecoveryData.AreaName = string.Empty;

            _reconnectRecoveryData.AreaSeekTime = 0;

            _reconnectRecoveryData.AreaStartTime = 0;

            _reconnectRecoveryData.StageRunningTime = 0;

            _reconnectRecoveryData.IsReconnect = false;
        }
    }
}
