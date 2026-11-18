using System;
using System.Collections.Generic;
using System.Threading;

namespace TexaXRFramework.Core.Utility
{

    /// <summary>
    /// 通用对象池，用于复用对象以减少GC
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly int _maxSize;          // 池最大容量
        private readonly Func<T> _createFunc;   // 对象创建函数
        private readonly Action<T> _actionOnGet;// 对象从池取出时的回调
        private readonly Action<T> _actionOnRelease; // 对象放回池时的回调
        private readonly Action<T> _actionOnDestroy; // 对象销毁时的回调
        private int _count;                     // 当前池内对象数量（线程安全计数）

        /// <summary>
        /// 对象池构造函数
        /// </summary>
        /// <param name="createFunc">创建新对象的函数</param>
        /// <param name="actionOnGet">对象从池取出时的操作</param>
        /// <param name="actionOnRelease">对象放回池时的操作</param>
        /// <param name="actionOnDestroy">对象销毁时的操作</param>
        /// <param name="defaultCapacity">初始容量</param>
        /// <param name="maxSize">最大容量</param>
        public ObjectPool(Func<T> createFunc,
                         Action<T> actionOnGet = null,
                         Action<T> actionOnRelease = null,
                         Action<T> actionOnDestroy = null,
                         int defaultCapacity = 10,
                         int maxSize = 1000)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _maxSize = maxSize > 0 ? maxSize : throw new ArgumentOutOfRangeException(nameof(maxSize));

            // 预初始化指定数量的对象
            for (int i = 0; i < defaultCapacity; i++)
            {
                T item = _createFunc();
                _pool.Push(item);
            }
            _count = _pool.Count;
        }

        /// <summary>
        /// 从池中获取一个对象
        /// </summary>
        /// <returns>池化对象</returns>
        public T Get()
        {
            T item;
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    item = _pool.Pop();
                    Interlocked.Decrement(ref _count);
                }
                else
                {
                    // 池为空时创建新对象
                    item = _createFunc();
                }
            }

            // 触发取出回调（如重置对象状态）
            _actionOnGet?.Invoke(item);
            return item;
        }

        /// <summary>
        /// 将对象放回池中
        /// </summary>
        /// <param name="item">要放回的对象</param>
        public void Release(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // 触发放回回调（如清理对象状态）
            _actionOnRelease?.Invoke(item);

            lock (_pool)
            {
                // 如果池已满，则销毁对象
                if (_count >= _maxSize)
                {
                    _actionOnDestroy?.Invoke(item);
                    return;
                }

                _pool.Push(item);
                Interlocked.Increment(ref _count);
            }
        }

        /// <summary>
        /// 清空对象池并销毁所有对象
        /// </summary>
        public void Clear()
        {
            lock (_pool)
            {
                if (_actionOnDestroy != null)
                {
                    foreach (var item in _pool)
                    {
                        _actionOnDestroy(item);
                    }
                }
                _pool.Clear();
                _count = 0;
            }
        }

        /// <summary>
        /// 当前池内对象数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 池的最大容量
        /// </summary>
        public int MaxSize => _maxSize;
    }
}