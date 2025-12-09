using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CsvReconTool.Core.Services
{
    public class ReconciliationLogger : IDisposable
    {
        private readonly string _logFilePath;
        private readonly ConcurrentQueue<string> _logQueue = new();
        private readonly StreamWriter _writer;
        private readonly object _writeLock = new();

        public ReconciliationLogger(string logFilePath)
        {
            _logFilePath = logFilePath;

            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(_logFilePath, append: false);
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public void LogError(string message)
        {
            Log("ERROR", message);
        }

        public void LogError(string message, Exception ex)
        {
            Log("ERROR", $"{message} - Exception: {ex.Message}");
            Log("ERROR", $"StackTrace: {ex.StackTrace}");
        }

        private void Log(string level, string message)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";

            lock (_writeLock)
            {
                _writer.WriteLine(logEntry);
                _writer.Flush();
            }

            Console.WriteLine(logEntry);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
