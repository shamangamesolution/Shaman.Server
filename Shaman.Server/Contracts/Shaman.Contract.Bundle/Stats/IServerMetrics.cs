namespace Shaman.Contract.Bundle.Stats
{
    public interface IServerMetrics
    {
        void TrackSendTickDuration(int maxDurationForSec, string listenerTag);
        void TrackSendersCount(string source, int count);
    }
}