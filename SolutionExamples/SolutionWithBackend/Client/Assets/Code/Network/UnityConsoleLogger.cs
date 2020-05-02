using System;
using Shaman.Common.Utils.Logging;

namespace Code.Network
{
    public class UnityConsoleLogger : IShamanLogger
    {
        private LogLevel _logLevel;
        
        public UnityConsoleLogger(LogLevel logLevel = LogLevel.Error | LogLevel.Debug | LogLevel.Info)
        {
            _logLevel = logLevel;
        }

        public void SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Error(string message)
        {
            if ((_logLevel & LogLevel.Error) == LogLevel.Error)
                UnityEngine.Debug.LogError($"{DateTime.UtcNow}|{Environment.TickCount} ERROR: {message}");
        }

        public void Error(Exception ex)
        {
            if ((_logLevel & LogLevel.Error) == LogLevel.Error)
                UnityEngine.Debug.LogError($"{DateTime.UtcNow}|{Environment.TickCount} ERROR: {ex}");            
        }

        public void Info(string message)
        {
            if ((_logLevel & LogLevel.Info) == LogLevel.Info)
                UnityEngine.Debug.Log($"{DateTime.UtcNow}|{Environment.TickCount} INFO: {message}");
        }

        public void Debug(string message)
        {
            if ((_logLevel & LogLevel.Debug) == LogLevel.Debug)
                UnityEngine.Debug.LogWarning($"{DateTime.UtcNow}|{Environment.TickCount} DEBUG: {message}");            
        }

        public void Initialize(SourceType source, string version, string subSource = "")
        {
            throw new NotImplementedException();
        }

        public void Info(string sourceName, string action, string message)
        {
            throw new NotImplementedException();
        }

        public void Warning(string sourceName, string action, string message)
        {
            throw new NotImplementedException();
        }

        public void Error(string sourceName, string action, string message)
        {
            throw new NotImplementedException();
        }
    }
}
