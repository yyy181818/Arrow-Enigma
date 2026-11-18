using System;
namespace TexaXRFramework.Core
{
    // 线程安全的非 MonoBehaviour 单例基类
    public class Singleton<T> where T : class, new()
    {
        // 用于存储单例实例
        private static readonly object lockObject = new object();
        private static T instance;

        public static T Instance
        {
            get
            {
                // 第一次检查，若实例已存在则直接返回
                if (instance == null)
                {
                    // 进入同步块，确保同一时间只有一个线程可以进入
                    lock (lockObject)
                    {
                        // 第二次检查，防止在等待锁的过程中其他线程已经创建了实例
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
    }
}