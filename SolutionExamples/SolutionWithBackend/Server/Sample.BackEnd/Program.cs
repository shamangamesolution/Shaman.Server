using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace Sample.BackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //read config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
            
            var logger = Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(Enum.Parse<LogEventLevel>(config["Serilog:MinimumLevel"], ignoreCase: true))
                .WriteTo.Loggly(customerToken: config["Serilog:customerToken"])
                .CreateLogger();
            
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
                        .AddSerilog(logger, dispose: true)
                        .AddConsole();
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
            
            //CreateWebHostBuilder(args).Build().Run();
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