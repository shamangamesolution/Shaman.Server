using System;
using Shaman.Messages.General.Entity;

namespace Shaman.SyncedRepositories.Syncers
{
    public interface IRepositorySyncer
    {
        Guid GetId();
        void Start(int checkConfirmationIntervalMs = 1000, int forceSyncThreshold = 20, int queueDepth = 100, int clearQueuesIntervalMs = 1000, int trackIntervalMs = 1000, int sendTimes = 3);
        void Sync();
        int GetCurrentRevision();
        void ConfirmChangeId(int playerIndex, int changeId);
        void Stop();
    }

    public interface IDataLightRepositorySyncer<T> : IRepositorySyncer 
        where T: DataLightBase
    {
    }
}