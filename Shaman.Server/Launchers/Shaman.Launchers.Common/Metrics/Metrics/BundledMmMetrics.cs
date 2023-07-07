namespace Shaman.Launchers.Common.Metrics.Metrics;

public class BundledMmMetrics: IMmMetrics
{
    private IMmMetrics _mmMetrics;

    public void ConfigureImplementation(IMmMetrics mmMetrics)
    {
        _mmMetrics = mmMetrics;
    }

    public void TrackSendTickDuration(int maxDurationForSec, string listenerTag)
    {
        _mmMetrics?.TrackSendTickDuration(maxDurationForSec, listenerTag);
    }

    public void TrackSendersCount(string source, int count)
    {
        _mmMetrics?.TrackSendersCount(source, count);
    }

    public void TrackPlayerAdded()
    {
        _mmMetrics?.TrackPlayerAdded();
    }

    public void TrackPlayerRemoved()
    {
        _mmMetrics?.TrackPlayerRemoved();
    }

    public void TrackPlayerCleared(int leftCount)
    {
        _mmMetrics?.TrackPlayerCleared(leftCount);
    }

    public void TrackMmCompleted(long ms)
    {
        _mmMetrics?.TrackMmCompleted(ms);
    }
}