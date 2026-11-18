using System;

namespace TexaXRFramework.Core.Net
{

    /// <summary>
    /// 统一响应接口，定义所有响应对象应具备的基本属性
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        int MsgID { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>
        string PlayerID { get; set; }

        /// <summary>
        /// 队伍ID
        /// </summary>
        string TeamID { get; set; }

        /// <summary>
        /// 响应结果码（0表示成功）
        /// </summary>
        int Result { get; set; }

        /// <summary>
        /// 响应数据（JSON字符串）
        /// </summary>
        string Data { get; set; }

        /// <summary>
        /// 重置响应对象状态，用于对象池复用
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// 响应适配器接口，扩展IResponse以支持与原始响应对象的转换
    /// </summary>
    public interface IResponseAdapter : IResponse, IDisposable
    {
        /// <summary>
        /// 从原始响应对象初始化适配器
        /// </summary>
        /// <param name="original">原始响应对象</param>
        void FromOriginal(object original);

        /// <summary>
        /// 转换为原始响应对象
        /// </summary>
        /// <returns>原始响应对象</returns>
        object ToOriginal();

        /// <summary>
        /// 释放状态检查
        /// </summary>
        bool IsDisposed { get; } 
    }
}
