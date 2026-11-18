using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ProtoMessagesNet;
using SharedModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 自建服网络逻辑
    /// </summary>
    public class SelfHostedServerNetworkLogic : INetworkLogic
    {
        private LBENetworkSyncManager _network;
        /// <summary>
        /// 玩家数据
        /// </summary>
        private LBEPlayerData CurPlayerData = new LBEPlayerData();
        /// <summary>
        /// 当前阶段逻辑
        /// </summary>
        public LBEStageLogicBase _curStage;

        /// <summary>
        /// 断线重连恢复数据
        /// </summary>
        private RecoveryData _reconnectRecoveryData = new RecoveryData();

        private Dictionary<string, GameObject> _neighborPrefabDic;

        private Dictionary<string, GameObject> _playerPrefabDic;
        private string _serverIP;
        private int _serverPort;
        private bool _isLogined = false;
        private int _teamNumber;

        private Dictionary<string, LBERemotePlayersPositionSyncComponent> _syncTransformsDic = new Dictionary<string, LBERemotePlayersPositionSyncComponent>();
        internal SelfHostedServerNetworkLogic(LBENetworkSyncManager networkSyncManager)
        {
            _network = networkSyncManager;
        }
        public string GetServerIP => _serverIP;

        public int GetServerPort => _serverPort;

        public bool ISCurPlayerDataValid => CurPlayerData != null;

        public string GetCurPlayerID => CurPlayerData.PlayerID;

        public string GetCurPlayerTeamID => CurPlayerData.TeamID;

        public string GetCurPlayerClientID => CurPlayerData.ClientID;

        public string GetCurPlayerCharacterName => CurPlayerData.CharacterName;

        public string GetCurPlayerNick => CurPlayerData.Nick;

        public bool LoginStatus => _isLogined;

        public LBEStageLogicBase GetCurStage => _curStage;

        public RecoveryData ReconnectRecoveryData => _reconnectRecoveryData;

        public void OnInit()
        {
            ReceiveLauncherParam();
            LBELoaderContentConfig.OnInit($"ContentConfig/AllStageConfigs");
            InitPrefabDictionaries();
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

        public void HandleMessage(int msgID, byte[] data, int exCode)
        {
            var id = (MessageID)msgID;
            switch (id)
            {
                //处理response返回
                case MessageID.GM_S2C_Response:
                    _network.HandleRPCResponse(exCode, data);
                    break;

                //处理服务器消息
                case MessageID.GM_S2C_Message:
                    break;

                // TODO:处理播控指令
                case MessageID.RPC_ContentControl:
                    var controlData = ProtoTransfer.DeserializeProtoBuf<SyncContentControlData>(data);

                    if (controlData != null)
                    {
                        ContentCtrCmdHandler(controlData.Cmd);
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
                        UpdatePlayerPosition(op);
                        break;
                    //case ProtoType.C2S_SynInteractionState:
                    //    // 不需要处理主态
                    //    if (op.playerID.Equals(CurPlayerData.PlayerID))
                    //    {
                    //        continue;
                    //    }
                    //    VectorConvertTool.ToUnityVector3(op.pos, ref _reusableNetPosition);
                    //    VectorConvertTool.ToUnityQuaternion(op.rotation, ref _reusableNetRotation);
                    //    _network.InteractiveSyncService.HandleSyncData(op.name1, int.Parse(op.name2), _reusableNetPosition, _reusableNetRotation, op.name3);
                    //    break;
                    // 其他客态操作
                    default:
                        _curStage?.OnOperation();
                        break;
                }
            }
        }
        public void OnConnected()
        {

        }

        public void OnDisconnected()
        {

        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _isLogined)
            {
                _ = HandleOnlineLogin();
            }
        }

        public void SyncAuthResultToServer()
        {
            //自建服不需要同步鉴权结果
        }

        #region 玩家同步操作

        private Vector3 _reusableNetPosition = Vector3.zero;
        private Quaternion _reusableNetRotation = Quaternion.identity;
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

        public void UpdatePlayerPosition(SyncOperationData data)
        {
            // 不需要处理主态
            if (data.playerID.Equals(CurPlayerData.PlayerID) || _network.PlayerPrefabs.Count == 0|| _network.NeighborPrefabs.Count==0)
            {
                return;
            }
         
            if (_syncTransformsDic.TryGetValue(data.playerID, out var st))
            {
                VectorConvertTool.ToUnityVector3(data.pos, ref _reusableNetPosition);

                VectorConvertTool.ToUnityQuaternion(data.rotation, ref _reusableNetRotation);

                st.DoSyncTransform(_reusableNetPosition, _reusableNetRotation, data.teamID== CurPlayerData.TeamID?true:false);
            }
            else
            {
                DoCreate(data.PrefabName, data.playerID, data.PlayerNick, data.pos, data.rotation, data.teamID == CurPlayerData.TeamID ? true : false);
            }
        }

        private Vector3 _reusablePos = Vector3.zero;
        private Quaternion _reusableRot = Quaternion.identity;
        private void DoCreate(string prefabsName, string identity, string nick, FVector3 netPos, FVector4 netRot, bool isCollisionPlayer = true)
        {
            VectorConvertTool.ToUnityVector3(netPos ?? new FVector3(), ref _reusablePos);
            VectorConvertTool.ToUnityQuaternion(netRot ?? new FVector4(), ref _reusableRot);

            var prefab = GetPrefabByType(prefabsName, isCollisionPlayer);
            if (prefab == null) return; //

            var goIns = CreateGameObject(prefabsName, _reusablePos, _reusableRot, prefab);
            if (goIns == null) return;

            goIns.SetActive(true);
            goIns.name = identity;

            var syncTransform = goIns.GetComponent<LBERemotePlayersPositionSyncComponent>();
            syncTransform?.OnInitNetObject(identity, nick);
        }
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
                    Debug.LogError($"{(isCollisionPlayer ? "非同组" : "同组")}玩家未找到预制体 {prefabsName}，使用默认：{_network.PlayerPrefabs[0].name}");
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
                    Debug.LogError($"{(isCollisionPlayer ? "非同组" : "同组")}玩家未找到预制体 {prefabsName}，使用默认：{_network.NeighborPrefabs[0].name}");
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

        #region 登录处理
        private void HandleLogin(string response)
        {
            CurPlayerData = JsonConvert.DeserializeObject<LBEPlayerData>(response);
            if (!_isLogined)
            {
                Debug.Log("[登录] 登录成功");

                _isLogined = true;
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

        public void OnSetCurStage(LBEStageLogicBase stage)
        {
            _curStage = stage;
        }
        #endregion

        #region 重连处理
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

        #region 阶段处理
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

        public void OnSwitchNextArea(string nextAreaName)
        {
            _curStage?.OnSwitchNextArea(nextAreaName);
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
        private void ContentCtrCmdHandler(int cmd)
        {
            Debug.Log($"[播控指令处理] 接收内容控制命令: {cmd} ");

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

            FillPlayerData(LauncherParamHelper.GetLauncherParam());
        }

        private void FillPlayerData(Dictionary<string, string> paramDic)
        {
            if (paramDic != null)
            {
                paramDic.TryGetValue(LauncherParamHelper.ContentNameIndex, out CurPlayerData.ContentName);//内容名称
                paramDic.TryGetValue(LauncherParamHelper.TeamIDIndex, out CurPlayerData.TeamID);//队伍id
                paramDic.TryGetValue(LauncherParamHelper.StartTimeIndex, out CurPlayerData.StartTime);//创建队伍时间   
                paramDic.TryGetValue(LauncherParamHelper.PlayerIDIndex, out CurPlayerData.PlayerID);//玩家id
                paramDic.TryGetValue(LauncherParamHelper.NickIndex, out CurPlayerData.Nick);//玩家昵称
                paramDic.TryGetValue(LauncherParamHelper.CharacterName, out CurPlayerData.CharacterName);//玩家角色
                paramDic.TryGetValue(LauncherParamHelper.TeamNumber, out string teamNumber);//玩家角色
                paramDic.TryGetValue(LauncherParamHelper.ServerIP, out _serverIP);//服务器IP
                paramDic.TryGetValue(LauncherParamHelper.ServerPort, out string serverPortInt);//服务器端口
                _serverPort = int.Parse(serverPortInt);
                _teamNumber = int.Parse(teamNumber);
            }
        }

        #endregion

        #region 业务辅助
        public int TeamNumber
        {
            get
            {
                return _teamNumber;
            }
        }
        #endregion
        public void ResetReconnectRecoveryData()
        {
            _reconnectRecoveryData.StageName=string.Empty;

            _reconnectRecoveryData.AreaName = string.Empty;

            _reconnectRecoveryData.AreaSeekTime = 0;

            _reconnectRecoveryData.AreaStartTime = 0;

            _reconnectRecoveryData.StageRunningTime = 0;

            _reconnectRecoveryData.IsReconnect = false;
    }
    }
}
