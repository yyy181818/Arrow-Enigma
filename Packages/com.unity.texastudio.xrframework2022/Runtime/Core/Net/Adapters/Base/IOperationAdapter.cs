using System.Collections.Generic;
using System.Threading.Tasks;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 操作指令适配器接口
    /// </summary>
    public interface IOperationAdapter
    {
        /// <summary>
        /// 添加操作指令到队列
        /// </summary>
        void EnqueueOperation(object operation);

        /// <summary>
        /// 处理并发送队列中的操作
        /// </summary>
        Task ProcessOperationsAsync();

        /// <summary>
        /// 清空操作队列
        /// </summary>
        void ClearQueue();

        /// <summary>
        /// 当前队列长度
        /// </summary>
        int QueueLength { get; }
    }
}
