using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace TexaXRFramework.Core.Utility
{
    /// <summary>
    /// 主线程调度器，用于跨线程调用Unity API
    /// </summary>
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();
        private static MonoBehaviour _coroutineRunner;

        /// <summary>
        /// 初始化调度器（在主线程调用）
        /// </summary>
        public static void Initialize(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// 将Action加入主线程执行队列
        /// </summary>
        public static void EnqueueAction(Action action)
        {
            if (action == null) return;
            _mainThreadActions.Enqueue(action);
        }

        /// <summary>
        /// 每帧在主线程执行队列中的Action（需在Update中调用）
        /// </summary>
        public static void Update()
        {
            while (_mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Main thread action failed: {e.Message}");
                }
            }
        }
    }
}
