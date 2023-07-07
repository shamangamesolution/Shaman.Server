
using Shaman.Contract.Bundle.Stats;

public interface IGameMetrics: IServerMetrics
{
    void TrackPeerJoin();
    void TrackRoomCreated();
    void TrackRoomDestroyed();
    void TrackPeerDisconnected(int amount = 1);
    void TrackMaxSendQueueSize(int size);
    void TrackAvgSendQueueSize(int size);
    void TrackTotalRoomLiveTime(int seconds);
    void TrackRoomTotalTrafficSent(int bytes);
    void TrackRoomTotalTrafficReceived(int bytes);
    void TrackRoomTotalMessagesSent(int count);
    void TrackRoomTotalMessagesReceived(int count);
    void TrackRoomTotalMessagesSent(int count, string messageName);
    void TrackRoomTotalMessagesReceived(int count, string messageName);
}