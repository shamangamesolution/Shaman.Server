namespace Shaman.Common.Server.Applications
{
    public interface IServerMetrics
    {
        void TrackSendTickDuration(int maxDurationForSec, string listenerTag);
    }
}