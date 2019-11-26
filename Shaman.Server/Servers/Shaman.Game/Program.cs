using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;


namespace Shaman.Game
{
    public class Program
    {
        internal static void Main(string[] args)
        {
            //read config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .AddEnvironmentVariables()
                .Build();

            Start(config);
        }

        public static void Start(IConfigurationRoot config)
        {
            var logEventLevel = Enum.Parse<LogEventLevel>(config["Serilog:MinimumLevel"], ignoreCase: true);
            var consoleLogLevel = Enum.Parse<LogLevel>(config["ConsoleLogLevel"], ignoreCase: true);
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(logEventLevel);

            if (!string.IsNullOrEmpty(config["Serilog:customerToken"]))
                loggerConfiguration.WriteTo.Loggly(customerToken: config["Serilog:customerToken"]);

            var logger = Log.Logger = loggerConfiguration.CreateLogger();

            SubscribeOnUnhandledException(logger);

            var ip = config["BindToIP"];
            if (string.IsNullOrWhiteSpace(ip))
            {
                Console.WriteLine("Unable to parse IPAddress from configuration file");
                return;
            }

            int port = 0;
            if (!int.TryParse(config["BindToPortHttp"].ToString(), out port))
            {
                Console.WriteLine("Unable to parse port number from configuration file");
                return;
            }

            //run host       
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 10, gracePeriod: TimeSpan.FromSeconds(30));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 10, gracePeriod: TimeSpan.FromSeconds(30));
                    options.Listen(IPAddress.Parse(ip), port);
                })
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddConsole()
                        .AddFilter<ConsoleLoggerProvider>("Microsoft", consoleLogLevel)
                        .AddSerilog(logger, dispose: true);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }


        internal static void SubscribeOnUnhandledException(ILogger logger)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    logger.Fatal(ex, "Unhandled exception occurs; sender : {@UnhandledExceptionSender}", sender);
                }
                else
                {
                    logger.Fatal(
                        "Unhandled exception occurs; exception : {@UnhandledException} sender : {@UnhandledExceptionSender}",
                        args.ExceptionObject, sender);
                }
            };
        }
    }
}