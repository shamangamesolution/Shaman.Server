using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Repositories.Syncers;
using Shaman.Messages.General.DTO.Events.RepositorySync;

namespace Shaman.Game.Repositories.Managers
{
    public class SyncersManager : ISyncersManager
    {
        private readonly IShamanLogger _logger;
        private readonly IConfirmationManager _confirmationManager;
        private readonly ITaskScheduler _taskScheduler;
        private List<IRepositorySyncer> _syncers = new List<IRepositorySyncer>();
        private object _mutex = new object();
        private Dictionary<Type, IRepositorySyncer> _typeToSyncers = new Dictionary<Type, IRepositorySyncer>();
        private Dictionary<Guid, IRepositorySyncer> _idToSyncers = new Dictionary<Guid, IRepositorySyncer>();
        
        public SyncersManager(ITaskScheduler taskScheduler, IShamanLogger logger, IConfirmationManager confirmationManager)
        {
            _taskScheduler = taskScheduler;
            _logger = logger;
            _confirmationManager = confirmationManager;
        }
        
        public void Start(int intervalMs)
        {
            _taskScheduler.ScheduleOnInterval(SyncAll, 0, intervalMs, true);
        }

        public void AddSyncer<T>(IRepositorySyncer syncer, int checkConfirmationInterval = 1000, int forceSyncThreshold = 20) where T:ConfirmChangeIdEventBase
        {
            lock (_mutex)
            {
                _syncers.Add(syncer);
                _typeToSyncers.Add(typeof(T), syncer);
                syncer.Start(checkConfirmationInterval, forceSyncThreshold);
                _idToSyncers.Add(syncer.GetId(), syncer);
            }
        }

        public void ProcessConfirmChangeIdEvent(int playerIndex, ConfirmChangeIdEventBase eve)
        {
            var type = eve.GetType();
            if (!_typeToSyncers.TryGetValue(type, out var syncer))
            {
                _logger.Error($"ProcessConfirmChangeIdEvent error: Can not get syncer for type {type}");
                return;
            }
            syncer.ConfirmChangeId(playerIndex, eve.ChangeId);
        }

        public int GetCurrentRevision(Guid syncerId)
        {
            if (!_idToSyncers.TryGetValue(syncerId, out var syncer))
                throw new Exception($"Syncer {syncerId} can not bye found");

            return syncer.GetCurrentRevision();
        }

        public void ConfirmAllChanges(Guid repoId, int playerIndex)
        {
            if (!_idToSyncers.TryGetValue(repoId, out var syncer))
            {
                _logger.Error($"ConfirmAllChanges error: can not get repo with id {repoId}");
                return;
            }
            
            _confirmationManager.ConfirmAllChanges(repoId, playerIndex);
        }

        public void SyncAll()
        {
            lock (_mutex)
            {
                _syncers.ForEach(s => s.Sync());
            }
        }

        public void Stop()
        {
            lock (_mutex)
            {
                _syncers.ForEach(s => s.Stop());
                _syncers.Clear();
                _typeToSyncers.Clear();
                _idToSyncers.Clear();
            }
        }
    }
}