using System;
using System.Collections.Generic;

namespace Shaman.SyncedRepositories.Managers
{
    public interface IConfirmationManager
    {
        void Start(int queueDepth = 100, int clearQueuesIntervalMs = 1000, int trackIntervalMs = 1000);
        void Stop();
        void ConfirmChangeId(int playerIndex, int changeId);
        void ConfirmAllChanges(Guid repoId, int playerIndex);
        Dictionary<int, float> GetPlayersMissRates(Guid repoId);
        int IncrementAndGetChangeId(Guid repoId);
    }
}