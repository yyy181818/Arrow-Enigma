using System.Collections.Concurrent;

namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// 对象池
    /// </summary>
    public static class MemoryPool
    {
        // 使用 ConcurrentQueue 来存储池中的缓冲区
        private static readonly ConcurrentQueue<byte[]> _bufferPool = new ConcurrentQueue<byte[]>();

        // 从池中租用缓冲区
        public static byte[] Rent(int size)
        {
            byte[] buffer;
            if (_bufferPool.TryDequeue(out buffer) && buffer.Length >= size)
            {
                return buffer;
            }

            // 如果池中没有合适大小的缓冲区，则分配新的缓冲区
            return new byte[size];
        }

        // 将缓冲区放回池中
        public static void Return(byte[] buffer)
        {
            _bufferPool.Enqueue(buffer);
        }
    }
}
