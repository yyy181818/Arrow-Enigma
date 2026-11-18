using Newtonsoft.Json;
using UnityEngine;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 网络操作业务服务（封装所有业务层网络调用）
    /// </summary>
    public class NetworkOperationService
    {
        private readonly LBENetworkSyncManager _network;
        private readonly ServerControlType _serverType;

        // 对象池（分别管理两种协议的操作数据）
        private readonly NetObjectPool<PBMessage.SyncOperationData> _pbOpPool;
        private readonly NetObjectPool<ProtoMessagesNet.SyncOperationData> _protoOpPool;

        public NetworkOperationService(LBENetworkSyncManager network)
        {
            _network = network;
            _serverType = network.serverControlType;

            // 初始化对象池及重置方法（关键：重置对象状态避免脏数据）
            _pbOpPool = new NetObjectPool<PBMessage.SyncOperationData>(ResetPbOperation, 500);
            _protoOpPool = new NetObjectPool<ProtoMessagesNet.SyncOperationData>(ResetProtoOperation, 500);
        }

        #region 对象池重置方法
        private void ResetPbOperation(PBMessage.SyncOperationData op)
        {
            op.cmd = 0;
            op.name1 = string.Empty;
            op.name2 = string.Empty;
            op.name3 = string.Empty;
            op.identity = string.Empty;
            op.pos = new PBMessage.FVector3();
            op.rotation = new PBMessage.FVector4();
            op.localScale = new PBMessage.FVector3();
        }

        private void ResetProtoOperation(ProtoMessagesNet.SyncOperationData op)
        {
            op.cmd = 0;
            op.pos = new ProtoMessagesNet.FVector3();
            op.rotation = new ProtoMessagesNet.FVector4();
        }
        #endregion

        #region 业务操作封装（对外提供的接口）
        /// <summary>
        /// 同步玩家位置旋转
        /// </summary>
        public void SendPlayerTransformSync(Vector3 position, Quaternion rotation,

            Vector3 localScale)
        {
            if (!_network.IsNetworkConnected) return;

            switch (_serverType)
            {
                case ServerControlType.PicoServer:
                    var pbOp = _pbOpPool.Get();
                    pbOp.cmd = (int)NS_LBEShareData.ProtoType.C2S_SyncTransform;
                    pbOp.identity = _network.NetworkLogic.GetCurPlayerID;
                    pbOp.name1 = _network.NetworkLogic.GetCurPlayerID;
                    pbOp.name2 = _network.NetworkLogic.GetCurPlayerCharacterName;
                    pbOp.name3 = _network.NetworkLogic.GetCurPlayerNick;
                    VectorConvertTool.FillVector3(pbOp.pos, position);
                    VectorConvertTool.FillQuaternion(pbOp.rotation, rotation);
                    VectorConvertTool.FillVector3(pbOp.localScale, localScale);
                    _network.AddOperation(pbOp);
                    break;

                case ServerControlType.SelfHostedServer:
                    var protoOp = _protoOpPool.Get();
                    protoOp.cmd = (int)SharedModels.ProtoType.C2S_SyncTransform;
                    VectorConvertTool.FillVector3(protoOp.pos, position);
                    VectorConvertTool.FillQuaternion(protoOp.rotation, rotation);
                    _network.AddOperation(protoOp);
                    break;
            }
        }

        /// <summary>
        /// 发送关卡切换操作
        /// </summary>
        public void SyncStageSwitch(string currentStage)
        {
            if (!_network.IsNetworkConnected) return;

            switch (_serverType)
            {
                case ServerControlType.PicoServer:
                    var pbOp = _pbOpPool.Get();
                    pbOp.cmd = (int)NS_LBEShareData.ProtoType.C2S_EntryStage;
                    pbOp.name1 = currentStage;
                    _network.AddOperation(pbOp);
                    break;

                case ServerControlType.SelfHostedServer:
                    var protoOp = _protoOpPool.Get();
                    protoOp.cmd = (int)SharedModels.ProtoType.C2S_EntryStage;
                    protoOp.extension1 = currentStage;
                    _network.AddOperation(protoOp);
                    break;
            }
        }

        /// <summary>
        /// 发送区域切换操作
        /// </summary>
        public void SyncAreaSwitch(string currentArea, string nextArea)
        {
            if (!_network.IsNetworkConnected) return;

            switch (_serverType)
            {
                case ServerControlType.PicoServer:
                    var pbOp = _pbOpPool.Get();
                    pbOp.cmd = (int)NS_LBEShareData.ProtoType.C2S_EntryArea;
                    pbOp.name1 = currentArea;
                    pbOp.name2 = nextArea;
                    _network.AddOperation(pbOp);
                    break;

                case ServerControlType.SelfHostedServer:
                    var protoOp = _protoOpPool.Get();
                    protoOp.cmd = (int)SharedModels.ProtoType.C2S_EntryArea;
                    protoOp.extension1 = currentArea;
                    protoOp.extension2 = nextArea;
                    _network.AddOperation(protoOp);
                    break;
            }
        }

        /// <summary>
        ///  同步鉴权结果到服务器
        /// </summary>
        public void SyncAuthResultToServer()
        {
            if (!_network.IsNetworkConnected) return;

            switch (_serverType)
            {
                case ServerControlType.PicoServer:
                    var pbOp = _pbOpPool.Get();
                    pbOp.cmd = (int)NS_LBEShareData. ProtoType.C2S_AuthVerify;
                    NS_LBEShareData.LBEAuthVerifyData lBEAuthVerifyData = new NS_LBEShareData.LBEAuthVerifyData();
                    lBEAuthVerifyData.AuthStatus = -1;
                    lBEAuthVerifyData.AuthMsg = LBEAuthorizationWrapper.AuthMsg;
                    pbOp.name1 = JsonConvert.SerializeObject(lBEAuthVerifyData);
                    _network.AddOperation(pbOp);
                    break;

                case ServerControlType.SelfHostedServer:
                    break;
            }
        }

        /// <summary>
        /// 发送互动同步
        /// </summary>
        public void SendInteractiveSync(string objectId, int state, Vector3 position, Quaternion rotation,string data)
        {
            if (!_network.IsNetworkConnected) return;

            switch (_serverType)
            {
                case ServerControlType.PicoServer:
                    var pbOp = _pbOpPool.Get();
                    pbOp.cmd = (int)NS_LBEShareData.ProtoType.C2S_SynInteractionState;
                    pbOp.name1 = objectId;
                    pbOp.name2 = state.ToString();
                    pbOp.name3 = data;
                    VectorConvertTool.FillVector3(pbOp.pos, position);
                    VectorConvertTool.FillQuaternion(pbOp.rotation, rotation);
                    _network.AddOperation(pbOp);
                    break;

                case ServerControlType.SelfHostedServer:
                    var protoOp = _protoOpPool.Get();
                    protoOp.cmd = (int)SharedModels.ProtoType.C2S_SynInteractionData;
                    protoOp.extension1=objectId;
                    protoOp.state= state;
                    protoOp.data=data;
                    VectorConvertTool.FillVector3(protoOp.pos, position);
                    VectorConvertTool.FillQuaternion(protoOp.rotation, rotation);

                    _network.AddOperation(protoOp);
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 回收操作对象（由适配器在发送后调用）
        /// </summary>
        internal void ReturnOperation(object op)
        {
            switch (op)
            {
                case PBMessage.SyncOperationData pbOp:
                    _pbOpPool.Return(pbOp);
                    break;
                case ProtoMessagesNet.SyncOperationData protoOp:
                    _protoOpPool.Return(protoOp);
                    break;
            }
        }
    }
}