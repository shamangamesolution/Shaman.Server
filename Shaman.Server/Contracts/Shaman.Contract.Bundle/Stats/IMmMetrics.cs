
using Shaman.Contract.Bundle.Stats;

public interface IMmMetrics: IServerMetrics
{
    void TrackPlayerAdded();
    void TrackPlayerRemoved();
    void TrackPlayerCleared(int leftCount);
    void TrackMmCompleted(long ms);
}