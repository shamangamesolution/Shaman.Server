namespace Shaman.Launchers.Common.Metrics.Metrics;

public class BundledGameMetrics : IGameMetrics
{
    private IGameMetrics _gameMetrics;

    public void ConfigureImplementation(IGameMetrics gameMetrics)
    {
        _gameMetrics = gameMetrics;
    }


    public void TrackSendTickDuration(int maxDurationForSec, string listenerTag)
    {
        _gameMetrics?.TrackSendTickDuration(maxDurationForSec, listenerTag);
    }

    public void TrackSendersCount(string source, int count)
    {
        _gameMetrics?.TrackSendersCount(source, count);
    }

    public void TrackPeerJoin()
    {
        _gameMetrics?.TrackPeerJoin();
    }

    public void TrackRoomCreated()
    {
        _gameMetrics?.TrackRoomCreated();
    }

    public void TrackRoomDestroyed()
    {
        _gameMetrics?.TrackRoomDestroyed();
    }

    public void TrackPeerDisconnected(int amount = 1)
    {
        _gameMetrics?.TrackPeerDisconnected(amount);
    }

    public void TrackMaxSendQueueSize(int size)
    {
        _gameMetrics?.TrackMaxSendQueueSize(size);
    }

    public void TrackAvgSendQueueSize(int size)
    {
        _gameMetrics?.TrackAvgSendQueueSize(size);
    }

    public void TrackTotalRoomLiveTime(int seconds)
    {
        _gameMetrics?.TrackTotalRoomLiveTime(seconds);
    }

    public void TrackRoomTotalTrafficSent(int bytes)
    {
        _gameMetrics?.TrackRoomTotalTrafficSent(bytes);
    }

    public void TrackTrafficSent(int bytes)
    {
        _gameMetrics?.TrackTrafficSent(bytes);
    }

    public void TrackRoomTotalTrafficReceived(int bytes)
    {
        _gameMetrics?.TrackRoomTotalTrafficReceived(bytes);
    }

    public void TrackTrafficReceived(int bytes)
    {
        _gameMetrics?.TrackTrafficReceived(bytes);
    }

    public void TrackRoomTotalMessagesSent(int count)
    {
        _gameMetrics?.TrackRoomTotalMessagesSent(count);
    }

    public void TrackRoomTotalMessagesReceived(int count)
    {
        _gameMetrics?.TrackRoomTotalMessagesReceived(count);
    }

    public void TrackRoomMessagesSent(int count, string messageName)
    {
        _gameMetrics?.TrackRoomMessagesSent(count, messageName);
    }

    public void TrackRoomMessagesReceived(int count, string messageName)
    {
        _gameMetrics?.TrackRoomMessagesReceived(count, messageName);
    }
}