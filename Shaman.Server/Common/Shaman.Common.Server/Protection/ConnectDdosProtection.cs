using System;
using System.Collections.Generic;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Server.Protection
{

    
    public interface IConnectDdosProtection
    {
        void OnPeerConnected(IPEndPoint endPoint);
        bool IsBanned(IPEndPoint endPoint);
        void Start();
        void Stop();
    }
    
    public class ConnectDdosProtection : IConnectDdosProtection
    {
        private readonly IProtectionManagerConfig _config;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IShamanLogger _logger;

        private IPendingTask _pendingTask, _bannedPendingTask;
        private Dictionary<string, int> _connectsFromIp = new Dictionary<string, int>();
        private Dictionary<string, DateTime> _bannedTill = new Dictionary<string, DateTime>();
        
        private object _mutex = new object();
        private object _bannedMutex = new object();
        
        public ConnectDdosProtection(
            IProtectionManagerConfig config, 
            ITaskSchedulerFactory taskSchedulerFactory,
            IShamanLogger logger)
        {
            _config = config;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _logger = logger;
        }

        private string GetIp(IPEndPoint endPoint)
        {
            return endPoint.Address.ToString();
        }

        public void OnPeerConnected(IPEndPoint endPoint)
        {
            lock (_mutex)
            {
                var ip = GetIp(endPoint);
                if (!_connectsFromIp.ContainsKey(ip))
                    _connectsFromIp.Add(ip, 1);
                else
                    _connectsFromIp[ip]++;
            }
        }

        public bool IsBanned(IPEndPoint endPoint)
        {
            lock (_bannedMutex)
            {
                var ip = GetIp(endPoint);
                return _bannedTill.ContainsKey(ip);
            }
        }

        private void BannedTick()
        {
            lock (_bannedMutex)
            {
                var toDelete = new HashSet<string>();
                foreach (var item in _bannedTill)
                {
                    if (item.Value <= DateTime.UtcNow)
                        toDelete.Add(item.Key);
                }

                foreach (var item in toDelete)
                {
                    _bannedTill.Remove(item);
                }
            }
        }
        
        private void Tick()
        {
            lock (_mutex)
            {
                foreach (var item in _connectsFromIp)
                {
                    if (item.Value >= _config.MaxConnectsFromSingleIp)
                    {
                        _logger.Error($"Ddos probably: ip {item.Key}");
                        if (!_bannedTill.ContainsKey(item.Key))
                            _bannedTill.Add(item.Key, DateTime.UtcNow.AddMilliseconds(_config.BanDurationMs));
                    }
                }
                _connectsFromIp.Clear();
            }
        }
        
        public void Start()
        {
            _pendingTask = _taskScheduler.ScheduleOnInterval(Tick, 0, _config.ConnectionCountCheckIntervalMs);
            _bannedPendingTask = _taskScheduler.ScheduleOnInterval(BannedTick, 0, _config.BanCheckIntervalMs);
        }

        public void Stop()
        {
            _taskScheduler.Remove(_pendingTask);
            _taskScheduler.Remove(_bannedPendingTask);
            _connectsFromIp.Clear();
            _bannedTill.Clear();
        }
    }
}