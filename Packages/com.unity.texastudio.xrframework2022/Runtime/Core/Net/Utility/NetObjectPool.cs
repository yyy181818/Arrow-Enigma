using System;
using System.Collections.Concurrent;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 线程安全的对象池
    /// </summary>
    /// <typeparam name="T">池对象类型（必须有无参构造函数）</typeparam>
    public class NetObjectPool<T> where T : new()
    {
        private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();
        private readonly Action<T> _resetAction; // 重置对象的方法
        private readonly int _maxSize; // 最大池大小

        public NetObjectPool(Action<T> resetAction = null, int maxSize = 1000)
        {
            _resetAction = resetAction;
            _maxSize = maxSize;
        }

        /// <summary>
        /// 从池获取对象
        /// </summary>
        public T Get()
        {
            if (_pool.TryTake(out T item))
            {
                _resetAction?.Invoke(item);
                return item;
            }
            T newItem = new T();
            _resetAction?.Invoke(newItem);
            return newItem;
        }

        /// <summary>
        /// 回收对象到池
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;
            if (_pool.Count < _maxSize)
            {
                _resetAction?.Invoke(item);
                _pool.Add(item);
            }
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear() => _pool.Clear();
    }
}