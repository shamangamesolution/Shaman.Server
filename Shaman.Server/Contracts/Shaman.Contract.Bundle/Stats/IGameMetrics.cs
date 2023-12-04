
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
    void TrackTrafficSent(int bytes);
    void TrackRoomTotalTrafficReceived(int bytes);
    void TrackTrafficReceived(int bytes);
    void TrackRoomTotalMessagesSent(int count);
    void TrackRoomTotalMessagesReceived(int count);
    void TrackRoomMessagesSent(int count, string messageName);
    void TrackRoomMessagesReceived(int count, string messageName);
}