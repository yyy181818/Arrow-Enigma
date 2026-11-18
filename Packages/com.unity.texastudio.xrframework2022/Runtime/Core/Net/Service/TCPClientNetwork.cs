using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
namespace TexaXRFramework.Core.Net
{
    /// <summary>
    /// TCP连接
    /// </summary>
    public class TCPClientNetwork 
    {
        #region 对外接口

        /// <summary>
        /// 最大重连次数
        /// </summary>
        public int MaxReconnectCount = int.MaxValue;

        /// <summary>
        /// 链接状态
        /// </summary>
        public bool IsConnected => _client?.Connected ?? false;

        /// <summary>
        /// 消息接收回调
        /// </summary>
        public event Action<int, byte[], int> OnMessageReceived;

        /// <summary>
        /// 链接断开回调
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// 链接回调
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// 构建链接
        /// </summary>
        /// <param name="serverIp">目标服务器IP</param>
        /// <param name="serverPort">目标服务器端口</param>
        public TCPClientNetwork(string serverIp, int serverPort, INetLogger logger)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _logger = logger != null ? logger : new LBEDefaultLogger("Client");
            MaxReconnectCount = MaxReconnectCount < 1 ? 20 : MaxReconnectCount;
        }

        /// <summary>
        /// 启动链接
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            // 清理之前的资源
            if (_cts != null)
            {
                if (!_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
                _cts.Dispose();
            }
            _cts = new CancellationTokenSource();

            await ConnectAsync();

            _receiveTask = Task.Run(() => ReceiveMessagesAsync(_cts.Token), _cts.Token);
            _heartbeatTask = Task.Run(() => SendHeartbeatsAsync(_cts.Token), _cts.Token);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="instructionId">消息ID</param>
        /// <param name="body">消息数据流</param>
        /// <param name="exCode">消息扩展码</param>
        /// <returns>异步任务</returns>
        public async Task SendMessageAsync(int instructionId, byte[] body, int exCode = 0)
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                byte[] header = new byte[16];
                BitConverter.GetBytes(instructionId).CopyTo(header, 0);
                BitConverter.GetBytes(body.Length).CopyTo(header, 4);
                BitConverter.GetBytes(1).CopyTo(header, 8); // 协议版本号
                BitConverter.GetBytes(exCode).CopyTo(header, 12); // 附加信息

                await _sendLock.WaitAsync();
                try
                {
                    // 发送消息头
                    await _stream.WriteAsync(header, 0, header.Length);
                    // 发送消息体
                    if (body.Length > 0)
                    {
                        await _stream.WriteAsync(body, 0, body.Length);
                    }
                }
                finally
                {
                    _sendLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"error sending message: {ex.Message}");
            }
        }

        /// <summary>
        /// 释放链接
        /// </summary>
        /// <returns></returns>
        public void StopAsync()
        {
            if (_cts != null)
            {
                if (!_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
                _cts.Dispose();
            }

            if (_stream != null)
            {
                _stream.Dispose();
            }

            _client?.Close();

            _logger.Log("[网络连接] 客户端已停止，释放链接.");
        }
        #endregion

        #region 具体实现
        private readonly string _serverIp;
        private readonly int _serverPort;
        private TcpClient _client;
        private INetLogger _logger;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private Task _receiveTask;
        private Task _heartbeatTask;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _finishedCts = new CancellationTokenSource();
        private const int HeartbeatInterval = 30; // 心跳间隔（秒）
        private const int ReconnectInterval = 5;  // 重连间隔（秒）
        private KeepAliveValue _kav = new KeepAliveValue();
        private async Task ConnectAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _client = new TcpClient();
                    _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    _client.Client.IOControl(IOControlCode.KeepAliveValues, _kav.KeepAliveTime, null);

                    await _client.ConnectAsync(_serverIp, _serverPort);
                    _stream = _client.GetStream();
                    _logger.Log($"[网络连接] 已连接到服务器 {_serverIp}:{_serverPort}");
                    OnConnected?.Invoke();
        
                    _reconnectCount = MaxReconnectCount;
                    break;
                }
                catch (Exception ex)
                {
                    await Task.Delay(ReconnectInterval * 1000, _cts.Token);
                }
            }
        }


        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    // 读取消息头部
                    byte[] headerBuffer = await ReadExactBytesAsync(16, cancellationToken);
                    if (headerBuffer == null)
                    {
                        break;
                    }

                    // 解析消息头
                    int instructionId = BitConverter.ToInt32(headerBuffer, 0);
                    int bodyLength = BitConverter.ToInt32(headerBuffer, 4);
                    int version = BitConverter.ToInt32(headerBuffer, 8);
                    int additionalInfo = BitConverter.ToInt32(headerBuffer, 12);

                    // 读取消息体
                    byte[] bodyBuffer = await ReadExactBytesAsync(bodyLength, cancellationToken);
                    if (bodyBuffer == null)
                    {
                        break;
                    }

                    // 触发消息接收事件
                    OnMessageReceived?.Invoke(instructionId, bodyBuffer, additionalInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[网络连接] 接收消息时出错: {ex.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke();
                await ReconnectAsync();
            }
        }

        public bool IsFinished
        {
            set
            {
                if (value)
                {
                    if (_finishedCts != null)
                    {
                        if (!_finishedCts.IsCancellationRequested)
                        {
                            _finishedCts.Cancel();
                        }
                        _finishedCts.Dispose();
                    }
                }
            }
        }
        
        private int _reconnectCount = int.MaxValue;
        
        /// <summary>
        /// 重连逻辑
        /// </summary>
        /// <returns></returns>
        private async Task ReconnectAsync()
        {
            _logger.Log($"[网络连接] 尝试重新连接，重连次数: {_reconnectCount}.");

            StopAsync();

            while (_reconnectCount > 0 && !_finishedCts.Token.IsCancellationRequested)
            {
                try
                {
                    _logger.Log($"[网络连接] 尝试重新连接，重连次数: {_reconnectCount}.");
                    _reconnectCount--;
                    await Task.Delay(3000, _finishedCts.Token); 
                    await StartAsync(); 
                    return;
                }
                catch (TaskCanceledException)
                {
                    _logger.Log("[网络连接] 重新连接已取消.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[网络连接] 重新连接失败: {ex.Message}. 正在重试...");
                }
            }
        }

        private async Task SendHeartbeatsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsConnected)
                {
                    // 发送心跳消息
                    await SendMessageAsync(0, new[] { (byte)1 });
                    await Task.Delay(HeartbeatInterval * 1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[网络连接] 心跳错误: {ex.Message}");
            }
        }

        private async Task<byte[]> ReadExactBytesAsync(int length, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[length];
            int bytesRead = 0;

            try
            {
                while (bytesRead < length)
                {
                    int read = await _stream.ReadAsync(buffer, bytesRead, length - bytesRead, cancellationToken);
                    if (read == 0)
                    {
                        // 服务器断开连接
                        return null;
                    }
                    bytesRead += read;
                }
            }
            catch (IOException ex)
            {
                return null;
            }
            catch (ObjectDisposedException)
            {
                //_logger.Log("Stream was disposed. Connection lost.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Log($"[网络连接] 读取字节时出现意外错误: {ex.Message}");
                return null;
            }

            return buffer;
        }
        #endregion
    }
}
