using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VpnClientNotes.Services
{
    /// <summary>
    /// Сервис для логирования действий и ошибок в файл.
    /// </summary>
    public static class LoggerService
    {
        private static readonly string LogFilePath = "system_logs.txt";

        /// <summary>
        /// Записывает сообщение об ошибке в файл логов.
        /// </summary>
        /// <param name="message">Текст ошибки</param>
        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        /// <summary>
        /// Записывает информационное сообщение в лог.
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        private static void Log(string level, string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}\n";
            File.AppendAllText(LogFilePath, logEntry);
        }
    }
}
