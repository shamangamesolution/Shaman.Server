using System;
using Shaman.Common.Utils.Logging;

namespace Shaman.Monkeys.Logging
{
    public class ShamanMonkeyLogger : IShamanLogger
    {
        private readonly ILogger _logger;

        public ShamanMonkeyLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void SetLogLevel(LogLevel logLevel)
        {
        }

        public void Error(string message)
        {
            _logger.Log(message);
        }


        public void Error(Exception ex)
        {
            _logger.Log($"{ex}");
        }

        public void Info(string message)
        {
        }

        public void Debug(string message)
        {
        }

        public void Initialize(SourceType source, string version, string subSource = "")
        {
        }

        public void Info(string sourceName, string action, string message)
        {
        }

        public void Warning(string sourceName, string action, string message)
        {
        }

        public void Error(string sourceName, string action, string message)
        {
            _logger.Log($"{sourceName}/{action}: {message}");
        }
    }
}