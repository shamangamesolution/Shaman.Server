using System.Diagnostics;

namespace Shaman.Contract.Common.Logging
{
    public static class ShamanLoggerExtensions
    {
        public static void Warning(this IShamanLogger shamanLogger, string message)
        {
            shamanLogger.LogWarning(message);
        }
        public static void Warning(this IShamanLogger shamanLogger, string sourceName, string action, string message)
        {
            shamanLogger.LogWarning(sourceName, action, message);
        }

        [Conditional("DEBUG")]
        [Conditional("LOGS")]
        public static void Info(this IShamanLogger shamanLogger, string sourceName, string action, string message)
        {
            shamanLogger.LogInfo(sourceName, action, message);
        }

        [Conditional("DEBUG")]
        [Conditional("LOGS")]
        public static void Info(this IShamanLogger shamanLogger, string message)
        {
            shamanLogger.LogInfo(message);
        }

        [Conditional("DEBUG")]
        [Conditional("LOGS")]
        public static void Debug(this IShamanLogger shamanLogger, string message)
        {
            shamanLogger.LogDebug(message);
        }
    }
}