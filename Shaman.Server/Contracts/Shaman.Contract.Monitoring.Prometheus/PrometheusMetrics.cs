using Prometheus;

namespace Shaman.Contracts.Monitoring.Prometheus;

public class PrometheusMetrics : IGameMetrics
{
    private static readonly Gauge RoomPeers = Metrics.CreateGauge("room_peers", "Room peers");

    private static readonly Gauge Rooms = Metrics.CreateGauge("rooms", "Rooms");

    private static readonly Histogram MaxSendQueueSize =
        Metrics.CreateHistogram("max_send_queue_size", "Max send queue size", new HistogramConfiguration
        {
            Buckets = new double[] {0, 1, 2, 3, 5, 7, 10, 13, 20, 30, 50}
        });

    private static readonly Histogram AvgSendQueueSize =
        Metrics.CreateHistogram("average_send_queue_size", "Average send queue size", new HistogramConfiguration
        {
            Buckets = new double[] {0, 1, 2, 3, 5, 7, 10, 13, 20, 30, 50}
        });

    private static readonly Histogram RoomLiveTime = Metrics.CreateHistogram("room_live_time", "Room live time",
        new HistogramConfiguration
        {
            Buckets = new double[] {1, 2, 5, 10, 20, 50, 100, 200, 400, 600, 800, 1000, 1500, 2000, 3000}
        });

    private static readonly Counter RoomTrafficSent = Metrics.CreateCounter("room_traffic_sent", "Room traffic sent");
    private static readonly Counter TrafficSent = Metrics.CreateCounter("traffic_sent", "Traffic sent");
    private static readonly Counter TrafficReceived = Metrics.CreateCounter("traffic_received", "Traffic received");

    private static readonly Counter RoomTrafficReceived =
        Metrics.CreateCounter("room_traffic_received", "Room traffic received");

    private static readonly Counter RoomTotalMessagesReceived =
        Metrics.CreateCounter("room_messages_received", "Room messages received");

    private static readonly Counter RoomTotalMessagesSent =
        Metrics.CreateCounter("room_messages_sent", "Room messages sent");

    private static readonly Histogram MaxSendTickDuration =
        Metrics.CreateHistogram("max_send_tick_duration", "Max send tick duration", new HistogramConfiguration
        {
            Buckets = new double[] {0, 1, 3, 7, 11, 15, 20, 30, 50, 70, 100, 300, 1000}
        });

    private static readonly Gauge PacketSenderPeers =
        Metrics.CreateGauge("packet_sender_peers", "Packet sender peers");

    private IMetricServer _metricServer;

    public PrometheusMetrics(int port)
    {
        _metricServer = new MetricServer(port).Start();
    }

    public void TrackPeerJoin()
    {
        RoomPeers.Inc();
    }

    public void TrackPeerDisconnected(int amount)
    {
        RoomPeers.Dec(amount);
    }

    public void TrackRoomCreated()
    {
        Rooms.Inc();
    }

    public void TrackRoomDestroyed()
    {
        Rooms.Dec();
    }

    public void TrackMaxSendQueueSize(int size)
    {
        MaxSendQueueSize.Observe(size);
    }

    public void TrackAvgSendQueueSize(int size)
    {
        AvgSendQueueSize.Observe(size);
    }

    public void TrackTotalRoomLiveTime(int seconds)
    {
        RoomLiveTime.Observe(seconds);
    }

    public void TrackRoomTotalTrafficSent(int bytes)
    {
        RoomTrafficSent.Inc(bytes);
    }

    public void TrackRoomTrafficSent(int bytes)
    {
        TrafficSent.Inc(bytes);
    }

    public void TrackRoomTotalTrafficReceived(int bytes)
    {
        RoomTrafficReceived.Inc(bytes);
    }

    public void TrackRoomTrafficReceived(int bytes)
    {
        TrafficReceived.Inc(bytes);
    }

    public void TrackRoomTotalMessagesSent(int count)
    {
        RoomTotalMessagesSent.Inc(count);
    }

    public void TrackRoomTotalMessagesReceived(int count)
    {
        RoomTotalMessagesReceived.Inc(count);
    }

    public void TrackRoomTotalMessagesSent(int count, string messageName)
    {
        RoomTotalMessagesSent.WithLabels(messageName).Inc(count);
    }

    public void TrackRoomTotalMessagesReceived(int count, string messageName)
    {
        RoomTotalMessagesReceived.WithLabels(messageName).Inc(count);
    }

    public void TrackSendTickDuration(int maxDurationForSec, string listenerTag)
    {
        MaxSendTickDuration.Observe(maxDurationForSec);
    }

    public void TrackSendersCount(string source, int count)
    {
        PacketSenderPeers.Inc(count);
    }
}