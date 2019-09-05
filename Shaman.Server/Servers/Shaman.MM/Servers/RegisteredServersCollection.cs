using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.Configuration;
using Shaman.Messages.MM;

namespace Shaman.MM.Servers
{
    public class RegisteredServersCollection : IRegisteredServerCollection
    {
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private IApplicationConfig _config;
        private IShamanLogger _logger;
        private object _syncCollection = new object();
        private ConcurrentDictionary<ServerIdentity, RegisteredServer> _servers = new ConcurrentDictionary<ServerIdentity, RegisteredServer>(new ServerIdentity.EqualityComparer());

        //debug
        private Guid _id;
        
        public RegisteredServersCollection(IShamanLogger logger, IApplicationConfig config, ITaskSchedulerFactory taskSchedulerFactory)
        {
            _logger = logger;
            _config = config;
            _taskSchedulerFactory = taskSchedulerFactory;
            _taskScheduler = _taskSchedulerFactory.GetTaskScheduler();
            _id = Guid.NewGuid();
            
            _taskScheduler.ScheduleOnInterval(() =>
            {
                lock (_syncCollection)
                {
                    var idsToUnregister = new List<ServerIdentity>();
                    foreach (var server in _servers)
                    {
                        if (!server.Value.ActualizedOnNotOlderThan(((MmApplicationConfig)_config).ServerInactivityTimeoutMs))
                            idsToUnregister.Add(server.Value.Id);
                    }
                    
                    foreach(var item in idsToUnregister)
                        UnregisterServer(item);
                }
            }, 0, ((MmApplicationConfig)_config).ServerUnregisterTimeoutMs);
            
            _logger.Debug($"ServerCollection constructor called. Id = {_id} ");
        }
        
        private RegisteredServer Get(ServerIdentity id)
        {
            lock (_syncCollection)
            {
                if (!_servers.TryGetValue(id, out var server))
                    return null;

                return server;
            }
        }
        
        public void RegisterServer(RegisteredServer server)
        {
            lock (_syncCollection)
            {
                server.ActualizedOn = DateTime.UtcNow;
                server.RegisteredOn = DateTime.UtcNow;

                _servers.TryAdd(server.Id, server);
                _logger.Info($"Server {server.Id} registered (create room url {server.CreateRoomUrl})");
            }
        }

        public void ActualizeServer(ServerIdentity id, Dictionary<ushort, int> peersCountPerPort)
        {
            var server = Get(id);
            
            lock (_syncCollection)
            {
                if (server == null)
                {
                    _logger.Error($"ActualizeServer error: server does not exist {id}");
                    return;
                }
                
                server.Actualize(peersCountPerPort);
                _logger.Debug($"Server {server.Id} actualized");

            }
        }

        public void UnregisterServer(ServerIdentity id)
        {
            var server = Get(id);
            
            lock (_syncCollection)
            {
                if (server == null)
                {
                    _logger.Error($"UnregisterServer error: server does not exist {id}");
                    return;
                }

                _servers.TryRemove(id, out server);
                _logger.Info($"Server {id} unregistered");
            }
        }

        public RegisteredServer GetLessLoadedServer()
        {
            lock (_syncCollection)
            {
                if (!_servers.Any(s => s.Value.ActualizedOnNotOlderThan(((MmApplicationConfig)_config).ServerInactivityTimeoutMs)))
                {
                    _logger.Error($"GetLessLoadedServer error: server collection is empty");
                    return null;
                }
                
                return _servers.Where(s => s.Value.ActualizedOnNotOlderThan(((MmApplicationConfig)_config).ServerInactivityTimeoutMs)).OrderBy(s => s.Value.TotalPeers).FirstOrDefault().Value;
            }
        }

        public List<RegisteredServer> GetAll()
        {
            lock (_syncCollection)
            {
                return _servers.Select(s=> s.Value).ToList();
            }
        }

        public void Clear()
        {
            lock (_syncCollection)
            {
                _servers.Clear();
            }
        }

        public bool Contains(ServerIdentity id)
        {
            lock (_syncCollection)
            {
                return _servers.ContainsKey(id);
            }
        }
    }
}