using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Histogram;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.ServerSharedUtilities.Metrics;

namespace Shaman.MM.Metrics
{
    public interface IMmMetrics: IServerMetrics
    {
        void TrackPlayerAdded();
        void TrackPlayerRemoved();
        void TrackPlayerCleared(int leftCount);
        void TrackMmCompleted(long ms);
    }

    public class MmMetrics : BasicMetrics, IMmMetrics
    {
        private static readonly HistogramOptions Gen0Collections = new HistogramOptions
            {Name = "Gen 0 Collections", MeasurementUnit = Unit.Items};

        private static readonly CounterOptions MmPeers = new CounterOptions {Name = "MM peers"};
        private static readonly HistogramOptions MmTime = new HistogramOptions {Name = "MM time"};

        public MmMetrics(IMetricsAgent metricsAgent, ITaskSchedulerFactory taskSchedulerFactory) : base(metricsAgent,
            taskSchedulerFactory)
        {
        }

        public void TrackPlayerAdded()
        {
            Metrics.Measure.Counter.Increment(MmPeers, 1);
        }

        public void TrackPlayerRemoved()
        {
            Metrics.Measure.Counter.Decrement(MmPeers, 1);
        }

        public void TrackPlayerCleared(int leftCount)
        {
            Metrics.Measure.Counter.Decrement(MmPeers, leftCount);
        }

        public void TrackMmCompleted(long ms)
        {
            Metrics.Measure.Histogram.Update(MmTime, ms);
        }
    }
}