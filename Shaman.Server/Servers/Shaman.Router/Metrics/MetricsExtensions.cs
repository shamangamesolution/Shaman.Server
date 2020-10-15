using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Hosting;
using App.Metrics.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shaman.Router.Metrics
{
    public static class MetricsExtensions
    {
        public static IServiceCollection AddCollectingRequestMetricsToGraphite(this IServiceCollection services,
            string graphiteUri,
            TimeSpan flushInterval, params string[] pathNodes)
        {
            var metricsRoot = MetricsFactory.CreateMetrics(graphiteUri, flushInterval, pathNodes);

            services.AddSingleton<IMetrics>(metricsRoot);
            services.AddSingleton(new MetricsOptions
            {
                Enabled = true,
                ReportingEnabled = true
            });
            services.AddSingleton(metricsRoot.Reporters);
            return services.AddMetricsReportingHostedService();
        }

        private static IServiceCollection AddMetricsReportingHostedService(
            this IServiceCollection services,
            EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler = null)
        {
            services.AddSingleton<IHostedService, MetricsReporterBackgroundService>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<MetricsOptions>();
                var metrics = serviceProvider.GetRequiredService<IMetrics>();
                var reporters = serviceProvider.GetService<IReadOnlyCollection<IReportMetrics>>();

                var instance = new MetricsReporterBackgroundService(metrics, options, reporters);

                if (unobservedTaskExceptionHandler != null)
                {
                    instance.UnobservedTaskException += unobservedTaskExceptionHandler;
                }

                return instance;
            });

            return services;
        }
    }
}