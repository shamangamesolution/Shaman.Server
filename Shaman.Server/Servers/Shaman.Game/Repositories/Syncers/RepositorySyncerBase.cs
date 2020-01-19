using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Game.Repositories.Managers;
using Shaman.Messages.General.DTO.Events.RepositorySync;
using Shaman.Messages.General.Entity;

namespace Shaman.Game.Repositories.Syncers
{
    public class RevisionHistory
    {
        public int Revision { get; set; }

        public DateTime CreatedOn { get; set; }
        
        public long PlayersConfirm { get; set; }


        
        public void SetConfirm(int playerIndex)
        {
            PlayersConfirm = PlayersConfirm | (1 << playerIndex);
        }

        public bool IsConfirmedBy(int playerIndex)
        {
            return (PlayersConfirm & (1 << playerIndex)) != 0;
        }
        
        public RevisionHistory(int revision)
        {
            Revision = revision;
            CreatedOn =  DateTime.UtcNow;
        }
    }
    
    public abstract class RepositorySyncerBase<T, TEvent> : IDataLightRepositorySyncer<T>
        where T: DataLightBase, new()
        where TEvent: ForceSyncEventBase, new()
    {
        private int _historyQueueLength = 3;
        
        private readonly ISyncedRepository<T> _repo;
        protected readonly IRoom Room;
        private readonly IShamanLogger _logger;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IPlayerRepository _playerRepo;
        private readonly ISerializer _serializer;
        private readonly IConfirmationManager _confirmationManager;

        private int _currentRevision = 0;
        private PendingTask _getPlayersForForSyncTask;
        private int _baseRevisionSaved = 0;
        //private EventBase[] _sendEvents;
        private readonly object _mutex = new object();
        private ConcurrentQueue<ChangesContainerInfo<T>> _queue = new ConcurrentQueue<ChangesContainerInfo<T>>();
        private int _revisionHistoryDepth = 50;
        private object _historyMutex = new object();
        private ConcurrentDictionary<int, long> _confirmationHistory = new ConcurrentDictionary<int, long>();
        private int _forceSyncThreshold;
        private Guid _id;
        private int _maxSendTimes = 3;
        private ConcurrentDictionary<int, int> _revisionSendTimes = new ConcurrentDictionary<int, int>();
        
        public RepositorySyncerBase(ISyncedRepository<T> repo, IRoom room, ITaskScheduler taskScheduler, IPlayerRepository playerRepo, ISerializer serializer, IConfirmationManager confirmationManager, IShamanLogger logger)
        {
            _repo = repo;
            Room = room;
            _taskScheduler = taskScheduler;
            _playerRepo = playerRepo;
            _serializer = serializer;
            _confirmationManager = confirmationManager;
            _logger = logger;
            _id = Guid.NewGuid();
        }

        protected abstract void SendEvents(List<ChangesContainerInfo<T>> list);

        protected int GetChangeId()
        {
            return _confirmationManager.IncrementAndGetChangeId(GetId());
        }
        
        private void CutQueue()
        {
            while (_queue.Count > _historyQueueLength)
            {
                _queue.TryDequeue(out var item);
                _revisionSendTimes.TryRemove(item.Revision, out var item1);
            }
        }

        private void CutHistoryQueue()
        {
            lock (_historyMutex)
            {
                while (_queue.Count > _revisionHistoryDepth)
                    _queue.TryDequeue(out var item);
            }
        }

        private void CheckConfirmations()
        {
            var missRates = _confirmationManager.GetPlayersMissRates(_id);
            foreach (var item in missRates)
            {
                if (item.Value >= _forceSyncThreshold)
                {
                    if (_playerRepo.IsPlayerExist(item.Key))
                    {
                        var sessionId = _playerRepo.GetPlayerSessionId(item.Key);
                        _logger.Error($"Forcing player {item.Key} to refresh repo with {typeof(TEvent)}");
                        Room.AddToSendQueue(new TEvent(), sessionId);
                    }
                }
            }
        }

        public Guid GetId()
        {
            return _id;
        }
        
        public void Start(int checkConfirmationIntervalMs = 1000, int forceSyncThreshold = 20)
        {
            _forceSyncThreshold = forceSyncThreshold;
            _confirmationManager.Start();
            _getPlayersForForSyncTask = _taskScheduler.ScheduleOnInterval(CheckConfirmations, checkConfirmationIntervalMs, checkConfirmationIntervalMs, true);
        }

        public void Sync()
        {
            lock (_mutex)
            {
                //cut queues
                CutQueue();
                CutHistoryQueue();
                
                var changesContainer = _repo.GetChanges();
                if (changesContainer.IsEmpty())
                    return;
                //copy container
                //TODO optimize copying
                var container =
                    _serializer.DeserializeAs<ChangesContainer<T>>(_serializer.Serialize(changesContainer));
                _queue.Enqueue(new ChangesContainerInfo<T>(container, _currentRevision));
                //flush current changes
                _repo.FlushChanges();
                //update current revision
                _currentRevision++;
                //send event

                var list = _queue.ToList();
                var revisionListToDelete = new List<int>();
                foreach (var item in list)
                {
                    if (!_revisionSendTimes.ContainsKey(item.Revision))
                        _revisionSendTimes.TryAdd(item.Revision, 1);

                    if (_revisionSendTimes[item.Revision] > _maxSendTimes)
                    {
                        revisionListToDelete.Add(item.Revision);
                        continue;
                    }

                    _revisionSendTimes[item.Revision]++;
                }

                foreach (var item in revisionListToDelete)
                {
                    list.RemoveAll(i => i.Revision == item);
                    _revisionSendTimes.TryRemove(item, out var removedItem);
                }

                SendEvents(list);
            }
        }

        public void ConfirmChangeId(int playerIndex, int revision)
        {
            _confirmationManager.ConfirmChangeId(playerIndex, revision);
        }

        public int GetCurrentRevision()
        {
            lock (_mutex)
            {
                return _currentRevision;
            }
        }

        
        public void Stop()
        {
            _queue.Clear();
            _confirmationManager.Stop();
            _taskScheduler.Remove(_getPlayersForForSyncTask);
        }
    }
}