using System;
using Microsoft.Extensions.Logging;
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

        public void Info(string message)
        {
            _logger.LogInformation(message);
        }

        public void Debug(string message)
        {
            _logger.LogDebug(message);
        }

        public void Info(string sourceName, string action, string message)
        {
            Info($"{sourceName}.{action}: {message}");
        }

        public void Warning(string sourceName, string action, string message)
        {
            _logger.LogWarning($"{sourceName}.{action}: {message}");
        }

        public void Error(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }
    }
}