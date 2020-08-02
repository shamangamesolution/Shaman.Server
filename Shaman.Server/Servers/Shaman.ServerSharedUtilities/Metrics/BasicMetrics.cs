using System;
using App.Metrics;
using App.Metrics.Histogram;
using Shaman.Common.Metrics;
using Shaman.Common.Server.Applications;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;

namespace Shaman.ServerSharedUtilities.Metrics
{
    public class BasicMetrics: IServerMetrics
    {
        private const string SendTickTag = "lr";
        private readonly IMetricsAgent _metricsAgent;
        protected IMetrics Metrics => _metricsAgent.Metrics;

        private static readonly HistogramOptions Gen0Collections = new HistogramOptions
            {Name = "Gen 0 Collections", MeasurementUnit = Unit.Items};

        private static readonly HistogramOptions Gen1Collections = new HistogramOptions
            {Name = "Gen 1 Collections", MeasurementUnit = Unit.Items};

        private static readonly HistogramOptions Gen2Collections = new HistogramOptions
            {Name = "Gen 2 Collections", MeasurementUnit = Unit.Items};

        private static readonly HistogramOptions GcTotalMemory = new HistogramOptions
            {Name = "GC Total Memory", MeasurementUnit = Unit.Bytes};

        private static readonly HistogramOptions PendingTasks = new HistogramOptions
            {Name = "PendingTasks", MeasurementUnit = Unit.Items};

        private static readonly HistogramOptions ExecutingTask = new HistogramOptions
            {Name = "ExecutingTask", MeasurementUnit = Unit.Items};

        private static readonly HistogramOptions ActiveTimers = new HistogramOptions
            {Name = "ActiveTimers", MeasurementUnit = Unit.Items};
        
        private static readonly HistogramOptions ActivePeriodicTimers = new HistogramOptions
            {Name = "ActivePeriodicTimers", MeasurementUnit = Unit.Items};
        
        private static readonly HistogramOptions ActivePeriodicSlTimers = new HistogramOptions
            {Name = "ActivePeriodicSlTimers", MeasurementUnit = Unit.Items};
        
        private static readonly HistogramOptions MaxSendTickDuration = new HistogramOptions
        {
            Name = "TickTime",
            MeasurementUnit = Unit.None,
        };
        
        private readonly ITaskScheduler _taskScheduler;

        protected BasicMetrics(IMetricsAgent metricsAgent, ITaskSchedulerFactory taskSchedulerFactory)
        {
            _metricsAgent = metricsAgent;

            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _taskScheduler.ScheduleOnInterval(CollectMemoryAndThreadsUsage, 0, 1000);
        }

        public void TrackSendTickDuration(int maxDurationForSec, string listenerTag)
        {
            Metrics.Measure.Histogram.Update(MaxSendTickDuration, new MetricTags(SendTickTag, listenerTag), maxDurationForSec);
        }
        private void CollectMemoryAndThreadsUsage()
        {
            Metrics.Measure.Histogram.Update(Gen0Collections, GC.CollectionCount(0));
            Metrics.Measure.Histogram.Update(Gen1Collections, GC.CollectionCount(1));
            Metrics.Measure.Histogram.Update(Gen2Collections, GC.CollectionCount(2));
            Metrics.Measure.Histogram.Update(GcTotalMemory, GC.GetTotalMemory(false));

            Metrics.Measure.Histogram.Update(PendingTasks, TaskScheduler.GetGlobalScheduledOnceTasksCount());
            Metrics.Measure.Histogram.Update(ExecutingTask, PendingTask.GetExecutingActionsCount());
            Metrics.Measure.Histogram.Update(ActiveTimers, PendingTask.GetActiveTimersCount());
            Metrics.Measure.Histogram.Update(ActivePeriodicTimers, PendingTask.GetActivePeriodicTimersCount());
            Metrics.Measure.Histogram.Update(ActivePeriodicSlTimers, PendingTask.GetActivePeriodicSlTimersCount());
        }
    }
}