using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing;
using Shaman.Router.Config;
using Shaman.Router.Data.Repositories.Interfaces;
using Shaman.Router.Models;

namespace Shaman.Router.Data.Providers;

public interface IStatesManager
{
    Task<string> GetState(ServerIdentity identity);
    Task SaveState(ServerIdentity identity, string state);
    void Start();
    void Stop();
}

public class StatesManager:IStatesManager
{
    private readonly IShamanLogger _logger;
    private readonly IStateRepository _stateRepository;
    private readonly IRouterServerInfoProvider _routerServerInfoProvider;
    private readonly ITaskScheduler _taskScheduler;
    private readonly IOptions<RouterConfiguration> _config;

    private Dictionary<int, StateInfo> _states = new Dictionary<int, StateInfo>();
    private bool _isRequestingNow = false;

    public StatesManager(IStateRepository stateRepository, IRouterServerInfoProvider routerServerInfoProvider, IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IOptions<RouterConfiguration> config)
    {
        _stateRepository = stateRepository;
        _routerServerInfoProvider = routerServerInfoProvider;
        _logger = logger;
        _config = config;
        _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
    }

    public async Task<string> GetState(ServerIdentity identity)
    {
        var server = _routerServerInfoProvider.GetAllServers().FirstOrDefault(s => s.Identity.Equals(identity));
        if (server == null)
        {
            _logger.Error($"Cant get state for server {identity}. No server in collection");
            return null;
        }
        
        if (!_states.TryGetValue(server.Id, out var state))
            return null;

        return state.SerializedState;
    }

    private async Task RefreshStates()
    {
        _states = (await _stateRepository.GetStates()).ToDictionary(k => k.ServerId, v => v);
    }

    public async Task SaveState(ServerIdentity identity, string state)
    {
        var server = _routerServerInfoProvider.GetAllServers().FirstOrDefault(s => s.Identity.Equals(identity));
        if (server == null)
        {
            _logger.Error($"Cant get state for server {identity}. No server in collection");
            return;
        }

        var now = DateTime.UtcNow;
        // races might occur, but who cares in this case
        if (_states.ContainsKey(server.Id))
            await _stateRepository.UpdateState(server.Id, state, now);
        else
            await _stateRepository.SaveState(server.Id, state, now);
        _states[server.Id] = new StateInfo
        {
            CreatedOn = now,
            SerializedState = state,
            ServerId = server.Id
        };
    }
    
    public void Start()
    {
        // initial load
        RefreshStates();
            
        _taskScheduler.ScheduleOnInterval(async () =>
        {
            if (_isRequestingNow)
                return;

            _isRequestingNow = true;

            try
            {
                await RefreshStates();
            }
            finally
            {
                _isRequestingNow = false;
            }

                
        }, _config.Value.ServerInfoListUpdateIntervalMs, _config.Value.ServerInfoListUpdateIntervalMs);
    }
    
    public void Stop()
    {
        _taskScheduler.RemoveAll();
    }
}