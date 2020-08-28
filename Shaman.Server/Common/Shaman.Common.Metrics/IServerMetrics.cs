namespace Shaman.Common.Metrics
{
    public interface IServerMetrics
    {
        void TrackSendTickDuration(int maxDurationForSec, string listenerTag);
    }
}