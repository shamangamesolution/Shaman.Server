using System;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Shaman.Common.Utils.Logging;
using LogLevel = Shaman.Common.Utils.Logging.LogLevel;

namespace Shaman.ServerSharedUtilities.Logging
{
    public class SerilogLogger: IShamanLogger
    {
        private ILogger _logger;
        private SourceType _source;
        private string _version;
        private LogLevel _logLevel;
        private string _subSource = "";
        
        public SerilogLogger(ILogger<SerilogLogger> logger)
        {
            _logger = logger;
        }

        public void SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }
        
        public void Initialize(SourceType source, string version, string subSource = "")
        {
            _source = source;
            _version = version;
            _subSource = subSource;
        }
        
        public void Error(string message)
        {
            if ((_logLevel & LogLevel.Error) == LogLevel.Error)
            {
                using (LogContext.PushProperty("version", _version))
                using (LogContext.PushProperty("sourceType", $"{_source}/{_subSource}"))
                {
                    _logger.LogCritical(message);
                }
            }
        }

        public void Error(Exception ex)
        {
            if ((_logLevel & LogLevel.Error) == LogLevel.Error)
            {
                using (LogContext.PushProperty("version", _version))
                using (LogContext.PushProperty("sourceType", $"{_source}/{_subSource}"))
                {
                    _logger.LogCritical(ex.ToString());
                }
            }
        }

        public void Info(string message)
        {
            if ((_logLevel & LogLevel.Info) == LogLevel.Info)
            {
                using (LogContext.PushProperty("version", _version))
                using (LogContext.PushProperty("sourceType", $"{_source}/{_subSource}"))
                {
                    _logger.LogInformation(message);
                }
            }
        }

        public void Debug(string message)
        {
            if ((_logLevel & LogLevel.Debug) == LogLevel.Debug)
            {
                using (LogContext.PushProperty("version", _version))
                using (LogContext.PushProperty("sourceType", $"{_source}/{_subSource}"))
                {
                    _logger.LogDebug(message);
                }
            }
        }

        public void Info(string sourceName, string action, string message)
        {
            Info($"{sourceName}.{action}: {message}");
        }

        public void Warning(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }

        public void Error(string sourceName, string action, string message)
        {
            Error($"{sourceName}.{action}: {message}");
        }
        

    }
}