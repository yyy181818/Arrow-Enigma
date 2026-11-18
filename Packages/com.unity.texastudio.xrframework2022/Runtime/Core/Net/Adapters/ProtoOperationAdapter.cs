using System.Collections.Generic;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// ProtoMessagesNet协议适配器
    /// </summary>
    public class ProtoOperationAdapter : BaseOperationAdapter<ProtoMessagesNet.SyncOperationData, ProtoMessagesNet.SyncOperations>
    {
        public ProtoOperationAdapter(TCPClientNetwork tcpClient,
            INetworkLogic networkLogic, int frameMaxMsgCount, int operationMaxPerMsg)
            : base(tcpClient, networkLogic, frameMaxMsgCount,
                  operationMaxPerMsg, (int)ProtoMessagesNet.MessageID.GM_Operation)
        {
        }

        protected override void InitializeOperationsBuffer(ProtoMessagesNet.SyncOperations buffer)
        {
            if (buffer.operations != null)
            {
                buffer.operations.Clear();
            }
        }

        protected override void SetOperationsCommonProperties(ProtoMessagesNet.SyncOperations operations)
        {
            operations.playerID = _networkLogic.GetCurPlayerID;
            operations.teamID = _networkLogic.GetCurPlayerTeamID;
            operations.clientID = _networkLogic.GetCurPlayerClientID;
        }

        protected override ICollection<ProtoMessagesNet.SyncOperationData> GetOperationsList(ProtoMessagesNet.SyncOperations operations)
        {
            return operations.operations;
        }

        protected override void SetOperationCommonProperties(ProtoMessagesNet.SyncOperationData operation)
        {
            operation.playerID = _networkLogic.GetCurPlayerID;
            operation.teamID = _networkLogic.GetCurPlayerTeamID;
        }

        protected override byte[] SerializeOperations(ProtoMessagesNet.SyncOperations operations)
        {
            return ProtoTransfer.SerializeProtoBuf(operations);
        }
    }
}
