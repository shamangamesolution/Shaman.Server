using System.Diagnostics;

namespace Shaman.Common.Utils.Logging
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
        public static void Info(this IShamanLogger shamanLogger, string sourceName, string action, string message)
        {
            shamanLogger.LogInfo(sourceName, action, message);
        }

        [Conditional("DEBUG")]
        public static void Info(this IShamanLogger shamanLogger, string message)
        {
            shamanLogger.LogInfo(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(this IShamanLogger shamanLogger, string message)
        {
            shamanLogger.LogDebug(message);
        }
    }
}