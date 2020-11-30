using System;
using Shaman.Messages.General.DTO.Events.RepositorySync;
using Shaman.SyncedRepositories.Syncers;

namespace Shaman.SyncedRepositories.Managers
{
    public interface ISyncersManager
    {
        void Start(int intervalMs);
        void AddSyncer<T>(IRepositorySyncer syncer, int checkConfirmationInterval = 1000, int forceSyncThreshold = 20, int queueDepth = 100, int clearQueuesIntervalMs = 1000, int trackIntervalMs = 1000, int sendTimes = 3) where T: ConfirmChangeIdEventBase;
        void ProcessConfirmChangeIdEvent(int playerIndex, ConfirmChangeIdEventBase eve);
        int GetCurrentRevision(Guid syncerId);
        void ConfirmAllChanges(Guid repoId, int playerIndex);
        void SyncAll();
        void Stop();
    }
}