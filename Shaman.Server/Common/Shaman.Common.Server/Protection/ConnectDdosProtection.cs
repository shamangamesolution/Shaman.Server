using System;
using System.Collections.Concurrent;
using System.Net;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle.Stats;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Server.Protection
{
    public interface IConnectDdosProtection
    {
        bool OnPeerConnected(IPEndPoint endPoint);
        bool IsBanned(IPEndPoint endPoint);
        void Start();
        void Stop();
    }

    public class ConnectDdosProtection : IConnectDdosProtection
    {
        private readonly IProtectionManagerConfig _config;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IShamanLogger _logger;
        private readonly IServerMetrics _serverMetrics;

        private IPendingTask _pendingTask, _bannedPendingTask;
        private readonly ConcurrentDictionary<string, int> _connectsFromIp = new();
        private readonly ConcurrentDictionary<string, DateTime> _bannedTill = new();
        private int _maxConnectionsFromIpOnTick = 0;

        public ConnectDdosProtection(
            IProtectionManagerConfig config,
            ITaskSchedulerFactory taskSchedulerFactory,
            IShamanLogger logger, IGameMetrics serverMetrics)
        {
            _config = config;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _logger = logger;
            _serverMetrics = serverMetrics;
        }

        private string GetIp(IPEndPoint endPoint)
        {
            return endPoint.Address.ToString();
        }

        public bool OnPeerConnected(IPEndPoint endPoint)
        {
            var ip = GetIp(endPoint);
            var count = _connectsFromIp.AddOrUpdate(ip, 1, (key, oldValue) => oldValue + 1);
            _logger.Info($"Connects from ip: {ip} {count}");
            if (_maxConnectionsFromIpOnTick < count) // accept accuracy loss due to possible race condition
                _maxConnectionsFromIpOnTick = count;
            if (count >= _config.MaxConnectsFromSingleIp)
            {
                _logger.Error($"Ddos probably: ip {ip}");
                // add or prolong ban
                _bannedTill.AddOrUpdate(ip, GetUtcNow().AddMilliseconds(_config.BanDurationMs),
                    (_, time) => time.AddMilliseconds(_config.BanDurationMs));
                return false;
            }

            return true;
        }

        public bool IsBanned(IPEndPoint endPoint) => _bannedTill.ContainsKey(GetIp(endPoint));

        private void BannedTick()
        {
            foreach (var item in _bannedTill)
            {
                if (item.Value <= GetUtcNow())
                    _bannedTill.TryRemove(item.Key, out _);
            }
        }

        private static DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        private void CheckTick()
        {
            _logger.Info($"Clearing connects ({_maxConnectionsFromIpOnTick}) ");
            _serverMetrics.TrackMaxConnectionsFromIp(_maxConnectionsFromIpOnTick);
            
            // accept moving max count to next scrape interval
            _maxConnectionsFromIpOnTick = 0;
            _connectsFromIp.Clear();
        }

        private bool _started;

        public void Start()
        {
            if (_started)
                return;

            _started = true;
            _logger.Warning(
                $"DDOS protection activated. Max connects from single ip: {_config.MaxConnectsFromSingleIp}, ban duration: {_config.BanDurationMs} ms, check interval: {_config.ConnectionCountCheckIntervalMs} ms, ban check interval: {_config.BanCheckIntervalMs} ms");
            _pendingTask = _taskScheduler.ScheduleOnInterval(CheckTick, 0, _config.ConnectionCountCheckIntervalMs);
            _bannedPendingTask = _taskScheduler.ScheduleOnInterval(BannedTick, 0, _config.BanCheckIntervalMs);
        }

        public void Stop()
        {
            _taskScheduler.Remove(_pendingTask);
            _taskScheduler.Remove(_bannedPendingTask);
            _connectsFromIp.Clear();
            _bannedTill.Clear();
            _started = false;
        }
    }
}