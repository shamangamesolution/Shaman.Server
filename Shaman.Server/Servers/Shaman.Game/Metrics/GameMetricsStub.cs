namespace Shaman.Game.Metrics
{
    public class GameMetricsStub : IGameMetrics
    {
        public void TrackPeerJoin()
        {
        }

        public void TrackRoomCreated()
        {
        }

        public void TrackRoomDestroyed()
        {
        }

        public void TrackPeerDisconnected(int amount)
        {
        }

        public void TrackMaxSendQueueSize(int size)
        {
        }

        public void TrackAvgSendQueueSize(int size)
        {
        }

        public void TrackTotalRoomLiveTime(int seconds)
        {
        }

        public void TrackRoomTotalTrafficSent(int bytes)
        {
        }

        public void TrackRoomTotalTrafficReceived(int bytes)
        {
        }

        public void TrackRoomTotalMessagesSent(int count)
        {
        }

        public void TrackRoomTotalMessagesReceived(int count)
        {
        }

        public void TrackRoomTotalMessagesSent(int count, string messageName)
        {
        }

        public void TrackRoomTotalMessagesReceived(int count, string messageName)
        {
        }

        public void TrackSendTickDuration(int maxDurationForSec, string listenerTag)
        {
        }
    }
}