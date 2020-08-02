using System;
using Shaman.Common.Utils.Logging;
using Shaman.Contract.Common.Logging;

namespace Shaman.Monkeys.Logging
{
    public class ShamanMonkeyLogger : IShamanLogger
    {
        private readonly ILogger _logger;

        public ShamanMonkeyLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Error(string message)
        {
            _logger.Log(message);
        }


        public void Error(Exception ex)
        {
            _logger.Log($"{ex}");
        }

        public void LogInfo(string message)
        {
        }

        public void LogDebug(string message)
        {
        }

        public void LogInfo(string sourceName, string action, string message)
        {
        }

        public void LogWarning(string sourceName, string action, string message)
        {
        }

        public void LogWarning(string message)
        {
        }

        public void Error(string sourceName, string action, string message)
        {
            _logger.Log($"{sourceName}/{action}: {message}");
        }
    }
}