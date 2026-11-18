using PBMessage;
using System;
using TexaXRFramework.Core.Net;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// PBMessage命名空间的S2CResponse适配器
    /// 实现IResponseAdapter接口，适配PBMessage.S2CResponse
    /// </summary>
    public class PBResponseAdapter : IResponseAdapter
    {
        // 原始响应对象
        private readonly S2CResponse _originalResponse;
        private readonly ResponseAdapterFactory _factory; // 持有工厂引用
        private bool _isDisposed; // 避免重复释放

        /// <summary>
        /// 构造函数
        /// </summary>
        public PBResponseAdapter(ResponseAdapterFactory factory)
        {
            _factory = factory;
            _originalResponse = new S2CResponse();
        }

        /// <summary>
        /// 从原始对象初始化适配器
        /// </summary>
        public void FromOriginal(object original)
        {
            if (original is S2CResponse pbResponse)
            {
                MsgID = pbResponse.msgID;
                PlayerID = pbResponse.playerID;
                TeamID = pbResponse.teamID;
                Result = pbResponse.result;
                Data = pbResponse.data;
            }
            else
            {
                throw new ArgumentException("Invalid response type for PBResponseAdapter", nameof(original));
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
