using System;

namespace Shaman.Common.Utils.Logging
{
    [Flags]
    public enum LogLevel
    {
        Error,
        Info,
        Debug
    }
    public enum SourceType
    {
        Router,
        BackEnd,
        GameServer,
        MatchMaker
    }
    public interface IShamanLogger
    {
        void Error(string message);
        void Error(Exception ex);
        void LogInfo(string message);
        void LogDebug(string message);
        void LogInfo(string sourceName, string action, string message);
        void LogWarning(string sourceName, string action, string message);        
        void Error(string sourceName, string action, string message);
    }
}