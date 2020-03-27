using System;

namespace Shaman.Common.Utils.Logging
{
    
    public class ConsoleLogger : IShamanLogger
    {
        private readonly string _prefix;
        private LogLevel _logLevel;
        private SourceType _source;
        private string _version;
        public ConsoleLogger(string prefix = "", LogLevel logLevel = LogLevel.Error | LogLevel.Debug | LogLevel.Info)
        {
            _prefix = prefix;
            _logLevel = logLevel;
        }

        public void SetLogLevel(LogLevel logLevel)
        {
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

        public void Info(string message)
        {
            if ((_logLevel & LogLevel.Info) == LogLevel.Info)
                Console.WriteLine($"{_prefix}{DateTime.UtcNow}|{Environment.TickCount} INFO: {message}");
        }

        public void Debug(string message)
        {
            if ((_logLevel & LogLevel.Debug) == LogLevel.Debug)
                Console.WriteLine($"{_prefix}{DateTime.UtcNow}|{Environment.TickCount} DEBUG: {message}");            
        }

        public void Initialize(SourceType source, string version, string subSource = "")
        {
            _source = source;
            _version = version;
        }

        public void Info(string sourceName, string action, string message)
        {
            Info($"{sourceName}.{action}: {message}");
        }

        public void Warning(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }

        public void Error(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }
    }
}