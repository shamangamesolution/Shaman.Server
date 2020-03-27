using System;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;

namespace Shaman.Common.Metrics
{
    public class MetricsSettings
    {
        public string GraphiteUrl { get; set; }
        public int ReportIntervalMs { get; set; }
        public string Path { get; set; }
    }

    public interface IMetricsAgent
    {
        IMetrics Metrics { get; }
    }

    public class MetricsAgent : IMetricsAgent
    {
        private readonly IMetricsRoot _metrics;
        private readonly TimeSpan _reportInterval;
        public IMetrics Metrics => _metrics;

        public MetricsAgent(MetricsSettings metricsSettings, params string[] additionalPathNodes)
        {
            _reportInterval = TimeSpan.FromMilliseconds(metricsSettings.ReportIntervalMs);
            _metrics = MetricsFactory.CreateMetrics(metricsSettings.GraphiteUrl,
                _reportInterval,
                metricsSettings.Path.Split('.').Concat(additionalPathNodes).ToArray()
            );

            ScheduleReport(null);
        }

        private void ScheduleReport(Task task)
        {
            Task.Delay(_reportInterval).ContinueWith(Report).ContinueWith(ScheduleReport);
        }

        private void Report(Task task)
        {
            try
            {
                foreach (var reporter in _metrics.Reporters)
                {
                    reporter.FlushAsync(_metrics.Snapshot.Get(reporter.Filter)).Wait();
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}