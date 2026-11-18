namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 操作适配器工厂
    /// </summary>
    public class OperationAdapterFactory
    {
        /// <summary>
        /// 创建适配器
        /// </summary>
        public static IOperationAdapter CreateAdapter(ServerControlType serverType, TCPClientNetwork tcpClient,
            INetworkLogic networkLogic, int frameMaxMsgCount, int operationMaxPerMsg)
        {
            return serverType switch
            {
                ServerControlType.SelfHostedServer => new ProtoOperationAdapter(tcpClient, networkLogic,
                    frameMaxMsgCount, operationMaxPerMsg),
                ServerControlType.PicoServer => new PBOperationAdapter(
                    tcpClient, networkLogic,
                    frameMaxMsgCount, operationMaxPerMsg),
                _ => throw new System.ArgumentOutOfRangeException(nameof(serverType), serverType, null)
            };
        }
    }
}
