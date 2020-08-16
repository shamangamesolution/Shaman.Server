using System;

namespace Shaman.Contract.Common.Logging
{
    [Flags]
    public enum LogLevel
    {
        Error,
        Info,
        Debug
    }
    public interface IShamanLogger
    {
        void Error(string message);
        void Error(Exception ex);
        void LogInfo(string message);
        void LogDebug(string message);
        [Obsolete("Use one-argument overload")]
        void LogInfo(string sourceName, string action, string message);
        [Obsolete("Use one-argument overload")]
        void LogWarning(string sourceName, string action, string message);        
        void LogWarning(string message);        
        [Obsolete("Use one-argument overload")]
        void Error(string sourceName, string action, string message);
    }
}