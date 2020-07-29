using System;
using Microsoft.Extensions.Logging;
using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Utils.Logging;

namespace Shaman.ServerSharedUtilities.Logging
{
    public class SerilogLogger : IShamanLogger
    {
        private readonly ILogger _logger;

        public SerilogLogger(ILogger<SerilogLogger> logger)
        {
            _logger = logger;
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }

        public void Error(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        public void LogInfo(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogInfo(string sourceName, string action, string message)
        {
            LogInfo($"{sourceName}.{action}: {message}");
        }

        public void LogWarning(string sourceName, string action, string message)
        {
            _logger.LogWarning($"{sourceName}.{action}: {message}");
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void Error(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }
    }
}