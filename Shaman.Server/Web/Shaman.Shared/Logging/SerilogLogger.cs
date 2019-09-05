using System;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Shaman.Common.Utils.Logging;
using LogLevel = Shaman.Common.Utils.Logging.LogLevel;

namespace Shaman.Shared.Logging
{
    public class SerilogLogger: IShamanLogger
    {
        
        private ILogger _logger;
        private SourceType _source;
        private string _version;
        
        public SerilogLogger(ILogger<SerilogLogger> logger)
        {
            _logger = logger;
        }

        public void SetLogLevel(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void Debug(string message)
        {
            throw new NotImplementedException();
        }

        public void Initialize(SourceType source, string version, string subSource = "")
        {
            _source = source;
            _version = version;
        }

        public void Info(string sourceName,string action, string message)
        {
            using (LogContext.PushProperty("version", _version))
            using (LogContext.PushProperty("sourceType", _source.ToString()))
            using (LogContext.PushProperty("sourceName", sourceName))
            using (LogContext.PushProperty("action", action))
            {
                _logger.LogInformation(message);
            }
        }

        public void Error(string sourceName,string action, string message)
        {
            using (LogContext.PushProperty("version", _version))
            using (LogContext.PushProperty("sourceType", _source.ToString()))
            using (LogContext.PushProperty("sourceName", sourceName))
            using (LogContext.PushProperty("action", action))
            {
                _logger.LogCritical(message);
            }
        }
        
        public void Warning(string sourceName, string action, string message)
        {
            using (LogContext.PushProperty("version", _version))
            using (LogContext.PushProperty("sourceType", _source.ToString()))
            using (LogContext.PushProperty("sourceName", sourceName))
            using (LogContext.PushProperty("action", action))
            {
                _logger.LogWarning(message);
            }
        }
        

    }
}