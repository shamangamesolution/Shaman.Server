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
        void SetLogLevel(LogLevel logLevel);
        void Error(string message);
        void Error(Exception ex);
        void Info(string message);
        void Debug(string message);   
        void Initialize(SourceType source, string version, string subSource = "");
        void Info(string sourceName, string action, string message);
        void Warning(string sourceName, string action, string message);        
        void Error(string sourceName, string action, string message);
    }
}