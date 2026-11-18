using System.Collections.Generic;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// PBMessage协议适配器
    /// </summary>
    public class PBOperationAdapter : BaseOperationAdapter<PBMessage.SyncOperationData, PBMessage.SyncOperations>
    {
        public PBOperationAdapter(TCPClientNetwork tcpClient,
            INetworkLogic networkLogic, int frameMaxMsgCount, int operationMaxPerMsg)
            : base(tcpClient, networkLogic, frameMaxMsgCount,
                  operationMaxPerMsg, (int)PBMessage.MessageID.GM_Operation)
        {
        }

        protected override void InitializeOperationsBuffer(PBMessage.SyncOperations buffer)
        {
            if (buffer.operations != null)
            {
                buffer.operations.Clear();
            }
        }

        protected override void SetOperationsCommonProperties(PBMessage.SyncOperations operations)
        {
            operations.playerID = _networkLogic.GetCurPlayerID;
            operations.teamID = _networkLogic.GetCurPlayerTeamID;
            operations.clientID = _networkLogic.GetCurPlayerClientID;
        }

        protected override ICollection<PBMessage.SyncOperationData> GetOperationsList(PBMessage.SyncOperations operations)
        {
            return operations.operations;
        }

        protected override void SetOperationCommonProperties(PBMessage.SyncOperationData operation)
        {
            operation.playerID = _networkLogic.GetCurPlayerID;
            operation.teamID = _networkLogic.GetCurPlayerTeamID;
        }

        protected override byte[] SerializeOperations(PBMessage.SyncOperations operations)
        {
            return ProtoTransfer.SerializeProtoBuf(operations);
        }
    }
}
