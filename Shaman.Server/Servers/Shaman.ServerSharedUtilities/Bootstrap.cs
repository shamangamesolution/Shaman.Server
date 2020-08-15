using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Shaman.Contract.Common.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Shaman.ServerSharedUtilities
{
    public class Bootstrap
    {
        public static void Launch<T>(SourceType sourceType, Action<LoggerConfiguration, IConfigurationRoot> configureLogging = null) where T : class
        {
            //read config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Launch<T>(sourceType, config, configureLogging);
        }

        public static void Launch<T>(SourceType sourceType, IConfigurationRoot config, Action<LoggerConfiguration, IConfigurationRoot> configureLogging = null) where T : class
        {
            var logEventLevel = Enum.Parse<LogEventLevel>(config["Serilog:MinimumLevel"], ignoreCase: true);
            var customerToken = config["Serilog:customerToken"];

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(logEventLevel);
            if (!string.IsNullOrEmpty(customerToken))
                loggerConfiguration
                    .WriteTo.Loggly(customerToken: customerToken);
            
            loggerConfiguration
                .Enrich.WithProperty("version", config["ServerVersion"])
                .Enrich.WithProperty("source", sourceType)
                .Enrich.FromLogContext();
            configureLogging?.Invoke(loggerConfiguration, config);
            Log.Logger = loggerConfiguration
                .CreateLogger();


            try
            {
                SubscribeOnUnhandledException();

                var ip = config["BindToIP"];
                if (string.IsNullOrWhiteSpace(ip))
                {
                    Console.WriteLine("Unable to parse IPAddress from configuration file");
                    return;
                }

                int port = 0, httpsPort = 0;
                if (!int.TryParse(config["BindToPortHttp"].ToString(), out port))
                {
                    Console.WriteLine("Unable to parse port number from configuration file");
                    return;
                }
                // if (!int.TryParse(config["BindToPortHttps"].ToString(), out var httpsPort))
                // {
                //
                // Console.WriteLine("Unable to parse port number from configuration file");
                //     return;
                // }

                //run host       
                var host = new WebHostBuilder()
                    .UseKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate =
                            new MinDataRate(bytesPerSecond: 10, gracePeriod: TimeSpan.FromSeconds(30));
                        options.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 10, gracePeriod: TimeSpan.FromSeconds(30));
                        options.Listen(IPAddress.Parse(ip), port);
//                    options.Listen(IPAddress.Parse(ip), httpsPort,
//                        listenOptions => { listenOptions.UseHttps("certificate.pfx", "***"); });
                    })
                    .UseConfiguration(config)
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureLogging(builder =>
                    {
                        var logLevel = MapLogLevel(logEventLevel);
                        builder
                            .AddSerilog(Log.Logger, dispose: true)
                            .AddFilter(level => level >= logLevel)
                            .AddConsole();
                    })
                    .UseStartup<T>()
                    .Build();

                host.Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static LogLevel MapLogLevel(LogEventLevel logEventLevel)
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                    return LogLevel.Trace;
                case LogEventLevel.Debug:
                    return LogLevel.Debug;
                case LogEventLevel.Information:
                    return LogLevel.Information;
                case LogEventLevel.Warning:
                    return LogLevel.Warning;
                case LogEventLevel.Error:
                    return LogLevel.Error;
                case LogEventLevel.Fatal:
                    return LogLevel.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logEventLevel), logEventLevel, null);
            }
        }

        internal static void SubscribeOnUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    Log.Fatal(ex, "Unhandled exception occurs; sender : {@UnhandledExceptionSender}", sender);
                }
                else
                {
                    Log.Fatal(
                        "Unhandled exception occurs; exception : {@UnhandledException} sender : {@UnhandledExceptionSender}",
                        args.ExceptionObject, sender);
                }
            };
        }
    }

}