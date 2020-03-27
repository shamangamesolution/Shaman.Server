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
        void Info(string message);
        void Debug(string message);   
        void Info(string sourceName, string action, string message);
        void Warning(string sourceName, string action, string message);        
        void Error(string sourceName, string action, string message);
    }
}