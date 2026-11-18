using Cysharp.Threading.Tasks;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 网络逻辑接口
    /// </summary>
    public interface INetworkLogic
    {
        /// <summary>
        /// 获取服务器IP
        /// </summary>
        /// <returns></returns>
        string GetServerIP { get; }

        /// <summary>
        /// 获取服务器端口
        /// </summary>
        /// <returns></returns>
        int GetServerPort { get; }

        /// <summary>
        /// 当前玩家数据是否有效
        /// </summary>
        /// <returns></returns>
        bool ISCurPlayerDataValid { get; }

        /// <summary>
        /// 获取当前玩家ID
        /// </summary>
        /// <returns></returns>
        string GetCurPlayerID { get; }

        /// <summary>
        /// 获取当前玩家的队伍ID
        /// </summary>
        /// <returns></returns>
        string GetCurPlayerTeamID { get; }

        /// <summary>
        /// 获取当前玩家的客户端ID
        /// </summary>
        /// <returns></returns>
        string GetCurPlayerClientID { get; }

        /// <summary>
        /// 获取当前玩家角色名称
        /// </summary>
        /// <returns></returns>
        string GetCurPlayerCharacterName { get; }

        /// <summary>
        /// 获取当前玩家昵称
        /// </summary>
        /// <returns></returns>
        string GetCurPlayerNick { get; }

        /// <summary>
        /// 登录状态
        /// </summary>
        bool LoginStatus { get; }

        /// <summary>
        /// 队内编号
        /// </summary>
        int TeamNumber { get; }


        /// <summary>
        /// 关卡逻辑
        /// </summary>
        LBEStageLogicBase GetCurStage { get; }

        /// <summary>
        /// 当前进度恢复数据
        /// </summary>
        RecoveryData ReconnectRecoveryData { get;}

        /// <summary>
        ///  重置重连恢复数据
        /// </summary>
        void ResetReconnectRecoveryData();

        /// <summary>
        /// 初始化逻辑
        /// </summary>
        void OnInit();

        /// <summary>
        /// 处理离线登录
        /// </summary>
        void HandleOfflineLogin();

        /// <summary>
        /// 在线登录
        /// </summary>
        /// <returns></returns>
        UniTaskVoid HandleOnlineLogin();

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="data"></param>
        /// <param name="exCode"></param>
        void HandleMessage(int msgID, byte[] data, int exCode);

        /// <summary>
        /// 连接成功回调
        /// </summary>
        void OnConnected();

        /// <summary>
        /// 断开连接回调
        /// </summary>
        void OnDisconnected();

        /// <summary>
        /// 设置当前场景逻辑
        /// </summary>
        /// <param name="stage"></param>
        void OnSetCurStage(LBEStageLogicBase stage);

        /// <summary>
        /// 场景切换
        /// </summary>
        /// <param name="nextStage"></param>
        void OnEntryStage(string nextStage);

        /// <summary>
        /// 区域切换
        /// </summary>
        /// <param name="nextAreaName"></param>
        void OnSwitchNextArea(string nextAreaName);

        /// <summary>
        /// 区域切换
        /// </summary>
        void OnSwitchNextArea();

        /// <summary>
        /// 不同队伍玩家位置同步组件注册
        /// </summary>
        /// <param name="positionSyncComponent"></param>
        void OnSyncTransformRegister(LBERemotePlayersPositionSyncComponent  positionSyncComponent);

        /// <summary>
        ///  不同队伍玩家位置同步组件移除
        /// </summary>
        /// <param name="positionSyncComponent"></param>
        void OnSyncTransformUnRegister(LBERemotePlayersPositionSyncComponent positionSyncComponent);

        void OnApplicationPause(bool pauseStatus);

        /// <summary>
        /// 同步鉴权结果到服务器
        /// </summary>
        void SyncAuthResultToServer();
    }
}
