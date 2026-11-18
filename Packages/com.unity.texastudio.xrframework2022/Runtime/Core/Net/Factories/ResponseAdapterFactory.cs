using System;
using TexaXRFramework.Core.Net;
using TexaXRFramework.Core.Utility;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 响应适配器工厂，负责创建和管理不同类型的响应适配器
    /// </summary>
    public class ResponseAdapterFactory
    {
        private readonly ServerControlType _serverControlType;
        private readonly ObjectPool<IResponseAdapter> _adapterPool;

        public ResponseAdapterFactory(ServerControlType controlType, int poolSize = 20)
        {
            _serverControlType = controlType;

            // 根据协议类型初始化对象池
            _adapterPool = new ObjectPool<IResponseAdapter>(
                createFunc: CreateAdapter,
                actionOnGet: adapter => adapter.Reset(), // 取出时重置状态
                actionOnRelease: null,
                actionOnDestroy: null,
                defaultCapacity: poolSize / 2,
                maxSize: poolSize
            );
        }

        /// <summary>
        /// 创建新的响应适配器
        /// </summary>
        private IResponseAdapter CreateAdapter()
        {
            return _serverControlType switch
            {
                ServerControlType.PicoServer => new PBResponseAdapter(this),
                ServerControlType.SelfHostedServer => new ProtoResponseAdapter(this),
                _ => throw new ArgumentOutOfRangeException(nameof(_serverControlType), "Unsupported protocol type")
            };
        }

        /// <summary>
        /// 从对象池获取适配器
        /// </summary>
        public IResponseAdapter GetAdapter() => _adapterPool.Get();

        /// <summary>
        /// 将适配器放回对象池
        /// </summary>
        public void ReleaseAdapter(IResponseAdapter adapter)
        {
            if (adapter != null && !adapter.IsDisposed)
            {
                _adapterPool.Release(adapter);
            }
        }

        /// <summary>
        /// 从原始响应对象创建并填充适配器
        /// </summary>
        public IResponseAdapter CreateFromOriginal(object originalResponse)
        {
            if (originalResponse == null)
                throw new ArgumentNullException(nameof(originalResponse));

            var adapter = GetAdapter();
            try
            {
                adapter.FromOriginal(originalResponse);
                return adapter;
            }
            catch
            {
                ReleaseAdapter(adapter);
                throw;
            }
        }
    }
}
