using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 基础操作适配器
    /// </summary>
    /// <typeparam name="TOperation">操作类型</typeparam>
    /// <typeparam name="TOperations">操作集合类型</typeparam>
    public abstract class BaseOperationAdapter<TOperation, TOperations> : IOperationAdapter
        where TOperation : class, new()
        where TOperations : class, new()
    {
        protected readonly TCPClientNetwork _tcpClient;
        protected readonly INetworkLogic _networkLogic;
        protected readonly Queue<TOperation> _operationQueue = new Queue<TOperation>();
        protected readonly int _frameMaxMsgCount;
        protected readonly int _operationMaxPerMsg;
        protected readonly int _messageId;
        protected readonly object _lockObj = new object();
        protected TOperations _operationsBuffer;

        public BaseOperationAdapter(TCPClientNetwork tcpClient, INetworkLogic networkLogic, int frameMaxMsgCount,
            int operationMaxPerMsg, int messageId)
        {
            _tcpClient = tcpClient;
            _networkLogic = networkLogic;
            _frameMaxMsgCount = frameMaxMsgCount;
            _operationMaxPerMsg = operationMaxPerMsg;
            _messageId = messageId;

            // 预初始化缓冲区
            _operationsBuffer = new TOperations();
            InitializeOperationsBuffer(_operationsBuffer);
        }

        /// <summary>
        /// 初始化操作集合缓冲区
        /// </summary>
        protected abstract void InitializeOperationsBuffer(TOperations buffer);

        /// <summary>
        /// 设置操作集合的公共属性
        /// </summary>
        protected abstract void SetOperationsCommonProperties(TOperations operations);

        /// <summary>
        /// 获取操作集合中的列表
        /// </summary>
        protected abstract ICollection<TOperation> GetOperationsList(TOperations operations);

        /// <summary>
        /// 设置单个操作的公共属性
        /// </summary>
        protected abstract void SetOperationCommonProperties(TOperation operation);

        /// <summary>
        /// 序列化操作集合
        /// </summary>
        protected abstract byte[] SerializeOperations(TOperations operations);

        public void EnqueueOperation(object operation)
        {
            if (operation is not TOperation typedOp)
            {
                Debug.LogWarning($"操作类型不匹配，预期：{typeof(TOperation)}，实际：{operation?.GetType()}");
                return;
            }

            // 快速检查，避免不必要的锁
            if (!_tcpClient.IsConnected) return;

            lock (_lockObj)
            {
                // 检查队列是否已满
                if (_operationQueue.Count >= _frameMaxMsgCount)
                {
                    Debug.LogWarning($"操作队列已满，当前容量：{_operationQueue.Count}/{_frameMaxMsgCount}");
                    return;
                }

                // 设置公共属性
                SetOperationCommonProperties(typedOp);

                _operationQueue.Enqueue(typedOp);
            }
        }

        public async Task ProcessOperationsAsync()
        {
            // 快速检查连接状态和队列长度
            if (!_tcpClient.IsConnected) return;

            int queueCount;
            lock (_lockObj)
            {
                queueCount = _operationQueue.Count;
            }

            if (queueCount == 0) return;

            // 准备要发送的操作
            var (operations, data) = PrepareOperations();
            if (data == null) return;

            try
            {
                await _tcpClient.SendMessageAsync(_messageId, data, 0)
                    .ConfigureAwait(false);

                var operationsList = GetOperationsList(operations);
                foreach (var op in operationsList)
                {
                    LBENetworkSyncManager.Instance.OperationService.ReturnOperation(op);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"发送操作失败：{ex.Message}");
                // 失败时可选择将操作放回队列
                // RestoreOperations(operations);
            }
        }

        private (TOperations, byte[]) PrepareOperations()
        {
            lock (_lockObj)
            {
                if (_operationQueue.Count == 0)
                    return (null, null);

                // 清空缓冲区列表
                var operationsList = GetOperationsList(_operationsBuffer);
                operationsList.Clear();

                // 设置公共属性
                SetOperationsCommonProperties(_operationsBuffer);

                // 取出最大允许的操作数量
                int takeCount = Math.Min(_operationMaxPerMsg, _operationQueue.Count);
                for (int i = 0; i < takeCount; i++)
                {
                    operationsList.Add(_operationQueue.Dequeue());
                }

                // 序列化
                var data = SerializeOperations(_operationsBuffer);
                return (_operationsBuffer, data);
            }
        }

        public void ClearQueue()
        {
            lock (_lockObj)
            {
                _operationQueue.Clear();
            }
        }

        public int QueueLength
        {
            get
            {
                lock (_lockObj)
                {
                    return _operationQueue.Count;
                }
            }
        }
    }
}
