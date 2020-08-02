using System;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Utils.Logging
{
    
    public class ConsoleLogger : IShamanLogger
    {
        private readonly string _prefix;
        private readonly LogLevel _logLevel;
        public ConsoleLogger(string prefix = "", LogLevel logLevel = LogLevel.Error | LogLevel.Debug | LogLevel.Info)
        {
            _prefix = prefix;
            _logLevel = logLevel;
        }

        public void Error(string message)
        {
            if ((_logLevel & LogLevel.Error) == LogLevel.Error)
                Console.WriteLine($"{_prefix}{DateTime.UtcNow}|{Environment.TickCount} ERROR: {message}");
        }

        public void Error(Exception ex)
        {
            if ((_logLevel & LogLevel.Error) == LogLevel.Error)
                Console.WriteLine($"{_prefix}{DateTime.UtcNow}|{Environment.TickCount} ERROR: {ex}");            
        }

        public void LogInfo(string message)
        {
            if ((_logLevel & LogLevel.Info) == LogLevel.Info)
                Console.WriteLine($"{_prefix}{DateTime.UtcNow}|{Environment.TickCount} INFO: {message}");
        }

        public void LogDebug(string message)
        {
            if ((_logLevel & LogLevel.Debug) == LogLevel.Debug)
                Console.WriteLine($"{_prefix}{DateTime.UtcNow}|{Environment.TickCount} DEBUG: {message}");            
        }

        public void LogInfo(string sourceName, string action, string message)
        {
            LogInfo($"{sourceName}.{action}: {message}");
        }

        public void LogWarning(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }

        public void LogWarning(string message)
        {
            Error(message);
        }

        public void Error(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }
    }
}