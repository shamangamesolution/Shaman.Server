using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Server.Messages;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.Contract.Common.Logging;
using Shaman.Routing.Balancing.Client;

namespace Shaman.Meta.Common
{
    // public interface IMetaProvider
    // {
    //     string GetFirstBackendUrl();
    //     string GetBackendUrl(int id);
    //     void Start(int getBackendListIntervalMs = 1000);
    //     void Stop();
    // }
    //
    // public class MetaProvider : IMetaProvider
    // {
    //     private readonly IRouterClient _routerClient;
    //     private readonly IShamanLogger _logger;
    //     private readonly ITaskScheduler _taskScheduler;
    //     private readonly ServerIdentity _serverIdentity;
    //
    //     private List<ServerInfo> _backends = new List<ServerInfo>();
    //     private int _getBackendsListRequestCount = 0;
    //     private ServerInfo _me;
    //     private IPendingTask _tickTask;
    //
    //     public MetaProvider(ITaskSchedulerFactory taskSchedulerFactory, IRouterClient routerClient,
    //         IServerIdentityProvider serverIdentityProvider, IShamanLogger logger)
    //     {
    //         _serverIdentity = serverIdentityProvider.Get();
    //         _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
    //         _routerClient = routerClient;
    //         _logger = logger;
    //         Load().Wait();
    //     }
    //
    //     public string GetFirstBackendUrl()
    //     {
    //         var backend = _backends.FirstOrDefault();
    //         if (backend == null)
    //             throw new Exception($"No backends in collection");
    //
    //         return BuildBackendUrl(backend);
    //     }
    //
    //     public string GetBackendUrl(int id)
    //     {
    //         var backend = _backends.FirstOrDefault(b => b.Id == id);
    //         if (backend == null)
    //             throw new Exception($"No backend with ID = {id}");
    //         return BuildBackendUrl(backend);
    //     }
    //
    //     private static string BuildBackendUrl(ServerInfo backend)
    //     {
    //         var protocol = backend.HttpsPort > 0 ? "https" : "http";
    //         var port = backend.HttpPort == 0 ? backend.HttpsPort : backend.HttpPort;
    //         return $"{protocol}://{backend.Address}:{port}";
    //     }
    //
    //     public void Start(int getBackendListIntervalMs = 1000)
    //     {
    //         _tickTask = _taskScheduler.ScheduleOnInterval(async () => await Load(), 2000, getBackendListIntervalMs);
    //     }
    //
    //     public void Stop()
    //     {
    //         _taskScheduler.Remove(_tickTask);
    //     }
    //
    //     private async Task Load()
    //     {
    //         var requestNumber = _getBackendsListRequestCount++;
    //
    //         //request backends list
    //         var serverInfos = await _routerClient.GetServerInfoList();
    //
    //         if (!serverInfos.Any())
    //             return;
    //
    //         var meList =
    //             serverInfos.Where(s => s.Identity.Equals(_serverIdentity)).ToArray();
    //         if (!meList.Any())
    //         {
    //             _logger.Error($"BackendProvider.Load error: can not find me({_serverIdentity}) in serve list");
    //             return;
    //         }
    //
    //         if (meList.Length > 1)
    //         {
    //             _logger.Error($"BackendProvider.Load attention: more than 1 servers matched me - (record ids: {string.Join<int>(",",meList.Select(m=>m.Id))}) in serve list");
    //         }
    //
    //         _me = meList.First();
    //
    //         var backends = serverInfos.Where(s =>
    //             s.ServerRole == ServerRole.BackEnd && s.Region == _me.Region &&
    //             s.IsApproved && s.ClientVersion == _me.ClientVersion).ToList();
    //
    //         if (!backends.Any())
    //             _logger.Error($"Received 0 backends from Router!");
    //         else
    //         {
    //             if (requestNumber == 1)
    //                 _logger.Info($"Received {backends.Count} backends from Router");
    //             _backends = backends;
    //         }
    //     }
    // }
}