using System;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Histogram;
using App.Metrics.Reporting.Graphite;
using NUnit.Framework;

namespace Shaman.Common.Metrics.Tests
{
    public class MetricsTests
    {
        [Test]
        public async Task TestMetricsSending()
        {
            var metricsRoot = MetricsFactory.CreateMetrics("net.tcp://0.0.0.0:2003", TimeSpan.FromSeconds(10),
                "SA3",
                "Photon",
                "1_0_0");

            metricsRoot.Measure.Histogram.Update(
                new HistogramOptions {Name = "fx", MeasurementUnit = Unit.Bytes}, 10);

            metricsRoot.Measure.Histogram.Update(
                new HistogramOptions {Name = "fx", MeasurementUnit = Unit.Bytes}, new MetricTags("tagName","tagValue"), 10);

            var reporter = metricsRoot.Reporters.OfType<GraphiteReporter>().Single();
            await reporter.FlushAsync(metricsRoot.Snapshot.Get());
        }
    }
}