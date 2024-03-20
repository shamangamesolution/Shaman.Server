using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Shaman.ServiceBootstrap
{
    public class Bootstrap
    {

        private static IConfigurationRoot GetConfig(string configRole)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.common.json", optional: false)
                .AddJsonFile($"appsettings.common.{configRole}.json", optional: false)
                .AddJsonFile($"appsettings.launcher.{configRole}.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.{configRole}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static Task LaunchWithCommonAndRoleConfig<T>(string configRole,
            Action<LoggerConfiguration, IConfiguration> configureLogging = null) where T : class
        {
            return BuildHostApp<T>(GetConfig(configRole), configureLogging).RunAsync();
        }

        public static void Launch<T>(IConfigurationRoot config,
            Action<LoggerConfiguration, IConfiguration> configureLogging = null) where T : class
        {
            BuildHostApp<T>(config, configureLogging).Run();
        }

        public static async Task RunWebApp<TStartup>(string[] args,
            Action<LoggerConfiguration, IConfiguration> configureLogging = null)
            where TStartup : IShamanWebStartup, new()
        {
            await (await BuildWebApp<TStartup>(args, configureLogging)).RunAsync();
        }

        public static Task<WebApplication> BuildWebApp<TStartup>(
            Action<LoggerConfiguration, IConfiguration> configureLogging = null)
            where TStartup : IShamanWebStartup, new()
        {
            return BuildWebApp<TStartup>(Array.Empty<string>(), configureLogging);
        }

        public static async Task<WebApplication> BuildWebApp<TStartup>(string[] args,
            Action<LoggerConfiguration, IConfiguration> configureLogging = null)
            where TStartup : IShamanWebStartup, new()
        {
            var startup = new TStartup();
            SubscribeOnUnhandledException();

            var webAppBuilder = WebApplication.CreateBuilder(args);
            var configurationManager = webAppBuilder.Configuration;
            var port = configurationManager["CommonSettings:BindToPortHttp"];
            UseSerilog(webAppBuilder.Host, configurationManager, configureLogging);
            UseKestrel(webAppBuilder.WebHost, port);
            webAppBuilder.Services
                .AddControllers(startup.AddMvcOptions)
                // this tells mvc where to search controllers
                .AddApplicationPart(typeof(TStartup).Assembly);
            startup.ConfigureServices(webAppBuilder.Services, configurationManager);
            var app = webAppBuilder.Build();
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms (In {RequestSizeKb:0.000}Kb, Out {ResponseSizeKb:0.000}Kb)";
                options.EnrichDiagnosticContext = (context, httpContext) =>
                {
                    context.Set(LogOutputTemplateNames.RequestSize, httpContext.Request.ContentLength.GetValueOrDefault() / 1000f);
                    context.Set(LogOutputTemplateNames.ResponseSize, httpContext.Response.Headers.ContentLength.GetValueOrDefault() / 1000f);
                };
            });
            foreach (var middleWare in startup.GetMiddleWares(app.Services))
                app.UseMiddleware(middleWare);
            app.UseRouting();
            startup.ConfigureApp(app);
            app.UseEndpoints(builder =>
                builder.MapControllerRoute("default", "{controller=Home}/{action=Index}"));
            await startup.Initialize(app.Services);
            return app;
        }

        private static IWebHostBuilder UseKestrel(IWebHostBuilder configureWebHostBuilder, string port)
        {
            return configureWebHostBuilder.UseKestrel(options =>
            {
                options.Limits.MinRequestBodyDataRate =
                    new MinDataRate(bytesPerSecond: 10, gracePeriod: TimeSpan.FromSeconds(30));
                options.Limits.MinResponseDataRate =
                    new MinDataRate(bytesPerSecond: 10, gracePeriod: TimeSpan.FromSeconds(30));
                options.ListenAnyIP(int.Parse(port));
            });
        }

        private static void UseSerilog(IHostBuilder configureHostBuilder, IConfiguration configurationManager,
            Action<LoggerConfiguration, IConfiguration> configureLogging = null)
        {
            configureHostBuilder
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
                        .ReadFrom.Configuration(configurationManager)
                        .Enrich.FromLogContext()
                        .Enrich.WithThreadId()
                        .Enrich.WithProperty("ServiceLabel", configurationManager["ServiceLabel"])
                        .Enrich.WithProperty("ServiceVersion", configurationManager["ServiceVersion"])
                        .Enrich.WithProperty("Host", Environment.MachineName);

                    configureLogging?.Invoke(configuration, configurationManager);
                    if (!IsSerilogConsoleDeclared(configurationManager))
                        configuration.WriteTo.Console(applyThemeToRedirectedOutput: true, theme: AnsiConsoleTheme.Code,
                            outputTemplate:
                            "[{Timestamp:dd-MM-yyyy HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");
                });
        }

        private static bool IsSerilogConsoleDeclared(IConfiguration configurationManager)
            => configurationManager.GetSection("Serilog:WriteTo").GetChildren()
                .Any(s => s.GetValue<string>("Name") == "Console");

        public static IHost BuildHostApp<TStartup>(IConfiguration configuration,
            Action<LoggerConfiguration, IConfiguration> configureLogging = null) where TStartup : class
        {
            SubscribeOnUnhandledException();

            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            var port = configuration["CommonSettings:BindToPortHttp"];
            UseSerilog(hostBuilder, configuration, configureLogging);
            return hostBuilder
                .ConfigureWebHostDefaults(builder =>
                    UseKestrel(builder.UseConfiguration(configuration), port)
                        .UseStartup<TStartup>())
                .Build();
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