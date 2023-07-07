using Prometheus;

namespace Shaman.Contracts.Monitoring.Prometheus;

public class PrometheusMetrics : IGameMetrics
{
    private static readonly Gauge RoomPeers = Metrics.CreateGauge("room_peers", "Room peers");

    private static readonly Gauge Rooms = Metrics.CreateGauge("rooms", "Rooms");

    private static readonly Histogram MaxSendQueueSize =
        Metrics.CreateHistogram("max_send_queue_size", "Max send queue size");

    private static readonly Histogram AvgSendQueueSize =
        Metrics.CreateHistogram("average_send_queue_size", "Average send queue size");

    private static readonly Histogram RoomLiveTime = Metrics.CreateHistogram("room_live_time", "Room live time");

    private static readonly Histogram RoomTrafficSent =
        Metrics.CreateHistogram("room_traffic_sent", "Room traffic sent");

    private static readonly Histogram RoomTrafficReceived =
        Metrics.CreateHistogram("room_traffic_received", "Room traffic received");

    private static readonly Histogram RoomTotalMessagesReceived =
        Metrics.CreateHistogram("room_messages_received", "Room messages received");

    private static readonly Histogram RoomTotalMessagesSent =
        Metrics.CreateHistogram("room_messages_sent", "Room messages sent");

    private static readonly Histogram MaxSendTickDuration =
        Metrics.CreateHistogram("max_send_tick_duration", "Max send tick duration");

    private static readonly Histogram PacketSenderPeers =
        Metrics.CreateHistogram("packet_sender_peers", "Packet sender peers");

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
        RoomTrafficSent.Observe(bytes);
    }

    public void TrackRoomTotalTrafficReceived(int bytes)
    {
        RoomTrafficReceived.Observe(bytes);
    }

    public void TrackRoomTotalMessagesSent(int count)
    {
        RoomTotalMessagesSent.Observe(count);
    }

    public void TrackRoomTotalMessagesReceived(int count)
    {
        RoomTotalMessagesReceived.Observe(count);
    }

    public void TrackRoomTotalMessagesSent(int count, string messageName)
    {
        RoomTotalMessagesSent.WithLabels(messageName).Observe(count);
    }

    public void TrackRoomTotalMessagesReceived(int count, string messageName)
    {
        RoomTotalMessagesReceived.WithLabels(messageName).Observe(count);
    }

    public void TrackSendTickDuration(int maxDurationForSec, string listenerTag)
    {
        MaxSendTickDuration.Observe(maxDurationForSec);
    }

    public void TrackSendersCount(string source, int count)
    {
        PacketSenderPeers.Observe(count);
    }
}