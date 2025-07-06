using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    /// <summary>
    /// 统一的日志记录器
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// 是否启用调试日志
        /// </summary>
        public static bool EnableDebugLogs = true;

        /// <summary>
        /// 是否启用信息日志
        /// </summary>
        public static bool EnableInfoLogs = true;

        /// <summary>
        /// 是否启用警告日志
        /// </summary>
        public static bool EnableWarningLogs = true;

        /// <summary>
        /// 是否启用错误日志
        /// </summary>
        public static bool EnableErrorLogs = true;

        /// <summary>
        /// 日志前缀
        /// </summary>
        private const string LOG_PREFIX = "[QuickAction]";

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="args">日志参数</param>
        public static void Debug(params object[] args)
        {
            if (EnableDebugLogs)
            {
                Log(LogLevel.Debug, args);
            }
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="args">日志参数</param>
        public static void Info(params object[] args)
        {
            if (EnableInfoLogs)
            {
                Log(LogLevel.Info, args);
            }
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="args">日志参数</param>
        public static void Warning(params object[] args)
        {
            if (EnableWarningLogs)
            {
                Log(LogLevel.Warning, args);
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="args">日志参数</param>
        public static void Error(params object[] args)
        {
            if (EnableErrorLogs)
            {
                Log(LogLevel.Error, args);
            }
        }

        /// <summary>
        /// 记录格式化日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public static void LogFormat(LogLevel level, string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(level, new object[] { message });
        }

        /// <summary>
        /// 内部日志记录方法
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="args">日志参数</param>
        private static void Log(LogLevel level, params object[] args)
        {
            if (args == null || args.Length == 0)
                return;

            // 将所有参数转换为字符串并用空格连接
            string message = string.Join(" ", args);
            string formattedMessage = $"{LOG_PREFIX} [{level}] {message}";

            // 查找是否有 UnityEngine.Object 类型的参数作为 context
            Object context = null;
            foreach (var arg in args)
            {
                if (arg is Object obj)
                {
                    context = obj;
                    break;
                }
            }

            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(formattedMessage, context);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage, context);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage, context);
                    break;
            }
        }

        /// <summary>
        /// 启用所有日志
        /// </summary>
        public static void EnableAllLogs()
        {
            EnableDebugLogs = true;
            EnableInfoLogs = true;
            EnableWarningLogs = true;
            EnableErrorLogs = true;
        }

        /// <summary>
        /// 禁用所有日志
        /// </summary>
        public static void DisableAllLogs()
        {
            EnableDebugLogs = false;
            EnableInfoLogs = false;
            EnableWarningLogs = false;
            EnableErrorLogs = false;
        }

        /// <summary>
        /// 只启用错误和警告日志
        /// </summary>
        public static void EnableOnlyErrorsAndWarnings()
        {
            EnableDebugLogs = false;
            EnableInfoLogs = false;
            EnableWarningLogs = true;
            EnableErrorLogs = true;
        }
    }
}