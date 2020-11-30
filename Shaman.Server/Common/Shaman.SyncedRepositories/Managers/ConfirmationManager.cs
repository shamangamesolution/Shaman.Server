using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.SyncedRepositories.Managers
{
    public class ConfirmationManager : IConfirmationManager
    {
        private int _queueDepth = 100;
        private int _clearQueuesIntervalMs = 1000;
        private int _trackIntervalMs = 200;
        
        private readonly IPlayerRepository _playerRepo;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IShamanLogger _logger;
        private ConcurrentDictionary<int, long> _confirmationHistory = new ConcurrentDictionary<int, long>();
        private ConcurrentDictionary<int, DateTime> _confirmationHistoryTime = new ConcurrentDictionary<int, DateTime>();
        private ConcurrentDictionary<int, Guid> _confirmationChangeIdToRepoId = new ConcurrentDictionary<int, Guid>();
        private ConcurrentDictionary<Guid, HashSet<int>> _confirmationRepoToChangeId = new ConcurrentDictionary<Guid, HashSet<int>>();
        
        private object _queueSync = new object();
        private ConcurrentQueue<int> _changesQueue = new ConcurrentQueue<int>();
        private IPendingTask _clearTask;
        private object _changeIdMutex = new object();
        private int _currentChangeId;
        
        public ConfirmationManager(IPlayerRepository playerRepo, ITaskScheduler taskScheduler, IShamanLogger logger)
        {
            _playerRepo = playerRepo;
            _taskScheduler = taskScheduler;
            _logger = logger;
        }

        public int IncrementAndGetChangeId(Guid repoId)
        {
            lock (_changeIdMutex)
            {
                _currentChangeId++;
                AddChangeId(repoId, _currentChangeId);
                return _currentChangeId;
            }
        }
        
        private void CutQueues()
        {
            lock (_queueSync)
            {
                while (_changesQueue.Count > _queueDepth)
                {
                    if (_changesQueue.TryDequeue(out var item))
                    {
                        _confirmationHistory.Remove(item, out var a);
                        _confirmationHistoryTime.Remove(item, out var b);
                        if (_confirmationChangeIdToRepoId.TryGetValue(item, out var repoId))
                        {
                            if (_confirmationRepoToChangeId.TryGetValue(repoId, out var repoItem))
                                repoItem.Remove(item);
                        }
                    }
                }
            }
        }
        
        public bool IsConfirmedBy(long value, int playerIndex)
        {
            return (value & (1 << playerIndex)) != 0;
        }

        public void Start(int queueDepth = 100, int clearQueuesIntervalMs = 1000, int trackIntervalMs = 1000)
        {
            _queueDepth = queueDepth;
            _clearQueuesIntervalMs = clearQueuesIntervalMs;
            _trackIntervalMs = trackIntervalMs;
            
            _clearTask = _taskScheduler.ScheduleOnInterval(CutQueues, 0, _clearQueuesIntervalMs, true);
        }

        public void Stop()
        {
            _taskScheduler.Remove(_clearTask);
        }

        public void ConfirmChangeId(int playerIndex, int changeId)
        {
            lock (_queueSync)
            {
                if (!_confirmationHistory.ContainsKey(changeId))
                    return;

                _confirmationHistory[changeId] = _confirmationHistory[changeId] | (1 << playerIndex);
            }
        }

        public void ConfirmAllChanges(Guid repoId, int playerIndex)
        {
            _confirmationRepoToChangeId.TryAdd(repoId, new HashSet<int>());
            foreach (var index in _confirmationRepoToChangeId[repoId])
            {
                ConfirmChangeId(playerIndex, index);

            }
        }

        public void AddChangeId(Guid repoId, int changeId)
        {
            lock (_queueSync)
            {
                _confirmationHistory.TryAdd(changeId, 0);
                _changesQueue.Enqueue(changeId);
                _confirmationHistoryTime.TryAdd(changeId, DateTime.UtcNow);
                _confirmationRepoToChangeId.TryAdd(repoId, new HashSet<int>());
                _confirmationRepoToChangeId[repoId].Add(changeId);
                _confirmationChangeIdToRepoId.TryAdd(changeId, repoId);
            }
        }

        public Dictionary<int, float> GetPlayersMissRates(Guid repoId)
        {
            var result = new Dictionary<int, float>();
            var indexes = _playerRepo.GetHumanPlayerIndexes().ToList();
            var confirmationMisses = new Dictionary<int, int>();
            if (!_confirmationRepoToChangeId.ContainsKey(repoId))
                throw new Exception($"GetPlayersMissRates error: no repo {repoId}");
            
            lock (_queueSync)
            {
                var totalChanges = _confirmationRepoToChangeId[repoId].Count;
                foreach (var index in indexes)
                {
                    confirmationMisses.Add(index, 0);
                    result.Add(index, 0);
                }

                foreach (var item in _confirmationRepoToChangeId[repoId])
                {
                    if ((DateTime.UtcNow - _confirmationHistoryTime[item]).TotalMilliseconds < _trackIntervalMs)
                        continue;
                    
                    foreach (var index in indexes)
                    {
                        var value = _confirmationHistory[item];
                        if (!IsConfirmedBy(value, index))
                        {
                            confirmationMisses[index]++;
                        }
                    }
                }


                foreach (var index in indexes)
                {
                    result[index] = 100 * (float) confirmationMisses[index] / totalChanges;
                    if (result[index] == 100)
                    {
                        _logger.Error($"");
                    }
                }

                return result;
            }
        }
    }
}