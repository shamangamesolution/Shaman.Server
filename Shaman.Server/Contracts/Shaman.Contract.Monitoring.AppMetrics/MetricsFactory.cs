using System;
using App.Metrics;
using App.Metrics.Formatters.Graphite;

namespace Shaman.Contract.Monitoring.AppMetrics
{
    public static class MetricsFactory
    {
        public static IMetricsRoot CreateMetrics(string graphiteUri, TimeSpan flushInterval, params string[] pathNodes)
        {
            return App.Metrics.AppMetrics.CreateDefaultBuilder()
                .Report.ToGraphite(g =>
                {
                    
                    g.FlushInterval = flushInterval;
                    g.Graphite.BaseUri = new Uri(graphiteUri);
                    var metricFields = new MetricFields();
                    metricFields.Histogram.OnlyInclude(HistogramFields.Count, HistogramFields.Max, HistogramFields.Min,
                        HistogramFields.P999, HistogramFields.Sum, HistogramFields.Mean);
                    metricFields.Meter.OnlyInclude(MeterFields.Count, MeterFields.Rate5M, MeterFields.Rate1M, MeterFields.SetItem);
                    metricFields.DefaultGraphiteMetricFieldNames();
                    g.MetricsOutputFormatter
                        = new MetricsGraphitePlainTextProtocolOutputFormatter(
                        new MetricsGraphitePlainTextProtocolOptions
                        {
                            MetricPointTextWriter = new CustomGraphitePointTextWriter(pathNodes)
                        }, metricFields);
                })
                .Build();
        }
    }
}