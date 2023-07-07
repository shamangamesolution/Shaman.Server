using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Histogram;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Contract.Monitoring.AppMetrics
{
    public class GameMetrics : BasicMetrics, IGameMetrics
    {
        private static readonly CounterOptions RoomPeers = new CounterOptions
            {Name = "RoomPeers", MeasurementUnit = Unit.None};

        private static readonly CounterOptions Rooms = new CounterOptions
            {Name = "Rooms", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions MaxSendQueueSize = new HistogramOptions
            {Name = "Max send queue size", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions AvgSendQueueSize = new HistogramOptions
            {Name = "Average send queue size", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions RoomLiveTime = new HistogramOptions
            {Name = "Room live time", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions RoomTrafficSent = new HistogramOptions
            {Name = "Room traffic sent", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions RoomTrafficReceived = new HistogramOptions
            {Name = "Room traffic received", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions RoomTotalMessagesReceived = new HistogramOptions
            {Name = "Room messages received", MeasurementUnit = Unit.None};

        private static readonly HistogramOptions RoomTotalMessagesSent = new HistogramOptions
            {Name = "Room messages sent", MeasurementUnit = Unit.None};

        public GameMetrics(IMetricsAgent metricsAgent, ITaskSchedulerFactory taskSchedulerFactory) : base(metricsAgent,
            taskSchedulerFactory)
        {
        }

        public void TrackPeerJoin()
        {
            Metrics.Measure.Counter.Increment(RoomPeers);
        }

        public void TrackPeerDisconnected(int amount)
        {
            Metrics.Measure.Counter.Decrement(RoomPeers, amount);
        }

        public void TrackRoomCreated()
        {
            Metrics.Measure.Counter.Increment(Rooms);
        }

        public void TrackRoomDestroyed()
        {
            Metrics.Measure.Counter.Decrement(Rooms);
        }

        public void TrackMaxSendQueueSize(int size)
        {
            Metrics.Measure.Histogram.Update(MaxSendQueueSize, size);
        }

        public void TrackAvgSendQueueSize(int size)
        {
            Metrics.Measure.Histogram.Update(AvgSendQueueSize, size);
        }

        public void TrackTotalRoomLiveTime(int seconds)
        {
            Metrics.Measure.Histogram.Update(RoomLiveTime, seconds);
        }

        public void TrackRoomTotalTrafficSent(int bytes)
        {
            Metrics.Measure.Histogram.Update(RoomTrafficSent, bytes);
        }

        public void TrackRoomTotalTrafficReceived(int bytes)
        {
            Metrics.Measure.Histogram.Update(RoomTrafficReceived, bytes);
        }

        public void TrackRoomTotalMessagesSent(int count)
        {
            Metrics.Measure.Histogram.Update(RoomTotalMessagesSent, count);
        }

        public void TrackRoomTotalMessagesReceived(int count)
        {
            Metrics.Measure.Histogram.Update(RoomTotalMessagesReceived, count);
        }

        public void TrackRoomTotalMessagesSent(int count, string messageName)
        {
            Metrics.Measure.Histogram.Update(RoomTotalMessagesSent, new MetricTags("message", messageName),
                count);
        }

        public void TrackRoomTotalMessagesReceived(int count, string messageName)
        {
            Metrics.Measure.Histogram.Update(RoomTotalMessagesReceived, new MetricTags("message", messageName),
                count);
        }
    }
}