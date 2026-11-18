using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;

namespace TexaXRFramework.Core
{

    /// <summary>
    /// 日志组件
    /// </summary>
    public static class LBELogger
    {
        private static readonly ConcurrentQueue<LogEntry> logQueue = new ConcurrentQueue<LogEntry>();
        private static Thread logThread;
        private static bool isRunning = false;
        private static StreamWriter writer;
        private static string logFilePath;
        private static readonly AutoResetEvent logEvent = new AutoResetEvent(false);
        private static readonly object syncLock = new object();
        private static readonly object syncLock2 = new object();
        private static int errorLogCount = 0;
        private static DateTime errorLogWindowStart = DateTime.Now;
        private const int ERROR_LOG_RATE_LIMIT = 50;
        private const int ERROR_LOG_WINDOW_MS = 1000;

        /// <summary>
        /// 初始化日志系统：创建日志文件、订阅 Unity 日志、启动后台线程
        /// </summary>
        public static void Initialize()
        {
            lock (syncLock2)
            {

                if (isRunning)
                    return;

                isRunning = true;
                string directory = Application.persistentDataPath;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string fileName = "log_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".txt";
                logFilePath = Path.Combine(directory, fileName);

                try
                {
                    writer = new StreamWriter(logFilePath, false);
                    writer.AutoFlush = false;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[日志记录器] 无法创建日志文件: " + ex.Message);
                }
                Application.logMessageReceivedThreaded += HandleUnityLog;

                logThread = new Thread(ProcessLogQueue);
                logThread.IsBackground = true;
                logThread.Start();

                LogInfo("[日志记录器] 日志组件已初始化. 日志文件: " + logFilePath);
            }
        }

        /// <summary>
        /// 后台线程：不断从队列中取出日志写入文件
        /// </summary>
        private static void ProcessLogQueue()
        {
            while (isRunning)
            {
                // 每 100 毫秒唤醒一次
                logEvent.WaitOne(100);
                while (logQueue.TryDequeue(out LogEntry entry))
                {
                    WriteLogEntry(entry);
                }
            }

            while (logQueue.TryDequeue(out LogEntry entry))
            {
                WriteLogEntry(entry);
            }
            lock (syncLock)
            {
                writer?.Flush();
                writer?.Close();
            }
        }

        private static void WriteLogEntry(LogEntry entry)
        {
            try
            {
                lock (syncLock)
                {
                    writer.WriteLine(entry.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[日志记录器] 日志写入错误: " + ex.Message);
            }
        }

        /// <summary>
        /// 关闭日志组件（建议在应用退出时调用）
        /// </summary>
        public static void Shutdown()
        {
            lock (syncLock2)
            {
                if (!isRunning || null == logThread)
                {
                    return;
                }
                isRunning = false;
                logEvent.Set();
                Application.logMessageReceivedThreaded -= HandleUnityLog;
                logThread.Join();
            }

        }

        /// <summary>
        /// 将日志记录项入队；如果是错误或异常日志，则同步写入（限流防止过高频率）
        /// </summary>
        private static void EnqueueLog(LBELogType type, string message, string stackTrace = null)
        {
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                LogType = type,
                Message = message,
                StackTrace = stackTrace
            };

            logQueue.Enqueue(entry);
            logEvent.Set();

            if (type == LBELogType.Error || type == LBELogType.Exception)
            {
                lock (syncLock)
                {
                    DateTime now = DateTime.Now;
                    if ((now - errorLogWindowStart).TotalMilliseconds > ERROR_LOG_WINDOW_MS)
                    {
                        errorLogCount = 0;
                        errorLogWindowStart = now;
                    }
                    errorLogCount++;
                    if (errorLogCount <= ERROR_LOG_RATE_LIMIT)
                    {
                        try
                        {
                            writer.WriteLine(entry.ToString());
                            writer.Flush();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("[日志记录器] 日志即时写入错误: " + ex.Message);
                        }
                    }
                }
            }
        }

        #region 公共接口

        public static void LogDebug(string message)
        {
            Debug.Log(message);
            EnqueueLog(LBELogType.Debug, message);
        }

        public static void LogInfo(string message)
        {
            Debug.Log(message);
            EnqueueLog(LBELogType.Info, message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
            EnqueueLog(LBELogType.Warning, message);
        }

        public static void LogError(string message)
        {
            Debug.LogError(message);
            EnqueueLog(LBELogType.Error, message, Environment.StackTrace);
        }

        public static void LogException(Exception ex)
        {
            Debug.LogException(ex);
            EnqueueLog(LBELogType.Exception, ex.Message, ex.StackTrace);
        }

        #endregion

        private static void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            LBELogType lbeType;
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                    lbeType = LBELogType.Error;
                    break;
                case LogType.Warning:
                    lbeType = LBELogType.Warning;
                    break;
                case LogType.Log:
                    lbeType = LBELogType.Info;
                    break;
                case LogType.Exception:
                    lbeType = LBELogType.Exception;
                    break;
                default:
                    lbeType = LBELogType.Info;
                    break;
            }
            EnqueueLog(lbeType, logString, stackTrace);
        }
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LBELogType
    {
        Debug,
        Info,
        Warning,
        Error,
        Exception
    }

    /// <summary>
    /// 单条日志记录项
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp;
        public LBELogType LogType;
        public string Message;
        public string StackTrace;

        public override string ToString()
        {
            // 格式：[时间] [级别] 消息
            string timeStr = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (!string.IsNullOrEmpty(StackTrace) && (LogType == LBELogType.Error || LogType == LBELogType.Exception))
            {
                return $"[{timeStr}] [{LogType}] {Message}\n{StackTrace}";
            }
            else
            {
                return $"[{timeStr}] [{LogType}] {Message}";
            }
        }
    }
}
