using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    /// <summary>
    /// Unified logger
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log level
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Whether to enable debug logs
        /// </summary>
        public static bool EnableDebugLogs = true;

        /// <summary>
        /// Whether to enable info logs
        /// </summary>
        public static bool EnableInfoLogs = true;

        /// <summary>
        /// Whether to enable warning logs
        /// </summary>
        public static bool EnableWarningLogs = true;

        /// <summary>
        /// Whether to enable error logs
        /// </summary>
        public static bool EnableErrorLogs = true;

        /// <summary>
        /// Log prefix
        /// </summary>
        private const string LOG_PREFIX = "[QuickAction]";

        /// <summary>
        /// Log debug message
        /// </summary>
        /// <param name="args">Log parameters</param>
        public static void Debug(params object[] args)
        {
            if (EnableDebugLogs)
            {
                Log(LogLevel.Debug, args);
            }
        }

        /// <summary>
        /// Log info message
        /// </summary>
        /// <param name="args">Log parameters</param>
        public static void Info(params object[] args)
        {
            if (EnableInfoLogs)
            {
                Log(LogLevel.Info, args);
            }
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        /// <param name="args">Log parameters</param>
        public static void Warning(params object[] args)
        {
            if (EnableWarningLogs)
            {
                Log(LogLevel.Warning, args);
            }
        }

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="args">Log parameters</param>
        public static void Error(params object[] args)
        {
            if (EnableErrorLogs)
            {
                Log(LogLevel.Error, args);
            }
        }

        /// <summary>
        /// Log formatted message
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="format">Format string</param>
        /// <param name="args">Format parameters</param>
        public static void LogFormat(LogLevel level, string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(level, new object[] { message });
        }

        /// <summary>
        /// Internal logging method
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="args">Log parameters</param>
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
        /// Enable all logs
        /// </summary>
        public static void EnableAllLogs()
        {
            EnableDebugLogs = true;
            EnableInfoLogs = true;
            EnableWarningLogs = true;
            EnableErrorLogs = true;
        }

        /// <summary>
        /// Disable all logs
        /// </summary>
        public static void DisableAllLogs()
        {
            EnableDebugLogs = false;
            EnableInfoLogs = false;
            EnableWarningLogs = false;
            EnableErrorLogs = false;
        }

        /// <summary>
        /// Only enable error and warning logs
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