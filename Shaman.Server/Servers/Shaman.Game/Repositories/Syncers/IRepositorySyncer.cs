using System;
using Shaman.Messages.General.Entity;

namespace Shaman.Game.Repositories.Syncers
{
    public interface IRepositorySyncer
    {
        Guid GetId();
        void Start(int checkConfirmationIntervalMs = 1000, int forceSyncThreshold = 20);
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