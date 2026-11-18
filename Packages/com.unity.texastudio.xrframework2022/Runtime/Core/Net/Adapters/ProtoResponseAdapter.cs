using ProtoMessagesNet;
using System;
namespace TexaXRFramework.Core.Net {
    /// <summary>
    /// ProtoMessagesNet命名空间的S2CResponse适配器
    /// 实现IResponseAdapter接口，适配ProtoMessagesNet.S2CResponse
    /// </summary>
    public class ProtoResponseAdapter : IResponseAdapter
    {
        // 原始响应对象
        private readonly S2CResponse _originalResponse;
        private readonly ResponseAdapterFactory _factory; // 持有工厂引用
        private bool _isDisposed; // 避免重复释放
        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtoResponseAdapter(ResponseAdapterFactory factory)
        {
            _factory = factory;
            _originalResponse = new S2CResponse();
        }

        /// <summary>
        /// 从原始对象初始化适配器
        /// </summary>
        public void FromOriginal(object original)
        {
            if (original is S2CResponse protoResponse)
            {
                MsgID = protoResponse.msgID;
                PlayerID = protoResponse.playerID;
                TeamID = protoResponse.teamID;
                Result = protoResponse.result;
                Data = protoResponse.data;
            }
            else
            {
                throw new ArgumentException("Invalid response type for ProtoResponseAdapter", nameof(original));
            }
        }

        /// <summary>
        /// 转换为原始响应对象
        /// </summary>
        public object ToOriginal()
        {
            return new S2CResponse
            {
                msgID = MsgID,
                playerID = PlayerID,
                teamID = TeamID,
                result = Result,
                data = Data
            };
        }
        // 新增属性实现
        public bool IsDisposed => _isDisposed;
        // 实现IDisposable接口
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                // 释放托管资源（放回对象池）
                _factory?.ReleaseAdapter(this);
            }
            _isDisposed = true;
        }
        // 实现IResponse接口属性
        public int MsgID { get; set; }
        public string PlayerID { get; set; }
        public string TeamID { get; set; }
        public int Result { get; set; }
        public string Data { get; set; }

        /// <summary>
        /// 重置对象状态，用于对象池复用
        /// </summary>
        public void Reset()
        {
            MsgID = 0;
            PlayerID = string.Empty;
            TeamID = string.Empty;
            Result = 0;
            Data = string.Empty;
        }
    }
}
